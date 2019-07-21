## HeapPadder

Derived from the Memgraph mod written by Padishar.
Copyright (c) 2016 Gerry Iles (Padishar)


This mod is designed to force Mono to keep significantly more free space in the heap, which can significantly reduce the frequency at which the heap fills up and the Mono garbage collection causes a stutter,

## Installation
Copy the HeapPadder folder from the zip file into the GameData folder of your KSP installation.

Regarding the padheap.cfg file:
If the default settings aren't working well for you, try the following steps:

1. Increase the padheap size in your install: \GameData\MemGraph\PluginData\MemGraph\padheap.cfg, change the "total" number to 4096 
   or even 6144. See the next section for an example
2. For example, if you have 16 GB:
	a. Look at your task manager to find out ﻿how much free ram you have when the game is running. 
	b. Do this calculation 16 Gb - ~5/6 GB from KSP - 1/2 GB Windows = 8 GB free
	c. This is the amount of RAM you can use for your padheap total size (in MegaByte 4GB=4096MB).  
	d.  Always have some Ram to spare of course. 

The mod uses a config file called:  padheap.cfg.  This file is not distributed with the mod, it is created the first time running the mod based on the amount of system memory found.

The mod has several default config files for different sizes of system memory.  These files are used if no padheap.cfg file is found, usually on the first time running.

If the config file "padheap.cfg" is found, then it will be used.  The mod will NOT overwrite the file, this is done so that any future updates to the mod won't wipe out any local config changes

There is nothing preventing you from use one of the suggested files, even if your system memory is different.  To use any of them, simply copy the desired file to:  padheap.cfg

Current files supplied allocate the following memory to the heap:

SuggestedFor_32g.cfg	8 gig of memory, used by default if system memory is greater than 20 gig of memory
SuggesetdFor_16g.cfg	4 gig of memory, used by default if system memory is between 8 and 20 gig of memory
SuggestedFor_4g.cfg		1/2 gig of memory, used by default if system memory is less than or equal to 4 gig
default_padheap.cfg		1 gig of memory, used by default if system memory is greater than 4 and less than or equal to 8 gig of memory

In the unlikely event that no files are found, a default configuration will be written out.

The code is released under the MIT license (see https://github.com/linuxgurugamer/heapPadder/blob/master/Graph.cs).

