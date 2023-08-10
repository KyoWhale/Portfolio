using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using State = Creature.State;
using Type = CreatureControl.Type;

public abstract class CreatureAnimator : MonoBehaviour
{
    protected Animator m_animator;
    protected Creature m_creature;
    protected Weapon m_weapon;

    [HideInInspector()] public CreatureIdleSMB idleSMB;
    [HideInInspector()] public CreatureMoveSMB moveSMB;
    [HideInInspector()] public CreatureAttackSMB attackSMB;
    [HideInInspector()] public CreatureControlSMB controlSMB;
    [HideInInspector()] public CreatureInteractSMB interactSMB;
    [HideInInspector()] public CreatureDieSMB dieSMB;

    private readonly static int hashIdle = Animator.StringToHash("Idle");
    private readonly static int hashAttack = Animator.StringToHash("Attacking");
    private readonly static int hashMove = Animator.StringToHash("Moving");
    // private readonly static int hashClimbing = Animator.StringToHash("Climbing");
    private readonly static int hashControlled = Animator.StringToHash("Controlled");
    private readonly static int hasStagger = Animator.StringToHash("Stagger");
    private readonly static int hashPushed = Animator.StringToHash("Pushed");
    private readonly static int hashKnuckbacked = Animator.StringToHash("Knuckbacked");
    private readonly static int hashDie = Animator.StringToHash("Die");

    protected virtual void Awake()
    {
        m_creature = GetComponent<Creature>();
        m_creature.navigation.walk += PlayMove;
        m_creature.attack.attack += PlayAttack;
        m_creature.control.startControlled += PlayControlled;

        m_animator = GetComponent<Animator>();
        idleSMB = m_animator.GetBehaviour<CreatureIdleSMB>();
        moveSMB = m_animator.GetBehaviour<CreatureMoveSMB>();
        attackSMB = m_animator.GetBehaviour<CreatureAttackSMB>();
        controlSMB = m_animator.GetBehaviour<CreatureControlSMB>();
        interactSMB = m_animator.GetBehaviour<CreatureInteractSMB>();
        dieSMB = m_animator.GetBehaviour<CreatureDieSMB>();
    }

    public virtual void OnEnable()
    {
        m_animator.SetBool(hashIdle, true);
        m_animator.SetBool(hashAttack, false);
        m_animator.SetBool(hashMove, false);
        m_animator.SetBool(hashDie, false);
    }

    private void PlayIdle()
    {
        m_animator.SetBool(hashIdle, true);
        m_animator.SetBool(hashAttack, false);
        m_animator.SetBool(hashMove, false);
    }

    private void PlayAttack(Weapon.Type type, int damage)
    {   
        m_animator.SetBool(hashIdle, false);
        m_animator.SetBool(hashAttack, true);
        m_animator.SetBool(hashMove, false);
    }

    private void PlayMove()
    {
        m_animator.SetBool(hashIdle, false);
        m_animator.SetBool(hashAttack, false);
        m_animator.SetBool(hashMove, true);
    }

    private void PlayControlled(Type controlType)
    {
        m_animator.SetTrigger(hashControlled);
        switch (controlType)
        {
            case Type.Stagger:
                m_animator.SetTrigger(hasStagger);
                break;
            case Type.Pushed:
                m_animator.SetTrigger(hashPushed);
                break;
            case Type.Knuckback:
                m_animator.SetTrigger(hashKnuckbacked);
                break;
        }
    }
    
    private void PlayDying()
    {
        m_animator.SetBool(hashDie, true);
    }
}
