using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacterController : ActorCharacterController
{
    private void Awake()
    {
        ActorsManager actorsManager = FindObjectOfType<ActorsManager>();

        if (actorsManager != null && gameObject.CompareTag("Player"))
            actorsManager.SetPlayer(gameObject);
    }

    public override void AdjustAnimationSpeed()
    {
        if (m_Animator != null)
        {
            if (m_InputHandler?.GetHorizontalInput() == 0 && m_InputHandler?.GetVerticalInput() == 0)
            {
                m_DesiredAnimationSpeed = 0f;
            }
            else
            {
                m_DesiredAnimationSpeed = Mathf.Clamp(CharacterVelocity.magnitude, 1f, 7f);
            }

            m_CurrentAnimationSpeed = Mathf.Lerp(m_CurrentAnimationSpeed, m_DesiredAnimationSpeed, m_TransitionSpeed);
            m_Animator.speed = m_CurrentAnimationSpeed;
        }
    }

    protected override void OnDie()
    {
        base.OnDie();

        EventManager.Broadcast(Events.PlayerDeathEvent);
    }
}
