using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grounder : MonoBehaviour
{
    [SerializeField] private float radius = 1;
    [SerializeField] private LayerMask layers;
    private Vector3 pos;

    private void Awake()
    {
        if (Physics.Raycast(transform.position, -Vector3.up, out var hit, float.PositiveInfinity,~layers))
        {
            transform.position = hit.point;
        }
    }
    void FixedUpdate()
    {
        var colls = Physics.OverlapSphere(transform.position, radius, ~layers);
        if (colls.Length > 0)
        {
            var closest = Vector3.positiveInfinity;
            var dist = float.PositiveInfinity;
            foreach (var item in colls)
            {
                var p = item.ClosestPoint(transform.position);
                var d = Vector3.Distance(p, transform.position);
                if (d < dist)
                {
                    closest = p;
                    dist = d;
                }
            }
            transform.position = closest;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        //Gizmos.DrawSphere(transform.position, 0.5f);
    }
}
