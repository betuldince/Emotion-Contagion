using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// After each NPC picks run/hide and a destination, optionally copy the local majority.
/// </summary>
public class BehaviorContagion : MonoBehaviour
{
    public void TryApplyMajority(VictimController self)
    {
        if (!Parameters.behaviourActive)
        {
            return;
        }

        int runCount = 0;
        int hideCount = 0;
        var runSpots = new List<Vector3>();
        var hideSpots = new List<Vector3>();
        var seen = new HashSet<VictimController>();

        Collider[] hits = Physics.OverlapSphere(self.transform.position, Parameters.contagionAOE);
        foreach (Collider hit in hits)
        {
            VictimController other = hit.GetComponentInParent<VictimController>();
            if (other == null || other == self || other.dead || other.IsFighting || !other.hasMadeChoice)
            {
                continue;
            }

            if (!seen.Add(other))
            {
                continue;
            }

            bool isRunning = other.initialRun && !other.isHidden;
            if (isRunning)
            {
                runCount++;
                runSpots.Add(other.initialDestination);
            }
            else
            {
                hideCount++;
                hideSpots.Add(other.initialDestination);
            }
        }

        // Nobody nearby, or tie — keep original choice
        if (runCount == 0 && hideCount == 0)
        {
            return;
        }

        if (runCount == hideCount)
        {
            return;
        }

        if (runCount > hideCount)
        {
            self.run = true;
            self.destination = MostCommonSpot(runSpots, self.initialDestination);
        }
        else
        {
            self.run = false;
            self.destination = MostCommonSpot(hideSpots, self.initialDestination);
        }
    }

    static Vector3 MostCommonSpot(List<Vector3> spots, Vector3 fallback)
    {
        if (spots.Count == 0)
        {
            return fallback;
        }

        var counts = new Dictionary<Vector3, int>();
        foreach (Vector3 spot in spots)
        {
            Vector3 key = RoundSpot(spot);
            if (!counts.ContainsKey(key))
            {
                counts[key] = 0;
            }

            counts[key]++;
        }

        int bestCount = 0;
        Vector3 bestSpot = fallback;
        bool tied = false;

        foreach (KeyValuePair<Vector3, int> entry in counts)
        {
            if (entry.Value > bestCount)
            {
                bestCount = entry.Value;
                bestSpot = entry.Key;
                tied = false;
            }
            else if (entry.Value == bestCount)
            {
                tied = true;
            }
        }

        return tied ? fallback : bestSpot;
    }

    static Vector3 RoundSpot(Vector3 position)
    {
        return new Vector3(
            Mathf.Round(position.x * 100f) / 100f,
            position.y,
            Mathf.Round(position.z * 100f) / 100f);
    }

    void OnDrawGizmosSelected()
    {
        float radius = Application.isPlaying ? Parameters.contagionAOE : Parameters.contagionAOE;
        Gizmos.color = new Color(0.2f, 0.85f, 0.35f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
