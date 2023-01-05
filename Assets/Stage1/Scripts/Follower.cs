using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;

public class Follower : MonoBehaviour
{
    public float radius;
    public float rotationSpeed = 2;
    public float speed;
    private Vector3 direction;
    private Quaternion finalRot = Quaternion.identity;
    public Transform target;
    
    private void Update()
    {
        var step = rotationSpeed * Time.deltaTime;
        transform.Translate(transform.forward * speed * Time.deltaTime, Space.World);
        if (Quaternion.Angle(finalRot, transform.rotation) > 0.1f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, finalRot, step);
        }

        direction = (target.transform.position - transform.position).normalized;
        finalRot = Quaternion.LookRotation(transform.position + direction * 1000f, Vector3.up);
    }
}
