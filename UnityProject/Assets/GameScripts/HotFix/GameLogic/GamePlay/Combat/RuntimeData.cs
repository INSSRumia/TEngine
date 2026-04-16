namespace GameLogic
{
    /// <summary>
    /// Combat 运行时数据的最小公共根类型。
    /// 只承载跨实体都需要的配置定位信息，具体战斗黑板由各子类继续扩展。
    /// </summary>
    [System.Serializable]
    public abstract class RuntimeData
    {
        /// <summary>
        /// 配置表主键，用于把运行时实例追溯回 Luban schema 的原始配置。
        /// </summary>
        public string ConfigId { get; set; }
        /// <summary>
        /// 当前实例使用的等级配置。
        /// </summary>
        public int Level { get; set; }
        /// <summary>
        /// 运行时唯一实例 ID，由各自 Factory 分配。
        /// </summary>
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
