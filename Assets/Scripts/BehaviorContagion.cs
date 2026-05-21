using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// After each NPC picks run/hide and a destination, optionally adjust from neighbors.
/// behaviourActive → majority; proportionActive → proportional + B; quorumActive → quorum threshold.
/// weightedbyDistance → linear weight w = 1 - distance/contagionAOE on all three models.
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
        GatherNeighborData(
            self,
            out int runCount,
            out int hideCount,
            out float runInfluence,
            out float hideInfluence,
            out List<Vector3> runSpots,
            out List<float> runSpotWeights,
            out List<Vector3> hideSpots,
            out List<float> hideSpotWeights);

        if (Parameters.weightedbyDistance)
        {
            if (runInfluence <= 0f && hideInfluence <= 0f)
            {
                return;
            }

            if (Mathf.Approximately(runInfluence, hideInfluence))
            {
                return;
            }

            if (runInfluence > hideInfluence)
            {
                self.run = true;
                self.destination = MostCommonSpotWeighted(runSpots, runSpotWeights, self.initialDestination);
            }
            else
            {
                self.run = false;
                self.destination = MostCommonSpotWeighted(hideSpots, hideSpotWeights, self.initialDestination);
            }
        }
        else
        {
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
    }

    public void TryApplyQuorum(VictimController self)
    {
        float q = Mathf.Clamp01(Parameters.quorumThreshold);

        GatherNeighborData(
            self,
            out int runCount,
            out int hideCount,
            out float runInfluence,
            out float hideInfluence,
            out List<Vector3> runSpots,
            out List<float> runSpotWeights,
            out List<Vector3> hideSpots,
            out List<float> hideSpotWeights);

        float s_run;
        float s_hide;

        if (Parameters.weightedbyDistance)
        {
            float totalInfluence = runInfluence + hideInfluence;
            if (totalInfluence <= 0f)
            {
                return;
            }

            s_run = runInfluence / totalInfluence;
            s_hide = hideInfluence / totalInfluence;
        }
        else
        {
            int nTotal = runCount + hideCount;
            if (nTotal == 0)
            {
                return;
            }

            s_run = (float)runCount / nTotal;
            s_hide = (float)hideCount / nTotal;
        }

        if (s_run >= q && s_hide < q)
        {
            self.run = true;
            self.destination = Parameters.weightedbyDistance
                ? MostCommonSpotWeighted(runSpots, runSpotWeights, self.initialDestination)
                : MostCommonSpot(runSpots, self.initialDestination);
        }
        else if (s_hide >= q && s_run < q)
        {
            self.run = false;
            self.destination = Parameters.weightedbyDistance
                ? MostCommonSpotWeighted(hideSpots, hideSpotWeights, self.initialDestination)
                : MostCommonSpot(hideSpots, self.initialDestination);
        }
    }

    public void TryApplyProportional(VictimController self)
    {
        float B = Mathf.Clamp01(Parameters.proportionStrengthB);

        GatherNeighborData(
            self,
            out int runCount,
            out int hideCount,
            out float runInfluence,
            out float hideInfluence,
            out List<Vector3> runSpots,
            out List<float> runSpotWeights,
            out List<Vector3> hideSpots,
            out List<float> hideSpotWeights);

        float priorRun = self.priorRun;
        float priorHide = 1f - priorRun;

        float P_run;
        float P_hide;

        if (Parameters.weightedbyDistance)
        {
            float totalInfluence = runInfluence + hideInfluence;
            if (totalInfluence <= 0f)
            {
                P_run = priorRun;
                P_hide = priorHide;
            }
            else
            {
                float s_run = runInfluence / totalInfluence;
                float s_hide = hideInfluence / totalInfluence;

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
        }
        else
        {
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
        }

        float u = Random.value;
        self.run = u < P_run;

        if (B > 0.5f)
        {
            var matchingSpots = self.run ? runSpots : hideSpots;
            var matchingWeights = self.run ? runSpotWeights : hideSpotWeights;
            Vector3 empirical = self.run ? PickEmpiricalRunDestination() : PickEmpiricalHideDestination();

            if (matchingSpots.Count > 0)
            {
                self.destination = Parameters.weightedbyDistance
                    ? MostCommonSpotWeighted(matchingSpots, matchingWeights, empirical)
                    : MostCommonSpot(matchingSpots, empirical);
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

    static void GatherNeighborData(
        VictimController self,
        out int runCount,
        out int hideCount,
        out float runInfluence,
        out float hideInfluence,
        out List<Vector3> runSpots,
        out List<float> runSpotWeights,
        out List<Vector3> hideSpots,
        out List<float> hideSpotWeights)
    {
        runCount = 0;
        hideCount = 0;
        runInfluence = 0f;
        hideInfluence = 0f;
        runSpots = new List<Vector3>();
        hideSpots = new List<Vector3>();
        runSpotWeights = new List<float>();
        hideSpotWeights = new List<float>();

        var seen = new HashSet<VictimController>();
        Collider[] hits = Physics.OverlapSphere(self.transform.position, Parameters.contagionAOE);
        float aoe = Parameters.contagionAOE;

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

            float w = 1f;
            if (Parameters.weightedbyDistance && aoe > 0f)
            {
                float distance = Vector3.Distance(self.transform.position, other.transform.position);
                w = Mathf.Clamp01(1f - distance / aoe);
            }

            bool isRunning = other.initialRun && !other.isHidden;
            if (isRunning)
            {
                runCount++;
                runInfluence += w;
                runSpots.Add(other.initialDestination);
                runSpotWeights.Add(w);
            }
            else
            {
                hideCount++;
                hideInfluence += w;
                hideSpots.Add(other.initialDestination);
                hideSpotWeights.Add(w);
            }
        }
    }

    static Vector3 MostCommonSpot(List<Vector3> spots, Vector3 fallback)
    {
        if (spots.Count == 0)
        {
            return fallback;
        }

        var unitWeights = new List<float>(spots.Count);
        for (int i = 0; i < spots.Count; i++)
        {
            unitWeights.Add(1f);
        }

        return MostCommonSpotWeighted(spots, unitWeights, fallback);
    }

    static Vector3 MostCommonSpotWeighted(List<Vector3> spots, List<float> weights, Vector3 fallback)
    {
        if (spots.Count == 0)
        {
            return fallback;
        }

        var totals = new Dictionary<Vector3, float>();
        for (int i = 0; i < spots.Count; i++)
        {
            Vector3 key = RoundSpot(spots[i]);
            float w = i < weights.Count ? weights[i] : 1f;
            if (!totals.ContainsKey(key))
            {
                totals[key] = 0f;
            }

            totals[key] += w;
        }

        float bestWeight = 0f;
        Vector3 bestSpot = fallback;
        bool tied = false;

        foreach (KeyValuePair<Vector3, float> entry in totals)
        {
            if (entry.Value > bestWeight)
            {
                bestWeight = entry.Value;
                bestSpot = entry.Key;
                tied = false;
            }
            else if (Mathf.Approximately(entry.Value, bestWeight))
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
