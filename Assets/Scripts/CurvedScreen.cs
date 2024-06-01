using UnityEngine;

// Assure que le GameObject possède les composants MeshFilter et MeshRenderer
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class CurvedScreen : MonoBehaviour
{
    [Range(1f, 10f)] public float width = 2.0f; // Largeur de l'écran
    [Range(1f, 10f)] public float height = 2.0f; // Hauteur de l'écran
    [Range(1f, 20f)] public float curvatureRadius = 5.0f; // Rayon de courbure
    [Range(6, 32)] public int segments = 10; // Nombre de segments pour la courbure

    private Vector3[] cornerPositions = new Vector3[4];
    public GameObject[] cornersMarkers;


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

        // Obtient le MeshFilter attaché au GameObject
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        // Crée un nouveau Mesh
        Mesh mesh = new Mesh();

        // Nombre total de vertices (chaque segment a deux vertices en haut et en bas)
        int vertexCount = (segments + 1) * 2;
        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uv = new Vector2[vertexCount];
        // Chaque segment a deux triangles (6 indices par segment)
        int[] triangles = new int[segments * 6];

        float halfHeight = height / 2.0f;
        float halfWidth = width / 2.0f;
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


            // AJOUT PERSO
            SetCornersPositions(ref cornersIndex, vertices, i);
            DisplayScreenCorners();
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

        // Assigne les vertices, UVs et triangles au mesh
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        // Recalcule les normales pour l'éclairage
        mesh.RecalculateNormals();

        // Assigne le mesh au MeshFilter
        meshFilter.mesh = mesh;

        // Assigne le mesh au MeshCollider
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }


    private void SetCornersPositions(ref int index, Vector3[] vertices, int i)
    {
        if (i == 0 || i == segments)
        {
            cornerPositions[index] = transform.rotation * vertices[i * 2]  + transform.position;
            cornersMarkers[index].transform.position = cornerPositions[index];
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
            cornersMarkers[i].transform.position = cornerPositions[i];
        }
    }






    //public Vector2 GetNormalizedHitPoint(Vector3 hitPoint)
    //{





    //}
}
