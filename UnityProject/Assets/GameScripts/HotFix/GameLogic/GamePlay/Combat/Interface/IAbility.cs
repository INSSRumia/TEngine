namespace GameLogic.GamePlay.Combat
{
    public interface IAbility
    {
        int Priority { get; }
        void OnAdd();
        void OnRemove();
    }
}
