using System;
using System.Collections.Generic;
using GameLogic.Gameplay.Combat;
using GameLogic.Gameplay.Combat.Marble;
using TEngine;
using UnityEngine;
using Object = UnityEngine.Object;
using ExpeditionTable = GameConfig.Gameplay.Expedition;

namespace GameLogic.Gameplay.Expedition
{
    public sealed class ExpeditionCombatSessionController : Singleton<ExpeditionCombatSessionController>, IUpdate
    {
        private readonly Dictionary<string, Marble> _playerMarbles = new Dictionary<string, Marble>();
        private readonly List<Marble> _enemyMarbles = new List<Marble>();

        private CombatSessionRequest _currentRequest;
        private Action<CombatSessionResult> _onCompleted;
        private CombatManager _combatManager;
        private GameObject _sessionRoot;
        private bool _isRunning;
        private float _runningTime;

        public bool IsRunning => _isRunning;

        public bool StartSession(CombatSessionRequest request, Action<CombatSessionResult> onCompleted)
        {
            if (_isRunning || request == null)
            {
                return false;
            }

            ClearSession();

            _currentRequest = request;
            _onCompleted = onCompleted;
            _combatManager = new CombatManager();
            _isRunning = true;
            _runningTime = 0f;

            _sessionRoot = new GameObject("ExpeditionCombatSessionRoot");
            SpawnAlliedMarbles(request.AlliedMarbles);
            SpawnEnemyMarbles(request.EnemyMarbles);
            Log.Info($"[远征战斗会话控制器] 开始会话 {request.SessionId} 友方:{_playerMarbles.Count} 敌方:{_enemyMarbles.Count}");
            return true;
        }

        public void OnUpdate()
        {
            if (!_isRunning || _combatManager == null)
            {
                return;
            }

            _runningTime += Time.deltaTime;
            if (_runningTime < 0.25f)
            {
                return;
            }

            var activeMarbles = _combatManager.GetAllActiveMarbles();
            if (activeMarbles == null || activeMarbles.Count == 0)
            {
                return;
            }

            int allyAlive = 0;
            int enemyAlive = 0;
            foreach (var marble in activeMarbles)
            {
                if (marble?.RuntimeData == null || !marble.RuntimeData.State.IsAlive)
                {
                    continue;
                }

                if (marble.RuntimeData.Camp == ExpeditionConstants.PlayerCamp)
                {
                    allyAlive++;
                }
                else if (marble.RuntimeData.Camp == ExpeditionConstants.EnemyCamp)
                {
                    enemyAlive++;
                }
            }

            if (allyAlive <= 0 || enemyAlive <= 0)
            {
                CompleteSession(allyAlive > 0 && enemyAlive <= 0);
            }
        }

        public void ClearSession()
        {
            foreach (var marble in _playerMarbles.Values)
            {
                if (marble != null)
                {
                    _combatManager?.Unregister(marble);
                    Object.Destroy(marble.gameObject);
                }
            }

            foreach (var marble in _enemyMarbles)
            {
                if (marble != null)
                {
                    _combatManager?.Unregister(marble);
                    Object.Destroy(marble.gameObject);
                }
            }

            _playerMarbles.Clear();
            _enemyMarbles.Clear();
            _currentRequest = null;
            _onCompleted = null;
            _combatManager = null;
            _isRunning = false;
            _runningTime = 0f;

            if (_sessionRoot != null)
            {
                Object.Destroy(_sessionRoot);
                _sessionRoot = null;
            }
        }

        private void CompleteSession(bool isVictory)
        {
            var result = BuildSessionResult(isVictory);
            var callback = _onCompleted;
            ClearSession();
            callback?.Invoke(result);
        }

        private CombatSessionResult BuildSessionResult(bool isVictory)
        {
            var result = new CombatSessionResult
            {
                IsVictory = isVictory,
                Summary = isVictory ? "Combat 胜利，队伍完成了本节点。" : "Combat 失败，远征被迫结束。",
            };

            for (int i = 0; i < _currentRequest.AlliedMarbles.Count; i++)
            {
                if(!_currentRequest.AlliedMarbles[i].HasValue)
                    continue;

                var snapshot = _currentRequest.AlliedMarbles[i].Value;
                _playerMarbles.TryGetValue(snapshot.PersistentId, out var marble);
                if(marble == null || marble.RuntimeData == null)
                    continue;

                var currentHp = marble.RuntimeData.State.Hp;
                var maxHp = marble.RuntimeData.State.MaxHp;
                var isDead = !marble.RuntimeData.State.IsAlive || currentHp <= 0;

                snapshot.CurrentHp = currentHp;
                snapshot.MaxHp = maxHp;
                snapshot.Exp = marble.RuntimeData.State.Exp;
                snapshot.Level = marble.RuntimeData.Level;
                snapshot.IsDead = isDead;

                result.MarbleResults.Add(snapshot);
            }

            return result;
        }

        private void SpawnAlliedMarbles(List<MarblePersistentData?> snapshots)
        {
            if (snapshots == null)
            {
                return;
            }

            for (int index = 0; index < snapshots.Count; index++)
            {
                if(!snapshots[index].HasValue)
                    continue;

                var snapshot = snapshots[index].Value;
                var marble = MarbleFactory.CreateMarble(snapshot.ConfigId, ExpeditionConstants.PlayerCamp, snapshot.Level);
                if (marble == null)
                {
                    continue;
                }

                marble.name = snapshot.PersistentId;
                marble.transform.SetParent(_sessionRoot.transform, false);
                marble.transform.position = new Vector3(-5f, 0f, GetLineOffset(index, snapshots.Count));
                marble.RuntimeData.State.Hp = Mathf.Clamp(snapshot.CurrentHp, 1, marble.RuntimeData.State.MaxHp);
                marble.RuntimeData.State.Exp = snapshot.Exp;
                _combatManager.Register(marble);
                _playerMarbles[snapshot.PersistentId] = marble;
            }
        }

        private void SpawnEnemyMarbles(List<ExpeditionTable.ExpeditionEnemyMarbleConfig> enemies)
        {
            if (enemies == null)
            {
                return;
            }

            for (int index = 0; index < enemies.Count; index++)
            {
                var enemy = enemies[index];
                var marble = MarbleFactory.CreateMarble(enemy.ConfigId, ExpeditionConstants.EnemyCamp, enemy.Level);
                if (marble == null)
                {
                    continue;
                }

                marble.name = enemy.EnemyId;
                marble.transform.SetParent(_sessionRoot.transform, false);
                marble.transform.position = new Vector3(5f, 0f, GetLineOffset(index, enemies.Count));
                _combatManager.Register(marble);
                _enemyMarbles.Add(marble);
            }
        }

        private static float GetLineOffset(int index, int total)
        {
            if (total <= 1)
            {
                return 0f;
            }

            var start = -1.5f * (total - 1);
            return start + index * 3f;
        }
    }
}
