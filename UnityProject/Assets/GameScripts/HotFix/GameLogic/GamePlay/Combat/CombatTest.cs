using TEngine;
using UnityEngine;
using System.Collections.Generic;
using GameLogic.Gameplay.Combat.Marble;
using Sirenix.OdinInspector;

namespace GameLogic.GamePlay.Combat
{
    /// <summary>
    /// 战斗测试组件 - 用于快速测试战斗逻辑
    /// 直接挂载在场景中，无需启动完整游戏流程
    /// </summary>
    public class CombatTest : MonoBehaviour
    {
        [Header("阵营1设置")]
        [SerializeField] private string _camp1ConfigId = "Marble_001";
        [SerializeField] private int _camp1Count = 3;
        [SerializeField] private int _camp1Camp = 1;

        [Header("阵营2设置")]
        [SerializeField] private string _camp2ConfigId = "Marble_001";
        [SerializeField] private int _camp2Count = 3;
        [SerializeField] private int _camp2Camp = 2;

        [Header("生成设置")]
        [SerializeField] private float _spawnRadius = 5f;
        [SerializeField] private Vector3 _spawnCenter = Vector3.zero;

        private CombatManager _combatManager;
        private readonly List<Marble> _testSoldiers = new List<Marble>();

        private void Awake()
        {
            _combatManager = new CombatManager();
            Log.Warning($"[CombatTest] CombatManager 已初始化");
        }

        private void OnDestroy()
        {
            ClearAllSoldiers();
        }

        [Button("生成所有士兵")]
        public void SpawnAllSoldiers()
        {
            SpawnCampSoldiers(_camp1ConfigId, _camp1Camp, _camp1Count);
            SpawnCampSoldiers(_camp2ConfigId, _camp2Camp, _camp2Count);
            Log.Warning($"[CombatTest] 共生成 {_testSoldiers.Count} 个士兵");
        }

        [Button("生成阵营1士兵")]
        public void SpawnCamp1Soldiers()
        {
            SpawnCampSoldiers(_camp1ConfigId, _camp1Camp, _camp1Count);
        }

        [Button("生成阵营2士兵")]
        public void SpawnCamp2Soldiers()
        {
            SpawnCampSoldiers(_camp2ConfigId, _camp2Camp, _camp2Count);
        }

        [Button("清理所有士兵")]
        public void ClearAllSoldiers()
        {
            foreach (var soldier in _testSoldiers)
            {
                if (soldier != null)
                {
                    _combatManager?.Unregister(soldier);
                    Destroy(soldier.gameObject);
                }
            }
            _testSoldiers.Clear();
            Log.Warning("[CombatTest] 已清理所有士兵");
        }

        [Button("打印战斗状态")]
        public void PrintCombatStatus()
        {
            var active = _combatManager?.GetAllActiveMarbles();
            if (active == null)
            {
                Log.Warning("[CombatTest] CombatManager 未初始化");
                return;
            }

            Log.Warning($"[CombatTest] === 战斗状态 ===");
            Log.Warning($"[CombatTest] 活跃单位数: {active.Count}");

            int camp1Alive = 0, camp2Alive = 0;
            foreach (var marble in active)
            {
                if (marble?.RuntimeData == null) continue;
                var status = marble.RuntimeData.IsAlive ? "存活" : "死亡";
                Log.Warning($"[CombatTest] [{marble.RuntimeData.InstId}] 阵营:{marble.RuntimeData.Camp} HP:{marble.RuntimeData.Hp}/{marble.RuntimeData.MaxHp} {status}");

                if (marble.RuntimeData.Camp == _camp1Camp) camp1Alive++;
                else if (marble.RuntimeData.Camp == _camp2Camp) camp2Alive++;
            }

            Log.Warning($"[CombatTest] 阵营1存活: {camp1Alive}, 阵营2存活: {camp2Alive}");
        }

        private void SpawnCampSoldiers(string configId, int camp, int count)
        {
            if (count <= 0)
            {
                Log.Warning($"[CombatTest] 阵营 {camp} 数量为0，跳过生成");
                return;
            }

            for (int i = 0; i < count; i++)
            {
                Vector3 spawnPos = GetSpawnPosition(camp, i, count);
                var soldier = MarbleFactory.CreateMarble(configId, camp);
                if (soldier != null)
                {
                    soldier.transform.position = spawnPos;
                    soldier.transform.SetParent(transform);
                    _combatManager.Register(soldier);
                    _testSoldiers.Add(soldier);

                    Log.Warning($"[CombatTest] 生成士兵 阵营:{camp} 位置:{spawnPos} InstId:{soldier.RuntimeData.InstId}");
                }
            }
        }

        private Vector3 GetSpawnPosition(int camp, int index, int total)
        {
            float angleStep = 360f / total;
            float angle = angleStep * index;
            float radius = _spawnRadius * 0.5f;

            float xOffset = camp == _camp1Camp ? -_spawnRadius : _spawnRadius;
            Vector3 campCenter = _spawnCenter + new Vector3(xOffset, 0, 0);

            float rad = angle * Mathf.Deg2Rad;
            float x = campCenter.x + Mathf.Cos(rad) * radius;
            float z = campCenter.z + Mathf.Sin(rad) * radius;

            return new Vector3(x, 0, z);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(_spawnCenter + new Vector3(-_spawnRadius, 0, 0), _spawnRadius * 0.5f);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_spawnCenter + new Vector3(_spawnRadius, 0, 0), _spawnRadius * 0.5f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_spawnCenter, _spawnRadius);
        }
    }
}
