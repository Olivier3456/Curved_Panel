using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCurvedUIObjectPosition : MonoBehaviour
{
    public RectTransform panelRectTransform;
    public float distanceFromPanel = 0.1f;

    void Start()
    {
        transform.position = panelRectTransform.position - panelRectTransform.forward * distanceFromPanel;
        transform.rotation = panelRectTransform.rotation;
    }
}
