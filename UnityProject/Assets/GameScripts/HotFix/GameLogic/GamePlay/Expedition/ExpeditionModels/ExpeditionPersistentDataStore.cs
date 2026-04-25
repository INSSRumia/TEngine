using System;
using System.Collections.Generic;

namespace GameLogic.Gameplay.Expedition
{
    [Serializable]
    public sealed class ExpeditionPersistentDataStore
    {
        public int Money;
        public List<MarblePersistentData?> Marbles = new List<MarblePersistentData?>();
        public ExpeditionResultSummary LastResult;

        public void EnsureInitialized()
        {
            if (Marbles.Count > 0)
            {
                return;
            }

            Money = 0;
            Marbles.Add(MarblePersistentData.CreateDefault("marble_player_1", "lancer", "先锋一号", 0));
            Marbles.Add(MarblePersistentData.CreateDefault("marble_player_2", "archer", "先锋二号", 0));
            Marbles.Add(MarblePersistentData.CreateDefault("marble_player_3", "soldier", "先锋三号", 0));
        }

        public MarblePersistentData? GetMarble(string persistentId)
        {
            return Marbles.Find(marble => marble.HasValue && marble.Value.PersistentId == persistentId);
        }

        public void SetMarble(MarblePersistentData marble)
        {
            for (int i = 0; i < Marbles.Count; i++)
            {
                if (!Marbles[i].HasValue || Marbles[i].Value.PersistentId != marble.PersistentId)
                {
                    continue;
                }

                Marbles[i] = marble;
                return;
            }

            Marbles.Add(marble);
        }
    }
}
