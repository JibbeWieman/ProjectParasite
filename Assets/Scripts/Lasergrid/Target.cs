using UnityEngine;

public class Target : MonoBehaviour
{
    public int laserDamage;

    public void Hit()
    {
        Debug.Log("Target Hit " + name);

        //Game_Manager.playerHealth.Damage(laserDamage);
    }
}