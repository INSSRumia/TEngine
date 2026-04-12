namespace GameLogic.Gameplay.Combat
{
    public interface IReceiveHeal
    {
        void ReceiveHeal(int value, ASC source = null);
    }
}
