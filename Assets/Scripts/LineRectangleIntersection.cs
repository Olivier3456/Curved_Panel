using UnityEngine;
using UnityEngine.UIElements;

public struct HeightData
{
    public float length;
    public Vector3 direction;
    public Vector3 intersection;
}

public class LineRectangleIntersection : MonoBehaviour
{
    private HeightData[] heightData = null;
    //public HeightData[] HeightDatas { get { return heightData; } }

    public CurvedScreen cs;

    public Transform hitPoint;


    void Update()
    {
        heightData = GetHeightData();
    }


    private HeightData[] GetHeightData()
    {
        Vector3[] corners = cs.cornerPositions;

        HeightData[] result = GetTrianglesHeight(corners);

        GetIntersection(ref result);

        return result;
    }



    private HeightData[] GetTrianglesHeight(Vector3[] corners)
    {
        HeightData[] heights = new HeightData[corners.Length];

        int j = 1;

        for (int i = 0; i < corners.Length; i++)
        {
            if (i == corners.Length - 1)
            {
                j = 0;
            }

            float edge1 = Vector3.Distance(hitPoint.position, corners[i]);
            float edge2 = Vector3.Distance(hitPoint.position, corners[j]);
            float edge3 = Vector3.Distance(corners[i], corners[j]); // The base of the triangle.
            float halfPerimeter = (edge1 + edge2 + edge3) * 0.5f;

            // Heron's formula:
            float area = Mathf.Sqrt(halfPerimeter * (halfPerimeter - edge1) * (halfPerimeter - edge2) * (halfPerimeter - edge3));

            // Height's formula:
            float height = (2 * area) / edge3;

            Vector3 direction;
            if (i > 0)
            {
                direction = (corners[i] - corners[i - 1]).normalized;
            }
            else
            {
                direction = (corners[i] - corners[corners.Length - 1]).normalized;
            }

            heights[i] = new HeightData() { length = height, direction = direction };

            j++;
        }

        return heights;
    }


    private void GetIntersection(ref HeightData[] heightData)
    {
        for (int i = 0; i < heightData.Length; i++)
        {
            heightData[i].intersection = hitPoint.position + heightData[i].direction * heightData[i].length;
            Debug.DrawLine(hitPoint.position, heightData[i].intersection, Color.red);
        }
    }




    //private HeightData GetLongestHeight(HeightData[] heightData)
    //{
    //    HeightData longestHeight = new HeightData();
    //    float maxDistance = 0;
    //    foreach (var item in heightData)
    //    {
    //        if (item.length > maxDistance)
    //        {
    //            maxDistance = item.length;
    //            longestHeight = item;
    //        }
    //    }

    //    return longestHeight;
    //}
}
