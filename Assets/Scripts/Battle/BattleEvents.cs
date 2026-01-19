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
            return typeof(IRestartable);
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

    // 게임 클리어 이벤트
    public class GameClearEvent : IEvent
    {
        public Type GetPublishType()
        {
            return typeof(BattleManager);
        }
    }

    // 스테이지 시작 이벤트
    public class StageStartedEvent : IEvent
    {
        public int StageNumber { get; private set; }

        public StageStartedEvent(int stageNumber)
        {
            StageNumber = stageNumber;
        }

        public Type GetPublishType()
        {
            return typeof(StageManager);
        }
    }

    // 스테이지 클리어 이벤트
    public class StageClearedEvent : IEvent
    {
        public int StageNumber { get; private set; }

        public StageClearedEvent(int stageNumber)
        {
            StageNumber = stageNumber;
        }

        public Type GetPublishType()
        {
            return typeof(StageManager);
        }
    }

    // 모든 스테이지 클리어 이벤트
    public class AllStagesClearedEvent : IEvent
    {
        public Type GetPublishType()
        {
            return typeof(StageManager);
        }
    }

    // 스테이지 타이머 업데이트 이벤트
    public class StageTimerUpdatedEvent : IEvent
    {
        public float RemainingTime { get; private set; }

        public StageTimerUpdatedEvent(float remainingTime)
        {
            RemainingTime = remainingTime;
        }

        public Type GetPublishType()
        {
            return typeof(StageManager);
        }
    }
}
