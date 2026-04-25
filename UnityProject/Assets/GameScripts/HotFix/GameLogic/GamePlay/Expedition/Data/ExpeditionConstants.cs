namespace GameLogic.Gameplay.Expedition
{
    public static class ExpeditionConstants
    {
        public const int PlayerCombatSide = 1;
        public const int EnemyCombatSide = 2;
    }

    public enum EnumExpeditionFlowPhase
    {
        None = 0,
        Preparing = 1,
        EnteringNode = 2,
        WaitingEventChoice = 3,
        InCombat = 4,
        ApplyingNodeResult = 5,
        Settling = 6,
        Finished = 7,
    }

    public enum EnumExpeditionEndReason
    {
        None = 0,
        Victory = 1,
        Defeat = 2,
    }

    public enum EnumExpeditionNodeProcessStatus
    {
        Pending = 0,
        Entered = 1,
        Resolved = 2,
    }
}
