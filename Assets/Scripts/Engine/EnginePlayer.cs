using System;
using UnityEngine;

public static class EnginePlayer
{
    public static Board board;
    public static MoveMaker moveMaker;
    static Engine engine;

    public static void Initialize()
    {
        board = Main.mainBoard;
        engine = Main.engine;
    }

    public static void Update()
    {
        // Get Engine Moves
        if (Graphic.isInMatch && ((board.enableWhiteEngine && board.isWhiteTurn) || (board.enableBlackEngine && !board.isWhiteTurn)))
        {
            BestMove();
        }
    }

    public static void BestMove()
    {
        Move move = engine.StartSearch(board.searchDepth);
            
        if (move.moveValue != 0)
        {
            Graphic.grabbedPieceObject = moveMaker.FindPieceObject(move.startSquare);

            moveMaker.MakeGraphicalMove(move, true);
        }
        else
        {
            Debug.Log("Null Move Returned");
        }
    }
}