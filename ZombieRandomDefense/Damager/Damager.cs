using UnityEngine;
using ControlType = CreatureControl.Type;
using System;

[RequireComponent(typeof(Collider))]
public abstract class Damager : MonoBehaviour
{
    private Collider m_collider;

    [Header("타겟")]
    [SerializeField]
    [Tooltip("체크된 플래그를 가진 Damageable에게 상호작용합니다. 그외 것과 접촉하게 되면 보통 오브젝트 풀로 돌아가게 됩니다.")]
    protected Alliance m_targetMask = Alliance.None;
    [SerializeField]
    [Tooltip("무시하고 지나가는 Damageable 모음입니다. TargetMask와 겹치는 것이 있으면 IgnoreMask가 우선순위를 가지지만, TargetMask와 IgnoreMask의 플래그가 겹치지 않아야 합니다.")]
    protected Alliance m_ignoreMask = Alliance.None;
    [SerializeField]
    [Tooltip("참으로 되어있으면, 이 Damager는 아군에게 작용합니다. 아군에게는 힐로 간주됩니다. 또한 Target 플래그가 아군으로 정확히 되어있는지 확인하시기 바랍니다.")]
    protected bool m_isToAlly = false;
    [SerializeField]
    [Tooltip("maxHitCount는 TargetMask를 가진 Damageable과 접촉할 수 있는 최대 카운트입니다. 지나간 개체 수가 이 값과 동일해질 때 Projectile은 풀로 돌아갑니다.")]
    protected int m_maxHitCount = 1;
    protected int m_currentHitCount = 0;

    [Header("데미지")]
    [SerializeField]
    [Tooltip("Damager가 Target에게 작용하는 크기입니다. 딜 혹은 힐 모두 이 값을 사용합니다.")]
    protected int m_damage = 1;
    [Tooltip("Damager가 Target에게 작용하는 군중제어기 타입입니다. 없다면 None을 체크하면 됩니다.")]
    [SerializeField]
    protected ControlType m_control = ControlType.None;

    public Alliance TargetMask { get => m_targetMask; }
    public Alliance IgnoreMask { get => m_ignoreMask; }
    public bool IsToAlly { get => m_isToAlly; }
    public ControlType controlType { get => m_control; protected set { m_control = value; } }
    public int damage { get => m_damage; protected set { m_damage = value; } }

    public event Action spawned;
    public event Action<Damageable> triggeredDamageable;
    public event Action disabled;

    protected virtual void Awake()
    {
        m_collider = GetComponent<Collider>();
    }

    protected virtual void OnEnable()
    {
        m_collider.enabled = true;
        m_currentHitCount = 0;

        spawned?.Invoke();
    }

    public bool ShouldInteractWith(Damageable damageable)
    {
        if (m_ignoreMask.HasFlag(damageable.alliance))
        {
            return false;
        }

        if (m_targetMask.HasFlag(damageable.alliance))
        {
            return true;
        }

        return false;
    }

    public virtual void TriggeredDamageable(Damageable damageable)
    {
        triggeredDamageable?.Invoke(damageable);
    }

    public abstract void TriggeredEnvironment();

    protected void BackToPool()
    {
        Debug.Log("TODO : Fix Pool Manager");
        gameObject.SetActive(false);
    }

    protected virtual void OnDisable()
    {
        m_collider.enabled = false;

        disabled?.Invoke();
    }
}