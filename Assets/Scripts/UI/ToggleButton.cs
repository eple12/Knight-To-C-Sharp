using UnityEngine;

public class ToggleButton : MonoBehaviour
{
    RectTransform handle;

    void Start()
    {
        handle = transform.GetChild(1).gameObject.GetComponent<RectTransform>();
    }

    public void Clicked(bool isOn)
    {
        if (isOn)
        {
            handle.anchoredPosition = new Vector2(30, 0);
        }
        else
        {
            handle.anchoredPosition = new Vector2(-30, 0);
        }
    }
}