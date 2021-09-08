using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI : MonoBehaviour
{
    NavMeshAgent agent;
    Animator anim;
    public Transform player;
    CanWeState currentState;

    //public Transform safeSpot;

    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
        currentState = new Idle(this.gameObject, agent, anim, player);
    }

    void Update()
    {
        currentState = currentState.Process();
    }
}
