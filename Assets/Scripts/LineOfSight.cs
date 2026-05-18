using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LineOfSight : MonoBehaviour
{
    public float viewRadius;
    [Range(0, 360)]
    public float viewAngle;

    public LayerMask targetMask;
    public LayerMask obstacleMask;
    public GameObject eyes;

    public List<Transform> visibleTargets = new List<Transform>();


    void Start()
    {
        StartCoroutine("FindTargetsWithDelay", 0.2f);
    }

    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }


    // Find victims within the shooter's visible range.
    void FindVisibleTargets()
    {
        visibleTargets.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(eyes.transform.position, viewRadius, targetMask);
        //sorting this by distance
        targetsInViewRadius = targetsInViewRadius.OrderBy(c => (eyes.transform.position - c.transform.position).sqrMagnitude).ToArray();


        if (targetsInViewRadius.Length == 0)
        {
            return;
        }

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                float distToTarget = Vector3.Distance(eyes.transform.position, target.position);

                if (!Physics.Raycast(eyes.transform.position, dirToTarget, distToTarget, obstacleMask))
                {
                    if (target.CompareTag("Victim"))
                    {
                        if (!target.GetComponent<VictimController>().dead)
                        {
                            visibleTargets.Add(target);
                        }
                    }
                }
            }
        }
    }


    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    private void OnDrawGizmos()
    {
        Vector3 frontRayPoint = eyes.transform.position + (eyes.transform.forward * viewRadius);

        // Left and right ray points are calculated by rotating the forward vector by half of the viewAngle
        Vector3 leftRayPoint = eyes.transform.position + DirFromAngle(-viewAngle / 2, false) * viewRadius;
        Vector3 rightRayPoint = eyes.transform.position + DirFromAngle(viewAngle / 2, false) * viewRadius;

        // Draw rays to represent the field of view
        Debug.DrawLine(eyes.transform.position, frontRayPoint, Color.cyan);
        Debug.DrawLine(eyes.transform.position, leftRayPoint, Color.magenta);
        Debug.DrawLine(eyes.transform.position, rightRayPoint, Color.yellow);
    }
}
