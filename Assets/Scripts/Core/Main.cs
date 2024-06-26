using UnityEngine;

public static class Main
{
    public static Board mainBoard;

    public static MoveMaker moveMaker;
    public static PositionLoader positionLoader;

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
        MoveOrder.Initialize();
    }

    public static void Update()
    {
        Graphic.Update();

        ThreadingManager.Update();
        EnginePlayer.Update();
    }
}