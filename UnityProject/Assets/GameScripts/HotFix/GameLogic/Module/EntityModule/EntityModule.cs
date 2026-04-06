using System.Collections.Generic;
using TEngine;
using System;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 实体生命周期调度模块。
    /// </summary>
    public sealed class EntityModule : Module, IEntityModule, IUpdateModule
    {
        private static readonly Comparison<Entity> EntityComparison = CompareEntity;

        private readonly List<Entity> _entities = new List<Entity>(256);
        private readonly List<Entity> _pendingAdd = new List<Entity>(64);
        private readonly HashSet<Entity> _pendingRemove = new HashSet<Entity>();
        private readonly List<Entity> _removeBuffer = new List<Entity>(64);

        private bool _isIterating;
        private long _registrationSequence;

        public int Count => _entities.Count + _pendingAdd.Count;

        public override int Priority => 0;

        public override void OnInit()
        {
        }

        public void Register(Entity entity)
        {
            if (!IsEntityAlive(entity))
            {
                return;
            }

            if (entity.IsRegistered || _pendingAdd.Contains(entity))
            {
                return;
            }

            if (_isIterating)
            {
                _pendingAdd.Add(entity);
                return;
            }

            AddEntityInternal(entity);
        }

        public void Unregister(Entity entity)
        {
            if (entity == null)
            {
                return;
            }

            if (!entity.IsRegistered && !_pendingAdd.Contains(entity))
            {
                return;
            }

            if (_isIterating)
            {
                _pendingRemove.Add(entity);
                return;
            }

            RemoveEntityInternal(entity);
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            MergePending();

            _isIterating = true;
            int count = _entities.Count;
            for (int i = 0; i < count; i++)
            {
                Entity entity = _entities[i];
                if (!CanDispatch(entity))
                {
                    _pendingRemove.Add(entity);
                    continue;
                }

                if (!entity.IsInitialized)
                {
                    entity.InternalInit();
                }

                if (!entity.IsStarted && entity.isActiveAndEnabled)
                {
                    entity.InternalStart();
                }

                if (!entity.isActiveAndEnabled)
                {
                    continue;
                }

                entity.InternalUpdate(elapseSeconds, realElapseSeconds);
            }

            for (int i = 0; i < count; i++)
            {
                Entity entity = _entities[i];
                if (!CanDispatch(entity) || !entity.IsStarted || !entity.isActiveAndEnabled)
                {
                    continue;
                }

                entity.InternalLateUpdate(elapseSeconds, realElapseSeconds);
            }

            _isIterating = false;
            MergePending();
        }

        public override void Shutdown()
        {
            _isIterating = false;
            MergePending();

            for (int i = 0; i < _entities.Count; i++)
            {
                Entity entity = _entities[i];
                if (entity == null)
                {
                    continue;
                }

                entity.IsRegistered = false;
                entity.InternalShutdown();
            }

            _entities.Clear();
            _pendingAdd.Clear();
            _pendingRemove.Clear();
            _registrationSequence = 0;
        }

        private static int CompareEntity(Entity left, Entity right)
        {
            int priorityResult = left.Priority.CompareTo(right.Priority);
            if (priorityResult != 0)
            {
                return priorityResult;
            }

            return left.RegistrationOrder.CompareTo(right.RegistrationOrder);
        }

        private static bool IsEntityAlive(Entity entity)
        {
            return entity != null && entity.gameObject != null;
        }

        private bool CanDispatch(Entity entity)
        {
            return entity != null &&
                   entity.gameObject != null &&
                   !_pendingRemove.Contains(entity);
        }

        private void AddEntityInternal(Entity entity)
        {
            _pendingRemove.Remove(entity);

            entity.IsRegistered = true;
            entity.IsShutdown = false;
            entity.RegistrationOrder = ++_registrationSequence;

            int insertIndex = _entities.BinarySearch(entity, Comparer<Entity>.Create(EntityComparison));
            if (insertIndex < 0)
            {
                insertIndex = ~insertIndex;
            }

            _entities.Insert(insertIndex, entity);
        }

        private void RemoveEntityInternal(Entity entity)
        {
            _pendingRemove.Remove(entity);
            _pendingAdd.Remove(entity);

            if (entity.IsRegistered)
            {
                entity.IsRegistered = false;
                entity.InternalShutdown();
            }

            _entities.Remove(entity);
        }

        private void MergePending()
        {
            if (_pendingRemove.Count > 0)
            {
                _removeBuffer.Clear();
                foreach (Entity entity in _pendingRemove)
                {
                    _removeBuffer.Add(entity);
                }

                for (int i = 0; i < _removeBuffer.Count; i++)
                {
                    RemoveEntityInternal(_removeBuffer[i]);
                }

                _pendingRemove.Clear();
                _removeBuffer.Clear();
            }

            if (_pendingAdd.Count <= 0)
            {
                return;
            }

            for (int i = 0; i < _pendingAdd.Count; i++)
            {
                Entity entity = _pendingAdd[i];
                if (!IsEntityAlive(entity))
                {
                    continue;
                }

                AddEntityInternal(entity);
            }

            _pendingAdd.Clear();
        }
    }
}
