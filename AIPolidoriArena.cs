using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIPolidoriArena : EnemyAI
{
    public GameObject blood;
    private float counter = 0;
    private float attackCounter = 0;
    private Transform grab;
    private NavMeshAgent navTarget;
    private float navOffSet;
    private float originOffSet;
    private float embraceStartTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        grab = transform.Find("Grab");
        Debug.Log(grab.name);
        Invoke("CheckPlayerStats", 2);
    }



     public override void MoveToEngage ()
    {
        base.MoveToEngage();

        counter += Time.deltaTime;
        attackCounter = 0;                                 //While moving, embrace ability wont be trigger, and it will reset the counter to zero
        if (distToTarget > 3 && distToTarget < 60)
        {
            if (counter >= 8)
            {
                counter = 0;
                transform.LookAt(target.transform.position);
                pa.rangedAttack = true;
                pa.Attack();
                pa.rangedAttack = false;

                Debug.Log("Attack");
            }
        }
    }

    public override void Attack ()
    {
        base.Attack();
        Debug.Log(distToTarget + "/" + ps.attacktRange);
        if (distToTarget < (ps.attacktRange + 0.1f))
        {
            counter = 0;                                    //if Polidori is attacked in Melee doesnt use its ranged attack. 
            attackCounter += 3;
            Debug.Log(attackCounter);

            if (attackCounter >= 9)                         //Less than three melee attacks doenst trigger Embrace ability. 
            {
                int diceRoll = Random.Range(0, 100);
                if (diceRoll <= attackCounter)
                {
                    ps.stateAI = "Cinematic";
                    state = EnemyAI.State.Cinematic;
                    anim.SetTrigger("AttackSpell1");
                    anim.SetBool("Embrace", true);
                    counter = 0;
                    Invoke("Ascension", 0.01f);
                    GetComponent<AudioSource>().Play();
                    PreparePC();
                    attackCounter = 0;
                    embraceStartTime = Time.realtimeSinceStartup;
                }
            }
        }
    }

    public override void Cinematic ()
    {
    //   base.Cinematic();
        Embrace();
    }

   

    private void Embrace()
    {
        anim.SetFloat("Forward", 0);
        anim.SetFloat("Turn", 0);
        nav.isStopped = true;
        float timeSinceEmbrace = Time.realtimeSinceStartup - embraceStartTime;
        if (timeSinceEmbrace >= 5 && timeSinceEmbrace <= 8)
        {
            blood.SetActive(true);
        }

        else if (timeSinceEmbrace >= 10)
        {
            navTarget.baseOffset = originOffSet;
            target.transform.position = new Vector3(target.transform.position.x, target.transform.position.y + 1, target.transform.position.z);
            target.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            anim.SetTrigger("IdleTrigger");
            anim.SetBool("Embrace", false);
            attackCounter = 0;
            //    transform.Find("ShockWave").gameObject.SetActive(false);     

        }
    }

    private void PreparePC()
    {
        Transform playerGrab = grab.transform.Find(target.name);
        navTarget = target.GetComponent<NavMeshAgent>();
        navTarget.isStopped = true;
        navTarget.enabled = false;
        target.transform.position = playerGrab.position;
        target.transform.rotation = playerGrab.rotation;
        target.GetComponent<ThirdPersonCharacter>().enabled = false;
        target.GetComponent<ThirdPersonUserControl>().enabled = false;
        navTarget.enabled = true;
        navTarget.isStopped = false;
        

        originOffSet = navTarget.baseOffset;
        if (target.name == "Rose")
        {
            navOffSet = 0.1f;
            navTarget.baseOffset = 0.1f;
        }
        else if (target.name == "Fred")
        {
            navOffSet = 1f;
            navTarget.baseOffset = 1f;
        }
        else if (target.name == "Oleg")
        {
            navOffSet = 0.5f;
            navTarget.baseOffset = 0.5f;
        }
        else if (target.name == "Nanna")
        {
            navOffSet = 0;
            navTarget.baseOffset = 0;
        }
        else

        {
            navOffSet = 0.2f;
            navTarget.baseOffset = 0.2f;
        }
        Debug.Log(navTarget.baseOffset);
        navTarget.baseOffset = navOffSet;
    //    navTarget.isStopped = true;
        
 //       navTarget.enabled = false;
        Rigidbody targetRb = target.GetComponent<Rigidbody>();
  //      targetRb.useGravity = false;
        targetRb.constraints = RigidbodyConstraints.FreezeAll;

    }


    
    private void Ascension()
    {
        if (navTarget.baseOffset <= navOffSet)
        {
            navTarget.baseOffset += 0.03f;
            Invoke("Ascension", 0.01f);
        }
    }


    private void BreakAbility ()
    {
        CancelInvoke("Ascension");
        target.GetComponent<ThirdPersonCharacter>().enabled = true;
        target.GetComponent<ThirdPersonUserControl>().enabled = true;
        navTarget.baseOffset = originOffSet;
        Rigidbody targetRb = target.GetComponent<Rigidbody>();
        targetRb.constraints = RigidbodyConstraints.None;
        target.GetComponent<PlayerStats>().AddjustHealth(-1000, gameObject, true);
        state = EnemyAI.State.Search;
        anim.SetBool("Embrace", false);
        nav.isStopped = false;
    }

    private void CheckPlayerStats ()
    {
        if (gc.players.Count < 2) 
        {
            ps.maxRegen = 0;
            ps.currentRegen = 0;
        }
    }
}
