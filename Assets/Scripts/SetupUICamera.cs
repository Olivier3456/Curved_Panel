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
        CreateMaterial();
        ApplyMaterialToObject();
        SetupCamera(panelRectTransform);
    }


    private void CreateRenderTexture()  // TODO: add parameters related to the dimensions of the panel to display, and others params if needed.
    {
        float width = panelRectTransform.rect.width * pixelsPerMeter;
        float height = panelRectTransform.rect.height * pixelsPerMeter;       

        rd = new RenderTexture((int)width, (int)height, 0);

        Debug.Log($"Render texture created. Width = {(int)width}, height = {(int)height}.");
    }

    private void CreateMaterial()
    {
        mat = new Material(Shader.Find("Standard"));
        mat.mainTexture = rd;
    }

    private void ApplyMaterialToObject()
    {
        screenToDisplayPanel.material = mat;
    }


    public void SetupCamera(RectTransform rectTransform)
    {
        cam = gameObject.AddComponent<Camera>();
        transform.position = panelRectTransform.position - panelRectTransform.forward * 1f;
        transform.rotation = panelRectTransform.rotation;
        cam.clearFlags = CameraClearFlags.Nothing;
        cam.cullingMask = 64;
        cam.orthographic = true;
        cam.targetTexture = rd;
    }
}
