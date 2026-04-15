using System.Collections.Generic;
using TEngine;
using UnityEngine;
using GameLogic.Gameplay.Combat;
using UnityEngine.Pool;

namespace GameLogic.Gameplay.Combat.Marble
{
    public partial class MarbleDamagePipelineAbility : MarbleAbility
    {
        public MarbleDamagePipelineAbility(GameConfig.Gameplay.Combat.MarbleDamagePipelineAbilityConfig config)
        {
            Priority = config.Priority;
        }

        public enum DamageStage
        {
            None = 0,
            Receive = 1,
            Calculate = 2,
            Apply = 3,
            Completed = 4,
        }

        public sealed class DamageContext : MemoryObject
        {
            public ASC Source { get; private set; }
            public ASC Target { get; private set; }
            public DamageStage Stage { get; set; }
            public int InputValue { get; set; }
            public int FinalValue { get; set; }

            public void Reset(ASC source, ASC target, int inputValue)
            {
                Source = source;
                Target = target;
                Stage = DamageStage.None;
                InputValue = inputValue;
                FinalValue = 0;
            }

            public override void Clear()
            {
                Source = null;
                Target = null;
                Stage = DamageStage.None;
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

        private readonly Queue<DamageContext> _pendingContexts = new Queue<DamageContext>();
        private bool _isProcessing;
        public DamageContext CurrentContext { get; private set; }

        public void Execute(int value, ASC source = null)
        {
            if (Owner == null || Owner.RuntimeData == null || value <= 0)
                return;

            var context = MemoryPool.Alloc<DamageContext>();
            context.Reset(source, Owner, value);

            if (_isProcessing)
            {
                _pendingContexts.Enqueue(context);
                return;
            }

            ProcessContext(context);
        }

        private void ProcessContext(DamageContext context)
        {
            if (context == null)
                return;

            var wasProcessing = _isProcessing;
            _isProcessing = true;
            CurrentContext = context;
            try
            {
                context.Stage = DamageStage.Receive;
                var lstAfterReceiveDamageAbilities = ListPool<IAfterReceiveDamage>.Get();
                Owner.GetAbilities<IAfterReceiveDamage>(ref lstAfterReceiveDamageAbilities);
                foreach (var ability in lstAfterReceiveDamageAbilities)
                    ability.AfterReceiveDamage(this);
                ListPool<IAfterReceiveDamage>.Release(lstAfterReceiveDamageAbilities);

                context.Stage = DamageStage.Calculate;
                context.FinalValue = Mathf.Max(0, Mathf.RoundToInt((context.InputValue + Owner.RuntimeData.DamageAddition) * Owner.RuntimeData.DamageMultiplier) - Owner.RuntimeData.Defense);
                var lstAfterCalculateDamageAbilities = ListPool<IAfterCalculateDamage>.Get();
                Owner.GetAbilities<IAfterCalculateDamage>(ref lstAfterCalculateDamageAbilities);
                foreach (var ability in lstAfterCalculateDamageAbilities)
                    ability.AfterCalculateDamage(this);
                ListPool<IAfterCalculateDamage>.Release(lstAfterCalculateDamageAbilities);

                if (context.FinalValue > 0)
                {
                    context.Stage = DamageStage.Apply;
                    ApplyDamage(context.FinalValue);

                    var lstAfterApplyDamageAbilities = ListPool<IAfterApplyDamage>.Get();
                    Owner.GetAbilities<IAfterApplyDamage>(ref lstAfterApplyDamageAbilities);
                    foreach (var ability in lstAfterApplyDamageAbilities)
                        ability.AfterApplyDamage(this);
                    ListPool<IAfterApplyDamage>.Release(lstAfterApplyDamageAbilities);
                }

                context.Stage = DamageStage.Completed;
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

        private void ApplyDamage(int damage)
        {
            int shield = Owner.RuntimeData.Shield;
            if(shield > 0)
            {
                shield = Mathf.Max(shield - damage, 0);
                Owner.RuntimeData.Shield = shield;
                Log.Info($"[MarbleDamagePipelineAbility] 护盾吸收了 {damage} 点伤害，剩余护盾: {shield}");
                return;
            }

            int hp = Owner.RuntimeData.Hp;
            if(hp > 0)
            {
                hp = Mathf.Max(hp - damage, 0);
                Log.Info($"[MarbleDamagePipelineAbility] 剩余血量: {hp}");
                Owner.RuntimeData.Hp = hp;
                return;
            }
        }
    }
}
