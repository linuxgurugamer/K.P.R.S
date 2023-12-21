# K.P.R.S.
Kerbal Public Radio Service mod for KSP

Idea suggested by AtomicTech
Toolbar button made by Flaticon.com
Shuffle icon made by https://www.freepik.com
Tower models made by @wyleg
Additional model work done by @AlphaMensae


https://discord.com/channels/1113137628305440818/1122881120942432337 

Dependencies
	Toolbar Controller
	ClickThroughBlocker
	SpaceTuxLibrary

Description

This mod provides broadcast radio transmitters and radio receivers.  There are a number of stations provided with the mod, and you can create your own.

Usage

Create one or more transmitters

A radio station will consist of the following:
	1. Antenna tower, either the composite or the individual pieces constructed on site.  Taller antennas will have greater range.
	2. A transmitter box (Note that a transmitter box uses 10 ec/sec)
	3. Power supply.  Will need to be able to supply the needs of the transmitter box(es) and amplifiers
	4. Optionally one or more amplifiers (each amplifier also needs 10 ec/sec)

1.	Create a radio station.  There are three antenna parts, which can be stacked on top of each
	other to make a taller tower, the taller it is, the greater the range.  There is also a composite
	antenna, which consists of all the antenna tower parts together which will animate to be raised.
1.5 To increase the power of the transmitter, attach multiple TransmitterBases to the antenna (not yet available)
2.	Once placed, right-click on the transmitter box to open the PAW and then select a channel
3.  After selecting a channel, you need to activate it, again by right-clicking on the transmitter base to open the PAW and activate the transmitter

Note:  The transmitter box by itself has a very limited range.  You need to add an antenna tower to get decent range.

Receive radio broadcast

1.  To receive, you need an antenna.  Currently all antennas have the ability to receive radio broadcasts, including command pods with built-in antennas.  Also, currently unmanned vessels also allow the receiver to be used
2.  Once on a vessel, click the radio button to open the radio panel
	1. Note:  The radio panel is scalable using the horizontal scroll bar at the top
3.  All active channels are listed in the scrollbox.  Click on a channel to select it.
4.  The five buttons right below are to save selected channels.  Right click on a button to either clear or save the current channel

To turn the radio on or off, click the power button on the left, the led to the left of the button will toggle between red for off and green for on

The lettered buttons currently are not implemented.  


The dial at the right is the volume dial.  You can adjust the volume either by clicking on the dial, or by using the ^ and v buttons just to the lower left of it

================================
Support

The Logs
These are text files that the game spits out for debugging purposes as it runs; if something broke horribly in-game, there will be something in here about it. You should upload the entire log as a file (i.e. not to pastebin); you can use dropbox or an equivalent host to upload the file. Make sure the entire file gets uploaded; you may have to zip it first, as logs can be very long.  DON'T paste the entire log into a thread post! That unnecessarily bloats the thread. Here is where you can find the log:
 
Windows:	%USERPROFILE%\AppData\LocalLow\Squad\Kerbal Space Program\Player.log
 
Mac OS X:	Open Console, on the left side of the window there is a menu that says 'Reports'. Scroll down the list and find the one which says 
			"Log Reports", click that.  A list of files will be displayed in the top part of the window, in there will be 
			Player.Log ( Files>~/Library/	Logs>Unity>Player.log ).  Right-click on it and select "Reveal in Finder".
 
Linux:		~/.config/unity3d/Squad/Kerbal\ Space\ Program/Player.log
 
