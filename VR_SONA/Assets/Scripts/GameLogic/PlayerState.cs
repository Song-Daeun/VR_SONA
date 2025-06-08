// 메인 게임 플레이어 상태를 저장하는 클래스
// (1) 마지막 입장 타일 좌표
// (2) 코인 개수

using UnityEngine;

public static class PlayerState
{
    public static Vector2Int LastEnteredTileCoords = new Vector2Int(-1, -1);
    public static int CoinCount = 100; // [리팩토링] CoinUIManager 변수 -> PlayerState
}