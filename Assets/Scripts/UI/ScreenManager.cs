using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    public GameObject mainScreen;
    public GameObject settingsScreen;

    public void SettingsButtonClicked()
    {
        mainScreen.SetActive(false);
        settingsScreen.SetActive(true);
    }

    public void BackButtonClicked()
    {
        mainScreen.SetActive(true);
        settingsScreen.SetActive(false);
    }
}