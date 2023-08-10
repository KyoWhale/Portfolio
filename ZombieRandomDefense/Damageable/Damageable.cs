using System;
using UnityEngine;

/// <summary>
/// Alliance는 시스템에 존재하는 개체들의 진영을 나타냅니다.
/// 보통은 자신의 진영 하나만 플래그로 표시하지만, Mask로도 사용할 수 있습니다.
/// </summary>
[System.Flags]
public enum Alliance
{
    None = 0,
    Human = 1 << 0,
    Zombie = 1 << 1,
    Neutral = 1 << 2, // 야생의 토끼 같은 동물 혹은 바위 등
    Obstacle = 1 << 3,
    All = Human | Zombie | Neutral | Obstacle
}

[RequireComponent(typeof(Collider))]
public class Damageable : MonoBehaviour
{
    protected Collider m_collider;

    [SerializeField] 
    protected int m_maxHealth = 1;
    protected int m_currentHealth;
    [SerializeField]
    [Tooltip("자신이 속한 진영입니다. 마스크가 아니기 때문에 하나만 체크해야합니다.")]
    protected Alliance m_alliance = Alliance.None;

    public Alliance alliance { get => m_alliance; protected set { m_alliance = value; } }

    /// <summary>
    /// GameObject가 Enabled되었을 때 발동하는 이벤트입니다.
    /// </summary>
    public event Action<Damageable> spawned;

    /// <summary>
    /// Damageable이 회복되었을 때 발동하는 이벤트입니다.
    /// </summary>
    public event Action<Damager> healed;

    /// <summary>
    /// GameObject가 피해를 입었을 때 발동하는 이벤트입니다.
    /// </summary>
    public event Action<Damager> damaged;

    /// <summary>
    /// GameObject가 Disabled되었을 때 발동하는 이벤트입니다.
    /// </summary>
    public event Action<Damageable> died;

    protected virtual void Awake()
    {
        m_collider = GetComponent<Collider>();
    }

    protected virtual void OnEnable()
    {
        m_collider.enabled = true;
        m_currentHealth = m_maxHealth;
        spawned?.Invoke(this);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        var damager = other.GetComponent<Damager>();
        if (damager == null || !damager.ShouldInteractWith(this))
        {
            return;
        }
        
        if (damager.IsToAlly)
        {
            TakeHeal(damager);
        }
        else
        {
            damager.TriggeredDamageable(this);
            TakeDamage(damager);
        }
    }
    
    public bool TakeDamage(Damager damager)
    {
        if (damager.damage <= 0)
        {
            return false;
        }

        m_currentHealth -= damager.damage;
        if (m_currentHealth <= 0)
        {
            gameObject.SetActive(false);
            return true;
        }
        else
        {
            damaged?.Invoke(damager);
            return false;
        }
    }

    public void TakeHeal(Damager damager)
    {
        if (damager.damage <= 0 || m_currentHealth == m_maxHealth)
        {
            return;
        }

        m_currentHealth = Math.Clamp(m_currentHealth + damager.damage, 0, m_maxHealth);
        healed?.Invoke(damager);
    }

    protected virtual void OnDisable()
    {
        m_collider.enabled = false;

        died?.Invoke(this);
    }
    
#if UNITY_EDITOR
    void OnValidate()
    {
        if (m_alliance != Alliance.None)
        {
            int count = 0;
            uint mask = ((uint)m_alliance);
            while (mask > 0)
            {
                if ((mask & 1) == 1)
                {
                    count++;
                }
                mask >>= 1;
            }
            if (count > 1)
            {
                m_alliance = Alliance.None;
                UnityEditor.EditorWindow.mouseOverWindow.ShowNotification(new GUIContent(("Alliance는 Mask로 사용하면 안 됩니다. 하나만 체크해야합니다.")));
            }
        }
    }
#endif
}
