using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public static class ThreadingManager
{
    static bool searchStarted = false;
    static bool cancelled = false;
    static bool startSearchRequested = false;
    static bool positionLoadingRequested = false;

    public static void RequestStartSearch()
    {
        if (startSearchRequested)
        {
            return;
        }

        startSearchRequested = true;
        
        Task.Factory.StartNew (() => Main.engine.StartSearch(EngineSettings.searchDepth), TaskCreationOptions.LongRunning);
    }

    public static void RequestPositionLoading()
    {
        if (positionLoadingRequested)
        {
            return;
        }

        // Not searching
        if (!Main.engine.IsSearching())
        {
            Main.positionLoader.AfterEngineCancelled();
            return;
        }

        positionLoadingRequested = true;

        EnginePlayer.CancelSearch();
    }

    // Called outside of the main thread
    public static void EngineCancelled()
    {
        cancelled = true;
    }

    public static void SearchStarted()
    {
        searchStarted = true;
    }

    // Main thread
    public static void Update()
    {
        if (searchStarted)
        {
            if (startSearchRequested)
            {
                EnginePlayer.SearchStarted();
                startSearchRequested = false;
            }

            searchStarted = false;
        }

        if (cancelled)
        {
            EnginePlayer.SearchFinished();

            if (positionLoadingRequested)
            {
                Main.positionLoader.AfterEngineCancelled();
                positionLoadingRequested = false;
            }


            cancelled = false;
        }
    }
}