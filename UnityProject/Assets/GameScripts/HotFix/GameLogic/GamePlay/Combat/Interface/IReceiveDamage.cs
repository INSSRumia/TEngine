namespace GameLogic.GamePlay.Combat
{
    public interface IReceiveDamage
    {
        void ReceiveDamage(int value, ASC source = null);
    }
}
