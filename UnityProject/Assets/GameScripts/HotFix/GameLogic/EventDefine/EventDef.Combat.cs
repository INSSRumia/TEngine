using TEngine;
namespace GameLogic
{   
    public static partial class EventDef
    {
        public static class Combat
        {
            public static EventId<Marble.Marble> MarbleDie = RuntimeId.ToRuntimeId("Combat.MarbleDie");
          
        }
    }
}
