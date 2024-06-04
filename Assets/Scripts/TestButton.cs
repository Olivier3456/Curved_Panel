using UnityEngine;

public class TestButton : MonoBehaviour
{
    int hoveredCount = 0;

    public void ButtonHovered()
    {
        Debug.Log("Button hovered! " + hoveredCount++);
    }


    public void ButtonClicked()
    {
        Debug.Log("Button clicked!");
    }
}
