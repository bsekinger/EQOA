﻿namespace ReturnHome.Server.Network.GameEvent.Events
{
    public class GameEventPingResponse : GameEventMessage
    {
        public GameEventPingResponse(Session session)
            : base(GameEventType.PingResponse, GameMessageGroup.UIQueue, session) { }
    }
}
