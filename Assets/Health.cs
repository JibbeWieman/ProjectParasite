using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [SerializeField]
    private float currentHealth;

    [Tooltip("Maximum amount of health")] public float MaxHealth = 10f;

    [Tooltip("Health ratio at which the critical health vignette starts appearing")]
    public float CriticalHealthRatio = 0.3f;

    [SerializeField] 
    private GameObject floatingText;

    public UnityAction<float, GameObject> OnDamaged;
    public UnityAction<float> OnHealed;
    public UnityAction OnDie;

    [HideInInspector]
    public float dmgBuff;


    public float CurrentHealth
    {
        get => currentHealth;
        set => currentHealth = value;
    }
    public bool Invincible { get; set; }
    public bool CanPickup() => CurrentHealth < MaxHealth;

    public float GetRatio() => CurrentHealth / MaxHealth;
    public bool IsCritical() => GetRatio() <= CriticalHealthRatio;

    bool m_IsDead;

    void Start()
    {
        CurrentHealth = MaxHealth;
    }

    //public void IncreaseDamage(float amount)
    //{
    //    dmgBuff += amount;
    //}

    public void Heal(float healAmount)
    {
        float healthBefore = CurrentHealth;
        CurrentHealth += healAmount;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);

        // Check if the player was dead and is now being revived
        if (healthBefore <= 0f && CurrentHealth > 0f)
        {
            m_IsDead = false; // Mark the player as no longer dead
            Debug.Log($"{gameObject.name} has been revived!");
        }

        // Call OnHealed action if healing occurred
        float trueHealAmount = CurrentHealth - healthBefore;
        if (trueHealAmount > 0f)
        {
            OnHealed?.Invoke(trueHealAmount);
        }
    }


    public void TakeDamage(float damage, GameObject damageSource)
    {
        if (Invincible)
            return;

        float healthBefore = CurrentHealth;
        CurrentHealth -= damage;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);

        //Trigger floating text
        if (floatingText && CurrentHealth > 0f)
            ShowFloatingText($"{damage}");

        // call OnDamage action
        float trueDamageAmount = healthBefore - CurrentHealth;
        if (trueDamageAmount > 0f)
        {
            OnDamaged?.Invoke(trueDamageAmount, damageSource);
        }

        HandleDeath();
    }

    public void Kill()
    {
        CurrentHealth = 0f;

        // call OnDamage action
        OnDamaged?.Invoke(MaxHealth, null);

        HandleDeath();
    }

    void HandleDeath()
    {
        if (m_IsDead)
            return;

        // call OnDie action
        if (CurrentHealth <= 0f)
        {
            m_IsDead = true;
            OnDie?.Invoke();
        }
    }

    private void ShowFloatingText(string textToShow)
    {
        //Can be optimized by Object Pooling
        var go = Instantiate(floatingText, transform.position + new Vector3(0, 1f, 0), Quaternion.identity, transform);
        go.transform.rotation = Quaternion.LookRotation(ActorsManager.FindActorById(Events.ActorPossesedEvent.CurrentActor).gameObject.transform.position);
        go.GetComponent<TextMeshPro>().text = textToShow;
    }
}