using UnityEngine;

public static class ThreadingManager
{
    static bool cancelled = false;
    static bool positionLoadingRequested = false;

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

    // Main thread
    public static void Update()
    {
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