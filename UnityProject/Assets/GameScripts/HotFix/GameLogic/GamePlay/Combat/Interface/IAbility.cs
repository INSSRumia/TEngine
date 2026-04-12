
namespace GameLogic.Gameplay.Combat
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
        EnumCombineType CombineType { get; set;}
        void OnAdd();
        void OnRemove();
    }
}
