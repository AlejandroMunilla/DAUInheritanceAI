using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour {

    public GameObject target;
    public GameObject guardTarget;
    public GameObject ghostAvatar;
    public Camera cam;
    public bool alive = true;
    public bool dead = false;
    public bool damResetTimer = true;
    public bool ghostAvatarBool = false;
    public bool aiming = false;

    public float attackTimer;
    public float timeBetweeenAttack = 2;
    public float distToTarget;
    public float soporDistance;
    public float maxGuardDistance = 15;
    public float minGuardDistance = 3;
    public float guardDistanceAttack = 6;
    public string attackAnim = "st";
    public string behaviour = "None";
    public Vector3 nextRot;

    private bool jumping;
    private CapsuleCollider cc;
    protected GameController gc;
    protected Animator anim;
    protected NavMeshAgent nav;
    protected PlayerAttack pa;
    protected Rigidbody rb;

    public float stunTime = 10;
    private float jumpTimer;
    private float jumpInitialHeight;
    protected PlayerStats ps;
    private float audioCounter = 0;
    private AudioSource audioSource;
    private bool loaded = false;

    //Multiplayer
    public float forward = 0;
    public float turn = 0;
    private ThirdPersonUserControl tpu;
    public bool curing = false;
    public GameObject playerBeingCured;

    public enum State
    {
        Idle,
        Search,
        Move,
        MoveToEngage,
        Follow,
        Guard,
        Attack,
        Stun,
        Dead,
        UsingAbility,
        UnderHit,
        Sopor,
        Entagled,
        GhostAvatarMove,
        GhostAvatarIdle,
        GhostCuring,
        PlayerFollow,
        PlayerIdle,
        Cinematic                           //Sort of Idle, doing knowing, letting Animations and external factors (e.g. Inheritance) to do the work
    }

    public State state;

    private void OnEnable()
    {
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        cc = GetComponent<CapsuleCollider>();
        pa = GetComponent<PlayerAttack>();
        ps = GetComponent<PlayerStats>();
        rb = GetComponent<Rigidbody>();
        pa.distanceToAttack = ps.attacktRange;
        attackTimer = Time.timeSinceLevelLoad;
        foreach (Camera ca in Camera.allCameras)
        {
            if (ca.name == "Camera1")
            {
                cam = ca;
            }
        }

        if (ghostAvatarBool == true)
        {
            state = State.GhostAvatarIdle;
            ghostAvatar = new GameObject();
            ghostAvatar.transform.position = transform.position;
            ghostAvatar.transform.rotation = transform.rotation;
            tpu = GetComponent<ThirdPersonUserControl>();
            pa.InstantiateBullets();
        }
        else if (gc.isRPG)
        {
            state = State.PlayerIdle;
        }

        else if (behaviour != "Guard")
        {
            state = State.Search;
        }
        else
        {
            state = State.Guard;
        }
      
        

        //For FP modes
        if (gc.sceneFP == true)
        {
      //      Debug.Log(ps.speedFP);
            if (gameObject.tag == "Enemy")
            {
                nav.speed = ps.speedFP;
            }

        }


        audioSource = GetComponent<AudioSource>();

        Invoke("DelayStart", 2);

        if (stunTime == 0)
        {
            stunTime = 10;
        }

    }

    private void OnDisable()
    {
        if (gameObject.activeSelf)
        {
            nav.isStopped = true;
        }
        StopCoroutine("FSM");
        state = State.Search;
        anim.SetFloat("Forward", 0);
        anim.SetFloat("Turn", 0);

    }

    private IEnumerator FSM ()
    {
        while (alive)
        {
            switch (state)
            {
                case State.Idle:
                    Idle();
                    yield return new WaitForSeconds(0.1f);
                    break;
                case State.Search:
                    yield return new WaitForSeconds(1.0f);
                    Search();                    
                    break;
                case State.Move:
                    Debug.Log("Move");
                    yield return new WaitForSeconds(0.1f);
                    break;
                case State.MoveToEngage:
                    MoveToEngage();
                    yield return new WaitForSeconds(0.01f);
                    break;

                case State.Follow:
                    Follow();
                    yield return new WaitForSeconds(0);
                    break;

                case State.Guard:
                    Guard();
                    yield return new WaitForSeconds(0);
                    break;

                case State.Attack:                    
                    Attack();
                    yield return new WaitForSeconds(timeBetweeenAttack);
                    break;
                case State.Dead:
                    Dead();
                    yield return new WaitForSeconds(0.2f);
                    break;
                case State.Stun:
             //       Debug.Log(gameObject.name + "/Stun");
                    yield return new WaitForSeconds(0.1f);
                    Stun();
                    break;
                case State.UsingAbility:
                    yield return new WaitForSeconds(0.5f);                   
                    break;
                case State.UnderHit:
             //       UnderHit();
                    yield return new WaitForSeconds(0.1f);
                    break;
                case State.Sopor:
                    Sopor();
                    yield return new WaitForSeconds(0.1f);
                    break;
                case State.Entagled:
                    Debug.Log(gameObject.name + "/Entagled");
                    yield return new WaitForSeconds(stunTime);
                    Entagled();
                    break;

                case State.GhostAvatarIdle:
                    yield return new WaitForSeconds(0.05f);
                    GhostAvatarIdle();
                    break;

                case State.GhostAvatarMove:
              //      Debug.Log(gameObject.name + "/GhostAvatar");
                    yield return new WaitForSeconds(0.05f);
                    GhostAvatarMove();
                    break;

                case State.GhostCuring:
                    //      Debug.Log(gameObject.name + "/GhostAvatar");
                    yield return new WaitForSeconds(0.05f);
                    GhostCuring();
                    break;


                case State.PlayerFollow:
                    //      Debug.Log(gameObject.name + "/GhostAvatar");
                    yield return new WaitForSeconds(0.05f);
                    PlayerFollow();
                    break;

                case State.PlayerIdle:
                    //      Debug.Log(gameObject.name + "/GhostAvatar");
                    yield return new WaitForSeconds(0.05f);
                    PlayerIdle();
                    break;

                case State.Cinematic:
                    //      Debug.Log(gameObject.name + "/GhostAvatar");
                    yield return new WaitForSeconds(0.1f);
                    Cinematic();
                    break;
            }
        }
        yield return null;
    }

	void Start ()
    {

	}

    private void DelayStart ()
    {
        if (gameObject.activeSelf)
        {
            StartCoroutine("FSM");
        }
     
    }

    private void Idle()
    {
        if (dead == false)
        {
            anim.SetFloat("Forward", 0);
        }
        else
        {
            state = State.Dead;
        }        
    }

    private void Search ()
    {
        

        if (dead == false)
        {
            if (behaviour == "Guard")
            {
                state = State.Guard;
            }
            else
            {
                target = ChoosePCbyDistance(gameObject);

                if (target != null)
                {
                    distToTarget = Vector3.Distance(target.transform.position, transform.position);

                    if (distToTarget < ps.attacktRange)
                    {
                        state = State.Attack;
                        transform.LookAt(new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z));
                        if (nav.isStopped == false)
                        {
                            nav.isStopped = true;
                        }
                    }
                    else
                    {
                        state = State.MoveToEngage;
                        if (nav.isStopped == true)
                        {
                            nav.isStopped = false;
                            nav.destination = target.transform.position;

                        }
                        anim.SetFloat("Forward", 1);
                    }
                }
            }
        }
        else
        {
            state = State.Dead;
        }
 
    }

    private void Guard ()
    {
        if (dead == false)
        {         
            anim.SetFloat("Forward", 0);
            float distToGuarded = Vector3.Distance(transform.position, guardTarget.transform.position);    
            if (distToGuarded > maxGuardDistance && behaviour == "Guard")   // disengage even from combat to follow your warden. 
            {
                target = null;
                state = State.Follow;
                anim.SetFloat("Forward", 1);
            }
            else
            {
                if (target != null)
                {
                    float distToTargetFromWarden = Vector3.Distance(transform.position, guardTarget.transform.position);

                    if (distToTargetFromWarden <= guardDistanceAttack)
                    {
                        float distToTarget = Vector3.Distance(transform.position, transform.position);
                        if (distToTarget < ps.attacktRange)
                        {
                            state = State.Attack;
                            transform.LookAt(new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z));
                            if (nav.isStopped == false)
                            {
                                nav.isStopped = true;
                            }
                        }
                        else
                        {
                            state = State.MoveToEngage;
                            if (nav.isStopped == true)
                            {
                                nav.isStopped = false;
                                nav.destination = target.transform.position;
                            }
                            anim.SetFloat("Forward", 1);
                        }
                    }
                    else
                    {
                        if (distToGuarded >= minGuardDistance)
                        {
                            target = null;
                            state = State.Follow;
                            anim.SetFloat("Forward", 1);
                        }                  
                    }
                }
                else
                {
                    target = ChoosePCbyDistance(guardTarget);
                    if (target == null)
                    {
                        if (distToGuarded >= minGuardDistance)
                        {
                            target = null;
                            state = State.Follow;
                            anim.SetFloat("Forward", 1);
                        }
                    }
                }       
            }
        }
        else
        {
            state = State.Dead;
        }
    }

    public virtual void MoveToEngage ()
    {
        if (anim.GetBool ("Stun") == true)
        {
            anim.SetBool("Stun", false);
        }

        if (ps.health == 0 || dead == true)
        {
            state = State.Dead;
        }
        else if (target != null)
        {
            if (target.GetComponent<PlayerStats>().invisible == false)
            {
                float distToGuarded = 0;
                if (guardTarget != null)
                {
                    distToGuarded = Vector3.Distance(transform.position, guardTarget.transform.position);
                }
                


                if (behaviour == "Guard" && distToGuarded > maxGuardDistance && guardTarget != null)
                {
                    // disengage even from combat to follow your warden. 
                    target = null;
                    state = State.Follow;
                    anim.SetFloat("Forward", 1);
                }
                else
                {
                    string tagTemmp = "";
                    string tagtemp2 = "";
                    if (gameObject.tag == "Enemy" && gameObject.tag == "EnemyAlly")
                    {
                        tagTemmp = "PlayerDead";
                        tagtemp2 = "AllyDead";
                    }
                    else if (gameObject.tag == "Player" && gameObject.tag == "Ally")
                    {
                        tagTemmp = "EnemyDead";
                        tagtemp2 = "EnemyAllyDead";
                    }

                    if (target.tag == tagTemmp || target.tag == tagtemp2)
                    {
                        ChangeToSearch();
                    }
                    else
                    {
                        distToTarget = Vector3.Distance(target.transform.position, transform.position);
                        bool rangedAttack = false;
                        bool withinScreen = false;
                        if (gc.arenaMode == false)
                        {
                            withinScreen = WithinScreen();
                        }

                        if (pa.rangedAttack == true)
                        {
                            if (withinScreen == true)
                            {
                                rangedAttack = true;
                            }
                        }
                        //              Debug.Log(distToTarget + "/" + ps.attacktRange);

                        if (audioSource != null)
                        {
                            if (distToTarget < 8 && audioCounter < Time.timeSinceLevelLoad)
                            {
                                audioSource.Play();
                                audioCounter = Time.timeSinceLevelLoad + 8;
                            }
                        }


                        //         Debug.Log(gameObject.name + "/" + distToTarget + "/" +  ps.attacktRange);
                        if (distToTarget < (ps.attacktRange - 0.1f) || rangedAttack == true)
                        {
                            state = State.Attack;
                            anim.SetTrigger("IdleTrigger");
                            anim.SetFloat("Forward", 0);

                            transform.LookAt(new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z));
                            if (nav.isStopped == false)
                            {
                                nav.isStopped = true;
                            }
                        }
                        else
                        {
                            if (nav.isStopped == true)
                            {
                                nav.isStopped = false;
                            }
                            GameObject tempTarget = ChoosePCbyDistance(gameObject);

                            if (tempTarget != null)
                            {
                                if (tempTarget != target)
                                {
                                    //                      Debug.Log(tempTarget);
                                    target = tempTarget;
                                }
                            }

        
                            nav.destination = target.transform.position;
                    //        transform.LookAt(new Vector3(transform.position.x, target.transform.position.y, transform.position.z));

                            transform.LookAt(target.transform);
                            anim.SetFloat("Forward", 1);
                        }
                    }
                }   
            }
            else
            {
                ChangeToSearch();
            }       

        }
        else
        {
            ChangeToSearch();
        }
    }

    private void Follow ()
    {
        nav.destination = guardTarget.transform.position;
        transform.LookAt(new Vector3(guardTarget.transform.position.x, transform.position.y, guardTarget.transform.position.z));

        //                    transform.LookAt(target.transform);
        anim.SetFloat("Forward", 1);

        float distToGuard = Vector3.Distance(guardTarget.transform.position, transform.position);
        if (distToGuard < minGuardDistance)
        {
            state = State.Guard;
            anim.SetTrigger("IdleTrigger");
            anim.SetFloat("Forward", 0);

            transform.LookAt(new Vector3(guardTarget.transform.position.x, transform.position.y, guardTarget.transform.position.z));
            if (nav.isStopped == false)
            {
                nav.isStopped = true;
            }
        }
        else
        {
            if (nav.isStopped == true)
            {
                nav.isStopped = false;
            }
        }
    }

    public virtual void Attack ()
    {
        if (ps.health == 0 || dead == true)
        {
            state = State.Dead;
        }
        else if (target != null)
        {
            if (audioSource != null)
            {
                if (audioCounter < Time.timeSinceLevelLoad)
                {
                    audioSource.Play();
                    audioCounter = Time.timeSinceLevelLoad + 8;
                }
            }

            if (target.GetComponent<PlayerStats>().invisible == false)
            {
                string tagTemmp = "";
                string tagtemp2 = "";
                if (gameObject.tag == "Enemy" && gameObject.tag == "EnemyAlly")
                {
                    tagTemmp = "PlayerDead";
                    tagtemp2 = "AllyDead";
                }
                else if (gameObject.tag == "Player" && gameObject.tag == "Ally")
                {
                    tagTemmp = "EnemyDead";
                    tagtemp2 = "EnemyAllyDead";
                }

                float distToGuarded = 0;
                if (guardTarget != null)
                {
                    distToGuarded = Vector3.Distance(transform.position, guardTarget.transform.position);
                }



                if (behaviour == "Guard" && distToGuarded > maxGuardDistance && guardTarget != null)  // disengage even from combat to follow your warden. 
                {
                    target = null;
                    state = State.Follow;
                    anim.SetFloat("Forward", 1);
                }
                else if (target.tag != tagTemmp && target.tag != tagtemp2 && target.GetComponent<PlayerStats>().health > 0)
                {
                    bool rangedAttack = false;
                    bool withinScreen = false;
                    if (gc.sceneFP == false)
                    {
                        withinScreen = WithinScreen();
                    }

                    if (pa.rangedAttack == true)
                    {
                        if (withinScreen == true)
                        {
                            rangedAttack = true;
                        }
                    }


                    distToTarget = Vector3.Distance(target.transform.position, transform.position);
                    Debug.Log(distToTarget);
                    if (distToTarget < (ps.attacktRange + 0.1f) || rangedAttack == true)
                    {                
                        transform.LookAt(target.transform);
                        if (nav.isStopped == false)
                        {
                            nav.isStopped = true;
                        }
                        anim.SetFloat("Forward", 0);
            //            Debug.Log(attackTimer + 2 + "/" + Time.timeSinceLevelLoad + "/" + pa.rangedAttack);
                        if ((attackTimer) < Time.timeSinceLevelLoad + timeBetweeenAttack)
                        {
                            attackTimer = Time.timeSinceLevelLoad + timeBetweeenAttack;
                            int animNo = Random.Range(1, 4);
                            if (attackAnim == "st")
                            {
                                anim.SetTrigger("Attack" + animNo.ToString());
                            }
                            else
                            {
                                anim.SetTrigger(attackAnim);
                            }
                      
                            if (pa.rangedAttack == false)
                            {
                                
                                pa.ResolveAttack(target);
                            }
                            else
                            {
                                transform.LookAt(target.transform.position);
                                pa.Attack();
                            }

                        }
                    }
                    else
                    {
                        state = State.MoveToEngage;
                        anim.SetTrigger("IdleTrigger");
                        anim.SetFloat("Forward", 1);

                        //           transform.LookAt(new Vector3(transform.position.x, target.transform.position.y, transform.position.z));
                        transform.LookAt(target.transform);
                        if (nav.isStopped == true)
                        {
                            nav.isStopped = false;
                        }
                    }
                }
                else
                {
                    ChangeToSearch();
                }
            }
            else
            {
                ChangeToSearch();
            }

        }
        else
        {
            ChangeToSearch();
        }


    }

    private void Stun ()
    {
    //    Debug.Log(gameObject.name + "/Stun" + stunTime + "/" + Time.timeSinceLevelLoad);

        if (stunTime <= Time.timeSinceLevelLoad)
        {
            if (ps.health > 0)
            {
                ChangeToSearch();
            }
            else
            {
                if (state != State.Dead)
                {
                    GetComponent<PlayerStats>().Death();
                    state = State.Dead;
                }
            }
        }
    }

    private void Entagled ()
    {
        Debug.Log(gameObject.name + "/Entangled");
        if (ps.health > 0)
        {
            state = State.Search;
            anim.SetBool("Stun", false);
            anim.SetTrigger("IdleTrigger");
            nav.isStopped = false;
        }
        else
        {
            if (state != State.Dead)
            {
                GetComponent<PlayerStats>().Death();
                state = State.Dead;
            }
        }

        transform.Find("Entagled").gameObject.SetActive(false);
    }
    

    private void Dead ()
    {
        if (nav.isStopped == false)
        {
            nav.isStopped = true;
            GetComponent<Rigidbody>().isKinematic = true;
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

        }

        if (anim.GetBool ("Dead") == false)
        {
            anim.SetBool("Dead", true);
        }

        if (target != null)
        {
            target = null;
        }
    }

    private void Sopor ()
    {

    }


    public virtual void Cinematic ()
    {
        if (ps.stateAI == "Normal")
        {
            state = State.Idle;
        }
    }

    private GameObject ChoosePCbyDistance (GameObject origin)
    {
        float distance = Mathf.Infinity;
        float distanceVisible = Mathf.Infinity;
        GameObject goChosen = null;
        GameObject goVisibleChosen = null;
        GameObject finalTarget = null;


        if (gc != null)
        {

            List<GameObject> potentialTargets = new List<GameObject>();
            List<GameObject> potentialTargets2 = new List<GameObject>();


            if (gameObject.tag == "Enemy")
            {
                potentialTargets = gc.players;
                potentialTargets2 = gc.allies;
            }
            else if (gameObject.tag == "Player" || gameObject.tag == "Ally")
            {
                potentialTargets = gc.enemies;
                potentialTargets2 = gc.enemyAllies;
            }



            if (potentialTargets != null)
            {

                foreach (GameObject go in potentialTargets)
                {
                    float distanceTo = Vector3.Distance(go.transform.position, transform.position);

                    if (go.tag != "PlayerDead"  && go.tag != "EnemyDead" && go.GetComponent<PlayerStats>().invisible == false)
                    {
                        if (PlayerIsVisible(go))
                        {
                            distanceVisible = distanceTo;
                            goVisibleChosen = go;
                        }
                        else if (distanceTo < distance)
                        {

                            distance = distanceTo;
                            goChosen = go;
                            //            Debug.Log(distance + "/" + goChosen.name);

                        }
                    }

                }

            }


            if (potentialTargets2 != null)
            {
                if (gc.allies.Count > 0)
                {
                    foreach (GameObject go in potentialTargets2)
                    {
                        float distanceTo = Vector3.Distance(go.transform.position, transform.position);

                        if (go.tag != "AllyDead" && go.tag != "EnemyDead" && go.GetComponent<PlayerStats>().invisible == false)
                        {
                            if (PlayerIsVisible(go))
                            {
                                distanceVisible = distanceTo;
                                goVisibleChosen = go;
                            }
                            else if (distanceTo < distance)
                            {

                                distance = distanceTo;
                                goChosen = go;
                                //            Debug.Log(distance + "/" + goChosen.name);

                            }
                        }

                    }
                }
            }



            if (goVisibleChosen != null)
            {
                finalTarget = goVisibleChosen;
                distToTarget = distanceVisible;
            }
            else
            {
                finalTarget = goChosen;
                distToTarget = distance;
            }


        }






        return finalTarget;
    }

    private bool WithinScreen()
    {
        bool withinScreen = true;
        Vector3 viewPos = cam.WorldToViewportPoint(transform.position);

        if (viewPos.x > 0.98f || viewPos.x < 0.02f )
        {
            withinScreen = false;
        }
        if (viewPos.y > 0.98f || viewPos.y < 0.02f)
        {
            withinScreen = false;
        }

        return withinScreen;
    }


    private void ChangeToSearch()
    {
    //    Debug.Log("Hit Change");
    if (nav == null)
        {
            nav = GetComponent<NavMeshAgent>();
        }
        nav.destination = transform.position;
        if (nav.isStopped == true)
        {
            nav.isStopped = false;
        }
        anim.SetFloat("Forward", 0);

        if (state == State.Stun)
        {
            anim.SetBool("Stun", false);
            anim.SetTrigger("IdleTrigger");
        }

        if (behaviour != "Guard")
        {
            state = State.Search;
            
        }
        else
        {
            state = State.Guard;
        }


        if (GetComponent<Rigidbody>() != null)
        {
            if (GetComponent<Rigidbody>().constraints == RigidbodyConstraints.FreezeAll)
            {
                GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            }
        } 

        CancelInvoke("InvokeHit");
    }

    public void ChangeToEntagled(float timeTo)
    {
        stunTime = timeTo;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
        if (anim == null)
        {
            anim = GetComponent<Animator>();
        }
        anim.SetBool("Stun", true);
        state = State.Entagled;
        if (nav == null)
        {
            nav = GetComponent<NavMeshAgent>();
        }
        nav.isStopped = true;
        GameObject effect = Instantiate(Resources.Load("Effects/Entagled"), transform.position, transform.rotation) as GameObject;
        effect.name = "Entagled";
        effect.transform.parent = transform;
    }


    public void ChangeToStun(float timeTo)
    {
        float remainingTime = stunTime - Time.timeSinceLevelLoad;
        if (remainingTime > 0)
        {
            stunTime = stunTime + timeTo;
        }
        else
        {
            stunTime = Time.timeSinceLevelLoad + stunTime;
        }
    //    stunTime = timeTo;
        anim.SetBool("Stun", true);
        state = State.Stun;
        nav.isStopped = true;

    }


    public void GotHit ()
    {
     //   Debug.Log("Hit." + gameObject.name);
        attackTimer = Time.timeSinceLevelLoad;

        //if enemy is not Stun, then it will change to Stun for one second. 
        if (state != State.Stun)
        {
            ChangeToStun(1.0f);
            if (anim == null)
            {
                anim = GetComponent<Animator>();
                nav = GetComponent<NavMeshAgent>();
            }
            Debug.Log(gameObject.name);
            if (anim == null)
            {
                anim = GetComponent<Animator>();
            }
            anim.SetFloat("Forward", 0);
            anim.SetTrigger("IdleTrigger");
            nav.isStopped = true;
       //     Invoke("InvokeHit", 0.01f);
        }
        // If enemy is stun <= 1 second, then it will set up stun time over a second. If above > 1 is due to spell or other state, dont do anything as this is only when hit
        else
        {
            float remainingTime = stunTime - Time.timeSinceLevelLoad;
     //       Debug.Log(remainingTime);
            if (remainingTime <= 1 )
            {
                stunTime = (Time.timeSinceLevelLoad + 1.0f);
            }
        }
       
        //     GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

    }

    /*
    private void UnderHit ()
    {
     //   Debug.Log("hit");
        anim.SetFloat("Forward", 0);
        nav.isStopped = true;
    }*/

    private bool PlayerIsVisible (GameObject go)
    {
        bool playerIsVisible = false;
        RaycastHit hit;
        Vector3 middlePoint = new Vector3(go.transform.position.x, go.transform.position.y + 0.5f, go.transform.position.y);
        Vector3 viewPoint = new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.y);
        Vector3 rayDirection = middlePoint - transform.position;
        if (Physics.Raycast (transform.position, rayDirection.normalized, out hit, 60  ))
        {
   //         Debug.Log(hit + "/" + hit.transform.gameObject + "/" + go.name);
            if (hit.transform.gameObject == go)
            {
                playerIsVisible = true;
            }
        }

        return playerIsVisible;
    }

    /*
    private void InvokeHit ()
    {
        if (dead == false)
        {
            Invoke("InvokeHit", 0.01f);
            if (state != State.UnderHit)
            {
                state = State.UnderHit;
            }
        }
        else
        {
            state = State.Dead;
        }


    }*/


    private void GhostAvatarIdle ()
    {
        if (dead == false)
        {
            distToTarget = Vector3.Distance(ghostAvatar.transform.position, transform.position);
     //       Debug.Log(distToTarget);
            if (distToTarget <= 0.15f)
            {
                float yTemp = Mathf.Lerp(transform.eulerAngles.y, ghostAvatar.transform.eulerAngles.y, 0.5f);
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, yTemp, transform.eulerAngles.z);
                anim.SetFloat("Forward", 0);
            }
            else
            {
                state = State.GhostAvatarMove;
                /*
                if (nav.isStopped == true)
                {
                    nav.isStopped = false;
                    nav.destination = ghostAvatar.transform.position;
                }*/
                anim.SetFloat("Forward", forward);
                anim.SetFloat("Turn", turn);
            }
        }
        else
        {
            state = State.Dead;
        }
    }

    private void GhostAvatarMove ()
    {
        if (dead == false)
        {
            float distanceAvatar = Vector3.Distance(ghostAvatar.transform.position, transform.position);
       //     Debug.Log(distanceAvatar);
            if (distanceAvatar > 0.15f)
            {
                transform.position = Vector3.Lerp(transform.position, ghostAvatar.transform.position, 0.3f);
                float yTemp = Mathf.Lerp(transform.eulerAngles.y, ghostAvatar.transform.eulerAngles.y, 0.5f);
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, yTemp, transform.eulerAngles.z);
                anim.SetFloat("Forward", 0);
                anim.SetFloat("Forward", forward);
                anim.SetFloat("Turn", turn);

            }
            else
            {
                state = State.GhostAvatarIdle;
                anim.SetTrigger("IdleTrigger");
                anim.SetFloat("Forward", 0);

                /*
                if (nav.isStopped == false)
                {
                    nav.isStopped = true;
                }*/
            }
        }
        else
        {
            state = State.Dead;
        }
    }   
    
    public void GhostJump ()
    {
        jumping = true;
        ps.jumpPos = transform.position;
        nav.isStopped = true;
        nav.enabled = false;
        cc.isTrigger = false;
        Invoke("CheckGround", 0);

        if (gameObject.name == "Rose")
        {
            //    rb.velocity = transform.up * 5;

            anim.SetTrigger("JumpTrigger");
            anim.SetBool("OnGround", false);
            rb.velocity = transform.up * 10;
        }
        else
        {
            rb.velocity = transform.up * 7;
            anim.SetBool("OnGround", false);
        }

        jumpTimer = Time.timeSinceLevelLoad;
        ps.jumpPos = transform.position;
        jumpInitialHeight = transform.position.y;

        anim.applyRootMotion = false;
        jumpTimer = Time.timeSinceLevelLoad;
    }

    public void GhostAim ()
    {
     //   Debug.Log("Aiming");
        aiming = true;
        pa.secondaryRangedActive = true;
        anim.SetTrigger("AimTrigger");
        anim.SetBool("Aiming", true);


        if (tpu.secondWeaponBack != null)
        {
            if (tpu.secondWeaponBack.activeSelf == true)
            {

                tpu.secondWeaponBack.SetActive(false);
                tpu.secondWeaponHand.SetActive(true);
                if (tpu.firstWeaponBack != null)
                {

                    foreach (GameObject go in tpu.firstWeaponBack)
                    {
                        go.SetActive(true);
                    }
                    foreach (GameObject go in tpu.firstWeaponsHand)
                    {
                        go.SetActive(false);
                    }
       //             pa.aimingPos = hit.point;
        //            target = hit.transform.gameObject;
                }
            }
        }
    }

    public void GhostStopAim ()
    {
   //     Debug.Log("No Aiming");
        aiming = false;
        anim.SetBool("Aiming", false);
        pa.secondaryRangedActive = false;
        if (tpu.secondWeaponBack != null)
        {
            if (tpu.secondWeaponBack.activeSelf == false)
            {

                tpu.secondWeaponBack.SetActive(true);
                tpu.secondWeaponHand.SetActive(false);
                if (tpu.firstWeaponBack != null)
                {

                    foreach (GameObject go in tpu.firstWeaponBack)
                    {
                        go.SetActive(false);
                    }
                    foreach (GameObject go in tpu.firstWeaponsHand)
                    {
                        go.SetActive(true);
                    }

                }
            }
        }
    }

    private void CheckGround ()
    {
        bool grounded = CheckGroundStatus();

        if (grounded == true & (jumpTimer + 0.5f) < Time.timeSinceLevelLoad)
        {
            CancelInvoke("CheckGround");
            nav.enabled = true;
            nav.isStopped = false;
            cc.isTrigger = true;
            jumping = false;
            if (gameObject.name == "Rose")
            {
                anim.SetTrigger("Idle");
                anim.SetBool("OnGround", true);
            }
            else
            {
                anim.SetBool("OnGround", true);
            }
        }
        else
        {
            Invoke("CheckGround", 0);
        }
    }

    private bool CheckGroundStatus()
    {
        RaycastHit hitInfo;

        //   Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * m_GroundCheckDistance));

        // 0.1f is a small offset to start the ray from inside the character
        // it is also good to note that the transform position in the sample assets is at the base of the character
        if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo))

        {
            float distance = Vector3.Distance(hitInfo.point, transform.position);

            if (distance < 0.2f)
            {
                return true;
            }
        }
        else
        {
            return false;
        }


        return false;
    }

    public void GhostAttack (Quaternion aimPos)
    {

        int randomNo = UnityEngine.Random.Range(1, 4);
        anim.SetTrigger("Attack" + randomNo);
        if (aimPos != new  Quaternion(0,0,0, 0))
        {
            aiming = true;
        }
        else
        {
            aiming = false;
        }



        bool missedTarget = true;
        if (pa.rangedAttack == true || pa.secondaryRangedActive == true)
        {
            missedTarget = false;
            pa.bullet = null;
            for (int cnt = 0; cnt < 8; cnt++)
            {
                if (pa.bullets[cnt].activeSelf == false)
                {
                    pa.bullet = pa.bullets[cnt];
                }
            }

            pa.bullet.GetComponent<BulletController>().damage =  0;
            Debug.Log(aimPos);
            pa.aimingQua = aimPos;

            if (pa.waitForAnimation == true)
            {
                pa.Invoke("WaitForAnimation", pa.waitAnimCounter);

            }
            else
            {
                pa.WaitForAnimation();
            }

        }

    }

    public void GhostCuring ()
    {

    }

    private void PlayerIdle ()
    {
        GameObject go = gc.activePlayer;
        float distanceGO = Vector3.Distance(go.transform.position, transform.position);
        if (distanceGO >= 4)
        {
            state = State.PlayerFollow;
            if (nav.isStopped == true)
            {
                nav.isStopped = false;
                nav.destination = go.transform.position;

            }
       //     anim.SetFloat("Forward", 1);
        }
        else
        {
            anim.SetFloat("Forward", 0);
            anim.SetFloat("Turn", 0);
            if (nav.isStopped == false)
            {
                nav.isStopped = true;
            }
        }
    }

    private void PlayerFollow ()
    {
        GameObject go = gc.activePlayer;
   //     Debug.Log(go.name + "/" + gameObject.name);
        float distanceGO = Vector3.Distance(go.transform.position, transform.position);
        if (distanceGO < 4)
        {
            state = State.PlayerIdle;
            anim.SetTrigger("IdleTrigger");
            anim.SetFloat("Forward", 0);

            transform.LookAt(new Vector3(go.transform.position.x, transform.position.y, go.transform.position.z));
            if (nav.isStopped == false)
            {
                nav.isStopped = true;
            }
        }
        else
        {
            if (nav.isStopped == true)
            {
                nav.isStopped = false;
            }
            nav.destination = go.transform.position;
            anim.SetFloat("Forward", 1);
        }
    }
}
