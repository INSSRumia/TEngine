namespace GameLogic.Marble
{
    /// <summary>
    /// 待结算值。伤害、治疗等均先写入此池，再由显式结算入口统一处理。
    /// </summary>
    public interface IPendingValue
    {
        int PendingDamage { get; set; }
        int PendingHeal { get; set; }
    }
}
