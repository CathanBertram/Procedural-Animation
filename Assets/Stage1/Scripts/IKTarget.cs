using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.Stage1.Scripts
{
    public class IKTarget : MonoBehaviour
    {
        [SerializeField] public Transform target;
        [SerializeField] private float maxDistance = 1;
        public float MaxDistance => maxDistance;
        [SerializeField] private bool moving = false;
        public float stepTime = 1;
        public IKChain chain;
        private Vector3 start;
        private Vector3 end;
        private float timer = 0;
        public bool Moving => moving;

        private Coroutine coroutine;

        private void Awake()
        {
            target.position = GetMidpoint();
            moving = false;
            timer = 0;
            stepping = true;
        }

        private void Start()
        {
            //target.position = transform.position;
        }

        public void Init()
        {
            target = new GameObject(gameObject.name + " Target").transform;
            //target.SetParent(transform);
            target.position = GetMidpoint();

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(target);
#endif
        }

        public void Init(float maxDist)
        {
            target = new GameObject(gameObject.name + " Target").transform;
            //target.SetParent(transform);
            target.position = GetMidpoint();
            maxDistance = maxDist;

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(target);
#endif
        }

        private Vector3 GetMidpoint()
        {
            return chain.RootTrans.position + (transform.position - chain.RootTrans.position).normalized * (Vector3.Distance(transform.position, chain.RootTrans.position) * 0.5f);
            //return (chain.RootTrans.position + transform.position) * 0.5f;
        }

        public float GetDistance()
        {
            return Vector3.Distance(transform.position, target.position);
        }

        private bool stepping = true;
        public bool Stepping => stepping;
        public void StartStep()
        {
            if (!stepping) return;

            //StartCoroutine(DoStep());
        }
        

        private void Update()
        {
            return;
            //Debug.Log($"{gameObject.name}, {transform.position}, {target.position}, {chain.GetPositionRelativeToRoot(transform)}, {chain.GetPositionRelativeToRoot(target)}");
            if (!moving && Vector3.Distance(transform.position, target.position) > maxDistance)
            {
                moving = true;
                start = transform.position;
                end = target.position;
            }
            else if (moving)
            {
                Step();
            }
        }
        
        private void Step()
        {
            timer += Time.deltaTime;
            if (timer > stepTime)
                timer = stepTime;
            transform.position = Vector3.Lerp(start, end, timer / stepTime);

            if (timer >= stepTime)
            {
                moving = false;
                timer = 0;
            }
        }     
    }
}