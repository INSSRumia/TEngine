using System.Collections.Generic;
using TEngine;
using UnityEngine;
using GameLogic.GamePlay.Combat;
using UnityEngine.Pool;

namespace GameLogic.Gameplay.Combat.Marble
{
    public partial class MarbleShieldHealPipelineAbility : Ability<Marble>, IReceiveShield
    {
        public enum ShieldStage
        {
            None = 0,
            Receive = 1,
            Calculate = 2,
            Apply = 3,
            Completed = 4,
        }

        public sealed class ShieldHealContext : MemoryObject
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

        private readonly Queue<ShieldHealContext> _pendingContexts = new Queue<ShieldHealContext>();
        private bool _isProcessing;
        public ShieldHealContext CurrentHealContext { get; private set; }

        public void ReceiveShield(int value, ASC source = null)
        {
            Execute(value, source);
        }

        public void Execute(int value, ASC source = null)
        {
            if (Owner == null || Owner.RuntimeData == null || value <= 0)
                return;

            var context = MemoryPool.Alloc<ShieldHealContext>();
            context.Reset(source, Owner, value);

            if (_isProcessing)
            {
                _pendingContexts.Enqueue(context);
                return;
            }

            ProcessContext(context);
        }

        private void ProcessContext(ShieldHealContext healContext)
        {
            if (healContext == null)
                return;

            var wasProcessing = _isProcessing;
            _isProcessing = true;
            CurrentHealContext = healContext;
            try
            {
                healContext.Stage = ShieldStage.Receive;
                var lstAfterReceiveShieldAbilities = ListPool<IAfterReceiveShield>.Get();
                Owner.GetAbilities<IAfterReceiveShield>(ref lstAfterReceiveShieldAbilities);
                foreach (var ability in lstAfterReceiveShieldAbilities)
                    ability.AfterReceiveShield(this);
                ListPool<IAfterReceiveShield>.Release(lstAfterReceiveShieldAbilities);

                healContext.Stage = ShieldStage.Calculate;
                healContext.FinalValue = Mathf.Max(0,
                    Mathf.RoundToInt((healContext.InputValue + Owner.RuntimeData.ShieldHealAddition) * Owner.RuntimeData.ShieldHealMultiplier));
                var lstAfterCalculateShieldAbilities = ListPool<IAfterCalculateShield>.Get();
                Owner.GetAbilities<IAfterCalculateShield>(ref lstAfterCalculateShieldAbilities);
                foreach (var ability in lstAfterCalculateShieldAbilities)
                    ability.AfterCalculateShield(this);
                ListPool<IAfterCalculateShield>.Release(lstAfterCalculateShieldAbilities);

                if (healContext.FinalValue > 0)
                {
                    healContext.Stage = ShieldStage.Apply;
                    var lstAfterApplyShieldAbilities = ListPool<IAfterApplyShield>.Get();
                    Owner.GetAbilities<IAfterApplyShield>(ref lstAfterApplyShieldAbilities);
                    foreach (var ability in lstAfterApplyShieldAbilities)
                        ability.AfterApplyShield(this);
                    ListPool<IAfterApplyShield>.Release(lstAfterApplyShieldAbilities);
                }

                healContext.Stage = ShieldStage.Completed;
            }
            finally
            {
                CurrentHealContext = null;
                MemoryPool.Dealloc(healContext);
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
