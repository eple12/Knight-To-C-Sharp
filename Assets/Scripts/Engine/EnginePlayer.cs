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
    public static bool isSearching = false;
    static bool cancelled = false;
    static CancellationTokenSource searchTimer;

    public static void Initialize()
    {
        board = Main.mainBoard;
        engine = Main.engine;
    }

    public static void Update()
    {
        // Get Engine Moves
        if (Graphic.isInMatch && 
        ((EngineSettings.enableWhiteEngine && board.isWhiteTurn) || (EngineSettings.enableBlackEngine && !board.isWhiteTurn)))
        {
            if (EngineSettings.useThreading)
            {
                if (!isSearching)
                {
                    RequestSearch();
                }
            }
            else
            {
                SingleThreadedSearch();
            }
        }
    }

    public static void SearchFinished()
    {
        if (cancelled)
        {
            return;
        }

        GetBestMove();
    }

    public static void CancelSearch()
    {
        cancelled = true;

        EndSearch();
    }

    public static void EndSearch()
    {
        engine?.TimeOut();
        searchTimer?.Cancel();
        
        isSearching = false;
    }

    static void RequestSearch()
    {
        // isSearching = true;
        // cancelled = false;

        // engine.BeforeThreadedSearch();

        ThreadingManager.RequestStartSearch();

        // Task.Factory.StartNew (() => engine.StartSearch(EngineSettings.searchDepth), TaskCreationOptions.LongRunning);

        
    }

    public static void SearchStarted()
    {
        isSearching = true;
        searchTimer = new CancellationTokenSource();
        Task.Delay(EngineSettings.searchMs, searchTimer.Token).ContinueWith((t) => {TimeOutThreadedSearch();});
    }

    static void GetBestMove()
    {
        isSearching = false;
        Move move = engine.GetMove();
        searchTimer.Cancel();
            
        PlayMove(move);
    }

    static void SingleThreadedSearch()
    {
        engine.StartSearch(EngineSettings.searchDepth);
        Move move = engine.GetMove();
            
        PlayMove(move);
    }

    static void PlayMove(Move move)
    {
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

    static void TimeOutThreadedSearch()
    {
        engine?.TimeOut();
    }
}