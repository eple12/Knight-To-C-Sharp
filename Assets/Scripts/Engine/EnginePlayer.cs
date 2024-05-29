using System;
using System.Threading;
using UnityEngine;

public static class EnginePlayer
{
    public static Board board;
    public static MoveMaker moveMaker;

    public static void Initialize()
    {
        board = Main.mainBoard;
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
        Move move = Engine.StartSearch(board.searchDepth);
            
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