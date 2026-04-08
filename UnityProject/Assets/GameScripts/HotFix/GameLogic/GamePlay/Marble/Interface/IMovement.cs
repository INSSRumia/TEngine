namespace GameLogic.Marble
{
    public interface IMovement
    {
        float Acceleration { get; set; }
        float TargetVelocity { get; set; }
        UnityEngine.Vector2 TargetDirection { get; set; }
    }
}
