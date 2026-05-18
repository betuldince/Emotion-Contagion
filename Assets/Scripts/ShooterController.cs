using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class ShooterController : MonoBehaviour
{
    public enum FSMStates
    {
        Idle,
        Patrol,
        Attack
    }

    [SerializeField] private int health = 100;

    [SerializeField] private float speed = 2.5f;
    [SerializeField] private float shootrate = .5f;
    [SerializeField] private float spawnDelay = 6f;
    [SerializeField] private float shootingDelay = 2f;
    [SerializeField] private float reactionDelay = 3f;
    [SerializeField] private GameObject muzzleFlash;
    [SerializeField] private AudioClip gunshot;
    [SerializeField] private AudioSource gunshotSource;
    [SerializeField] private AudioClip[] footstepSounds;    // an array of footstep sounds that will be randomly selected from.
    [SerializeField] private AudioSource footstepSource;    // a second audio source so that footsteps sounds can be adjusted independently from gunshots

    [SerializeField] private GameObject patrolPathParent;
    private List<GameObject> patrolPath = new List<GameObject>();
    private Vector3 nextDestination;

    private FSMStates currentState;
    private Animator anim;
    private float elapsedTime = 0;
    private int currentDestinationIndex = 0;
    private NavMeshAgent agent;
    private LineOfSight los;
    private Transform currentTarget = null;
    private ParticleSystem flash;
    private bool allowPatrol = false;
    private bool allowShooting = false;
    private float waitTime;
    private bool idlelocked = false;
    private bool shootingLocked = false;


    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        if (allowPatrol)
        {
            switch (currentState)
            {
                case FSMStates.Patrol:
                    UpdatePatrolState();
                    break;
                case FSMStates.Attack:
                    UpdateAttackState();
                    break;
                case FSMStates.Idle:
                    UpdateIdleState();
                    break;
            }
        }

        anim.SetFloat("velocity", agent.velocity.magnitude);
        elapsedTime += Time.deltaTime;
    }

    void Initialize()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = gameObject.GetComponent<Animator>();
        los = GetComponent<LineOfSight>();
        flash = muzzleFlash.GetComponent<ParticleSystem>();

        foreach (Transform child in patrolPathParent.transform)
        {
            patrolPath.Add(child.gameObject);
            print(child);
        }

        // might need to adjust the order of these 3 lines
        currentState = FSMStates.Idle;
        StartCoroutine("Teleport");
        StartCoroutine("StartRoamingTimer", spawnDelay);
    }
    IEnumerator Teleport()
    {
        print(patrolPath[0]);
        gameObject.transform.position = patrolPath[0].transform.position;
        yield return new WaitForSeconds(.5f);
    }

    IEnumerator StartRoamingTimer(float timer)
    {
        yield return new WaitForSeconds(timer);
        //Debug.Log("Start Roaming");
        FindNextPoint();
        allowPatrol = true;
        currentState = FSMStates.Patrol;
        StartCoroutine("StartShootingTimer", shootingDelay);
    }

    IEnumerator StartShootingTimer(float timer)
    {
        yield return new WaitForSeconds(timer);
        Debug.Log("Start Shooting");
        allowShooting = true;
    }

    void FindNextPoint()
    {
        if (currentDestinationIndex >= patrolPath.Count)
        {

            allowPatrol = false;
            currentState = FSMStates.Idle;
            return;
        }

        nextDestination = patrolPath[currentDestinationIndex].transform.position;
        waitTime = patrolPath[currentDestinationIndex].GetComponent<PatrolSpot>().waitTime;

        currentDestinationIndex++;

        // Loops the shooter's path
        //currentDestinationIndex = (currentDestinationIndex + 1) % patrolPath.Count;

        agent.SetDestination(nextDestination);
    }

    void UpdatePatrolState()
    {
        idlelocked = false;

        agent.isStopped = false;
        anim.SetInteger("animState", 1);

        agent.stoppingDistance = 0;

        agent.speed = speed;

        if (Vector3.Distance(transform.position, nextDestination) < .5f)
        {
            if (waitTime > 0)
            {
                currentState = FSMStates.Idle;
            }
            else
            {
                FindNextPoint();
            }
            FindNextPoint();
        }

        if (allowShooting)
        {
            if (los.visibleTargets.Count > 0)
            {
                Debug.Log("visible targets: " + los.visibleTargets.Count);
                currentState = FSMStates.Attack;
                currentTarget = los.visibleTargets[0];
            }
        }

        FaceTarget(nextDestination);

        agent.SetDestination(nextDestination);
    }

    void UpdateAttackState()
    {
        anim.SetInteger("animState", 3);

        idlelocked = false;

        agent.isStopped = true;

        if (currentTarget == null)
        {
            currentState = FSMStates.Patrol;
            return;
        }
        else if (!los.visibleTargets.Contains(currentTarget))
        {
            currentState = FSMStates.Patrol;
            return;
        }

        Debug.DrawLine(transform.position, currentTarget.position, Color.red);

        FaceTarget(currentTarget.position);

        if (!shootingLocked)
        {
            Shoot();
        }
    }
    void Shoot()
    {
        if (elapsedTime >= shootrate)
        {
            anim.SetInteger("animState", 2);
            print("in Shoot at:" + Time.timeSinceLevelLoad);

            var animDuration = anim.GetCurrentAnimatorStateInfo(0).length;
            elapsedTime = 0.0f;
        }
    }

    // Called in an animation event when animState is set to 2
    void Shooting()
    {
        print("Current target: " + currentTarget);
        if (!SimController.firstShot)
        {
            StartCoroutine("ReactionTimer", reactionDelay);
        }

        currentTarget.GetComponent<VictimController>().TakeDamage();

        los.visibleTargets.Remove(currentTarget);
        currentTarget = null;
    }

    IEnumerator ReactionTimer(float timer)
    {
        SimController.firstShot = true;
        yield return new WaitForSeconds(timer);
        SimController.panicStarted = true;
        Physics.IgnoreLayerCollision(9, 10, false);
    }

    void UpdateIdleState()
    {

        agent.isStopped = true;
        anim.SetInteger("animState", 0);

        if (allowShooting)
        {
            if (los.visibleTargets.Count > 0)
            {
                currentState = FSMStates.Attack;
                currentTarget = los.visibleTargets[0];
            }
        }
        if (allowPatrol)
        {
            if (!idlelocked)
            {
                idlelocked = true;
                Invoke("EndIdle", waitTime);
            }
        }
    }

    void EndIdle()
    {
        currentState = FSMStates.Patrol;
        FindNextPoint();
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log("Shooter took " + damage + " damage. Remaining health: " + health);

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Shooter has been killed.");
        Destroy(gameObject); // Remove the shooter from the scene
    }

    void FaceTarget(Vector3 target)
    {
        Vector3 directionTarget = (target - transform.position).normalized;
        directionTarget.y = 0;
        if ( (directionTarget != Vector3.zero))
        {
            Quaternion lookRotation = Quaternion.LookRotation(directionTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 10 * Time.deltaTime);
        }
    }

    // These functions are called by animation events
    void PlayFootstep()
    {
        int n = Random.Range(1, footstepSounds.Length);
        AudioClip temp = footstepSounds[n];
        footstepSource.PlayOneShot(temp);
        // move picked sound to index 0 so it's not picked next time
        footstepSounds[n] = footstepSounds[0];
        footstepSounds[0] = temp;
    }

    // Called in an animation event when animState is set to 2
    void PlayGunshot()
    {
        gunshotSource.PlayOneShot(gunshot);
    }

    void MuzzleFlash()
    {
        flash.Play();
    }
}
