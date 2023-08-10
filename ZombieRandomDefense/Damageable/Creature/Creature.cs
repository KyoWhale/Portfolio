using System;
using UnityEngine;

[RequireComponent(typeof(CreatureNavigation))]
[RequireComponent(typeof(CreatureControl))]
[RequireComponent(typeof(CreatureAttack))]
[RequireComponent(typeof(CreatureAnimator))]
public abstract class Creature : Damageable
{
    /// <summary>
    /// State has value that orders priority.
    /// If current state is higher priority than state to change,
    /// It can not be applied.
    /// </summary>
    public enum State
    {
        Idle = 0,
        Attacking = 10,
        Walking = 20,
        Controlled = 30,
        Died = 40,
        Custom
    }

    [SerializeField] CreatureAnimator m_animator;
    [SerializeField] CreatureTargetter m_targetter;
    [SerializeField] CreatureNavigation m_navigation;
    [SerializeField] CreatureAttack m_attack;
    [SerializeField] CreatureControl m_control;

    [SerializeField] protected State m_startState;
    protected State m_state = State.Idle;

    public CreatureTargetter targetter { get => m_targetter; }
    public CreatureNavigation navigation { get => m_navigation; }
    public CreatureAttack attack { get => m_attack; }
    public CreatureControl control { get => m_control; }
    public CreatureAnimator animator { get => m_animator; }

    protected virtual void Start()
    {
        died += OnDie;
        damaged += control.TakeControlled;
        healed += control.TakeControlled;

        attack.enabled = false;
        control.enabled = false;
        navigation.enabled = false;

        ChangeState(m_startState);
    }

    public bool TryChangeState(State newState)
    {
        if (!CanChangeState(newState))
        {
            return false;
        }

        ChangeState(newState);

        return true;
    }

    public bool CanChangeState(State newState)
    {
        if ((int)m_state < (int)newState)
        {
            return true;
        }

        switch (m_state)
        {
            case State.Walking:
                switch (newState)
                {
                    case State.Idle:
                        return true;
                    default:
                        return false;
                }
            case State.Attacking:
                switch (newState)
                {
                    case State.Idle:
                        return true;
                    default:
                        return false;
                }
            case State.Controlled:
                switch (newState)
                {
                    case State.Idle:
                    case State.Controlled:
                        return true;
                    default:
                        return false;
                }
            default:
                return false;
        }
    }

    protected virtual void ChangeState(State newState)
    {
        Debug.Log("ChangeState");
        switch (newState)
        {
            case State.Idle:
                if (targetter.currentTarget)
                {
                    Attacking();
                    m_state = State.Attacking;
                    return;
                }
                Idle();
                break;
            case State.Walking:
                Walking();
                break;
            case State.Attacking:
                Attacking();
                break;
            case State.Controlled:
                Controlled();
                break;
            case State.Died:
                Died();
                break;
            default:
                Idle();
                break;
        }
        
        m_state = newState;
    }

    protected virtual void Idle()
    {
        m_collider.enabled = true;
        targetter.enabled = true;
        
        navigation.enabled = false;
        control.enabled = false;
        attack.enabled = false;
    }

    protected virtual void Walking()
    {
        navigation.enabled = true;
        control.enabled = false;
    }

    protected virtual void Attacking()
    {
        control.enabled = false;
        attack.enabled = true;
    }

    protected virtual void Controlled()
    {
        navigation.enabled = false;
        control.enabled = true;
        attack.enabled = false;
    }

    protected virtual void Died()
    {
        m_collider.enabled = false;
        targetter.enabled = false;

        navigation.enabled = false;
        control.enabled = false;
        attack.enabled = false;
    }

    public void OnDie(Damageable damageable)
    {
        Died();
        gameObject.SetActive(false);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (m_collider && m_navigation && 
            m_attack && m_control && m_targetter && m_animator)
        {
            return;
        }

        m_collider = GetComponent<CapsuleCollider>();
        m_targetter = GetComponentInChildren<CreatureTargetter>();
        m_navigation = GetComponent<CreatureNavigation>();
        m_attack = GetComponent<CreatureAttack>();
        m_control = GetComponent<CreatureControl>();
        m_animator = GetComponent<CreatureAnimator>();

        if (!(m_collider && m_navigation && 
            m_attack && m_control && m_targetter && m_animator))
        {
            Debug.LogError("Some of Creature Component is Not Added");
        }
    }
#endif
}
