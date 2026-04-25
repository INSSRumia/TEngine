using System;
using System.Collections.Generic;

namespace GameLogic.Gameplay.Expedition
{
    [Serializable]
    public sealed class ExpeditionPersistentDataStore
    {
        public int Money;
        public List<MarblePersistentData?> LstMarbles = new ();
        public ExpeditionResultSummary LastResult;

        public void EnsureInitialized()
        {
            if (LstMarbles.Count > 0)
            {
                return;
            }

            Money = 0;
            LstMarbles.Add(MarblePersistentData.CreateDefault("marble_player_1", "lancer", "先锋一号", 0));
            LstMarbles.Add(MarblePersistentData.CreateDefault("marble_player_2", "archer", "先锋二号", 0));
            LstMarbles.Add(MarblePersistentData.CreateDefault("marble_player_3", "soldier", "先锋三号", 0));
        }

        public MarblePersistentData? GetMarble(string persistentId)
        {
            return LstMarbles.Find(marble => marble.HasValue && marble.Value.PersistentId == persistentId);
        }

        public void SetMarble(MarblePersistentData marble)
        {
            for (int i = 0; i < LstMarbles.Count; i++)
            {
                if (!LstMarbles[i].HasValue || LstMarbles[i].Value.PersistentId != marble.PersistentId)
                {
                    continue;
                }

                LstMarbles[i] = marble;
                return;
            }

            LstMarbles.Add(marble);
        }
    }
}
