using UnityEngine;

public static class Main
{
    public static Board mainBoard;

    public static MoveMaker moveMaker;

    public static void Initialize()
    {
        Zobrist.GenerateZobristTable();
        PreComputedData.Initialize();
        
        mainBoard = new Board();

        InitializeAll();
    }

    static void InitializeAll()
    {
        EnginePlayer.Initialize();
        Engine.Initialize();
        MoveOrder.Initialize();
    }

    public static void Update()
    {
        EnginePlayer.Update();
    }
}