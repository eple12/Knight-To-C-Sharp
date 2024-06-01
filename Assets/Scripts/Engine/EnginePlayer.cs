using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public static class EnginePlayer
{
    public static Board board;
    public static MoveMaker moveMaker;
    static Engine engine;
    static bool isSearching = false;

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
            if (!isSearching)
            {
                RequestSearch();
            }
            else
            {
                if (!engine.isSearching)
                {
                    GetBestMove();
                }
            }
        }
    }

    static void RequestSearch()
    {
        // Called only once
        isSearching = true;

        engine.isSearching = true;
        Task.Factory.StartNew (() => engine.StartSearch(board.searchDepth), TaskCreationOptions.LongRunning);
    }

    static void GetBestMove()
    {
        isSearching = false;
        Move move = engine.GetMove();
            
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