using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class DamagerLauncher : MonoBehaviour
{
    private ParticleSystem m_launcher;
    [SerializeField] ParticleSystem m_damager;

    public float speed
    {
        get
        {
            return m_launcher.main.startSpeedMultiplier;
        }
        
        set
        {
            var main = m_launcher.main;
            main.startSpeedMultiplier = value;
        }
    }
    public float duration
    {
        get
        {
            return m_launcher.main.duration;
        }
        
        set
        {
            var main = m_launcher.main;
            main.duration = value;
        }
    }

    private void Awake()
    {
        m_launcher = GetComponent<ParticleSystem>();
    }

    public virtual void Play()
    {
        m_launcher.Play();
    }

    public virtual void Stop()
    {
        m_launcher.Stop();
    }

    public virtual void Clear()
    {
        m_launcher.Clear();
    }
}
