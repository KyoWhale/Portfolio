using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CreatureControl: CreatureComponent
{
    public enum Type { None, Hit, Stagger, Pushed, Knuckback, Falling, Landing } 

    [Header("Resistance of Control")]
    [SerializeField] protected bool m_lookOriginWhenStunned = false;
    [SerializeField] protected float m_stunReduceRatio = 0;

    [SerializeField] protected bool m_lookOriginWhenPushed = false;
    [SerializeField] protected float m_pushReduceRatio = 0;
    
    [SerializeField] protected bool m_lookOriginWhenKnuckback = false;
    [SerializeField] protected float m_knuckbackReduceRatio = 0;

    protected Type m_currentControlType;

    public event Action<Type> startControlled;
    public event Action endControlled;

    protected virtual void Start()
    {
        m_creature.animator.controlSMB.exit += ControlEnd;
    }

    public virtual void TakeControlled(Damager damager)
    {
        var lookPosition = damager.transform.position;
        lookPosition.y = transform.position.y;
        // TODO: 각 방향에 따라 내적 외적 하여 그 방향에 맞는 애니메이션 재생하도록
        transform.LookAt(lookPosition);

        m_currentControlType = damager.controlType;
        m_creature.TryChangeState(Creature.State.Controlled);

        startControlled?.Invoke(damager.controlType);
    }

    private void ControlEnd()
    {
        m_currentControlType = Type.None;
        m_creature.TryChangeState(Creature.State.Idle);
        endControlled?.Invoke();
    }
}
