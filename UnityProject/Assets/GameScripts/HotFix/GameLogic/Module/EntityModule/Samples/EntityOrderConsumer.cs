using TEngine;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 实体生命周期迁移样例：消费者。
    /// 通过 Entity 生命周期获取依赖，不依赖 Unity Start/Update 执行顺序。
    /// </summary>
    public class EntityOrderConsumer : Entity
    {
        [SerializeField] private EntityOrderProducer _producer;

        private int _cachedValue;

        protected override void OnEntityStart()
        {
            if (_producer == null)
            {
                Log.Warning("[EntityOrderConsumer] Producer 未绑定");
                return;
            }

            _cachedValue = _producer.Value;
            Log.Info($"[EntityOrderConsumer] Start, Read Producer Value={_cachedValue}, Priority={Priority}");
        }

        protected override void OnEntityUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if (_producer == null)
            {
                return;
            }

            if (_cachedValue == _producer.Value)
            {
                return;
            }

            _cachedValue = _producer.Value;
            Log.Info($"[EntityOrderConsumer] Sync Producer Value={_cachedValue}");
        }

        protected override void OnEntityShutdown()
        {
            _cachedValue = 0;
            Log.Info("[EntityOrderConsumer] Shutdown");
        }
    }
}
