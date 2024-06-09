using TMPro;
using UnityEngine;

public class MateStateText : MonoBehaviour
{
    public GameObject textObj;
    TextMeshProUGUI text;

    MateChecker.MateState currentState;

    public void Start()
    {
        text = textObj.GetComponent<TextMeshProUGUI>();
        text.text = "";
        
        currentState = MateChecker.MateState.None;
    }

    public void Update()
    {
        if (ListenerUI.mateState != currentState)
        {
            text.text = MateChecker.ToString(ListenerUI.mateState);
            currentState = ListenerUI.mateState;
        }
    }
}