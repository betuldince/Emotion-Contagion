// Edit by Alyssa Franczak
// Edit by Nutchanon Yongsatianchot 
// Originally from USC

using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SocialPlatforms;

public class VictimController : MonoBehaviour
{
    [SerializeField] private bool gore = Parameters.gore;
    [SerializeField] private float rotate_speed = 1f;
    [SerializeField] private float waitingTime = 0;
    [SerializeField][Range(0f, 1f)] private float screamVol = .8f;
    [SerializeField][Range(0f, 1f)] private float groanVol = .4f;

    [SerializeField] private AudioClip deathScream;
    [SerializeField] private AudioClip deadGroan;
    [SerializeField] private Object bloodPuddle;
    [SerializeField] private Object bloodGush;

    // Used by NPCSpeakController to determine if theyre hidden yet
    [HideInInspector] public bool isHidden = false;
    [HideInInspector] public bool dead = false;
    public float speed = 3;
    private GameObject shooter;

    [SerializeField] private float attackRange = 1.5f; // Range within which the victim can attack
    [SerializeField] private int attackDamage = 10;    // Damage dealt to the shooter
    [SerializeField] private float attackCooldown = 1.5f; // Time between attacks

    private float lastAttackTime; // Tracks time of the last attack

    private bool run = false;
    private bool fight = false;
    private bool changeBaseOffset = false;
    private bool isWaiting = true;
    private bool isRotateWaiting = true;
    private bool callOnce = true;

    private NavMeshAgent agent;
    private Animator anim;
    private CapsuleCollider cap;
    private AudioSource audioSource;

    private Vector3 destination;

    /*    int maxHealth = 2;
        int curHealth;*/



    void Start()
    {
        //curHealth = maxHealth;
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        cap = GetComponent<CapsuleCollider>();
        audioSource = GetComponent<AudioSource>();
        anim.SetBool("death", false);
        ApplyMovementSpeed(speed);
        agent.isStopped = true;
        rotate_speed += Random.Range(-1f, 1f);
        fight = ProbabilityUtils.RandomTrue(Parameters.fightProbability);
        shooter = GameObject.FindGameObjectWithTag("Shooter");

        Debug.Log(this + " is fighting: " + fight);

        Invoke(nameof(SelectRunHideFight), 1);
    }

    void SelectRunHideFight()
    {
        if (fight)
        {
            AttackShooter();
        }
        else
        {
            //Debug.Log("selecting run/hide");
            // modify this to instead be a ratio of what the people around them are doing * need a base decision  
            float rand = Random.Range(0, 1);
            float probability = ProbabilityUtils.EvaluateDefaultSpline(rand);
            run = ProbabilityUtils.RandomTrue(probability);

            if (run)
            {
                SelectRunDestination();
            }
            else
            {
                SelectHideDestination();
            }
        }
    }

    void SelectRunDestination()
    {
        //Debug.Log("running");
        RunDestinations rd = GameObject.FindAnyObjectByType<RunDestinations>();
        destination = rd.SelectDestination().position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(destination, out hit, 1.0f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            Debug.LogError(this + " destination not on NavMesh: " + destination);
        }
        agent.SetDestination(destination);
        Debug.Log(this + "'s run destination: " + destination);
    }

    void SelectHideDestination()
    {
        //Debug.Log("hiding");
        HideDestinations hd = GameObject.FindAnyObjectByType<HideDestinations>();
        destination = hd.SelectDestination().position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(destination, out hit, 1.0f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            Debug.LogError(this + " destination not on NavMesh: " + destination);
        }
        agent.SetDestination(destination);
        Debug.Log(this + "'s hide destination: " + destination);
    }

    void AttackShooter()
    {
        if (shooter != null)
        {
            // Set initial destination to the shooter 
            agent.SetDestination(shooter.transform.position);
        }
        else
        {
            Debug.LogWarning("Shooter not found. Ensure the Shooter object has the 'Shooter' tag.");
        }
    }

    void Update()
    {
        if (!dead) Decide();
    }

    void AnimationTransition()
    {
        if (agent.velocity.magnitude > 1.0f && changeBaseOffset)
        {
            agent.baseOffset = -0.01f;
            anim.applyRootMotion = false;
            changeBaseOffset = false;
        }
        //??? crouching animation??
        if (cap.isTrigger == false && callOnce)
        {
            cap.center = new Vector3(0, 1, 0);
            cap.height = 2 * cap.height;
            callOnce = false;
        }

        //animation transition
        //curRot = transform.rotation.eulerAngles.y;
        //changeInRot = (curRot - lastRot) / Time.deltaTime;
        //lastRot = curRot;
        //anim.SetFloat("rotation", changeInRot);

        anim.SetFloat("velocity", agent.velocity.magnitude);
    }

    //new refractor of Survive function
    void Decide()
    {
        // could put inside panicStarted, but test before cleaning
        if (SimController.firstShot && !SimController.panicStarted)
        {
            if (isRotateWaiting) { StartCoroutine(WaitRotate(Random.Range(0.25f, 2f))); return; }
            GameObject shooter = GameObject.Find("Shooter");
            var lookPos = shooter.transform.position - transform.position;
            lookPos.y = 0;
            var rotation = Quaternion.LookRotation(lookPos);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotate_speed);
        }

        //If the scenario has not started yet (i.e., gun sound), don't do anything 
        if (!SimController.panicStarted)
        {
            return;
        }

        if (isWaiting) 
        {
            isWaiting = false;
            StartCoroutine(WaitTimer(waitingTime)); return; 
        }
        if (fight && shooter != null)
        {
            agent.SetDestination(shooter.transform.position);

            // Check if within attack range and attack
            if (Vector3.Distance(transform.position, shooter.transform.position) <= attackRange)
            {
                if (Time.time - lastAttackTime >= attackCooldown)
                {
                    DealDamageToShooter();
                    lastAttackTime = Time.time;
                }
            }
        }


        CheckDistance();
        AnimationTransition();
    }

    void CheckDistance()
    {
        if (!agent.pathPending && agent.hasPath && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (agent.isStopped) return;

            if (fight) 
            {
              
            }
            else if (run)
            {
                Debug.Log(this + " arrived at exit: " + destination);
                Debug.Log(this + "'s position: " + transform.position);
                Destroy(gameObject);
            }
            else
            {
                Debug.Log(this + " arrived at hiding spot: " + destination);
                Debug.Log(this + "'s position: " + transform.position);
                agent.isStopped = true;
                AnimationTransition();
                ToggleCrouch();
            }
        }
    }

    IEnumerator WaitRotate(float timer)
    {
        yield return new WaitForSeconds(timer);
        isRotateWaiting = false;

        //anim.applyRootMotion = false;
        //anim.SetBool("stand",true);
        //StartCoroutine("StopAnimation",0.5f);
    }

    // Wait time for the agent before the shooting starts.
    IEnumerator WaitTimer(float timer)
    {
        yield return new WaitForSeconds(timer);
        anim.applyRootMotion = false;
        if (!dead) agent.isStopped = false;
        Debug.Log(this + "'s wait timer finished");
    }

    /// <summary>
    /// Sets base walk speed (e.g. from emotional contagion) and syncs NavMeshAgent.speed.
    /// </summary>
    public void ApplyMovementSpeed(float walkSpeed)
    {
        speed = walkSpeed;
        SyncAgentSpeed();
    }

    void SyncAgentSpeed()
    {
        if (agent == null || !agent.enabled)
        {
            return;
        }

        float crouchFactor = anim.GetBool("crouching") ? 0.25f : 1f;
        agent.speed = speed * crouchFactor;
    }

    // Change the crouching or standing position of the victim.
    void ToggleCrouch()
    {
        if (anim.GetBool("crouching"))
        {
            anim.SetBool("crouching", false);
        }
        else
        {
            anim.SetBool("crouching", true);
            isHidden = true;
        }

        SyncAgentSpeed();
    }

    // Reduce the health point of the victim that is being shot by the shooter.
    public void TakeDamage()
    {
        if (!dead)
        {
            if (gore)
            {
                audioSource.volume = screamVol;
                audioSource.PlayOneShot(deathScream);
                Invoke(nameof(Groan), 2);
                Debug.Log("Screaming");

                Vector3 shooterPosition = GameObject.FindGameObjectWithTag("Shooter").transform.position;
                Vector3 directionToShooter = (transform.position - shooterPosition).normalized;
                Quaternion gushRotation = Quaternion.LookRotation(directionToShooter);
                Vector3 gushPosition = transform.position;
                gushPosition.y = 1;
                Instantiate(bloodGush, gushPosition, gushRotation, this.transform);
                Vector3 puddlePosition = transform.position;
                puddlePosition.y = 0.01f;
                Instantiate(bloodPuddle, puddlePosition, Quaternion.Euler(-90, 0, 0));
            }
            dead = true;
            Debug.Log(this + "has been hit at " + Time.timeSinceLevelLoad);
            anim.SetBool("death", true);
            cap.isTrigger = true;
            cap.center = new Vector3(0, 0, 0);
            agent.isStopped = true;
            agent.enabled = false;
        }
    }

    void DealDamageToShooter()
    {
        ShooterController shooterController = shooter.GetComponent<ShooterController>();
        if (shooterController != null)
        {
            shooterController.TakeDamage(attackDamage);
            Debug.Log(this + " dealt " + attackDamage + " damage to shooter.");
        }
        else
        {
            Debug.LogWarning("Shooter does not have a ShooterController script attached!");
        }
    }

    void Groan()
    {
        if (deadGroan != null)
        {
            audioSource.clip = deadGroan;
            audioSource.volume = groanVol;
            // may or may not want to loop - test
            audioSource.loop = true;
            audioSource.Play();
        }
    }
}
