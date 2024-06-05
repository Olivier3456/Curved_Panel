using UnityEngine;

public class Tests : MonoBehaviour
{

    public GameObject hitPointMarker;

    private GameObject currentHitGo = null;

    public RectTransform panelRectTransform;



    private void Start()
    {
        CurvedScreen.Create(panelRectTransform, 5, 24, 256, 0.5f, 5);
    }







    void Update()
    {
        RaycastHit hit;
        float distance = 100f;
        Vector3 origin = transform.position;
        Vector3 direction = transform.up;

        Debug.DrawLine(origin, origin + (direction * distance));

        if (Physics.Raycast(origin, direction, out hit, distance))
        {
            if (hit.collider.gameObject.TryGetComponent(out CurvedScreen cs))
            {
                GameObject go = cs.Hit(hit.point);
                if (go != null && go.TryGetComponent(out TestButton tbt))
                {
                    tbt.ButtonHovered();
                }
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
