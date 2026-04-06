using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 可被 EntityModule 统一调度的实体接口。
    /// </summary>
    public interface IEntityModule
    {
        /// <summary>
        /// 注册实体到调度模块。
        /// </summary>
        /// <param name="entity">实体。</param>
        void Register(Entity entity);

        /// <summary>
        /// 从调度模块反注册实体。
        /// </summary>
        /// <param name="entity">实体。</param>
        void Unregister(Entity entity);

        /// <summary>
        /// 当前已注册的实体数量。
        /// </summary>
        int Count { get; }
    }
}
