using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// After each NPC picks run/hide and a destination, optionally adjust from neighbors.
/// behaviourActive → majority; proportionActive → proportional + B; quorumActive → quorum threshold.
/// </summary>
public class BehaviorContagion : MonoBehaviour
{
    public void ApplyContagion(VictimController self)
    {
        if (Parameters.behaviourActive)
        {
            TryApplyMajority(self);
        }

        if (Parameters.proportionActive)
        {
            TryApplyProportional(self);
        }

        if (Parameters.quorumActive)
        {
            TryApplyQuorum(self);
        }
    }

    public void TryApplyMajority(VictimController self)
    {
        int runCount = 0;
        int hideCount = 0;
        var runSpots = new List<Vector3>();
        var hideSpots = new List<Vector3>();
        GatherNeighborCounts(self, out runCount, out hideCount, runSpots, hideSpots);

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

    public void TryApplyQuorum(VictimController self)
    {
        float q = Mathf.Clamp01(Parameters.quorumThreshold);

        int runCount = 0;
        int hideCount = 0;
        var runSpots = new List<Vector3>();
        var hideSpots = new List<Vector3>();
        GatherNeighborCounts(self, out runCount, out hideCount, runSpots, hideSpots);

        int nTotal = runCount + hideCount;
        if (nTotal == 0)
        {
            return;
        }

        float s_run = (float)runCount / nTotal;
        float s_hide = (float)hideCount / nTotal;

        if (s_run >= q && s_hide < q)
        {
            self.run = true;
            self.destination = MostCommonSpot(runSpots, self.initialDestination);
        }
        else if (s_hide >= q && s_run < q)
        {
            self.run = false;
            self.destination = MostCommonSpot(hideSpots, self.initialDestination);
        }
    }

    public void TryApplyProportional(VictimController self)
    {
        float B = Mathf.Clamp01(Parameters.proportionStrengthB);

        int runCount = 0;
        int hideCount = 0;
        var runSpots = new List<Vector3>();
        var hideSpots = new List<Vector3>();
        GatherNeighborCounts(self, out runCount, out hideCount, runSpots, hideSpots);

        float priorRun = self.priorRun;
        float priorHide = 1f - priorRun;

        float P_run;
        float P_hide;

        int nTotal = runCount + hideCount;
        if (nTotal == 0)
        {
            P_run = priorRun;
            P_hide = priorHide;
        }
        else
        {
            float s_run = (float)runCount / nTotal;
            float s_hide = (float)hideCount / nTotal;

            float rawRun = (1f - B) * priorRun + B * s_run;
            float rawHide = (1f - B) * priorHide + B * s_hide;
            float sum = rawRun + rawHide;
            if (sum <= 0f)
            {
                P_run = priorRun;
                P_hide = priorHide;
            }
            else
            {
                P_run = rawRun / sum;
                P_hide = rawHide / sum;
            }
        }

        float u = Random.value;
        self.run = u < P_run;

        if (B > 0.5f)
        {
            var matchingSpots = self.run ? runSpots : hideSpots;
            Vector3 empirical = self.run ? PickEmpiricalRunDestination() : PickEmpiricalHideDestination();

            if (matchingSpots.Count > 0)
            {
                self.destination = MostCommonSpot(matchingSpots, empirical);
            }
            else
            {
                self.destination = empirical;
            }
        }
        else
        {
            self.destination = self.run
                ? PickEmpiricalRunDestination()
                : PickEmpiricalHideDestination();
        }
    }

    static void GatherNeighborCounts(
        VictimController self,
        out int runCount,
        out int hideCount,
        List<Vector3> runSpots,
        List<Vector3> hideSpots)
    {
        runCount = 0;
        hideCount = 0;
        runSpots.Clear();
        hideSpots.Clear();

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

    static Vector3 PickEmpiricalRunDestination()
    {
        RunDestinations rd = Object.FindAnyObjectByType<RunDestinations>();
        return SampleNavMeshDestination(rd.SelectDestination().position);
    }

    static Vector3 PickEmpiricalHideDestination()
    {
        HideDestinations hd = Object.FindAnyObjectByType<HideDestinations>();
        return SampleNavMeshDestination(hd.SelectDestination().position);
    }

    static Vector3 SampleNavMeshDestination(Vector3 target)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(target, out hit, 1.0f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return target;
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
        Gizmos.color = new Color(0.2f, 0.85f, 0.35f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, Parameters.contagionAOE);
    }
}
