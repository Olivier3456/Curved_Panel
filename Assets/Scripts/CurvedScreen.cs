using System.Drawing;
using UnityEngine;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class CurvedScreen : MonoBehaviour
{
    private float width;
    private float height;
    [Range(1f, 20f)] public float curvatureRadius = 5.0f;
    [Range(6, 32)] public int segments = 12;

    [HideInInspector] public Vector3[] cornerPositions = new Vector3[4];
    private Vector3 centerPoint;

    public bool displayMarkers = true;
    public GameObject[] cornersMarkers;
    public GameObject curveCenterPointMarker;
    public GameObject leftProjectionPointMarker;
    public GameObject rightProjectionPointMarker;

    [Space(10)]
    public RectTransform panelRectTransform;


    private void Awake()
    {
        SetScreenDimensions();
        GenerateCurvedMesh();
    }


    // DEBUG
    private void Update()
    {
        GenerateCurvedMesh();
    }


    

    public void SetScreenDimensions()
    {
        width = panelRectTransform.rect.width;
        height = panelRectTransform.rect.height;
    }


    private void GenerateCurvedMesh()
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
        float halfWidth = width / 2.0f;
        // Calcule l'angle total que l'�cran doit couvrir
        float angleExtent = width / curvatureRadius;
        // Divise l'angle total par le nombre de segments
        float angleStep = angleExtent / segments;

        // Cr�ation des vertices et des coordonn�es UV
        for (int i = 0; i <= segments; i++)
        {
            // Calcule l'angle pour le segment courant
            float angle = -angleExtent / 2 + i * angleStep;
            // Calcule les positions x et z en utilisant les fonctions trigonom�triques
            float x = curvatureRadius * Mathf.Sin(angle);
            // Inverser la direction de courbure pour obtenir une forme concave
            float z = curvatureRadius * (Mathf.Cos(angle) - 1);

            // Position du vertex inf�rieur
            vertices[i * 2] = new Vector3(x, -halfHeight, z);
            // Position du vertex sup�rieur
            vertices[i * 2 + 1] = new Vector3(x, halfHeight, z);

            // Coordonn�es UV pour les vertices
            uv[i * 2] = new Vector2((float)i / segments, 0);
            uv[i * 2 + 1] = new Vector2((float)i / segments, 1);


            GetCornersPositions(ref cornersIndex, vertices, i);
            if (displayMarkers)
            {
                DisplayScreenCorners();
            }
        }


        // Cr�ation des triangles pour chaque segment
        for (int i = 0; i < segments; i++)
        {
            int start = i * 2; // Vertex de d�part du segment courant
            int nextStart = start + 2; // Vertex de d�part du segment suivant

            // Premier triangle
            triangles[i * 6] = start;
            triangles[i * 6 + 1] = start + 1;
            triangles[i * 6 + 2] = nextStart;

            // Deuxi�me triangle
            triangles[i * 6 + 3] = start + 1;
            triangles[i * 6 + 4] = nextStart + 1;
            triangles[i * 6 + 5] = nextStart;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        // Recalcule les normales pour l'�clairage
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;

        GetComponent<MeshCollider>().sharedMesh = mesh;
        GetCurveCenterPoint(halfHeight);
    }


    private void GetCurveCenterPoint(float halfHeight)
    {
        centerPoint = transform.position - (transform.forward * curvatureRadius);
        
        if (displayMarkers && curveCenterPointMarker != null)
        {
            curveCenterPointMarker.transform.position = centerPoint;
        }
    }


    private void GetCornersPositions(ref int index, Vector3[] vertices, int i)
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
        for (int i = 0; i < cornerPositions.Length; i++)
        {
            if (cornersMarkers[i] != null)
            {
                cornersMarkers[i].transform.position = cornerPositions[i];
            }
        }
    }


    public Vector2 GetNormalizedHitPoint(Vector3 hitPoint)
    {
        // X
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

        Debug.Log($"NormalizedHitPoint: {normalizedHitPoint}");
        if (displayMarkers)
        {
            leftProjectionPointMarker.transform.position = leftEdgeProjection;
            rightProjectionPointMarker.transform.position = rightEdgeProjection;
        }

        return normalizedHitPoint;
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
}
