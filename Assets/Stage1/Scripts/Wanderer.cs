using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class Wanderer : MonoBehaviour
{
    public NavMeshAgent agent;
    public Vector3 targetPos;
    public float radius;

    private void Awake()
    {
        targetPos = Random.insideUnitSphere * radius;
        targetPos.y = transform.position.y;
        agent.SetDestination(targetPos);
    }
    private void Update()
    {
        targetPos.y = transform.position.y;

        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            targetPos = Random.insideUnitSphere * radius;
            targetPos.y = transform.position.y;
            agent.SetDestination(targetPos);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 1f);
    }
}
