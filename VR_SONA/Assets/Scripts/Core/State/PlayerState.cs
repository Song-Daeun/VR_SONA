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
}