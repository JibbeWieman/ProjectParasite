using System.Collections.Generic;
using UnityEngine;

public class ActorsManager : MonoBehaviour
{
    private int ActorAmount = 0;

    [Tooltip("List of all actors in the scene.")]
    public List<Actor> Actors { get; private set; } = new();
    public GameObject Player { get; private set; }

    public void SetPlayer(GameObject player) => Player = player;

    private void Start()
    {
        AssignActorID();
    }

    private void AssignActorID()
    {
        Player.GetComponent<Actor>().SetID(0);

        foreach (Actor actor in Actors)
        {
            actor.SetID(ActorAmount);
            ActorAmount++;
        }
    }

    public Actor FindActorById(int id)
    {
        return Actors.Find(actor => actor.id == id);
    }
}