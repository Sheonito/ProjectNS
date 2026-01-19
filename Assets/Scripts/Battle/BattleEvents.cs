using System;
using Percent111.ProjectNS.Event;
using Percent111.ProjectNS.Player;
using Percent111.ProjectNS.UI;

namespace Percent111.ProjectNS.Battle
{
    // 게임 재시작 이벤트
    public class GameRestartEvent : IEvent
    {
        public Type GetPublishType()
        {
            return typeof(GameOverPopup);
        }
    }

    // 게임 오버 이벤트
    public class GameOverEvent : IEvent
    {
        public Type GetPublishType()
        {
            return typeof(PlayerDeathState);
        }
    }
}
