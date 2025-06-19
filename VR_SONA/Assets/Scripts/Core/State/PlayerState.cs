using UnityEngine;

public static class PlayerState
{
    // 플레이어 위치 상태
    public static Vector2Int LastEnteredTileCoords = new Vector2Int(-1, -1);
    
    // 게임 초기 설정값들
    public static float InitialGameTime = 480f; 
    public static int InitialCoins = 800;       
    public static int MissionCost = 100;        

    // 게임 상태 관리 
    public enum GameState
    {
        Playing,        // 게임 진행 중
        Success,        // 성공으로 게임 종료
        FailedCoinLack, // 코인 부족으로 실패
        FailedTimeUp,   // 시간 만료로 실패
        Paused          // 일시 정지
    }

    public static GameState CurrentGameState = GameState.Playing;

    // 게임 상태 확인 메서드
    public static bool IsGameEnded()
    {
        return CurrentGameState != GameState.Playing && CurrentGameState != GameState.Paused;
    }

    public static bool IsGamePlaying()
    {
        return CurrentGameState == GameState.Playing;
    }

    public static bool IsGameSuccess()
    {
        return CurrentGameState == GameState.Success;
    }

    public static bool IsGameFailed()
    {
        return CurrentGameState == GameState.FailedCoinLack || 
               CurrentGameState == GameState.FailedTimeUp;
    }

    public static bool CanShowUI()
    {
        return CurrentGameState == GameState.Playing;
    }

    // 게임 상태 변경 메서드
    public static void SetGameSuccess()
    {
        CurrentGameState = GameState.Success;
    }

    public static void SetGameFailedCoinLack()
    {
        CurrentGameState = GameState.FailedCoinLack;
    }

    public static void SetGameFailedTimeUp()
    {
        CurrentGameState = GameState.FailedTimeUp;
    }

    public static void ResetGameState()
    {
        CurrentGameState = GameState.Playing;
    }

    // 게임 일시 정지
    public static void PauseGame()
    {
        if (CurrentGameState == GameState.Playing)
        {
            CurrentGameState = GameState.Paused;
        }
    }

    // 게임 일시 정지 해제
    public static void ResumeGame()
    {
        if (CurrentGameState == GameState.Paused)
        {
            CurrentGameState = GameState.Playing;
        }
    }
}