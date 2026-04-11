namespace GameLogic
{
    [System.Serializable]
    public abstract class RuntimeData
    {
        public string ConfigId { get; set; }
        public int InstId { get; set; }

        public RuntimeData(string configId, int instId)
        {
            SetData(configId, instId);
        }

        public void SetData(string configId, int instId)
        {
            ConfigId = configId;
            InstId = instId;
        }

    }
}
