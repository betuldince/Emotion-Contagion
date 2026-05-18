using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class HideDestinations : MonoBehaviour
{
    // destinations[0] = Kitchen
    // destinations[1] = Cafeteria
    // destinations[2] = ***Other 1***
    // destinations[3] = ***Other 2***
    public Transform[] destinations;
    public Transform[] kitchenSpots;
    public Transform[] cafSpots;
    public Transform[] miscSpots;

    // Start is called before the first frame update
    void Start()
    {
        kitchenSpots = destinations[0].GetComponentsInChildren<Transform>();
        cafSpots = destinations[1].GetComponentsInChildren<Transform>();
        miscSpots = destinations[2].GetComponentsInChildren<Transform>();

        print("kitchenSpots length: " + kitchenSpots.Length);
        print("cafSpots length: " + cafSpots.Length);
        print("miscSpots length: " + miscSpots.Length);
    }

    // Selects a destination based on the probability from the human trials
    public Transform SelectDestination()
    {
        if (ProbabilityUtils.RandomTrue(.49f))
        {
            int index = Random.Range(1, kitchenSpots.Length);
            print("kitchen spot:" + kitchenSpots[index] + "; index: " + index);
            return kitchenSpots[index];
        }
        else if (ProbabilityUtils.RandomTrue(.31f))
        {
            int index = Random.Range(1, cafSpots.Length);
            print("caf spot:" + cafSpots[index] + "; index: " + index);
            return cafSpots[index];
        }
        else //.19f
        {
            // split between a few alternate locations
            int index = Random.Range(1, miscSpots.Length);
            print("misc spot:" + miscSpots[index] + "; index: " + index);
            return miscSpots[index];
        }
    }
}
