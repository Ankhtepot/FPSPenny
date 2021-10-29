using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class ZombieController : MonoBehaviour
{
    [SerializeField] private GameObject ragDoll;
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private STATE state = STATE.Idle;
    [SerializeField] private int health;
    public float walkingSpeed = 10;
    public float runningSpeed = 20;
    [SerializeField] private float damageAmount = 5;
    public GameObject target;

    private static readonly int Walk = Animator.StringToHash("Walk");
    private static readonly int Death = Animator.StringToHash("Death");
    private static readonly int Run = Animator.StringToHash("Run");
    private static readonly int Attack = Animator.StringToHash("Attack");

    private enum STATE
    {
        NONE = 0,
        Idle,
        Wander,
        Attack,
        Chase,
        Dead
    }

    private void Start()
    {
        TurnOffAnimBools();
    }

    void Update()
    {
        // animator.SetBool(Walk, Input.GetKey(KeyCode.Alpha1));
        //
        // animator.SetBool(Attack, Input.GetKey(KeyCode.Alpha2));
        //
        // animator.SetBool(Run, Input.GetKey(KeyCode.Alpha3));
        //
        // animator.SetBool(Death, Input.GetKey(KeyCode.Alpha4));

        if (!target && !GameStats.gameOver)
        {
            target = GameObject.FindGameObjectWithTag("Player");
            return;
        }

        switch (state)
        {
            case STATE.NONE:
                break;
            case STATE.Idle:
                IdleState();
                break;
            case STATE.Wander:
                WanderState();
                break;
            case STATE.Attack:
                AttackState();
                break;
            case STATE.Chase:
                ChaseState();
                break;
            case STATE.Dead:
                DeathState();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void DamagePlayer()
    {
        if (target)
        {
            target.GetComponent<FPController>().TakeHit(damageAmount);
        }
        else
        {
            state = STATE.Idle;
        }
    }

    public void TakeDamage()
    {
        health--;

        if (health > 0) return;
        
        if (Random.Range(0, 4) > 1)
        {
            state = STATE.Dead;
        }
        else
        {
            RagDollDeath();
        }
    }

    private void IdleState()
    {
        if (CanSeePlayer())
        {
            state = STATE.Chase;
        }
        else if (Random.Range(0, 5000) < 5)
        {
            state = STATE.Wander;
        }
    }

    private void WanderState()
    {
        if (!agent.hasPath)
        {
            float newX = transform.position.x + Random.Range(-5, 5);
            float newZ = transform.position.z + Random.Range(-5, 5);
            float newY = Terrain.activeTerrain.SampleHeight(new Vector3(newX, 0, newZ));
            var destination = new Vector3(newX, newY, newZ);

            agent.SetDestination(destination);
            agent.stoppingDistance = 0.2f;
            agent.speed = walkingSpeed;

            TurnOffAnimBools();
            animator.SetBool(Walk, true);
        }

        if (CanSeePlayer()) state = STATE.Chase;
        else if (Random.Range(0, 5000) < 5)
        {
            TurnOffAnimBools();
            agent.ResetPath();
            state = STATE.Idle;
        }
    }

    private void AttackState()
    {
        if (GameStats.gameOver)
        {
            TurnOffAnimBools();
            state = STATE.Wander;
            return;
        }
        
        TurnOffAnimBools();
        animator.SetBool(Attack, true);

        transform.LookAt(target.transform.position);

        if (DistanceToPlayer() > agent.stoppingDistance + 2)
        {
            state = STATE.Chase;
        }
    }

    private void ChaseState()
    {
        if (GameStats.gameOver)
        {
            TurnOffAnimBools();
            state = STATE.Wander;
            return;
        }
        
        agent.SetDestination(target.transform.position);
        agent.stoppingDistance = 3;
        agent.speed = runningSpeed;
        TurnOffAnimBools();
        animator.SetBool(Run, true);

        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            state = STATE.Attack;
        }

        if (ForgetPlayer())
        {
            state = STATE.Wander;
            agent.ResetPath();
        }
    }

    private void DeathState()
    {
        Destroy(agent);
        TurnOffAnimBools();
        animator.SetBool(Death, true);
        foreach (var source in GetComponents<AudioSource>())
        {
            source.volume = 0;
        }

        
        GetComponent<SinkBody>().StartSink();
    }

    private void RagDollDeath()
    {
        var rd = Instantiate(ragDoll, transform.position, transform.rotation);
        rd.transform.Find("Hips").GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * 10000);
        Destroy(gameObject);
    }

    private bool CanSeePlayer()
    {
        if (DistanceToPlayer() > 10) return false;

        return true;
    }

    bool ForgetPlayer()
    {
        return DistanceToPlayer() > 20;
    }

    private float DistanceToPlayer()
    {
        return GameStats.gameOver 
            ? Mathf.Infinity 
            : Vector3.Distance(target.transform.position, transform.position);
    }

    void TurnOffAnimBools()
    {
        animator.SetBool(Walk, false);
        animator.SetBool(Run, false);
        animator.SetBool(Attack, false);
        animator.SetBool(Death, false);
    }
}