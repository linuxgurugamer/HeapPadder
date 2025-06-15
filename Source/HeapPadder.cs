using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static KSP.UI.Screens.MessageSystem;

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

            if (Versioning.version_major == 1 && Versioning.version_minor >= 8 && Versioning.Revision >= 0)
            {
                ScreenMessages.PostScreenMessage("HeapPadder disabled in this version of KSP (only works on 1.8 and below)", 
                    15, ScreenMessageStyle.LOWER_CENTER);
                Destroy(this);
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
