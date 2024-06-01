using UnityEngine;

public static class Main
{
    public static Board mainBoard;

    public static MoveMaker moveMaker;

    public static Engine engine;

    public static void Initialize()
    {
        Zobrist.GenerateZobristTable();
        PreComputedData.Initialize();
        
        mainBoard = new Board();

        engine = new Engine();

        InitializeAll();
    }

    static void InitializeAll()
    {
        EnginePlayer.Initialize();
        engine = new Engine();
        MoveOrder.Initialize();
    }

    public static void Update()
    {
        EnginePlayer.Update();
    }
}