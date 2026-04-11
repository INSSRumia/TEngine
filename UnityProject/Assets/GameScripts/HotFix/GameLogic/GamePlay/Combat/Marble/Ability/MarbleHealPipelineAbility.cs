using System.Collections.Generic;
using TEngine;
using UnityEngine;
using GameLogic.GamePlay.Combat;

namespace GameLogic.Gameplay.Combat.Marble
{
    public partial class MarbleHealPipelineAbility : Ability<Marble>
    {
        public enum HealStage
        {
            None = 0,
            Receive = 1,
            Calculate = 2,
            Apply = 3,
            Completed = 4,
        }

        public sealed class HealContext : MemoryObject
        {
            public ASC Source { get; private set; }
            public ASC Target { get; private set; }
            public HealStage Stage { get; set; }
            public int InputValue { get; set; }
            public int FinalValue { get; set; }

            public void Reset(ASC source, ASC target, int inputValue)
            {
                Source = source;
                Target = target;
                Stage = HealStage.None;
                InputValue = inputValue;
                FinalValue = 0;
            }

            public override void Clear()
            {
                Source = null;
                Target = null;
                Stage = HealStage.None;
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

        private readonly Queue<HealContext> _pendingContexts = new Queue<HealContext>();
        private bool _isProcessing;
        public HealContext CurrentContext { get; private set; }

        public override int Priority => 10000;

        public void Execute(int value, ASC source = null)
        {
            if (Owner == null || Owner.RuntimeData == null || value <= 0)
                return;

            var context = MemoryPool.Alloc<HealContext>();
            context.Reset(source, Owner, value);

            if (_isProcessing)
            {
                _pendingContexts.Enqueue(context);
                return;
            }

            ProcessContext(context);
        }

        private void ProcessContext(HealContext context)
        {
            if (context == null)
                return;

            var wasProcessing = _isProcessing;
            _isProcessing = true;
            CurrentContext = context;
            try
            {
                context.Stage = HealStage.Receive;
                foreach (var ability in Owner.GetAbilities<IAfterReceiveHeal>())
                {
                    ability.AfterReceiveHeal(this);
                }

                context.Stage = HealStage.Calculate;
                context.FinalValue = Mathf.Max(0,
                    Mathf.RoundToInt((context.InputValue + Owner.RuntimeData.HealAddition) * Owner.RuntimeData.HealMultiplier));
                foreach (var ability in Owner.GetAbilities<IAfterCalculateHeal>())
                {
                    ability.AfterCalculateHeal(this);
                }

                if (context.FinalValue > 0)
                {
                    context.Stage = HealStage.Apply;
                    foreach (var ability in Owner.GetAbilities<IAfterApplyHeal>())
                    {
                        ability.AfterApplyHeal(this);
                    }
                }

                context.Stage = HealStage.Completed;
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
