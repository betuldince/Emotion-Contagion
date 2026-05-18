using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SimController : MonoBehaviour
{
    public static bool firstShot = false;
    public static bool panicStarted = false;
    public static bool doorsAreNowLocked = false;
    public static float doorTimerStatic;

    public float doorTimer = 20;
    public bool lockAllDoors = true;

    bool playOnce = true;
    GameObject[] doors;


    void Awake()
    {
        Physics.IgnoreLayerCollision(9, 10);
        doors = GameObject.FindGameObjectsWithTag("Door");
    }

    void Start()
    {
        doorTimerStatic = doorTimer;
        lockAllDoors = false;
    }

    void Update()
    {
        if (panicStarted && playOnce && lockAllDoors)
        {
            StartCoroutine("DoorTimer", doorTimer);
            playOnce = false;
        }
    }

    IEnumerator DoorTimer(float timer)
    {
        yield return new WaitForSeconds(timer);
        doorsAreNowLocked = true;
    }
}
