/* School Shooter Project
 * Written By: Earl Landicho & Runhe Zhu
 * Updated: 10/9/19
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenDoor : MonoBehaviour
{
    public bool isSliding = false;
    public bool isLeft = false;
    public bool isNegative = false;
    public bool doNotLockThis = false;
    public bool sideDoor = false;

    public float doorLockWaitTime = 2.0f;

    [HideInInspector]
    public bool lockDoors = false;

    bool callOnce = true;
    Animator anim;
    Collider col1;
    
    int colliderCount = 0;

    void Start()
    {
        anim = transform.GetChild(0).GetComponent<Animator>();
        col1 = gameObject.GetComponent<BoxCollider>();
        //col2 = transform.GetChild(0).GetComponent<BoxCollider>();

        anim.SetBool("isLeft",isLeft);
        anim.SetBool("isNegative",isNegative);
        anim.SetBool("isSliding",isSliding);

        anim.SetBool("open", false);
    }

    void Update()
    {
        if(SimController.doorsAreNowLocked && !doNotLockThis && callOnce)
        {
            anim.SetBool("open", false);
            col1.enabled = false; //close the door
            callOnce = false;
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Shooter") || col.CompareTag("Victim") || col.CompareTag("Player"))
        {
            if( !sideDoor && (gameObject.transform.position.z > col.transform.position.z) )
            {
                anim.SetBool("isLeft", !isLeft);
            }else if( sideDoor && (gameObject.transform.position.x > col.transform.position.x) )
            {
                anim.SetBool("isLeft", !isLeft);
            } 
            else
            {
                anim.SetBool("isLeft", isLeft);
            }
            
            anim.SetBool("open", true);
            //transform.GetChild(0).transform.GetChild(0).GetComponent<MeshCollider>().enabled = false;
            colliderCount++;
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("Shooter") || col.CompareTag("Victim") || col.CompareTag("Player"))
        {
            colliderCount--; 
            if(colliderCount == 0) { StartCoroutine("doorLockWait", doorLockWaitTime); }
        }
    }

    // Waiting time between the player/victims leave the collider and the door closes.
    IEnumerator doorLockWait(float timer)
    {
        yield return new WaitForSeconds(timer);
        //after waiting for 2 sec and still 0 -> close.
        if(colliderCount == 0) { 
            anim.SetBool("open", false); 
        } 
        //transform.GetChild(0).transform.GetChild(0).GetComponent<MeshCollider>().enabled = true;
    }
}
