using System;

namespace ReturnHome.Server.Entity.Actions
{
    public interface IAction
    {
        Tuple<IActor, IAction> Act();

        void RunOnFinish(IActor actor, IAction action);
    }
}
