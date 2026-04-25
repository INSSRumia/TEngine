using System;
using System.Collections.Generic;
using TEngine;

namespace GameLogic.Gameplay.Expedition
{
    [Serializable]
    public sealed class ExpeditionPersistentDataStore
    {
        [NonSerialized]
        private bool _isInitialized;

        public int Money;
        public List<MarblePersistentData?> LstMarbles = new ();
        public ExpeditionResultSummary LastResult;

        public void EnsureInitialized()
        {
            if (_isInitialized || HasExistingProgress())
                return;

            var campConfig = ExpeditionConfigBridge.ResolveCurrentCampConfig();
            if (campConfig == null)
            {
                Log.Warning("[远征持久化] 初始化已中止，因为无法解析当前开局阵营配置。");
                return;
            }

            Money = campConfig.InitialMoney;
            LstMarbles.Clear();
            for (int i = 0; i < campConfig.LstInitialMarbles.Count; i++)
            {
                var marbleSpawnConfig = campConfig.LstInitialMarbles[i];
                if (marbleSpawnConfig == null)
                    continue;

                var marbleInstId = $"{campConfig.CampConfigId}_marble_{i + 1}";
                LstMarbles.Add(MarblePersistentData.CreateDefault(marbleInstId, marbleSpawnConfig));
            }

            _isInitialized = true;
            Log.Info($"[远征持久化] 已按开局配置完成初始化。campConfigId:{campConfig.CampConfigId} money:{Money} marbleCount:{LstMarbles.Count}");
        }

        public MarblePersistentData? GetMarble(string marbleInstId)
        {
            return LstMarbles.Find(marble => marble.HasValue && marble.Value.MarbleInstId == marbleInstId);
        }

        public void SetMarble(MarblePersistentData marble)
        {
            for (int i = 0; i < LstMarbles.Count; i++)
            {
                if (!LstMarbles[i].HasValue || LstMarbles[i].Value.MarbleInstId != marble.MarbleInstId)
                {
                    continue;
                }

                LstMarbles[i] = marble;
                return;
            }

            LstMarbles.Add(marble);
        }

        private bool HasExistingProgress()
        {
            return Money != 0 || LastResult != null || LstMarbles.Count > 0;
        }
    }
}
