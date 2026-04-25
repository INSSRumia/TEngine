using System.Collections.Generic;
using TEngine;
using UnityEngine;
using GameLogic.Gameplay.Combat;
using UnityEngine.Pool;

namespace GameLogic.Gameplay.Combat.Marble
{
    /// <summary>
    /// Marble 伤害结算主干能力。
    /// 它负责维护一次受伤请求的阶段上下文，并串行处理嵌套伤害，
    /// 避免多个 Ability 直接改写血量/护盾时丢失统一的阶段边界。
    /// </summary>
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
            /// <summary>
            /// 单次伤害请求的上下文。
            /// 其它实现 IAfterReceiveDamage / IAfterCalculateDamage / IAfterApplyDamage 的能力
            /// 都通过 CurrentContext 读取或修正本次结算数据。
            /// </summary>
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
                // 如果当前正在结算，则把新伤害排队，避免在同一条栈上递归破坏阶段顺序。
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
                // 这里使用 RuntimeData.Config 中的加成/倍率/防御做统一计算，
                // 其它能力可在 Calculate 阶段后继续修正 FinalValue。
                context.FinalValue = Mathf.Max(0, Mathf.RoundToInt((context.InputValue + Owner.RuntimeData.Config.DamageAddition) * Owner.RuntimeData.Config.DamageMultiplier) - Owner.RuntimeData.Config.Defense);
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
            int shield = Owner.RuntimeData.State.Shield;
            if(shield > 0)
            {
                // 当前实现为“护盾先完整吸收当次伤害，不溢出到生命”。
                shield = Mathf.Max(shield - damage, 0);
                Owner.RuntimeData.State.Shield = shield;
                Log.Info($"[弹珠伤害管线能力] 护盾吸收了 {damage} 点伤害，剩余护盾: {shield}");
                return;
            }

            int hp = Owner.RuntimeData.State.Hp;
            if(hp > 0)
            {
                hp = Mathf.Max(hp - damage, 0);
                Log.Info($"[弹珠伤害管线能力] 剩余血量: {hp}");
                Owner.RuntimeData.State.Hp = hp;
                return;
            }
        }
    }
}
