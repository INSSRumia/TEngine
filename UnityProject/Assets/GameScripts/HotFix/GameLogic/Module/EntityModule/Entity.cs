using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 由 EntityModule 统一调度的实体基类。
    /// </summary>
    public class Entity : MonoBehaviour
    {
        [SerializeField] private int _priority;

        /// <summary>
        /// 实体调度优先级。数值越小越先执行。
        /// </summary>
        public int Priority => _priority;

        internal long RegistrationOrder { get; set; }
        internal bool IsRegistered { get; set; }
        internal bool IsInitialized { get; set; }
        internal bool IsStarted { get; set; }
        internal bool IsShutdown { get; set; }

        /// <summary>
        /// Unity 生命周期仅负责桥接注册。
        /// </summary>
        protected virtual void Awake()
        {
            GameModule.Entity.Register(this);
        }

        /// <summary>
        /// Unity 生命周期仅负责桥接反注册。
        /// </summary>
        protected virtual void OnDestroy()
        {
            GameModule.Entity.Unregister(this);
        }

        /// <summary>
        /// 实体初始化（仅一次）。
        /// </summary>
        protected virtual void OnEntityAwake()
        {
        }

        /// <summary>
        /// 实体首次启动（仅一次）。
        /// </summary>
        protected virtual void OnEntityStart()
        {
        }

        /// <summary>
        /// 实体逐帧更新。
        /// </summary>
        protected virtual void OnEntityUpdate(float elapseSeconds, float realElapseSeconds)
        {
        }

        /// <summary>
        /// 实体后置更新。
        /// </summary>
        protected virtual void OnEntityLateUpdate(float elapseSeconds, float realElapseSeconds)
        {
        }

        /// <summary>
        /// 实体关闭清理（仅一次）。
        /// </summary>
        protected virtual void OnEntityShutdown()
        {
        }

        internal void InternalInit()
        {
            if (IsInitialized)
            {
                return;
            }

            IsInitialized = true;
            OnEntityAwake();
        }

        internal void InternalStart()
        {
            if (IsStarted)
            {
                return;
            }

            IsStarted = true;
            OnEntityStart();
        }

        internal void InternalUpdate(float elapseSeconds, float realElapseSeconds)
        {
            OnEntityUpdate(elapseSeconds, realElapseSeconds);
        }

        internal void InternalLateUpdate(float elapseSeconds, float realElapseSeconds)
        {
            OnEntityLateUpdate(elapseSeconds, realElapseSeconds);
        }

        internal void InternalShutdown()
        {
            if (IsShutdown)
            {
                return;
            }

            IsShutdown = true;
            OnEntityShutdown();
        }
    }
}
