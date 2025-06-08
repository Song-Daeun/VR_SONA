// [리팩토링] 게임로직과 코인UI 분리 CoinUIManager -> CoinManager
// 코인 체크 기능과 차감 기능 담당하는 클래스


using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class CoinManager
{
    private const int MissionCost = 100;

    public static bool HasEnoughCoins()
    {
        return PlayerState.CoinCount >= MissionCost;
    }

    public static bool SubtractCoinsForMission()
    {
        if (HasEnoughCoins())
        {
            PlayerState.CoinCount -= MissionCost;
            return true;
        }
        else
        {
            Debug.Log("코인이 부족합니다");
            return false;
        }
    }
}