using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;
using UnityEngine.Splines;
/*
Current Implementation uses Liu et al.'s model from "A perception-based emotion contagion model in crowd emergent evacuation simulation"
Network: Average
Content: Arousal
Behavior: Speed
*/

public class EmotionalContaigon : MonoBehaviour
{
    [Range(0f, 1f)][SerializeField] float initialEmotionValue = 0f;
    [Range(0f, 1f)][SerializeField] float emotionValue = 0f;
    [Range(0f, 1f)][SerializeField] float panicThreshold = 0.5f;

    [SerializeField] float contaigonRadius = 1.5f;
    [SerializeField] float contaigonRate = 1f;

    [SerializeField] float distanceMin = 5f;
    [SerializeField] float distanceMax = 20f;

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
                        if (emotionValue < 1)
                        {
                            RecieveEmotion();
                        }
                        elapsedTime = 0f;
                    }
                }
            }
        }
    }

    void RecieveEmotion()
    {
        // Number of agents in radius
        float n = 0f;
        // Sum of n's emotionValues
        float sum = 0f;
        // Average value of nearby emotions
        float avg = 0f;

        // Find all colliders within 5 meters
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, contaigonRadius);
        foreach (Collider hitCollider in hitColliders)
        {
            // Check if the collider's game object has the EmotionalContaigon component
            EmotionalContaigon otherEmotion = hitCollider.GetComponent<EmotionalContaigon>();
            if (otherEmotion != null && otherEmotion != this)
            {
                n++;
                sum += otherEmotion.emotionValue;
            }
        }
        avg = sum / n;
        if (emotionValue < avg)
        {
            emotionValue = Mathf.Clamp01(initialEmotionValue + ((avg - emotionValue) / n)); // initial article also multiplies by a modifier and susceptability (personality)
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
    }

    void UpdateSpeed()
    {
        if (emotionValue < panicThreshold)
        {
            victimController.speed = initialSpeed * (1 + emotionValue);
        } else
        {  
            victimController.speed = initialSpeed * (1 + (speedModifier * emotionValue));
        }
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
