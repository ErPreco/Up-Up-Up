using UnityEngine;

[ExecuteAlways]
public class GizmosDrawer : MonoBehaviour
{
    [SerializeField] private bool drawGizmos;

    [Space]
    [SerializeField] private Transform end;
    private Transform start;

    [Space]
    [SerializeField] private Transform endCircleCast;
    [SerializeField] private float radius;
    [SerializeField] [Range(0, 1)] private float lerp;
    private Vector2 center;

    [Space]
    [SerializeField] private bool detectCollisions;
    [SerializeField] private bool misureDistance;
    [SerializeField] private bool drawCircleCast;

    void Start()
    {
        start = transform;
    }

    void Update()
    {
        if (!Application.isPlaying)
        {
            if (detectCollisions)
            {
                var ar = Physics2D.LinecastAll(start.position, end.position);
                print("is hitting: " + (ar.Length != 0));
            }
        }

        if (misureDistance)
        {
            print("distance: " + Vector2.Distance(start.position, end.position));
        }

        if (drawCircleCast)
        {
            center = Vector2.Lerp(transform.position, endCircleCast.position, lerp);
        }
    }

    void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        if (radius > 0)
        {
            Gizmos.DrawWireSphere(center, radius);
        }

        if (end != null)
        {
            Gizmos.DrawLine(start.position, end.position);
        }
    }
}
