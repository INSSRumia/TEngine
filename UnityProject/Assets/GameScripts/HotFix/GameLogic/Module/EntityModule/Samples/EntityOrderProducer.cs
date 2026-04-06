using TEngine;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 实体生命周期迁移样例：生产者。
    /// </summary>
    public class EntityOrderProducer : Entity
    {
        [SerializeField] private int _bootValue = 1;

        public int Value { get; private set; }

        protected override void OnEntityStart()
        {
            Value = _bootValue;
            Log.Info($"[EntityOrderProducer] Start, Value={Value}, Priority={Priority}");
        }

        protected override void OnEntityShutdown()
        {
            Value = 0;
            Log.Info("[EntityOrderProducer] Shutdown");
        }
    }
}
