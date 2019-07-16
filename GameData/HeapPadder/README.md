# HeapPadder
Copyright (c) 2016 Gerry Iles (Padishar)

This mod is designed to force Mono to keep 
significantly more free space in the heap, which can significantly reduce the frequency at which the heap fills up and the Mono 
garbage collection causes a stutter,

## Installation
Copy the HeapPadder folder from the zip file into the GameData folder of your KSP installation.

Regarding the padheap.cfg file:
If the default settings aren't working well for you, try the following steps:

1. Increase the﻿ padheap size in your install: \GameData\MemGraph\PluginData\MemGraph\padheap.cfg, change the "total" number to 4096 
   or even 6144. See the next section for an example
2. For example, if you have 16 GB:
	a. Look at your task manager to find out ﻿how much free ram you have when the game is running. 
	b. Do this calculation 16 Gb - ~5/6 GB from KSP - 1/2 GB Windows = 8 GB free
	c. This is the amount of RAM you can use for your padheap total size (in MegaByte 4GB=4096MB).  
	d.  Always have some Ram to spare of course. 
Padheap is the volume memgraph is using to save the junk and than deletes it when﻿ its﻿ full.



The code is released under the MIT license (see https://github.com/Gerry1135/MemGraph/blob/master/Graph.cs).

Updated by LinuxGuruGamer
