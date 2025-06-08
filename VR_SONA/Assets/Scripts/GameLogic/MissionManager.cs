// 실질적으로 어떤 미션씬을 로드할지 '결정'만 하는 클래스. 로드는 MissionSceneLoader에서 담당.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MissionType
{
    None,
    Mission1,
    Mission2
}

public class MissionManager : MonoBehaviour
{
    private MissionType[,] missionMap = new MissionType[3, 3]
    {
        { MissionType.Mission1, MissionType.Mission2, MissionType.Mission1 },
        { MissionType.None,     MissionType.Mission2, MissionType.Mission2 },
        { MissionType.Mission2, MissionType.Mission1, MissionType.None }
    };

    public static MissionManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    public MissionType GetMissionType(Vector2Int coords)
    {
        if (coords.x < 0 || coords.x >= 3 || coords.y < 0 || coords.y >= 3)
            return MissionType.None;

        return missionMap[coords.x, coords.y];
    }

    public string GetSceneNameFromMission(MissionType mission)
    {
        switch (mission)
        {
            case MissionType.Mission1:
                return "MissionBasketballScene";
            case MissionType.Mission2:
                return "MissionWaterRushScene";
            default:
                return null;
        }
    }
}