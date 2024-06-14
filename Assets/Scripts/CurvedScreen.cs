using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class CurvedScreen : MonoBehaviour
{
    private RectTransform panelRectTransform;
    private float curvatureRadius;
    private float raycastZOffset;

    private Vector3[] cornerPositions = new Vector3[4]; // Corners position of the curved screen.
    private float width;
    private float height;

    private GameObject cameraGameObject;
    private RenderTexture rdTex;


    private static LayerMask layerMask = LayerMask.GetMask(Conf.CURVED_UI_LAYER);
    public static LayerMask GetLayerMask() { return layerMask; }

    private static List<CurvedScreen> curvedScreensInstantiated = new List<CurvedScreen>();



    /// <summary>
    /// Initializes and shows the curved panel.
    /// </summary>
    /// <param name="panelRectTransform">Rect transform of the 2D panel to render in 3D.</param>
    /// <param name="curvatureRadius">Radius of the curve.</param>
    /// <param name="segments">Number of segments of the curve.</param>
    /// <param name="pixelsPerMeter">Number of pixels of the render texture displayed on the curved panel, per meter of the original 2D panel.</param>
    /// <param name="distanceFromPanel">Z offset of the curved screen from the position of the original 2D panel.</param>
    /// <param name="raycastZOffset">Z offset of the origin of the raycast witch shoots the original 2D panel.</param>
    public static CurvedScreen Create(RectTransform panelRectTransform, float curvatureRadius = 5f, int segments = 24, int pixelsPerMeter = 256, float distanceFromPanel = 0f, float raycastZOffset = 0.5f)
    {
        // Sets the orinal 2D panel on the appropriate layer: it won't be saw by the main camera, only by its own camera.
        int layer = LayerMask.NameToLayer(Conf.CURVED_UI_LAYER);
        SetLayer(panelRectTransform, layer);

        GameObject go = new GameObject($"Curved Screen for object {panelRectTransform.gameObject.name}");
        CurvedScreen cs = go.AddComponent<CurvedScreen>();
        cs.Initialize(panelRectTransform, curvatureRadius, segments, pixelsPerMeter, distanceFromPanel, raycastZOffset);

        curvedScreensInstantiated.Add(cs);

        return cs;
    }


    private static void SetLayer(Transform transform, int layer)
    {
        transform.gameObject.layer = layer;

        foreach (Transform trans in transform)
        {
            SetLayer(trans, layer);
        }
    }


    private void Initialize(RectTransform panelRectTransform, float curvatureRadius, int segments, int pixelsPerMeter, float distanceFromPanel, float raycastZOffset)
    {
        this.panelRectTransform = panelRectTransform;
        this.curvatureRadius = curvatureRadius;
        this.raycastZOffset = raycastZOffset;

        transform.SetParent(panelRectTransform);

        SetPosition(gameObject, distanceFromPanel);
        SetScreenDimensions();
        GenerateCurvedMesh(segments);

        CreateRenderTexture(pixelsPerMeter);
        Material mat = new Material(Shader.Find("Unlit/Transparent"));
        mat.mainTexture = rdTex;
        GetComponent<Renderer>().material = mat;
        SetupCamera();
    }


    private void CreateRenderTexture(int pixelsPerMeter)
    {
        float width = panelRectTransform.rect.width * pixelsPerMeter * panelRectTransform.localScale.x;
        float height = panelRectTransform.rect.height * pixelsPerMeter * panelRectTransform.localScale.y;

        rdTex = new RenderTexture((int)width, (int)height, 24);

        Debug.Log($"Render texture created. Width = {(int)width}, height = {(int)height}.");
    }


    private void SetupCamera()
    {
        cameraGameObject = new GameObject($"Curved Screen Camera for object {panelRectTransform.gameObject.name}");

        float camOffset = 0.1f;
        SetPosition(cameraGameObject, camOffset);
        cameraGameObject.transform.SetParent(panelRectTransform);

        Camera cam = cameraGameObject.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0, 0, 0, 0);
        cam.orthographic = true;
        cam.nearClipPlane = camOffset - 0.01f;
        cam.farClipPlane = camOffset + 0.01f;

        float height = panelRectTransform.rect.height * panelRectTransform.localScale.y;
        float width = panelRectTransform.rect.width * panelRectTransform.localScale.x;
        cam.orthographicSize = height * 0.5f;
        float aspectRatio = width / height;
        cam.aspect = aspectRatio;

        cam.cullingMask = layerMask;
        cam.targetTexture = rdTex;
    }


    private void SetPosition(GameObject go, float distanceFromPanel)
    {
        go.transform.position = panelRectTransform.position - panelRectTransform.forward * distanceFromPanel;
        go.transform.rotation = panelRectTransform.rotation;
    }


    private void SetScreenDimensions()
    {
        width = panelRectTransform.rect.width * panelRectTransform.localScale.x;
        height = panelRectTransform.rect.height * panelRectTransform.localScale.y;
    }


    private void GenerateCurvedMesh(int segments)
    {
        int cornersIndex = 0;

        MeshFilter meshFilter = GetComponent<MeshFilter>();

        Mesh mesh = new Mesh();

        int vertexCount = (segments + 1) * 2;
        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uv = new Vector2[vertexCount];
        int[] triangles = new int[segments * 6];
        float halfHeight = height / 2.0f;
        float angleExtent = width / curvatureRadius;
        float angleStep = angleExtent / segments;

        for (int i = 0; i <= segments; i++)
        {
            float angle = -angleExtent / 2 + i * angleStep;
            float x = curvatureRadius * Mathf.Sin(angle);
            float z = curvatureRadius * (Mathf.Cos(angle) - 1);

            vertices[i * 2] = new Vector3(x, -halfHeight, z);
            vertices[i * 2 + 1] = new Vector3(x, halfHeight, z);

            uv[i * 2] = new Vector2((float)i / segments, 0);
            uv[i * 2 + 1] = new Vector2((float)i / segments, 1);


            GetCornersPositions(ref cornersIndex, vertices, i, segments);
        }

        for (int i = 0; i < segments; i++)
        {
            int start = i * 2;
            int nextStart = start + 2;

            triangles[i * 6] = start;
            triangles[i * 6 + 1] = start + 1;
            triangles[i * 6 + 2] = nextStart;

            triangles[i * 6 + 3] = start + 1;
            triangles[i * 6 + 4] = nextStart + 1;
            triangles[i * 6 + 5] = nextStart;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;

        GetComponent<MeshCollider>().sharedMesh = mesh;
    }


    private Vector3 GetCurveCenterPoint()
    {
        return transform.position - (transform.forward * curvatureRadius);
    }


    private void GetCornersPositions(ref int index, Vector3[] vertices, int i, int segments)
    {
        if (i == 0 || i == segments)
        {
            cornerPositions[index] = transform.rotation * vertices[i * 2] + transform.position;
            index++;
        }

        if (index == 1)
        {
            cornerPositions[index] = cornerPositions[index - 1] + transform.up * height;
            index++;
        }
        else if (index == 3)
        {
            cornerPositions[index] = cornerPositions[index - 1] + transform.up * height;
            index++;
        }
    }


    /// <summary>
    /// Sends a raycast to the real panel, after calculating the good coordinates. Returns the GameObject hit.
    /// </summary>
    /// <param name="hitPoint">The world coordinates of the raycast hit point.</param>
    public GameObject Hit(Vector3 hitPoint)
    {
        // X
        Vector3 centerPoint = GetCurveCenterPoint();
        Vector3 axisDirection = transform.up;
        Vector3 leftEdgeProjection = ProjectPointOnLine(hitPoint, axisDirection, cornerPositions[0]);
        Vector3 rightEdgeProjection = ProjectPointOnLine(hitPoint, axisDirection, cornerPositions[2]);
        Vector3 fromCenterToLeft = leftEdgeProjection - centerPoint;
        Vector3 fromCenterToRight = rightEdgeProjection - centerPoint;
        Vector3 fromCenterToHitPoint = hitPoint - centerPoint;
        float leftAngle = Vector3.Angle(fromCenterToLeft, fromCenterToHitPoint);
        float rightAngle = Vector3.Angle(fromCenterToRight, fromCenterToHitPoint);
        float x = leftAngle / (leftAngle + rightAngle);

        // Y
        Vector3 cornerDownLeft = cornerPositions[0];
        float disanceToCorner = Vector3.Distance(cornerDownLeft, leftEdgeProjection);
        float y = disanceToCorner / height;

        Vector2 normalizedHitPoint = new Vector2(x, y);

        return CastRayToPanel(normalizedHitPoint);
    }


    private GameObject CastRayToPanel(Vector2 normalizedHitPoint)
    {
        float localX = (normalizedHitPoint.x - 0.5f) * width;
        float localY = (normalizedHitPoint.y - 0.5f) * height;
        Vector3 localPoint = new Vector3(localX, localY, 0);
        Vector3 rotatedPoint = panelRectTransform.rotation * localPoint;
        Vector3 offset = panelRectTransform.forward * -raycastZOffset;
        Vector3 raycastOrigin = panelRectTransform.position + rotatedPoint + offset;
        Vector3 raycastDirection = panelRectTransform.forward;

        RaycastHit hit;
        if (Physics.Raycast(raycastOrigin, raycastDirection, out hit, raycastZOffset * 1.1f, layerMask))
        {
            return hit.collider.gameObject;
        }
        else
        {
            return null;
        }
    }


    private Vector3 ProjectPointOnLine(Vector3 point, Vector3 lineDirection, Vector3 linePoint)
    {
        Vector3 lineToPoint = point - linePoint;
        float projectionLength = Vector3.Dot(lineToPoint, lineDirection);
        Vector3 projectionPoint = linePoint + lineDirection * projectionLength;
        return projectionPoint;
    }


    public void Dispose()
    {
        Destroy(gameObject);
    }


    private void OnDestroy()
    {
        //Debug.Log("Destroying curved screen.");

        curvedScreensInstantiated.Remove(this);

        if (cameraGameObject != null)
        {
            Destroy(cameraGameObject);
        }

        if (panelRectTransform != null)
        {
            SetLayer(panelRectTransform, LayerMask.NameToLayer("Default"));
        }

        if (rdTex != null && rdTex.IsCreated())
        {
            rdTex.Release();
        }
    }
}
