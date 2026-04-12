namespace GameLogic.GamePlay.Combat
{
    public enum AbilityCategory
    {
        Core,
        Optional,
        Dynamic,
    }

    public interface IAbility
    {
        int InstId { get; }
        int Priority { get; set;}
        AbilityCategory Category { get; set;}
        void OnAdd();
        void OnRemove();
    }
}
