using System;
using Percent111.ProjectNS.Enemy;
using Percent111.ProjectNS.Event;
using Percent111.ProjectNS.Player;
using Percent111.ProjectNS.Scene;

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

    // 플레이어 생성 완료 이벤트 (EnemyPool 초기화용)
    public class PlayerSpawnedEvent : IEvent
    {
        public PlayerDataProvider PlayerData { get; private set; }

        public PlayerSpawnedEvent(PlayerDataProvider playerData)
        {
            PlayerData = playerData;
        }

        public Type GetPublishType()
        {
            return typeof(BattleManager);
        }
    }

    // EnemyPool 초기화 완료 이벤트
    public class EnemyPoolInitializedEvent : IEvent
    {
        public EnemyPool EnemyPool { get; private set; }

        public EnemyPoolInitializedEvent(EnemyPool enemyPool)
        {
            EnemyPool = enemyPool;
        }

        public Type GetPublishType()
        {
            return typeof(InGameSceneEntry);
        }
    }
}
