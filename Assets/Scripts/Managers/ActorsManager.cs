﻿using System.Collections.Generic;
using UnityEngine;

public class ActorsManager : MonoBehaviour
{
    private int ActorAmount = 1;

    [Tooltip("List of all actors in the scene.")]
    public static List<Actor> Actors { get; private set; } = new();
    public GameObject Player { get; private set; }

    public void SetPlayer(GameObject player) => Player = player;

    private void Start()
    {
        AssignActorID();
    }

    private void AssignActorID()
    {
        foreach (Actor actor in Actors)
        {
            actor.SetID(ActorAmount);
            ActorAmount++;
        }
        Player.GetComponent<Actor>().SetID(0);
    }

    public static Actor FindActorById(int id) => Actors.Find(actor => actor.id == id);
}