using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HeapPadder
{
    [KSPAddon(KSPAddon.Startup.Instantly, false)]
    public class HeapPadder : MonoBehaviour
    {
         internal static HeapPadder instance = null;
        internal PadHeap padHeap;

        public void Awake()
        {
            if (instance != null)
            {
                gameObject.DestroyGameObject();
                return;
            }

            DontDestroyOnLoad(gameObject);
            instance = this;
            padHeap = new PadHeap();
        }

        void Start()
        {

            padHeap.Pad();
        }
    }
}
