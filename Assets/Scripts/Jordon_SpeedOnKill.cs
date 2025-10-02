// Jordon_SpeedOnKill.cs
// Simple move speed gain when this unit kills an enemy.
using UnityEngine;
using UnityEngine.AI;

public class Jordon_SpeedOnKill : MonoBehaviour
{
    public float moveSpeed = 3.5f;
    public float gainOnKill = 0.15f;
    NavMeshAgent agent;

    void Start(){ agent = GetComponent<NavMeshAgent>(); if (agent) agent.speed = moveSpeed; }

    public void Gain()
    {
        moveSpeed += gainOnKill;
        if (agent) agent.speed = moveSpeed;
    }
}
