using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public List<EnemyAI> Enemies { get; private set; }
    public int NumberOfEnemiesTotal { get; private set; }
    public int NumberOfEnemiesRemaining => Enemies.Count;

    void Awake()
    {
        Enemies = new List<EnemyAI>();
    }

    public void RegisterEnemy(EnemyAI enemy)
    {
        Enemies.Add(enemy);

        NumberOfEnemiesTotal++;
    }

    public void UnregisterEnemy(EnemyAI enemyKilled)
    {
        int enemiesRemainingNotification = NumberOfEnemiesRemaining - 1;

        EnemyKillEvent evt = Events.EnemyKillEvent;
        evt.Enemy = enemyKilled.gameObject;
        evt.RemainingEnemyCount = enemiesRemainingNotification;
        EventManager.Broadcast(evt);

        // removes the enemy from the list, so that we can keep track of how many are left on the map
        Enemies.Remove(enemyKilled);
    }
}