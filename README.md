# FireDevil
Mod for Cult of the Lamb

Index
-----------
* [Disclaimers](#disclaimers)
* [Installation](#installation)
* [Content](#content)
* [FAQ](#faq)
* [Build](#build)

Disclaimers
-----------
* I do not take any responsibility for broken saves or any other damage. Use this software at your own risk.
* Please DON'T REPORT BUGS you encounter to Massive Monster while mods are active.
* BE AWARE that all mods are WIP and may fail.

Installation
-----------
* Download [Unity Mod Manager](https://www.nexusmods.com/site/mods/21).
* Unpack the manager anywhere.
* Download a [release](https://github.com/Truinto/CultOfTheLamb-Truinto/releases).
* Copy "UnityModManagerConfigLocal.xml" from my mod's zip into the manager's folder.
* Start Unity Mod Manager and select the game Cult Of The Lamb. Select your game folder (for me "GOG Galaxy\Games\Cult of the Lamb").
* Click install.
* Switch to the mod tab and drop the zip file into the manager.
* Have fun.

Content
-----------
* Enables murder interaction regardless of doctrine. You can probably break the introduction with this.
* Lumber and stone have infinite resources
* Display job description in follower thoughts
* Menu to toggle doctines
* Menu to set probability of certain weapons to show up
* Option to disable most fleece penalties
* Increase shrine/outhouse/silo storage
* Option to have follower loyalty overflow to next level
* Option to instantly pickup mine resources
* Change follower skin
* Add/remove follower traits

FAQ
-----------
Q: My controller can't go right? \
A: If you press F10 it opens a report box. This prevents right movement for some reason. Press F10 again to fix it. If you need to mod's menu a lot, I recommend setting a different hotkey (like F11).

Build
-----------
* Clone repo
* (Re-)install nuget AssemblyPublicizer by vikigenius
* Rename Directory.Build.props.default to Directory.Build.props.user
* Open and edit Directory.Build.props.user with your game location