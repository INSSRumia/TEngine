using System.Collections.Generic;
using TEngine;
using UnityEngine;
using GameLogic.GamePlay.Common;

namespace GameLogic.Marble
{
    public partial class MarbleShieldPipelineAbility : Ability<MarbleRuntimeData>, IReceiveShield
    {
        public enum ShieldStage
        {
            None = 0,
            Receive = 1,
            Calculate = 2,
            Apply = 3,
            Completed = 4,
        }

        public sealed class ShieldContext : MemoryObject
        {
            public ASC Source { get; private set; }
            public ASC Target { get; private set; }
            public ShieldStage Stage { get; set; }
            public int InputValue { get; set; }
            public int FinalValue { get; set; }

            public void Reset(ASC source, ASC target, int inputValue)
            {
                Source = source;
                Target = target;
                Stage = ShieldStage.None;
                InputValue = inputValue;
                FinalValue = 0;
            }

            public override void Clear()
            {
                Source = null;
                Target = null;
                Stage = ShieldStage.None;
                InputValue = 0;
                FinalValue = 0;
            }

            public override void InitFromPool()
            {
                Clear();
            }

            public override void RecycleToPool()
            {
                Clear();
            }
        }

        private readonly Queue<ShieldContext> _pendingContexts = new Queue<ShieldContext>();
        private bool _isProcessing;
        public ShieldContext CurrentContext { get; private set; }

        public override int Priority => 10000;

        public void ReceiveShield(int value, ASC source = null)
        {
            Execute(value, source);
        }

        public void Execute(int value, ASC source = null)
        {
            if (Owner == null || Owner.RuntimeData == null || value <= 0)
                return;

            var context = MemoryPool.Alloc<ShieldContext>();
            context.Reset(source, Owner, value);

            if (_isProcessing)
            {
                _pendingContexts.Enqueue(context);
                return;
            }

            ProcessContext(context);
        }

        private void ProcessContext(ShieldContext context)
        {
            if (context == null)
                return;

            var wasProcessing = _isProcessing;
            _isProcessing = true;
            CurrentContext = context;
            try
            {
                context.Stage = ShieldStage.Receive;
                foreach (var ability in Owner.GetAbilities<IAfterReceiveShield>())
                {
                    ability.AfterReceiveShield(this);
                }

                context.Stage = ShieldStage.Calculate;
                context.FinalValue = Mathf.Max(0,
                    Mathf.RoundToInt((context.InputValue + Owner.RuntimeData.ShieldAddition) * Owner.RuntimeData.ShieldMultiplier));
                foreach (var ability in Owner.GetAbilities<IAfterCalculateShield>())
                {
                    ability.AfterCalculateShield(this);
                }

                if (context.FinalValue > 0)
                {
                    context.Stage = ShieldStage.Apply;
                    foreach (var ability in Owner.GetAbilities<IAfterApplyShield>())
                    {
                        ability.AfterApplyShield(this);
                    }
                }

                context.Stage = ShieldStage.Completed;
            }
            finally
            {
                CurrentContext = null;
                MemoryPool.Dealloc(context);
                _isProcessing = wasProcessing;
            }

            if (wasProcessing)
                return;

            while (_pendingContexts.Count > 0)
            {
                ProcessContext(_pendingContexts.Dequeue());
            }
        }
    }
}
