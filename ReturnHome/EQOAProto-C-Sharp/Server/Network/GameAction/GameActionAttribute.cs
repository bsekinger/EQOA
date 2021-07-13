using System;

namespace ReturnHome.Server.Network.GameAction
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class GameActionAttribute : Attribute
    {
        //public GameActionType Opcode { get; }

        //public GameActionAttribute(GameActionType opcode)
        //{
            //Opcode = opcode;
        //}
    }
}
