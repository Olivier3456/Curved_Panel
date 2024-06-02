using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupUICamera : MonoBehaviour
{
    private Camera cam;
    private RenderTexture rd;
    private Material mat;

    public RectTransform panelRectTransform;
    public Renderer screenToDisplayPanel;
    public int pixelsPerMeter = 256;


    private void Start()
    {
        CreateRenderTexture();
        mat = new Material(Shader.Find("Unlit/Transparent"));   // DON'T FORGET TO INCLUDE THIS SHADER IN "ALWAYS INCLUDED SHADERS" LIST
        mat.mainTexture = rd;
        screenToDisplayPanel.material = mat;
        SetupCamera(panelRectTransform);
    }


    private void CreateRenderTexture()
    {
        float width = panelRectTransform.rect.width * pixelsPerMeter;
        float height = panelRectTransform.rect.height * pixelsPerMeter;

        rd = new RenderTexture((int)width, (int)height, 24);

        Debug.Log($"Render texture created. Width = {(int)width}, height = {(int)height}.");
    }
    
    public void SetupCamera(RectTransform rectTransform)
    {
        cam = gameObject.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0, 0, 0, 0);
        cam.orthographic = true;
        cam.cullingMask = 64;
        cam.nearClipPlane = 0.01f;
        cam.targetTexture = rd;
    }
}
