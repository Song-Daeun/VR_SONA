using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BingoBoard : MonoBehaviour
{
    public int rows = 3;
    public int cols = 3;
    public TileData[,] tiles = new TileData[3, 3];

    public Transform[,] tilePositions = new Transform[3, 3]; // 타일 위치 저장용
}
