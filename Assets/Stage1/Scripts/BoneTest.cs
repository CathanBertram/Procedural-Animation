using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.Stage1.Scripts
{
    public class BoneTest : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnDrawGizmos()
        {
            var children = transform.GetComponentsInChildren<Transform>();

            foreach (var child in children)
            {
                if (child == transform) continue;

                var scale = Vector3.Distance(child.position, child.parent.position) * 0.1f;
                Handles.matrix = Matrix4x4.TRS(child.position, Quaternion.FromToRotation(Vector3.up, child.parent.position - child.position), new Vector3(scale, Vector3.Distance(child.parent.position, child.position), scale));
                Handles.color = Color.green;
                Handles.DrawWireCube(Vector3.up * 0.5f, Vector3.one);
            }
        }
    }
}