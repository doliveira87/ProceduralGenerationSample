using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProceduralGeneratorTest : MonoBehaviour
{
    [SerializeField]
    private DungeonGenerator dungeonGenerator;

    private bool areRectsReady;

    void Awake()
    {
        areRectsReady = false;
    }

    void Update()
    {
        if(dungeonGenerator.IsDone && !areRectsReady)
        {
            areRectsReady = true;
            GenerateCoolLevelWithRects(dungeonGenerator.GetResultantRooms());
        }
    }

    private void GenerateCoolLevelWithRects(List<Room> allRects)
    {
        //Do cool stuff!
    }
}