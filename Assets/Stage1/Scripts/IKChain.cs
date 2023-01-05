using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

namespace Assets.Stage1.Scripts
{
    public class IKChain : MonoBehaviour
    {
        [SerializeField] private string name;
        public IKTarget target;
        public int chainLength;
        public int iterations;
        public float breakDistance = 0.001f;

        [SerializeField] private Transform rootTrans;
        public Transform RootTrans => rootTrans;
        [SerializeField] private Transform[] bones;
        public Transform[] Bones => bones;
        [SerializeField] private float[] boneLengths;
        [SerializeField] private float completeLength;
        public float CompleteLength => completeLength;
        private Vector3[] bonePositions;
        private Vector3[] boneRotations;
        private Vector3[] boneStartDirection;
        private Quaternion[] boneStartRotation;
        public RotationalConstraint[] constraints;
        private Quaternion targetStartRotation;
        private bool initialised = false;

        private Vector3 targetPos;
        private Quaternion targetRot;
        public bool canStep;

        public Vector2 pitchConstraint;
        public Vector2 yawConstraint;
        public Vector2 rollConstraint;


        public event Action<bool> Step;
        public void StepAction(bool tf) { Step?.Invoke(tf); }

        private void Awake()
        {
            if(!initialised)
                Init();
        }

        private void FixedUpdate()
        {
            return;
            if (target.GetDistance() > target.MaxDistance && canStep && target.Stepping)
            {
                StepAction(true);
                Debug.Log("dostep");
                target.StartStep();
            }
        }

        public bool NeedStep()
        {
            if (stepping) return false;
            return target.GetDistance() > target.MaxDistance;
        }
        private void LateUpdate()
        {
            IK();
        }

        public void Init()
        {
            //Get Root
            rootTrans = transform;
            for (int i = 0; i <= chainLength; i++)
            {
                if (rootTrans.parent == null)
                {
                    chainLength = i;
                    break;
                }
                rootTrans = rootTrans.parent;
            }

            //Initialize Arrays
            if (bones == null)
                bones = new Transform[chainLength + 1];
            if (bonePositions == null)
                bonePositions = new Vector3[chainLength + 1];
            if(boneRotations == null)
                boneRotations = new Vector3[chainLength + 1];
            if(boneLengths == null)
                boneLengths = new float[chainLength + 1];
            if(boneStartDirection == null)
                boneStartDirection = new Vector3[chainLength + 1];
            if(boneStartRotation == null)
                boneStartRotation = new Quaternion[chainLength + 1];
            if(constraints == null)
                constraints = new RotationalConstraint[chainLength + 1];

            //Create Target
            if (target == null)
            {
                var t = new GameObject(gameObject.name + " Target").AddComponent<IKTarget>();
                target = t.GetComponent<IKTarget>();
                SetPositionRelativeToRoot(target.transform, GetPositionRelativeToRoot(transform));
                target.chain = this;
                target.Init(completeLength * 0.33f);
            }
            targetStartRotation = GetRotationRelativeToRoot(target.transform);

            var curr = transform;
            completeLength = 0;

            for (int i = bones.Length - 1; i >= 0; i--)
            {
                bones[i] = curr;
                boneStartRotation[i] = GetRotationRelativeToRoot(curr);

                if (i == bones.Length - 1)
                    boneStartDirection[i] = GetPositionRelativeToRoot(target.transform) - GetPositionRelativeToRoot(curr);
                else
                {
                    boneStartDirection[i] = GetPositionRelativeToRoot(bones[i + 1]) - GetPositionRelativeToRoot(curr);
                    boneLengths[i] = boneStartDirection[i].magnitude;
                    completeLength += boneLengths[i];
                }

                curr = curr.parent;
            }
            initialised = true;

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        public bool stepping;
        public float stepTime = 1;
        public void StartStep()
        {
            if (stepping) return;

            StartCoroutine(DoStep());
        }
        private IEnumerator DoStep()
        {
            stepping = true;
            var start = transform.position;
            var end = target.target.position;
            float timer = 0;
            while (Vector3.Distance(target.transform.position, end) > 0.01f)
            {
                timer += Time.deltaTime;
                if (timer > target.stepTime)
                    timer = target.stepTime;
                target.transform.position = Vector3.Lerp(start, end, timer / target.stepTime);
                yield return new WaitForEndOfFrame();
            }
            stepping = false;
            StepAction(false);
        }

        public void IK()
        {
            if (!IKInit()) return;

            if (!ReachCheck()) return;


            for (int i = 0; i < iterations; i++)
            {
                Backward();             

                Forward();

                // Check if close enough
                if (DistanceCheck())
                    break;
            }

            End();
        }

        private bool IKInit()
        {
            // Return if no target is set
            if (target == null) return false;
            // Return if not initialised
            if (!initialised) return false;

            //Get starting positions
            for (int i = 0; i < bones.Count(); i++)
            {
                bonePositions[i] = GetPositionRelativeToRoot(bones[i]);
                boneRotations[i] = GetEulerRotationRelativeToRoot(bones[i]);
            }
            targetPos = GetPositionRelativeToRoot(target.transform);
            targetRot = GetRotationRelativeToRoot(target.transform);

            return true;
        }
        private bool ReachCheck()
        {
            //Check if within reach
            if ((targetPos - GetPositionRelativeToRoot(bones[0])).sqrMagnitude >= completeLength * completeLength)
            {
                var direction = (targetPos - bonePositions[0]).normalized;

                for (int i = 1; i < bonePositions.Length; i++)
                    bonePositions[i] = bonePositions[i - 1] + direction * boneLengths[i - 1];

                End();

                return false;
            }
            return true;
        }
        private void End()
        {
            for (int i = 0; i < bonePositions.Length; i++)
            {
                if (i == bonePositions.Length - 1)
                    SetRotationRelativeToRoot(bones[i], Quaternion.Inverse(targetRot) * targetStartRotation * Quaternion.Inverse(boneStartRotation[i]));
                else
                    SetRotationRelativeToRoot(bones[i], Quaternion.FromToRotation(boneStartDirection[i], bonePositions[i + 1] - bonePositions[i]) * Quaternion.Inverse(boneStartRotation[i]));
                SetPositionRelativeToRoot(bones[i], bonePositions[i]);
                SetRotationRelativeToRoot(bones[i], Quaternion.Euler(boneRotations[i]));

            }
        }
        private void Backward()
        {
            // Loop backwards through each bone and set position to the end of the previous bone (use target position for leaf bone)
            for (int i = bonePositions.Length - 1; i > 0; i--)
            {

                if (i == bonePositions.Length - 1)
                    bonePositions[i] = targetPos;
                else
                {
                    var line = bonePositions[i] - bonePositions[i + 1];
                    bonePositions[i] = bonePositions[i + 1] + line.normalized * boneLengths[i];

                    //var line2 = bonePositions[i + 2] - bonePositions[i + 1];

                    //float pitch = Vector3.SignedAngle(line, line2, Vector3.forward);
                    //float yaw = Vector3.SignedAngle(line, line2, Vector3.up);
                    //float roll = Vector3.SignedAngle(line, line2, Vector3.right);   
                    Vector3 forward;
                    if (i == bonePositions.Length - 2)
                    {
                        forward = Vector3.down;
                    }
                    else
                    {
                        forward = (bonePositions[i + 1] - bonePositions[i + 2]).normalized;
                    }

                    var up = Vector3.Cross(forward, Vector3.right);
                    var right = Vector3.Cross(forward, Vector3.up);

                    float pitch = Vector3.SignedAngle(bonePositions[i + 1], bonePositions[i], forward);
                    float yaw = Vector3.SignedAngle(bonePositions[i + 1], bonePositions[i], up);
                    float roll = Vector3.SignedAngle(bonePositions[i + 1], bonePositions[i], right);
                    var constraint = constraints[i + 1];
                    pitch = Mathf.Clamp(pitch, constraint.PitchConstraint.x, constraint.PitchConstraint.y);
                    yaw = Mathf.Clamp(yaw, constraint.YawConstraint.x, constraint.YawConstraint.y);
                    roll = Mathf.Clamp(roll, constraint.RollConstraint.x, constraint.RollConstraint.y);
                    var rotation = Quaternion.Euler(pitch, yaw, roll);
                    bonePositions[i] = rotation * (bonePositions[i] - bonePositions[i + 1]) + bonePositions[i + 1];
                }
            }
        }

        private void Forward()
        {
            for (int i = 1; i < bonePositions.Length; i++)
                bonePositions[i] = bonePositions[i - 1] + (bonePositions[i] - bonePositions[i - 1]).normalized * boneLengths[i - 1];
        }

        private bool DistanceCheck()
        {
            return (bonePositions[bonePositions.Length - 1] - targetPos).sqrMagnitude < breakDistance * breakDistance;
        }

        public Vector3 GetPositionRelativeToRoot(Transform t)
        {
            if (rootTrans == null)
                return t.position;

            return Quaternion.Inverse(rootTrans.rotation) * (t.position - rootTrans.position);
        }
        private void SetPositionRelativeToRoot(Transform t, Vector3 pos)
        {
            if (rootTrans == null)
                t.position = pos;
            else
                t.position = rootTrans.rotation * pos + rootTrans.position;
        }

        public Quaternion GetRotationRelativeToRoot(Transform t)
        {
            if (rootTrans == null)
                return t.rotation;

            return Quaternion.Inverse(t.rotation) * rootTrans.rotation;
        }

        public Vector3 GetEulerRotationRelativeToRoot(Transform t)
        {
            if (rootTrans == null)
                return t.rotation.eulerAngles;

            return (Quaternion.Inverse(t.rotation) * rootTrans.rotation).eulerAngles;
        }
        private void SetRotationRelativeToRoot(Transform t, Quaternion rot)
        {
            if (rootTrans == null)
                t.rotation = rot;
            else
                t.rotation = rootTrans.rotation * rot;
        }

        private void OnValidate()
        {
            if (name == "")
            {
                name = gameObject.name;
            }
            gameObject.name = name;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            if (stepping) Gizmos.color = Color.green;

            Gizmos.DrawSphere(target.transform.position, 0.25f);
        }

        public void SetAllConstraints(Vector2 p, Vector2 y, Vector2 r)
        {
            foreach (var item in constraints)
            {
                item.SetConstraints(p, y, r);
            }
        }
    }

    [CustomEditor(typeof(IKChain))]
    public class IKChainEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var t = (IKChain)target;

            if (GUILayout.Button("Init"))
            {
                t.Init();
            }
            if (GUILayout.Button("IK"))
            {
                t.IK();
            }
        }

        public void OnSceneGUI()
        {
            var t = (IKChain)target;
            var transform = t.transform;
            var curr = transform;
            for (int i = 0; i < t.chainLength; i++)
            {
                if (curr.parent == null) break;
                var scale = Vector3.Distance(curr.position, curr.parent.position) * 0.1f;
                Handles.matrix = Matrix4x4.TRS(curr.position, Quaternion.FromToRotation(Vector3.up, curr.parent.position - curr.position), new Vector3(scale, Vector3.Distance(curr.parent.position, curr.position), scale));
                Handles.color = Color.red;
                Handles.DrawWireCube(Vector3.up * 0.5f, Vector3.one);
                if (curr.parent == null) break;

                curr = curr.parent;
            }           
        }
    }
}