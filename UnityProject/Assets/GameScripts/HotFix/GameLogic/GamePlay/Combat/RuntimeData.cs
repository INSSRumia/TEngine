namespace GameLogic
{
    [System.Serializable]
    public abstract class RuntimeData
    {
        public string ConfigId { get; set; }
        public int Level { get; set; }
        public int InstId { get; set; }

        public RuntimeData(string configId, int level, int instId)
        {
            SetData(configId, level, instId);
        }

        public void SetData(string configId, int level, int instId)
        {
            ConfigId = configId;
            Level = level;
            InstId = instId;
        }

    }
}
