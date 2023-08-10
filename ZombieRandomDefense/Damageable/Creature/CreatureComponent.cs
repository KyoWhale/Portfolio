using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CreatureComponent : MonoBehaviour
{
    protected Creature m_creature;
    protected CreatureAnimator m_animator;

    protected virtual void Awake()
    {
        m_creature = GetComponent<Creature>();
        m_animator = GetComponent<CreatureAnimator>();
    }
}
