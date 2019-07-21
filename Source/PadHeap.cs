/*
 Copyright (c) 2016 Gerry Iles (Padishar)

 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:

 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.

 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 THE SOFTWARE.
*/

using System;
using System.IO;
//using KSP.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

namespace HeapPadder
{

    class Item8
    {
        public Item8 next = null;
    }

    // The following odd classes are used to force space allocation.  
#pragma warning disable CS0169
    class Item16
    {
        public Item16 next = null;
        double d2; // Here to force space allocation
    }

    class Item24
    {
        public Item24 next = null;
        double d2;// Here to force space allocation
        double d3;// Here to force space allocation
    }
#pragma warning restore  CS0169

    class PadHeap
    {
        const String configFilename = "GameData/HeapPadder/PluginData/padheap.cfg";
        const String defaultConfigFilename = "GameData/HeapPadder/PluginData/default_padheap.cfg";

        string[] defaultFileData =
        {
            "8 : 1",
            "16: 1",
            "24: 1",
            "32: 1",
            "40: 1",
            "48: 1",
            "64: 1",
            "80: 1",
            "96: 1",
            "112: 1",
            "144: 1",
            "176: 1",
            "208: 1",
            "240: 1",
            "296: 1",
            "352: 1",
            "432: 1",
            "664: 0",
            "800: 0",
            "1008: 0",
            "1344: 0",
            "2032: 0",
            "total: 1024"

        };

        Item8 head8 = null;
        Item16 head16 = null;
        Item24 head24 = null;


        int[] lengths = new int[] { 8, 16, 24, 32, 40, 48, 64, 80, 96, 112, 144, 176, 208, 240, 296, 352, 432, 664, 800, 1008, 1344, 2032 };
        int[] weights = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        int[] counts = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        object[][] heads = new object[][] { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null };

        public void Report()
        {
            long curMem = GC.GetTotalMemory(false);
            Log.Info("HeapAdder Report, scene: " + HighLogic.LoadedScene.ToString() + ", memory = " + (curMem / 1024) + " KB");
        }

        public void Pad()
        {
            Log.Info("HeapPadder.Pad");
            try
            {
                UpdateFromConfig();

                Log.Info("The highest generation is " + GC.MaxGeneration);
                long curMem = GC.GetTotalMemory(false);
                long startMem = curMem;
                Log.Info("Pad started, memory = " + (curMem / 1024) + " KB");


                head8 = null;
                head16 = null;
                head24 = null;
                for (int i = 0; i < heads.Length; i++)
                    heads[i] = null;

                GC.Collect();
                curMem = GC.GetTotalMemory(false);
                long minMem = curMem;
                Log.Info("After discard and collect, memory = " + (curMem / 1024) + " KB");

                // Do the small sizes with custom classes
                Pad8();
                Pad16();
                Pad24();

                // Do the rest of the sizes with arrays of object
                for (int i = 3; i < lengths.Length; i++)
                    PadArray(i);

                curMem = GC.GetTotalMemory(false);
                Log.Info("After padding, memory = " + (curMem / 1024) + " KB");

                GC.Collect();
                curMem = GC.GetTotalMemory(false);
                Log.Info("After final collect, memory = " + (curMem / 1024) + " KB");
                ScreenMessages.PostScreenMessage("HeapPadder, initial mem: " + ((int)startMem / 1024).ToString("F0") +
                    ", minMem: " + ((int)minMem / 1024).ToString("F0") + ", final mem: " + ((int)curMem / 1024).ToString("F0"), 10f, ScreenMessageStyle.UPPER_CENTER);

            }
            catch (Exception e)
            {
                Log.Info(e.ToString());
            }
        }

        int GetMem()
        {
            var si = SystemInfo.systemMemorySize;
            Log.Info("Physical RAM (bytes): " + si.ToString());
            int m = si / 1024;
            ScreenMessages.PostScreenMessage("HeapPadder, System Memory Size: " + m + " gig", 10f, ScreenMessageStyle.UPPER_CENTER);
            return (int)m;

        }
        void UpdateFromConfig()
        {
            for (int i = 0; i < counts.Length; i++)
            {
                weights[i] = 0;
                counts[i] = 0;
            }

            int totalWeight = 0;
            if (!File.Exists(configFilename))
            {
                Log.Info("No config file, copying default");
                int mem = GetMem();
                string fname = "default_padheap.cfg";
                if (mem <= 4)
                    fname = "SuggestedFor_4g.cfg";
                if (mem >4 && mem <= 8)
                    fname = "default_padheap.cfg";
                if (mem > 8 && mem <= 20)
                    fname = "SuggestedFor_16g.cfg";
                if (mem >=20)
                    fname = "SuggestedFor_32g.cfg";

                ScreenMessages.PostScreenMessage("HeapPadder, no config file, using default: " + fname, 10f, ScreenMessageStyle.UPPER_CENTER);
                if (File.Exists(fname))
                {
                    Log.Info("Copying: " + defaultConfigFilename + "  to: " + configFilename);
                    String[] lines = File.ReadAllLines(defaultConfigFilename);
                    File.WriteAllLines(configFilename, lines);
                }
                else
                {
                    string s = "Default config file: " + fname + " missing, using built-in defaults";
                    Log.Info(s);
                    ScreenMessages.PostScreenMessage("HeapPadder, " + s, 10f, ScreenMessageStyle.UPPER_CENTER);
                    File.WriteAllLines(configFilename, defaultFileData);
                }
            }
            if (File.Exists(configFilename))
            {
                Log.Info("Found: " + configFilename);
                String[] lines = File.ReadAllLines(configFilename);
                String[] line;

                //
                // The following is to allow comments and blank lines in the file
                // Was easier to prefilter it rather than filter while processing
                //
                List<string> lineLst = new List<string>();
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Length == 0 || lines[i].Substring(0, 1) == "#")
                        continue;
                    lineLst.Add(lines[i]);
                }
                lines = lineLst.ToArray();

                for (int i = 0; i < weights.Length; i++)
                {
                    line = lines[i].Split(':');
                    if (line.Length == 2)
                    {
                        String val = line[1].Trim();
                        ReadInt32(val, ref weights[i]);
                        totalWeight += weights[i];
                    }
                    else
                    {
                        Log.Error("Invalid line in padheap.cfg: '" + lines[i] + "'");
                    }
                }

                int sizeMegs = 0;
                line = lines[weights.Length].Split(':');
                ReadInt32(line[1].Trim(), ref sizeMegs);
                Log.Info("totalPages: " + sizeMegs * 256 + ", totalWeight; " + totalWeight);
                if (sizeMegs > 0)
                {
                    int totalPages = sizeMegs * 256;    // 256 4k pages per meg
                    for (int i = 0; i < counts.Length; i++)
                    {
                        counts[i] = (weights[i] * totalPages) / totalWeight;
                    }
                }
            }
            else
                Log.Info("Can't find padheap.cfg");

        }

        void Pad8()
        {
            long count = counts[0];

            long lastMem = GC.GetTotalMemory(false);
            Item8 temp = null;
            Item8 test;
            while (count > 0)
            {
                // Allocate a block
                test = new Item8();

                long curMem = GC.GetTotalMemory(false);
                if (curMem == lastMem + 4096)
                {
                    // Add the block to the keep list
                    test.next = head8;
                    head8 = test;
                    count--;
                }
                else
                {
                    // Store the block in the temp list
                    test.next = temp;
                    temp = test;
                }

                lastMem = curMem;
            }
        }

        void Pad16()
        {
            long count = counts[1];

            long lastMem = GC.GetTotalMemory(false);
            Item16 temp = null;
            Item16 test;
            while (count > 0)
            {
                // Allocate a block
                test = new Item16();

                long curMem = GC.GetTotalMemory(false);
                if (curMem == lastMem + 4096)
                {
                    // Add the block to the keep list
                    test.next = head16;
                    head16 = test;
                    count--;
                }
                else
                {
                    // Store the block in the temp list
                    test.next = temp;
                    temp = test;
                }

                lastMem = curMem;
            }
        }

        void Pad24()
        {
            long count = counts[2];

            long lastMem = GC.GetTotalMemory(false);
            Item24 temp = null;
            Item24 test;
            while (count > 0)
            {
                // Allocate a block
                test = new Item24();

                long curMem = GC.GetTotalMemory(false);
                if (curMem == lastMem + 4096)
                {
                    // Add the block to the keep list
                    test.next = head24;
                    head24 = test;
                    count--;
                }
                else
                {
                    // Store the block in the temp list
                    test.next = temp;
                    temp = test;
                }

                lastMem = curMem;
            }
        }

        void PadArray(int index)
        {
            int bytes = lengths[index];
            int refCount = (bytes - 24) / 8;
            long count = counts[index];

            long lastMem = GC.GetTotalMemory(false);
            object[] temp = null;
            object[] test;
            while (count > 0)
            {
                // Allocate a block
                test = new object[refCount];

                long curMem = GC.GetTotalMemory(false);
                if (curMem == lastMem + 4096)
                {
                    // Add the block to the keep list
                    test[0] = heads[index];
                    heads[index] = test;
                    count--;
                }
                else
                {
                    // Store the block in the temp list
                    test[0] = temp;
                    temp = test;
                }

                lastMem = curMem;
            }
        }

        void ReadInt32(String str, ref Int32 variable)
        {
            Int32 value = 0;
            if (Int32.TryParse(str, out value))
                variable = value;
        }
    }
}
