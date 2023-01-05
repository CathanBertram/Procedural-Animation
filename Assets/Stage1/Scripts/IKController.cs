using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Stage1.Scripts
{
    public class IKController : MonoBehaviour
    {
        [HideInInspector] public List<IKChain> ikChains;

        public void GetChains()
        {
            ikChains = GetComponentsInChildren<IKChain>().ToList();
        }
    }
}