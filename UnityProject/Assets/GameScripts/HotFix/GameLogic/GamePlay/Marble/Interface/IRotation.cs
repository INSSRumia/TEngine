namespace GameLogic.Marble
{
    public interface IRotation
    {
        float AngularAcceleration { get; set; }
        float TargetAngularVelocity { get; set; }
    }
}
