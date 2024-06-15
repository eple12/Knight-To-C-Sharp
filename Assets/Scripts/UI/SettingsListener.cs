using UnityEngine;
using UnityEngine.UI;

public class SettingsListener : MonoBehaviour
{
    public GameObject enableWhiteEngine;
    public GameObject enableBlackEngine;

    Toggle enableWhiteEngineToggle;
    Toggle enableBlackEngineToggle;

    void Start()
    {
        enableWhiteEngineToggle = enableWhiteEngine.GetComponent<Toggle>();
        enableBlackEngineToggle = enableBlackEngine.GetComponent<Toggle>();
    }

    void Update()
    {
        if (!Main.engine.IsSearching() || Main.mainBoard.isWhiteTurn)
        {
            EngineSettings.enableBlackEngine = enableBlackEngineToggle.isOn;
        }
        
        if (!Main.engine.IsSearching() || !Main.mainBoard.isWhiteTurn)
        {
            EngineSettings.enableWhiteEngine = enableWhiteEngineToggle.isOn;
        }
    }

    // public void WhiteEngineEnableButtonClicked(bool on)
    // {
    //     if (!on && Main.engine.IsSearching() && Main.mainBoard.isWhiteTurn)
    //     {
    //         EnginePlayer.EndSearch();
    //     }
    // }

    // public void BlackEngineEnableButtonClicked(bool on)
    // {
    //     if (!on && Main.engine.IsSearching() && !Main.mainBoard.isWhiteTurn)
    //     {
    //         EnginePlayer.EndSearch();
    //     }
    // }
}