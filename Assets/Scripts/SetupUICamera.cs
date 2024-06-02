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
    [Space(20)]
    public Material debug_Mat;
    //public RenderTexture debug_RenderTexture;



    private void Start()
    {
        CreateRenderTexture();
        //CreateMaterial();
        ApplyMaterialToObject();
        SetupCamera(panelRectTransform);
    }


    private void CreateRenderTexture()
    {
        float width = panelRectTransform.rect.width * pixelsPerMeter;
        float height = panelRectTransform.rect.height * pixelsPerMeter;

        rd = new RenderTexture((int)width, (int)height, 24);

        debug_Mat.mainTexture = rd;

        Debug.Log($"Render texture created. Width = {(int)width}, height = {(int)height}.");
    }


    //private void SetRenderTextureSize()   // We can't adjust a Render texture size after its creation...
    //{
    //    float width = panelRectTransform.rect.width * pixelsPerMeter;
    //    float height = panelRectTransform.rect.height * pixelsPerMeter;

    //    debug_RenderTexture.width = (int)width;
    //    debug_RenderTexture.height = (int)height;               

    //    Debug.Log($"Render texture size changed. Width = {(int)width}, height = {(int)height}.");
    //}



    //private void CreateMaterial()
    //{
    //    mat = new Material(Shader.Find("Standard"));
    //    //ToFadeMode();
    //    mat.mainTexture = rd;
    //}


    //private void ToFadeMode()
    //{
    //    mat.SetOverrideTag("RenderType", "Transparent");
    //    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
    //    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
    //    mat.SetInt("_ZWrite", 0);
    //    mat.DisableKeyword("_ALPHATEST_ON");
    //    mat.EnableKeyword("_ALPHABLEND_ON");
    //    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
    //    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    //}


    private void ApplyMaterialToObject()
    {
        //screenToDisplayPanel.material = mat;
        screenToDisplayPanel.material = debug_Mat;
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
