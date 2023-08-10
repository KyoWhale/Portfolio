using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureTargetter : CreatureComponent
{
    [SerializeField] bool m_focusClosest = false;
    [SerializeField] bool m_shouldFrequentlySearch = false;
    [SerializeField] float m_searchTime = 1;

    protected LinkedList<Damageable> m_targets = new LinkedList<Damageable>();
    private float m_remainSearchTime;
    
    [SerializeField] Alliance m_targetMask;
    public Damageable currentTarget;

    public event Action<Damageable> findTarget;
    public event Action lostTarget;
    public event Action diedTarget;

    protected override void Awake()
    {
        m_creature = GetComponentInParent<Creature>();
    }

    protected virtual void Start()
    {
        m_remainSearchTime = m_searchTime;
    }

    protected virtual void Update()
    {
        SearchForNewTarget();
    }

    private void SearchForNewTarget()
    {
        if (!m_shouldFrequentlySearch)
        {
            return;
        }

        if (m_remainSearchTime < 0)
        {
            return;
        }

        m_remainSearchTime -= Time.deltaTime;
        if (m_remainSearchTime > 0)
        {
            return;
        }

        m_remainSearchTime = m_searchTime;
        if (TryFindTarget(out Damageable newTarget))
        {
            if (currentTarget == newTarget)
            {
                return;
            }

            currentTarget = newTarget;
            findTarget?.Invoke(currentTarget);
        }
        else
        {
            if (currentTarget != null)
            {
                currentTarget.died -= OnTargetDie;
                lostTarget?.Invoke();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Damageable damageable = other.gameObject.GetComponent<Damageable>();
        if (damageable == null || !m_targetMask.HasFlag(damageable.alliance))
        {
            return;
        }

        // TODO: [REFACTORING] if is not targettable return;
        // or use [layer : targetOnlyXXX] wisly
        
        damageable.died += OnTargetDie;
        m_targets.AddLast(damageable);
        if (m_targets.Count == 1)
        {
            currentTarget = damageable;
            findTarget?.Invoke(damageable);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Damageable damageable = other.GetComponent<Damageable>();
        if (damageable == null)
        {
            return;
        }

        if (!m_targets.Remove(damageable))
        {
            return;
        }
        damageable.died -= OnTargetDie;
        if (currentTarget == damageable)
        {
            OnTargetExit();
        }
    }

    private void OnTargetExit()
    {
        currentTarget = null;
        lostTarget?.Invoke();

        if (TryFindTarget(out Damageable newTarget))
        {
            currentTarget = newTarget;
            findTarget?.Invoke(newTarget);
        }
    }

    private void OnTargetDie(Damageable damageable)
    {
        damageable.died -= OnTargetDie;
        m_targets.Remove(damageable);
        if (currentTarget == damageable)
        {
            currentTarget = null;
            diedTarget?.Invoke();
        }
        
        if (TryFindTarget(out Damageable newTarget))
        {
            currentTarget = newTarget;
            findTarget?.Invoke(newTarget);
        }
        else
        {
            lostTarget?.Invoke();
        }
    }

    private bool TryFindTarget(out Damageable newTarget)
    {
        if (!HasTarget())
        {
            newTarget = null;
            return false;
        }

        if (m_targets.Count == 1)
        {
            newTarget = m_targets.First.Value;
            return true;
        }

        if (m_focusClosest)
        {
            newTarget = GetClosestTarget();
        }
        else
        {
            newTarget = GetOldestTarget();
        }
        return true;
    }

    public bool HasTarget()
    {
        if (m_targets.Count == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public Damageable GetClosestTarget()
    {
        if (m_targets.Count == 0)
        {
            return null;
        }

        float minDistance = float.MaxValue;
        Damageable closestTarget = m_targets.First.Value;
        foreach (var target in m_targets)
        {
            Vector3 vector = target.transform.position - transform.position;
            if (vector.sqrMagnitude < minDistance)
            {
                minDistance = vector.sqrMagnitude;
                closestTarget = target;
            }
        }
        return closestTarget;
    }

    public Damageable GetOldestTarget()
    {
        if (m_targets.Count == 0)
        {
            return null;
        }

        return m_targets.First.Value;
    }

// #if UNITY_EDITOR
//     private void OnDrawGizmosSelected()
//     {
//         foreach (var target in m_targets)
//         {
//             Gizmos.DrawSphere(target.transform.position, 1);
//         }
//     }
// #endif
}
