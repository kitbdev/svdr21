using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[SelectionBase]
public class Health : MonoBehaviour
{
    [Header("Health")]
    [ReadOnly]
    [SerializeField]
    float m_currentHealth;
    public float currentHealth
    {
        get { return m_currentHealth; }
        protected set { m_currentHealth = value; UpdateHealth(); }
    }
    public float maxHealth = 1;
    /// <summary>
    /// heal regeneration rate per second. stops when full. -1 disables
    /// </summary>
    public float regenRate = -1;
    public bool destroyOnDie = false;
    /// <summary>
    /// Finds all child HitBoxes on OnEnable and registers the hit event
    /// </summary>
    public bool autoRegisterHitboxes = true;
    /// <summary>
    /// Seconds after a hit that future hits will be ignored
    /// </summary>
    public float hitInvincibleDur = 0.5f;
    protected float lastDamageTime = 0;
    [Header("Events")]
    public UnityEvent dieEvent;
    public UnityEvent damageEvent;
    public UnityEvent healthUpdateEvent;
    [HideInInspector]
    public HitArgs lastHitArgs = null;
    public bool manualInvincible = false;

    [HideInInspector] [SerializeField] HitBox[] hitBoxes;

    bool isHitInvincible => hitInvincibleDur > 0 && Time.time < lastDamageTime + hitInvincibleDur;
    public bool isInvincible => manualInvincible || isHitInvincible;
    // max health negative means true invincibility
    public bool isDead => currentHealth <= 0 && maxHealth >= 0;
    public bool isFull => currentHealth >= maxHealth;

    private void Awake()
    {
        RestoreHealth();
    }
    private void OnEnable()
    {
        if (autoRegisterHitboxes)
        {
            RegisterAllChildHitboxes();
        }
    }
    private void OnDisable()
    {
        UnregisterAllHitboxes();
        dieEvent.RemoveAllListeners();
        damageEvent.RemoveAllListeners();
        healthUpdateEvent.RemoveAllListeners();
    }
    public void RegisterAllChildHitboxes()
    {
        hitBoxes = transform.GetComponentsInChildren<HitBox>();
        foreach (var hitbox in hitBoxes)
        {
            hitbox.hitEvent.AddListener(TakeDamage);
        }
    }
    public void UnregisterAllHitboxes()
    {
        foreach (var hitbox in hitBoxes)
        {
            hitbox.hitEvent.RemoveAllListeners();
        }
    }
    private void Update()
    {
        if (regenRate > 0 && !isFull)
        {
            currentHealth = Mathf.Min(currentHealth + regenRate * Time.deltaTime, maxHealth);
        }
    }
    public void RestoreHealth()
    {
        currentHealth = maxHealth;
    }
    public void Heal(float amount)
    {
        currentHealth += amount;
    }
    public void TakeDamage(HitArgs args)
    {
        // VRDebug.Log(name + " hit by " + args.attacker + " for " + args.damage, debugContext: this);
        if (isDead)
        {
            VRDebug.Log(name + " is already dead", debugContext: this);
            return;
        } else if (isInvincible)
        {
            VRDebug.Log(name + " is invincible", debugContext: this);
            return;
        }
        lastHitArgs = args;
        currentHealth -= args.damage;
        lastDamageTime = Time.time;
        damageEvent.Invoke();
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    public void Die()
    {
        dieEvent.Invoke();
        if (destroyOnDie)
        {
            Destroy(gameObject);
        }
    }
    public void UpdateHealth()
    {
        healthUpdateEvent.Invoke();
    }
}