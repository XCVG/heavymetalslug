# HEAVY METAL SLUG

**A 2.5D Metal Slug clone made for Old Games Remaster jam**

**XCVG Systems 2020**

## Introduction

It's Metal Slug, but in 2.5D (3D graphics and 2D gameplay) and with a heavy metal twist! Metal Slug is a series of side-scrolling shooters originally developed by Nazca and published by SNK for the Neo Geo arcade machines and home consoles. They're truly fantastic games with very refined gameplay and tons of nice details.

My adaptation is a game that can be played.

## How to play

This is a single-stick, side-scrolling shooter. The directional control moves your character and can be used to aim to a limited degree. Jump is on a separate button, along with shoot and grenade. The arcade experience is also simulated with dedicated buttons for coin insert and start.

A few different control schemes are supported:

**WASD, one-handed**

WASD to move, ctrl to shoot, space to jump, alt for grenade. 1 to insert coin, enter to start.

**WASD, two-handed**

WASD to move, j to shoot, i to jump, o for grenade. This kinda-sorta simulate a Neo Geo stick. 1 to insert coin, enter to start.

**Arrow keys**

Arrow keys to move, z to shoot, x to jump, c for grenade. 1 to insert coin, enter to start.

**XInput Controller**

Left stick to move, A to shoot, B to jump, X for grenade. This matches the Neo Geo CD controller. View/back for coin, menu/start for start.

Analogue joystick input is supported and absolutely recommended. I tested and played with an Xbox One controller.

If you add "ShowMainMenu" to CustomConfigFlags in the config.json (%AppData%/XCVG Systems/Heavy Metal Slug on Windows, %user%/XCVG Systems/Heavy Metal Slug on most other platforms) it will show the main menu.

I think you can still remap controls through the Unity launcher. Holt Alt when launching the game to make it show.

## Features and Omissions

Heavy Metal Slug was built in about a week, with my energy rapidly draining toward the end of it. Originally I was going to clone Metal Slug and all its mechanics as closely as possible. I did manage to do most of that: all of the basics like player movement, bullets, and enemies, plus a few extras like grenades, the arcade simulation stuff, and a few scripted sequences. However, there are a few big features that didn't make it in:

* The Metal Slug itself, or vehicles of any sort
* Multiple playable characters. It's partially implemented, missing half the switching logic and the actual sprites.
* Two-player couch co-op mode. Most of the logic is there but it's missing a few pieces and won't work.
* Attract mode. The attract screen is static without an actual attract sequence.
* High scores. One of these days I'll build a generalized module for this.

## Known Issues

* The "Quit Game" button in the in-game menu sends you back to the attract screen. Exit using taskbar, alt-f4, finder, killall, etc.
* Some of the buildings in the town have no roofs
* Some of the buildings can be partially clipped through. This is just mismatched colliders I think
* Control remapping is there but kinda broken and kinda useless. I don't have a good InputMapper that supports controllers yet. Once I add Rewired support to CommonCore I might backport it here.
* Gamepad mappings are probably broken on macOS and Linux. I have no way to test this, sorry.

## Acknowledgments

midimelody.ru for the sketchy 90s-esque MIDI music, as well as Sabaton, Metallica, Nightwish, Within Temptation and Arch Enemy for the original songs, and Peter Pawłowski (foobar2000) kode54 (MIDI plugin), and Alexey Khokholov (Nuked OPL emulation) for the software tools I used to convert/render the MIDIs. 

Nazca and SNK for the original Metal Slug, which are still awesome games you should check out some time. If you don't own a Neo Geo (which most of us don't, let's be honest) they've released ports to various other platforms.

Unity Technologies, Newtonsoft, and Microsoft for the underlying technologies that make this possible.

Various contributors to OpenGameArt and the Free Sound Project, of which there are too many to list here. These are listed in full in the CREDITS file and in the CommonCore documentation where applicable.




