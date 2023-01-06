using Assets.Stage1.Scripts;
using Codice.Client.Common.GameUI;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.Stage1.Scripts
{
    [CustomEditor(typeof(IKController), true)]
    public class IKControllerEditor : Editor
    {
        bool[] chainFoldoutBools = new bool[0];
        bool[] chainIdentifyBools = new bool[0];
        float[] chainScaleFloats = new float[0];
        Vector2[] pitchConstraints = new Vector2[0];
        Vector2[] yawConstraints = new Vector2[0];
        Vector2[] rollConstraints = new Vector2[0];
        Vector2 pitchConstraint = new Vector2();
        Vector2 yawConstraint = new Vector2();
        Vector2 rollConstraint = new Vector2();
        bool setConstraints;
        bool setIndividualConstraints;
        bool constraintDropDown;
        int indexToConstrain;
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            IKController ikController = (IKController)target;
            setConstraints = false;
            setIndividualConstraints = false;

            if (chainFoldoutBools == null) chainFoldoutBools = new bool[1];
            if (chainIdentifyBools == null) chainFoldoutBools = new bool[1];
            if (chainScaleFloats == null) chainFoldoutBools = new bool[1];
            if (pitchConstraints == null) chainFoldoutBools = new bool[1];
            if (yawConstraints == null) chainFoldoutBools = new bool[1];
            if (rollConstraints == null) chainFoldoutBools = new bool[1];
            if (ikController.ikChains == null) ikController.ikChains = new List<IKChain>();

            
            if (chainFoldoutBools.Count() != ikController.ikChains.Count)
                chainFoldoutBools = new bool[ikController.ikChains.Count];
            if (chainIdentifyBools.Count() != ikController.ikChains.Count)
                chainIdentifyBools = new bool[ikController.ikChains.Count];
            if (chainScaleFloats.Count() != ikController.ikChains.Count)
                chainScaleFloats = new float[ikController.ikChains.Count];
            if (pitchConstraints.Count() != ikController.ikChains.Count)
                pitchConstraints = new Vector2[ikController.ikChains.Count];
            if (yawConstraints.Count() != ikController.ikChains.Count)
                yawConstraints = new Vector2[ikController.ikChains.Count];
            if (rollConstraints.Count() != ikController.ikChains.Count)
                rollConstraints = new Vector2[ikController.ikChains.Count];


            if (GUILayout.Button("Get Chains"))
            {
                //ikController.GetChains();
                ikController.ikChains = ikController.GetComponentsInChildren<IKChain>().ToList(); 
                chainFoldoutBools = new bool[ikController.ikChains.Count];
                chainIdentifyBools = new bool[ikController.ikChains.Count];
                chainScaleFloats = new float[ikController.ikChains.Count];
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Collapse All"))
                ToggleAll(false);
            if (GUILayout.Button("Expand All"))
                ToggleAll(true);
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Initialise All Chains"))
                InitAll(ikController);

            constraintDropDown = EditorGUILayout.Foldout(constraintDropDown, "Constraints");
            if (constraintDropDown)
            {
                pitchConstraint = EditorGUILayout.Vector2Field("Pitch Constraint", pitchConstraint);
                yawConstraint = EditorGUILayout.Vector2Field("Yaw Constraint", yawConstraint);
                rollConstraint = EditorGUILayout.Vector2Field("Roll Constraint", rollConstraint);
                indexToConstrain = EditorGUILayout.IntField("Index To Constrain", indexToConstrain);
                if (GUILayout.Button("Set All Constraints"))
                {
                    setConstraints = true;
                }
                if (GUILayout.Button("Set Individual Constraint"))
                {
                    setConstraints = true;
                    setIndividualConstraints = true;
                }
            }



            if (ikController.ikChains != null)
                AddChains(ikController, setConstraints, setIndividualConstraints);

            serializedObject.ApplyModifiedProperties();
        }
        private void ToggleAll(bool b)
        {
            if (chainFoldoutBools == null) return;

            for (int i = 0; i < chainFoldoutBools.Count(); i++)
            {
                chainFoldoutBools[i] = b;
            }
        }
        private void InitAll(IKController ikController)
        {
            foreach (var item in ikController.ikChains)
            {
                item.Init();
                item.target.transform.SetParent(ikController.transform);
            }
        }
        private void AddChains(IKController ikController, bool setConstraints, bool individualConstraint, int index = 0)
        {
            var chains = ikController.ikChains;
            for (int i = 0; i < chains.Count; i++)
            {
                //GUILayout.Label();
                chainFoldoutBools[i] = EditorGUILayout.Foldout(chainFoldoutBools[i], chains[i].gameObject.name);
                if (chainFoldoutBools[i])
                {
                    SerializedObject o = new SerializedObject(chains[i]);

                    EditorGUILayout.PropertyField(o.FindProperty("name"), new GUIContent("name"));
                    EditorGUILayout.PropertyField(o.FindProperty("target"), new GUIContent("target"));
                    EditorGUILayout.PropertyField(o.FindProperty("chainLength"), new GUIContent("chainLength"));
                    EditorGUILayout.PropertyField(o.FindProperty("iterations"), new GUIContent("iterations"));
                    EditorGUILayout.PropertyField(o.FindProperty("breakDistance"), new GUIContent("breakDistance"));
                    EditorGUILayout.PropertyField(o.FindProperty("rootTrans"), new GUIContent("rootTrans"));
                    EditorGUILayout.PropertyField(o.FindProperty("bones"), new GUIContent("bones"));
                    EditorGUILayout.PropertyField(o.FindProperty("boneLengths"), new GUIContent("boneLengths"));
                    EditorGUILayout.PropertyField(o.FindProperty("completeLength"), new GUIContent("completeLength"));
                    EditorGUILayout.PropertyField(o.FindProperty("constraints"), new GUIContent("constraints"));
                    chainScaleFloats[i] = EditorGUILayout.FloatField("Gizmo Scale", chainScaleFloats[i]);
                    EditorGUILayout.PropertyField(o.FindProperty("pitchConstraint"), new GUIContent("pitchConstraint"));
                    EditorGUILayout.PropertyField(o.FindProperty("yawConstraint"), new GUIContent("yawConstraint"));
                    EditorGUILayout.PropertyField(o.FindProperty("rollConstraint"), new GUIContent("rollConstraint"));
                    EditorGUILayout.PropertyField(o.FindProperty("pole"), new GUIContent("pole"));

                    if (chainScaleFloats[i] <= 0)
                        chainScaleFloats[i] = 1;
                    
                    if (GUILayout.Button("Identify"))                  
                        chainIdentifyBools[i] = !chainIdentifyBools[i];

                    if (GUILayout.Button("Init Chain"))
                        chains[i].Init();

                    if (GUILayout.Button("Set All Constraints"))
                        chains[i].SetAllConstraints(pitchConstraints[i], yawConstraints[i], rollConstraints[i]);

                    if (setConstraints)
                    {
                        if (individualConstraint)
                        {
                            if (chains[i].constraints.Count() > index)
                            {
                                var constraint = chains[i].constraints[index];
                                var bone = chains[i].Bones[index];
                                constraint.PitchConstraint = pitchConstraint;
                                constraint.YawConstraint = yawConstraint;
                                constraint.RollConstraint = rollConstraint;
                                constraint.PitchConstraint.x += bone.rotation.x;
                                constraint.PitchConstraint.y += bone.rotation.x;
                                constraint.YawConstraint.x += bone.rotation.y;
                                constraint.YawConstraint.y += bone.rotation.y;
                                constraint.RollConstraint.x += bone.rotation.z;
                                constraint.RollConstraint.y += bone.rotation.z;
                            }
                        }
                        else
                        {
                            foreach (var item in chains[i].constraints)
                            {
                                item.PitchConstraint = pitchConstraint;
                                item.YawConstraint = yawConstraint;
                                item.RollConstraint = rollConstraint;
                            }

                            for (int j = 0; j < chains[i].Bones.Count(); j++)
                            {
                                var constraint = chains[i].constraints[j];
                                var bone = chains[i].Bones[j];

                                var x = Static.WrapAngle(bone.localEulerAngles.x);
                                var y = Static.WrapAngle(bone.localEulerAngles.y);
                                var z = Static.WrapAngle(bone.localEulerAngles.z);

                                if (j == 0)
                                {
                                    x = Static.WrapAngle(bone.eulerAngles.x);
                                    y = Static.WrapAngle(bone.eulerAngles.y);
                                    z = Static.WrapAngle(bone.eulerAngles.z);
                                }    

                                Debug.Log(j + " " + x + " " + y + " " + z);
                                constraint.PitchConstraint = pitchConstraint;
                                constraint.YawConstraint = yawConstraint;
                                constraint.RollConstraint = rollConstraint;

                                constraint.PitchConstraint.x = Static.WrapAngle(pitchConstraint.x + x);
                                constraint.PitchConstraint.y = Static.WrapAngle(pitchConstraint.y + x);
                                constraint.YawConstraint.x = Static.WrapAngle(yawConstraint.x + y);
                                constraint.YawConstraint.y = Static.WrapAngle(yawConstraint.y + y);
                                constraint.RollConstraint.x = Static.WrapAngle(rollConstraint.x + z);
                                constraint.RollConstraint.y = Static.WrapAngle(rollConstraint.y + z);

                                //constraint.PitchConstraint.x += x;
                                //constraint.PitchConstraint.y += x; 
                                //constraint.YawConstraint.x += y;
                                //constraint.YawConstraint.y += y; 
                                //constraint.RollConstraint.x += z;
                                //constraint.RollConstraint.y += z;
                            }
                            chains[i].pitchConstraint = pitchConstraint;
                            chains[i].yawConstraint = yawConstraint;
                            chains[i].rollConstraint = rollConstraint;
                        }                      
                    }
                    o.ApplyModifiedProperties();
                }            
            }

            //foreach (var chain in ikController.ikChains)
            //{
            //    GUILayout.Label(chain.gameObject.name);
            //    SerializedObject o = new SerializedObject(chain);

            //    EditorGUILayout.PropertyField(o.FindProperty("target"), new GUIContent("target"));
            //    EditorGUILayout.PropertyField(o.FindProperty("chainLength"), new GUIContent("chainLength"));
            //    EditorGUILayout.PropertyField(o.FindProperty("iterations"), new GUIContent("iterations"));
            //    EditorGUILayout.PropertyField(o.FindProperty("breakDistance"), new GUIContent("breakDistance"));
            //    EditorGUILayout.PropertyField(o.FindProperty("rootTrans"), new GUIContent("rootTrans"));
            //    EditorGUILayout.PropertyField(o.FindProperty("bones"), new GUIContent("bones"));
            //    EditorGUILayout.PropertyField(o.FindProperty("boneLengths"), new GUIContent("boneLengths"));
            //    EditorGUILayout.PropertyField(o.FindProperty("completeLength"), new GUIContent("completeLength"));

            //    //EditorGUILayout.BeginHorizontal();
            //    //EditorGUILayout.PrefixLabel("target");
            //    //chain.target = EditorGUILayout.ObjectField(chain.target, typeof(Transform), true) as Transform;
            //    //EditorGUILayout.EndHorizontal();
            //    o.ApplyModifiedProperties();
            //}
        }
        private void OnSceneGUI()
        {
            IKController ikController = (IKController)target;
            var chains = ikController.ikChains;

            if (chains != null)
            {
                for (int i = 0; i < chains.Count; i++)
                {
                    var transform = chains[i].transform;
                    var curr = transform;
                    Handles.color = Color.red;

                    if (chainIdentifyBools != null && chainIdentifyBools.Count() == chains.Count && chainIdentifyBools[i] == true)
                        Handles.color = Color.green;

                    for (int j = 0; j < chains[i].chainLength; j++)
                    {
                        if (curr.parent == null) break;
                        var scale = Vector3.Distance(curr.position, curr.parent.position) * 0.1f;                        
                        Handles.matrix = Matrix4x4.TRS(curr.position, Quaternion.FromToRotation(Vector3.up, curr.parent.position - curr.position), new Vector3(scale, Vector3.Distance(curr.parent.position, curr.position), scale));

                        var width = 1f;
                        if (chainScaleFloats != null && chainScaleFloats.Count() == chains.Count)
                            width = chainScaleFloats[i];

                        Handles.DrawWireCube(Vector3.up * 0.5f, new Vector3(width, 1, width));
                        if (curr.parent == null) break;

                        curr = curr.parent;
                    }
                }

                SceneView.RepaintAll();
            }
        }
       
    }
}