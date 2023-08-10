using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public abstract class CreatureAttack : CreatureComponent
{
    [SerializeField] protected int m_damage = 1;
    [SerializeField] protected float m_attackDelay = 1f;
    [Tooltip("If this is false, attack will be fire when current destination reached")]
    [SerializeField] protected bool m_attackImmiditate = true;
    [Tooltip("Delay when ")]
    [SerializeField] protected float m_attackPostDelay = 0.05f;
    
    protected float m_attackTimerLeft;
    protected Damageable m_currentTarget;

    public int damage { get => m_damage; protected set { m_damage = value; } }
    public bool attackImmiditate { get => m_attackImmiditate; }
    public Damageable currentTarget { get => m_currentTarget; }

    public event Action<Weapon.Type, int> attack;

    protected virtual void Start()
    {
        m_creature.targetter.findTarget += OnTargetFound;
        m_creature.targetter.lostTarget += OnTargetLost;
        m_creature.control.endControlled += OnControlEnd;
    }

    protected virtual void OnEnable()
    {
        if (!m_creature.targetter.HasTarget())
        {
            m_creature.TryChangeState(Creature.State.Idle);
            return;
        }
        m_currentTarget = m_creature.targetter.currentTarget;
        m_attackTimerLeft = m_attackDelay;
    }

    protected virtual void Update()
    {
        if (m_currentTarget == null)
        {
            m_creature.TryChangeState(Creature.State.Idle);
            return;
        }
        TryAttack();
    }

    protected virtual void TryAttack()
    {
        m_attackTimerLeft -= Time.deltaTime;
        if (m_attackTimerLeft <= 0)
        {
            Attack();
            m_attackTimerLeft = m_attackDelay;
        }
    }

    protected virtual void Attack()
    {
        
        attack?.Invoke(Weapon.Type.Melee, m_damage);
    }

    protected virtual void OnTargetFound(Damageable newTarget)
    {
        m_currentTarget = newTarget;
        m_creature.TryChangeState(Creature.State.Attacking);
    }

    protected virtual void OnTargetLost()
    {
        m_currentTarget = null;
        m_creature.TryChangeState(Creature.State.Idle);
    }

    protected virtual void OnControlEnd()
    {
        if (m_creature.targetter.HasTarget())
        {
            m_creature.TryChangeState(Creature.State.Attacking);
        }
    }
}