using UnityEngine;
using UnityEngine.Events;

public abstract class AbilityBase : MonoBehaviour
{
    public class MyFloatEvent : UnityEvent<float> { }
    public MyFloatEvent OnAbilityUse = new MyFloatEvent();

    [Header("Ability Info")]
    public string title;
    public Sprite icon;
    public float cooldownTime = 1;
    [HideInInspector] public bool canUse = true;


    public void TriggerAbility()
    {
        if (canUse)
        {
            OnAbilityUse.Invoke(cooldownTime);
            Ability();
            Game_Manager.Instance.StartCooldown(this, cooldownTime);
        }

    }
    public abstract void Ability();

}

//public new string name;
//public Sprite icon;
//public float cooldownTime;
//public float activeTime;

//public virtual void Activate(GameObject parent) { }
//public virtual void BeginCooldown(GameObject parent) { }