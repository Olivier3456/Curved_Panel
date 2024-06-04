using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class CurvedScreen : MonoBehaviour
{
    private RectTransform panelRectTransform;
    private float curvatureRadius;
    private float raycastZOffset;
    private LayerMask layerMask;

    private Vector3[] cornerPositions = new Vector3[4]; // Corners position of the curved screen.
    private float width;
    private float height;

    private RenderTexture rdTex;

    [Space(10), Header("Debug")]
    public bool debug_mode = true;
    public GameObject[] cornersMarkers;
    public GameObject curveCenterPointMarker;
    public GameObject leftProjectionPointMarker;
    public GameObject rightProjectionPointMarker;
    public GameObject raycasOriginMarker;


    /// <summary>
    /// Initializes and shows the curved panel.
    /// </summary>
    /// <param name="panelRectTransform">Rect transform of the 2D panel to render in 3D. The object must be on the layer designed for Curved UI elements.</param>
    /// <param name="curvatureRadius">The radius of the curve.</param>
    /// <param name="segments">The number of segments of the curve.</param>
    /// <param name="pixelsPerMeter">Number of pixels of the render texture displayed on the curved panel, per meter of the original 2D panel.</param>
    /// <param name="raycastZOffset">The distance of the origin of the raycast witch shoots the original 2D panel.</param>
    /// <param name="distanceFromPanel">The distance of the curved screen from the position of the original 2D panel.</param>
    public static CurvedScreen CreateCurvedPanel(RectTransform panelRectTransform, float curvatureRadius, int segments = 24, int pixelsPerMeter = 256, float raycastZOffset = 0.5f, float distanceFromPanel = 0.25f)
    {
        // Sets the orinal 2D panel on the right layer: it won't be saw by the main camera, only by its own camera. Same thing for raycasts.
        SetPanelToCurvedLayer(panelRectTransform);

        GameObject go = new GameObject($"{panelRectTransform.gameObject.name} curved panel");
        CurvedScreen cs = go.AddComponent<CurvedScreen>();
        cs.Initialize(panelRectTransform, curvatureRadius, segments, pixelsPerMeter, raycastZOffset, distanceFromPanel);
        return cs;
    }


    private static void SetPanelToCurvedLayer(Transform panelRectTransform)
    {
        panelRectTransform.gameObject.layer = LayerMask.NameToLayer("Curved UI");

        foreach (Transform tr in panelRectTransform)
        {
            SetPanelToCurvedLayer(tr);
        }
    }


    private void Initialize(RectTransform panelRectTransform, float curvatureRadius, int segments = 24, int pixelsPerMeter = 256, float raycastZOffset = 0.5f, float distanceFromPanel = 0.25f)
    {
        this.panelRectTransform = panelRectTransform;
        this.curvatureRadius = curvatureRadius;
        this.raycastZOffset = raycastZOffset;
        layerMask = LayerMask.GetMask("Curved UI");

        SetPosition(distanceFromPanel);
        SetScreenDimensions();
        GenerateCurvedMesh(segments);

        CreateRenderTexture(pixelsPerMeter);
        Material mat = new Material(Shader.Find("Unlit/Transparent"));   // DON'T FORGET TO INCLUDE THIS SHADER IN "ALWAYS INCLUDED SHADERS" LIST
        mat.mainTexture = rdTex;
        GetComponent<Renderer>().material = mat;
        SetupCamera(panelRectTransform);
    }


    private void CreateRenderTexture(int pixelsPerMeter)
    {
        float width = panelRectTransform.rect.width * pixelsPerMeter * panelRectTransform.localScale.x;
        float height = panelRectTransform.rect.height * pixelsPerMeter * panelRectTransform.localScale.y;

        rdTex = new RenderTexture((int)width, (int)height, 24);

        Debug.Log($"Render texture created. Width = {(int)width}, height = {(int)height}.");
    }


    private void SetupCamera(RectTransform rectTransform)
    {
        Camera cam = gameObject.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0, 0, 0, 0);
        cam.orthographic = true;
        cam.orthographicSize = 5 * panelRectTransform.localScale.x;
        cam.cullingMask = layerMask;
        cam.nearClipPlane = 0.01f;
        cam.targetTexture = rdTex;
    }


    private void SetPosition(float distanceFromPanel)
    {
        transform.position = panelRectTransform.position - panelRectTransform.forward * distanceFromPanel;
        transform.rotation = panelRectTransform.rotation;
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

        // Nombre total de vertices (chaque segment a deux vertices en haut et en bas)
        int vertexCount = (segments + 1) * 2;
        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uv = new Vector2[vertexCount];
        // Chaque segment a deux triangles (6 indices par segment)
        int[] triangles = new int[segments * 6];

        float halfHeight = height / 2.0f;
        // Calcule l'angle total que l'écran doit couvrir
        float angleExtent = width / curvatureRadius;
        // Divise l'angle total par le nombre de segments
        float angleStep = angleExtent / segments;

        // Création des vertices et des coordonnées UV
        for (int i = 0; i <= segments; i++)
        {
            // Calcule l'angle pour le segment courant
            float angle = -angleExtent / 2 + i * angleStep;
            // Calcule les positions x et z en utilisant les fonctions trigonométriques
            float x = curvatureRadius * Mathf.Sin(angle);
            // Inverser la direction de courbure pour obtenir une forme concave
            float z = curvatureRadius * (Mathf.Cos(angle) - 1);

            // Position du vertex inférieur
            vertices[i * 2] = new Vector3(x, -halfHeight, z);
            // Position du vertex supérieur
            vertices[i * 2 + 1] = new Vector3(x, halfHeight, z);

            // Coordonnées UV pour les vertices
            uv[i * 2] = new Vector2((float)i / segments, 0);
            uv[i * 2 + 1] = new Vector2((float)i / segments, 1);


            GetCornersPositions(ref cornersIndex, vertices, i, segments);
            if (debug_mode)
            {
                DisplayScreenCorners();
            }
        }


        // Création des triangles pour chaque segment
        for (int i = 0; i < segments; i++)
        {
            int start = i * 2; // Vertex de départ du segment courant
            int nextStart = start + 2; // Vertex de départ du segment suivant

            // Premier triangle
            triangles[i * 6] = start;
            triangles[i * 6 + 1] = start + 1;
            triangles[i * 6 + 2] = nextStart;

            // Deuxième triangle
            triangles[i * 6 + 3] = start + 1;
            triangles[i * 6 + 4] = nextStart + 1;
            triangles[i * 6 + 5] = nextStart;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        // Recalcule les normales pour l'éclairage
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;

        GetComponent<MeshCollider>().sharedMesh = mesh;
    }


    private Vector3 GetCurveCenterPoint()
    {
        Vector3 centerPoint = transform.position - (transform.forward * curvatureRadius);

        if (debug_mode && curveCenterPointMarker != null)
        {
            curveCenterPointMarker.transform.position = centerPoint;
        }

        return centerPoint;
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


    private void DisplayScreenCorners()
    {
        if (cornersMarkers != null)
        {
            for (int i = 0; i < cornerPositions.Length; i++)
            {
                if (cornersMarkers[i] != null)
                {
                    cornersMarkers[i].transform.position = cornerPositions[i];
                }
            }
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

        if (debug_mode && leftProjectionPointMarker != null && rightProjectionPointMarker != null)
        {
            leftProjectionPointMarker.transform.position = leftEdgeProjection;
            rightProjectionPointMarker.transform.position = rightEdgeProjection;
        }

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

        if (debug_mode)
        {
            Debug.DrawLine(raycastOrigin, raycastOrigin + (raycastDirection * raycastZOffset * 1.1f), Color.red);

            if (raycasOriginMarker != null)
            {
                raycasOriginMarker.transform.position = raycastOrigin;
            }
        }

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
        // Ensure the direction vector is normalized
        //Vector3 direction = lineDirection.normalized;

        // Vector from the point on the line to the point to be projected
        Vector3 lineToPoint = point - linePoint;

        // Project the vector onto the direction of the line
        float projectionLength = Vector3.Dot(lineToPoint, lineDirection);

        // Calculate the projection point
        Vector3 projectionPoint = linePoint + lineDirection * projectionLength;

        return projectionPoint;
    }


    public void DestroyCurvedScreen()
    {
        Destroy(gameObject);
    }


    private void OnDestroy()
    {
        if (rdTex != null && rdTex.IsCreated())
        {
            rdTex.Release();
        }
    }
}
