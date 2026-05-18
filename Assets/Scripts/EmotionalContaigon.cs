using System.Collections.Generic;
using UnityEngine;
/*
 * Liu et al. (2018) "A perception-based emotion contagion model in crowd emergent evacuation simulation"
 * Eq. (7): initial emotion e0 from distance to incident (InitialPanic)
 * Eq. (8): Me = average neighbor emotion
 * Eq. (9): if e < Me then e := e0 + (Me - e) / Ne * w * Ab
 * Eq. (10): v = v0(1+e) if e < el, else v0(1+K*e)
 */

public class EmotionalContaigon : MonoBehaviour
{
    [Range(0f, 1f)][SerializeField] float initialEmotionValue = 0f;
    [Range(0f, 1f)][SerializeField] float emotionValue = 0f;

    [Tooltip("Liu et al. el — critical threshold for speed formula (Eq. 10)")]
    [Range(0f, 1f)][SerializeField] float panicThreshold = 0.45f;

    [SerializeField] float contaigonRadius = 1.5f;
    [Tooltip("Liu et al. Te — emotion contagion interval (seconds)")]
    [SerializeField] float contaigonRate = 1f;

    [Tooltip("Liu et al. dfmin — full arousal within this distance to shooter (m)")]
    [SerializeField] float distanceMin = 5f;
    [Tooltip("Liu et al. dfmax — no direct arousal beyond this distance (m)")]
    [SerializeField] float distanceMax = 20f;

    [Tooltip("Liu et al. w — adjustment parameter (paper supermarket example uses 6)")]
    [SerializeField] float emotionAdjustmentW = 100f;
    [Tooltip("Liu et al. Ab — emotion absorption coefficient (0.6–1.0 by personality; default 0.8)")]
    [SerializeField] float absorptionCoefficient = 0.8f;

    [Tooltip("Liu et al. K — speed multiplier when e >= el (Eq. 10)")]
    [SerializeField] float speedModifier = 2f;
    
    float initialSpeed;

    float elapsedTime;
    bool callOnce = true;

    VictimController victimController;

    // Start is called before the first frame update
    void Start()
    {
        
        victimController = GetComponent<VictimController>();
        contaigonRadius = Parameters.contagionAOE;
        initialSpeed = victimController.speed;
    }

    // Update is called once per frame
    void Update()
    {
        if (!victimController.dead)
        {
            if (SimController.panicStarted)
            {
                if (callOnce)
                {
                    InitialPanic();
                    callOnce = false;
                }
                else
                {
                    elapsedTime += Time.deltaTime;

                    if (elapsedTime >= contaigonRate)
                    {
                        RecieveEmotion();
                        elapsedTime = 0f;
                    }
                }
            }
        }
    }

    void RecieveEmotion()
    {
        // Eq. (8): unique neighbors within Re (one entry per agent, not per collider)
        var neighbors = new HashSet<EmotionalContaigon>();
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, contaigonRadius);
        foreach (Collider hitCollider in hitColliders)
        {
            EmotionalContaigon otherEmotion = hitCollider.GetComponentInParent<EmotionalContaigon>();
            if (otherEmotion != null && otherEmotion != this)
            {
                neighbors.Add(otherEmotion);
            }
        }

        int n = neighbors.Count;
        if (n > 0)
        {
            float sum = 0f;
            foreach (EmotionalContaigon other in neighbors)
            {
                sum += other.emotionValue;
            }

            float Me = sum / n; // Eq. (8)
            float e = emotionValue;

            // Eq. (9): only pull up when below neighborhood average
            if (e < Me)
            {
                float e0 = initialEmotionValue;
                float delta = ((Me - e) / n) * emotionAdjustmentW * absorptionCoefficient;
                emotionValue = Mathf.Clamp01(e0 + delta);
            }
        }

        UpdateSpeed();
    }

    // Calculates the initial panic intensity when the shooting starts
    void InitialPanic()
    {
        Vector3 shooterPos = GameObject.FindGameObjectWithTag("Shooter").transform.position;
        float distance = Vector3.Distance(this.transform.position, shooterPos);
        if (distance < distanceMin)
        {
            emotionValue = 1f;
        }
        else if (distance >= distanceMin && distance <= distanceMax)
        {
            emotionValue = 1 - ((distance - distanceMin) / (distanceMax - distanceMin));
        } else
        {
            emotionValue = 0;
        }

        initialEmotionValue = emotionValue;
        print(this + "'s initial ev: " + emotionValue);
        UpdateSpeed();
    }

    void UpdateSpeed()
    {
        // Eq. (10)
        float walkSpeed;
        if (emotionValue < panicThreshold)
        {
            walkSpeed = initialSpeed * (1f + emotionValue);
        }
        else
        {
            walkSpeed = initialSpeed * (1f + speedModifier * emotionValue);
        }

        victimController.ApplyMovementSpeed(walkSpeed);
    }

    private void OnDrawGizmos()
    {
        Color colorStart = Color.blue;
        Color colorEnd = Color.red;

        // Lerp the color based on emotionValue, clamping between 0 and 1
        Gizmos.color = Color.Lerp(colorStart, colorEnd, Mathf.Clamp01(emotionValue));

        Gizmos.DrawWireSphere(transform.position, contaigonRadius);
    }
}
