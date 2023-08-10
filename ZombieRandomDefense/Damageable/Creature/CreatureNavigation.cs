using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class CreatureNavigation : CreatureComponent
{
    [SerializeField][Range(0.02f, 5)] 
    protected float m_pathUpdateTime = 1;

    [SerializeField] protected NavMeshAgent m_navAgent;
    protected Vector3 m_destination;

    public event Action walk;
    public event Action walkEnd;
    public event Action reached;

    protected virtual void OnEnable()
    {
        m_navAgent.enabled = true;

        if (m_navAgent.isOnNavMesh)
        {
            m_navAgent.isStopped = false;
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError("Agent is Not on NavMesh");
#endif
        }

        m_destination = FindDestination();
        m_navAgent.SetDestination(m_destination);

        walk?.Invoke();
    }

    protected virtual void Update()
    {
        if (m_navAgent.pathPending)
        {
            return;
        }
        ShouldAttack();
        PathUpdate();
    }

    protected virtual bool ShouldAttack()
    {
        if (m_creature.attack.attackImmiditate && m_creature.targetter.HasTarget())
        {
            return m_creature.TryChangeState(Creature.State.Attacking);
        }
        else if (m_creature.targetter.HasTarget() &&
                m_navAgent.stoppingDistance > m_navAgent.remainingDistance)
        {
            return m_creature.TryChangeState(Creature.State.Attacking);
        }
        return false;
    }

    protected virtual void PathUpdate()
    {
        // if () // 목표 거리 안으로 들어왔을 때
        // {
        //     reached?.Invoke();
        // }

        // NavMesh Link mesh 이용해서 장애물을 뛰어넘나드는 것
        // 좀비 : 뛰어넘는 가중치, 파괴하는 가중치, 돌아가는 가중치 중 가장 작은 것
        if (m_navAgent.pathStatus == NavMeshPathStatus.PathComplete)
        {
            Debug.Log("PathComplete");
        }
        if (m_navAgent.pathStatus == NavMeshPathStatus.PathPartial)
        {
            Debug.Log("PathPartial");
        }
    }

    protected virtual Vector3 FindDestination()
    {
        return new Vector3(transform.position.x, 0, -3);
    }

    protected virtual void OnDisable()
    {
        if (m_navAgent.isOnNavMesh)
        {
            m_navAgent.isStopped = true;
        }
        m_navAgent.enabled = false;
        walkEnd?.Invoke();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (m_navAgent == null)
        {
            m_navAgent = GetComponent<NavMeshAgent>();
        }
    }
#endif
}
