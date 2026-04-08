namespace GameLogic.Marble
{
    public interface IHealth
    {
        bool IsAlive { get; set; }
        int Hp { get; set; }
        int MaxHp { get; set; }
    }
}
