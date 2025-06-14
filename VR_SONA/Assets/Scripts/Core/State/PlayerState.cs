using UnityEngine;

public static class PlayerState
{
    // ================================ //
    // 플레이어 위치 상태
    // ================================ //
    public static Vector2Int LastEnteredTileCoords = new Vector2Int(-1, -1);
    
    // ================================ //
    // 게임 초기 설정값들
    // ================================ //
    public static float InitialGameTime = 480f; // 8분 (480초)
    public static int InitialCoins = 800;       // 시작 코인
    public static int MissionCost = 100;        // 미션 비용

    // ================================ //
    // 게임 상태 관리 (새로 추가)
    // ================================ //
    public enum GameState
    {
        Playing,        // 게임 진행 중
        Success,        // 성공으로 게임 종료
        FailedCoinLack, // 코인 부족으로 실패
        FailedTimeUp,   // 시간 만료로 실패
        Paused          // 일시 정지
    }

    public static GameState CurrentGameState = GameState.Playing;

    // ================================ //
    // 게임 상태 확인 메서드들
    // ================================ //
    // 게임이 종료되었는지 확인 (성공/실패 관계없이)
    public static bool IsGameEnded()
    {
        return CurrentGameState != GameState.Playing && CurrentGameState != GameState.Paused;
    }

    // 게임이 진행 중인지 확인
    public static bool IsGamePlaying()
    {
        return CurrentGameState == GameState.Playing;
    }

    // 게임이 성공으로 끝났는지 확인
    public static bool IsGameSuccess()
    {
        return CurrentGameState == GameState.Success;
    }

    // 게임이 실패로 끝났는지 확인
    public static bool IsGameFailed()
    {
        return CurrentGameState == GameState.FailedCoinLack || 
               CurrentGameState == GameState.FailedTimeUp;
    }

    // UI 표시 가능 여부 확인 (GameEnd UI 제외)
    public static bool CanShowUI()
    {
        return CurrentGameState == GameState.Playing;
    }

    // ================================ //
    // 게임 상태 변경 메서드들
    // ================================ //
    // 게임 상태를 성공으로 설정
    public static void SetGameSuccess()
    {
        CurrentGameState = GameState.Success;
        Debug.Log("🎉 PlayerState: 게임 상태를 성공으로 변경");
    }

    // 게임 상태를 코인 부족 실패로 설정
    public static void SetGameFailedCoinLack()
    {
        CurrentGameState = GameState.FailedCoinLack;
        Debug.Log("💸 PlayerState: 게임 상태를 코인 부족 실패로 변경");
    }

    // 게임 상태를 시간 만료 실패로 설정
    public static void SetGameFailedTimeUp()
    {
        CurrentGameState = GameState.FailedTimeUp;
        Debug.Log("⏰ PlayerState: 게임 상태를 시간 만료 실패로 변경");
    }

    // 게임 상태를 진행 중으로 리셋
    public static void ResetGameState()
    {
        CurrentGameState = GameState.Playing;
        Debug.Log("🔄 PlayerState: 게임 상태를 진행 중으로 리셋");
    }

    // 게임 일시 정지
    public static void PauseGame()
    {
        if (CurrentGameState == GameState.Playing)
        {
            CurrentGameState = GameState.Paused;
            Debug.Log("⏸️ PlayerState: 게임 일시 정지");
        }
    }

    // 게임 일시 정지 해제
    public static void ResumeGame()
    {
        if (CurrentGameState == GameState.Paused)
        {
            CurrentGameState = GameState.Playing;
            Debug.Log("▶️ PlayerState: 게임 재개");
        }
    }

    // ================================ //
    // 디버그 정보 출력
    // ================================ //
    // 현재 게임 상태를 로그로 출력
    public static void LogCurrentState()
    {
        Debug.Log($"🎮 현재 게임 상태: {CurrentGameState}");
        Debug.Log($"   - 게임 종료됨: {IsGameEnded()}");
        Debug.Log($"   - 게임 진행 중: {IsGamePlaying()}");
        Debug.Log($"   - UI 표시 가능: {CanShowUI()}");
    }
}