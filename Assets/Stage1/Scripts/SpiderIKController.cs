using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

namespace Assets.Stage1.Scripts
{
    public class SpiderIKController : IKController
    {
        [SerializeField] private float heightOffset;
        private int stepCount;
        private bool canStep;

        private void Awake()
        {
            stepCount = 0;
            foreach (var item in ikChains)
            {
                heightOffset += item.CompleteLength * 0.5f;
            }
            heightOffset /= ikChains.Count;

            foreach (var item in ikChains)
            {
                item.canStep = true;
                item.Step += StepToggle;
            }
        }

        private void StepToggle(bool isStepping)
        {
            if (isStepping)
                stepCount++;
            else
                stepCount--;
        }
        private void Update()
        {
            SetPos();
            //foreach (var item in ikChains)
            //{
            //    if (item.NeedStep() && stepCount < ikChains.Count / 2)
            //    {
            //        stepCount++;
            //        item.StartStep();
            //    }
            //}

            //for (int i = 0; i < ikChains.Count; i++)
            //{
            //    if (ikChains[i].NeedStep())
            //    {
            //        if (i != 0)
            //            if (ikChains[i - 1].stepping) continue;
            //        if (i >= ikChains.Count)
            //            if (ikChains[i + 1].stepping) continue;

            //        ikChains[i].StartStep();
            //    }
            //}
            StepController();
        }

        private void StepController()
        {
            int i = 0;
            while (i < ikChains.Count)
            {
                if (ikChains[i].NeedStep())
                {
                    for (int j = i - 2; j < i + 2; j++)
                    {
                        if (j == i) continue;
                        if (j < 0)
                            continue;
                        else if (j >= ikChains.Count)
                            continue;
                        if ((j == i + 1 && i % 2 == 1) || (j == i - 1 && i % 2 == 0)) continue;

                        if (ikChains[j].stepping)
                        {
                            goto END;
                        }
                    }


                    ikChains[i].StartStep();
                    if (i % 2 == 0)
                        i += 3;
                    else
                        i++;

                    continue;
                }
            END:;
                i++;
            }
        }
        
        private void SetPos()
        {
            var avgPos = Vector3.zero;
            foreach (var item in ikChains)
            {
                avgPos += item.target.transform.position;
            }
            avgPos /= ikChains.Count;
            avgPos.y += heightOffset;
            transform.position = avgPos;

            var f = ikChains[0].target.transform.position + ikChains[1].target.transform.position;
            f *= 0.5f;
            var b = ikChains[ikChains.Count - 1].target.transform.position + ikChains[ikChains.Count - 2].target.transform.position;
            b.y += heightOffset;
            b *= 0.5f;
            var dir = (f - b).normalized;
            var relPos = transform.position + dir * 1000f;
            relPos.y = heightOffset;
            //transform.forward = dir;
            transform.rotation = Quaternion.LookRotation(relPos, Vector3.up);
        }
    }
}