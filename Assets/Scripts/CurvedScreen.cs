using System.Drawing;
using UnityEngine;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class CurvedScreen : MonoBehaviour
{
    [Range(1f, 10f)] public float width = 2.0f;
    [Range(1f, 10f)] public float height = 2.0f;
    [Range(1f, 20f)] public float curvatureRadius = 5.0f;
    [Range(6, 32)] public int segments = 12;

    [HideInInspector] public Vector3[] cornerPositions = new Vector3[4];
    private Vector3 centerPoint;
    private Vector3 axisDirection;

    public bool displayMarkers = true;
    public GameObject[] cornersMarkers;
    public GameObject curveCenterPointMarker;
    public GameObject leftProjectionPointMarker;
    public GameObject rightProjectionPointMarker;


    private void Awake()
    {
        GenerateCurvedMesh();
    }

    private void Update()
    {
        GenerateCurvedMesh();
    }


    void GenerateCurvedMesh()
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
        GetCenterPointAndAxisOfTheCurve(halfHeight);
    }


    private void GetCenterPointAndAxisOfTheCurve(float halfHeight)
    {
        centerPoint = transform.position - (transform.forward * curvatureRadius);
        axisDirection = transform.up;
        if (displayMarkers && curveCenterPointMarker != null)
        {
            curveCenterPointMarker.transform.position = centerPoint;
            Debug.DrawLine(centerPoint - axisDirection * halfHeight, centerPoint + axisDirection * halfHeight, UnityEngine.Color.red, 10f);
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




    private Vector3 leftEdgeProjection;
    private Vector3 rightEdgeProjection;
    public Vector2 GetNormalizedHitPoint(Vector3 hitPoint)
    {
        GetHitPointProjectionsOnVerticalEdges(hitPoint);

        //Vector3 fromCenterToUpLeftCorner = cornerPositions[1] - centerPoint;
        //Vector3 fromCenterToUpRightCorner = cornerPositions[3] - centerPoint;
        //Vector3 fromCenterToHitPoint = hitPoint - centerPoint;
        //float leftAngle = Vector3.Angle(fromCenterToUpLeftCorner, fromCenterToHitPoint);
        //float rightAngle = Vector3.Angle(fromCenterToUpRightCorner, fromCenterToHitPoint);
        //float x = leftAngle / (leftAngle + rightAngle);

        //Debug.Log($"left angle = {leftAngle}, right angle = {rightAngle}.");

        //float y = 

        Vector2 normalizedHitPoint = Vector2.zero;
        //Debug.Log($"NormalizedHitPoint.x: {x}");
        return normalizedHitPoint;
    }



    private void GetHitPointProjectionsOnVerticalEdges(Vector3 point)
    {
        Vector3 direction = axisDirection; // Line along the y-axis
        Vector3 pointLeft = cornerPositions[0]; // Line passes through the origin
        Vector3 pointRight = cornerPositions[2]; // Line passes through the origin

        Vector3 projectedPointLeft = ProjectPointOnLine(point, direction, pointLeft);
        Vector3 projectedPointRight = ProjectPointOnLine(point, direction, pointRight);

        if (displayMarkers)
        {
            leftProjectionPointMarker.transform.position = projectedPointLeft;
            rightProjectionPointMarker.transform.position = projectedPointRight;
        }

        //Debug.Log("Projected Point: " + projectedPointLeft);
    }


    public static Vector3 ProjectPointOnLine(Vector3 point, Vector3 lineDirection, Vector3 linePoint)
    {
        // Ensure the direction vector is normalized
        Vector3 direction = lineDirection.normalized;

        // Vector from the point on the line to the point to be projected
        Vector3 lineToPoint = point - linePoint;

        // Project the vector onto the direction of the line
        float projectionLength = Vector3.Dot(lineToPoint, direction);

        // Calculate the projection point
        Vector3 projectionPoint = linePoint + direction * projectionLength;

        return projectionPoint;
    }

}
