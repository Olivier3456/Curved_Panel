using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastRay : MonoBehaviour
{

    public GameObject hitPointMarker;

    private GameObject currentHitGo = null;


    void Update()
    {
        RaycastHit hit;
        float distance = 100f;
        Vector3 origin = transform.position;
        Vector3 direction = transform.up;

        Debug.DrawLine(origin, origin + (direction * distance));

        if (Physics.Raycast(origin, direction, out hit, distance))
        {
            if (currentHitGo != hit.collider.gameObject)
            {
                currentHitGo = hit.collider.gameObject;

                Debug.Log($"Touched Something: {hit.collider.name}.");


            }

            if (currentHitGo.transform.TryGetComponent(out CurvedScreen cs))
            {
                //cs.GetNormalizedHitPoint(hit.point);
            }

            if (hitPointMarker != null)
            {
                hitPointMarker.transform.position = hit.point;
            }
        }
        else
        {
            currentHitGo = null;
        }
    }
}
