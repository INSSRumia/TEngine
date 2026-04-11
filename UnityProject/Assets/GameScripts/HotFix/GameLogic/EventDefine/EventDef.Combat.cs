using TEngine;
namespace GameLogic
{   
    public static partial class EventDef
    {
        public static class Combat
        {
            public static EventId<Gameplay.Combat.Marble.Marble> MarbleDie = RuntimeId.ToRuntimeId("Combat.MarbleDie");
          
        }
    }
}
