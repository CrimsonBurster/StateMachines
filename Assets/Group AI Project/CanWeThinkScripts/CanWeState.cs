using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CanWeState
{
    public enum STATE { IDLE, PATROL, PURSUE, ATTACK, SAFE, LIAMNEESAN, BLIND };

    public enum EVENT { ENTER, UPDATE, EXIT };

    public STATE name;
    protected EVENT stage;
    protected GameObject npc;
    protected Animator anim;
    protected Transform player;
    protected CanWeState nextState;
    protected NavMeshAgent agent;


    float visDist = 10.0f;
    float visAngle = 30.0f;
    float shootDist = 7.0f;

    public CanWeState(GameObject _npc, NavMeshAgent _agent, Animator _anim, Transform _player)
    {
        npc = _npc;
        agent = _agent;
        anim = _anim;
        stage = EVENT.ENTER;
        player = _player;
    }

    public virtual void Enter() { stage = EVENT.UPDATE; }
    public virtual void Update() { stage = EVENT.UPDATE; }
    public virtual void Exit() { stage = EVENT.EXIT; }

    public CanWeState Process()
    {
        if (stage == EVENT.ENTER) Enter();
        if (stage == EVENT.UPDATE) Update();
        if (stage == EVENT.EXIT)
        {
            Exit();
            return nextState;
        }
        return this;
    }

    public bool CanSeePlayer()
    {
        Vector3 direction = player.position - npc.transform.position;
        float angle = Vector3.Angle(direction, npc.transform.forward);

        if(direction.magnitude < visDist && angle < visAngle)
        {
            return true;
        }
        return false;
    }

    public bool CanAttackPlayer()
    {
        Vector3 direction = player.position - npc.transform.position;
        if(direction.magnitude < shootDist)
        {
            return true;
        }
        return false;
    }

    //public bool PlayerBehind()
    //{
    //    Vector3 direction = npc.transform.position - player.position;
    //    float angle = Vector3.Angle(direction, npc.transform.forward);

    //    if(direction.magnitude < 2 && angle < 30)
    //    {
    //        return true;
    //    }
    //    return false;
    //}
}

public class Idle : CanWeState
{
    public Idle(GameObject _npc, NavMeshAgent _agent, Animator _anim, Transform _player) : base(_npc, _agent, _anim, _player)
    {
        name = STATE.IDLE;
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        Debug.Log("is idle");
        if (CanSeePlayer())
        {
            nextState = new Pursue(npc, agent, anim, player);
            stage = EVENT.EXIT;
        }

        else if(Random.Range(0,100) < 10)
        {
            nextState = new Patrol(npc, agent, anim, player);
            stage = EVENT.EXIT;
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}

public class Patrol : CanWeState
{
    int currentIndex = -1;

    public Patrol(GameObject _npc, NavMeshAgent _agent, Animator _anim, Transform _player) : base(_npc, _agent, _anim, _player)
    {
        name = STATE.PATROL;
        agent.speed = 5;
        agent.isStopped = false;
    }

    public override void Enter()
    {
        float lastDist = Mathf.Infinity;
        for(int i = 0; i < GameEnvironment.Singleton.CheckPoints.Count; i++)
        {
            GameObject thisWP = GameEnvironment.Singleton.CheckPoints[i];
            float distance = Vector3.Distance(npc.transform.position, thisWP.transform.position);
            if(distance < lastDist)
            {
                currentIndex = i - 1;
                lastDist = distance;
            }
        }
        base.Enter();
    }

    public override void Update()
    {
        Debug.Log("is Patroling");
        if(agent.remainingDistance < 2)
        {
            if(currentIndex >= GameEnvironment.Singleton.CheckPoints.Count - 1)
            {
                currentIndex = 0;
            }
            else
            {
                currentIndex++;
            }
            agent.SetDestination(GameEnvironment.Singleton.CheckPoints[currentIndex].transform.position);
        }
        if (Random.Range(0, 10000) < 1)
        {
            nextState = new LiamNeeson(npc, agent, anim, player);
            stage = EVENT.EXIT;
        }

        if (CanSeePlayer())
        {
            nextState = new Pursue(npc, agent, anim, player);
            stage = EVENT.EXIT;
        }

        //else if (PlayerBehind())
        //{
        //    nextState = new Safe(npc, agent, anim, player);
        //    stage = EVENT.EXIT;
        //}
    }

    public override void Exit()
    {
        base.Exit();
    }
}

public class Pursue: CanWeState
{
    public Pursue(GameObject _npc, NavMeshAgent _agent, Animator _anim, Transform _player) : base(_npc, _agent, _anim, _player)
    {
        name = STATE.PURSUE;
        agent.speed = 5;
        agent.isStopped = false;
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        Debug.Log("is pursuing");
        agent.SetDestination(player.position);
        if (agent.hasPath)
        {
            if (!CanSeePlayer())
            {
                nextState = new Patrol(npc, agent, anim, player);
                stage = EVENT.EXIT;
            }
            
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}

public class Attack : CanWeState
{
    float rotationSpeed = 2.0f;
    
    public Attack(GameObject _npc, NavMeshAgent _agent, Animator _anim, Transform _player) : base(_npc, _agent, _anim, _player)
    {
        name = STATE.ATTACK;
    }

    public override void Enter()
    {
        agent.isStopped = true;
        base.Enter();
    }

    public override void Update()
    {
        Debug.Log("is attacking");
        Vector3 direction = player.position - npc.transform.position;
        float angle = Vector3.Angle(direction, npc.transform.forward);
        direction.y = 0;

        npc.transform.rotation = Quaternion.Slerp(npc.transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * rotationSpeed);
        if (!CanAttackPlayer())
        {
            nextState = new Idle(npc, agent, anim, player);
            stage = EVENT.EXIT;
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}

public class LiamNeeson : CanWeState
{
    float rotationSpeed = 20.0f;
    bool isBlind;

    public LiamNeeson(GameObject _npc, NavMeshAgent _agent, Animator _anim, Transform _player) : base(_npc, _agent, _anim, _player)
    {
        name = STATE.LIAMNEESAN;
    }

    public override void Enter()
    {
        agent.isStopped = false;
        agent.speed = 40;
        base.Enter();
    }

    public override void Update()
    {
        Debug.Log("is liam neeson");
        agent.SetDestination(player.position);
        Vector3 direction = player.position - npc.transform.position;
        float angle = Vector3.Angle(direction, npc.transform.forward);
        direction.y = 0;
        npc.transform.rotation = Quaternion.Slerp(npc.transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * rotationSpeed);

        if (Input.GetKeyDown("space"))
        {
            nextState = new Blind(npc, agent, anim, player);
            stage = EVENT.EXIT;
        }
    }

    public override void Exit()
    {
        base.Exit();
    }

}

public class Blind : CanWeState
{
    public Blind(GameObject _npc, NavMeshAgent _agent, Animator _anim, Transform _player) : base(_npc, _agent, _anim, _player)
    {
        name = STATE.BLIND;
    }

    public override void Enter()
    {
        agent.isStopped = true;
        base.Enter();
    }

    public override void Update()
    {
        Debug.Log("is blind");
        if (Random.Range(0, 1000) < 1)
        {
            nextState = new Idle(npc, agent, anim, player);
            stage = EVENT.EXIT;
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}

//public class Safe : State
//{
//    public GameObject safeSpot;
//    public Safe(GameObject _npc, NavMeshAgent _agent, Animator _anim, Transform _player) : base(_npc, _agent, _anim, _player)
//    {
//        name = STATE.SAFE;
//        safeSpot = GameObject.FindGameObjectWithTag("Safe");
//    }

//    public override void Enter()
//    {
//        agent.speed = 10;
//        agent.isStopped = false;
//        agent.SetDestination(safeSpot.transform.position);
//        base.Enter();
//    }

//    public override void Update()
//    {
//        if(agent.remainingDistance < 1)
//        {
//            agent.speed = 0;
//            nextState = new Idle(npc, agent, anim, player);
//            stage = EVENT.EXIT;
//        }
//    }

//    public override void Exit()
//    {
//        base.Exit();
//    }
//}
