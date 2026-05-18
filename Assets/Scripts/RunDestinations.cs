using UnityEngine;


// thius script should be put on a SimManager empty object in the scene
public class RunDestinations : MonoBehaviour
{
    // destinations[0] = North Exit
    // destinations[1] = East (Far Right Top) Exit
    // destinations[2] = Courtyard Exit
    public Transform[] destinations;

    // Start is called before the first frame update
    void Start()
    {
        // initialize destinations, or set in the inspector
    }

    // Selects a destination based on the probability from the human trials
    // Could also use x,y coordinates if needs to be static, but there might be more work to actually set the destination
    public Transform SelectDestination()
    {
        if (ProbabilityUtils.RandomTrue(.82f))
        {
            return destinations[0];
        }
        else if (ProbabilityUtils.RandomTrue(.13f))
        {
            return destinations[1];
        } 
        else
        {
            return destinations[2];
        }
    }
}
