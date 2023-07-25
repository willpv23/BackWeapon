# BackWeapon

-----

VERSION 3.0.0

Adjusting your settings is now easier than ever!

Added a new menu (default key: F5) using RAGENativeUI that will allow you to change most of the configuration settings from within the game and update them "live." This includes setting the position of the weapon, which will display a placeholder weapon in order to show where your weapons will appear. All of the other settings can be changed by this menu except the accepted weapons list, which must be manually edited within the BackWeapon.ini file. The only setting that requires the plugin to be reloaded is EnableAI. Changing this in game will not have an effect until the plugin is reloaded.

Settings can also be changed manually in the .ini file and reloaded via the Reload ini File menu option. The .ini file is no longer provided with the download and is instead generated if it does not exist on first load of the plugin. If it does exist, the plugin will check for missing values and add them (in this case, the menu key is the only new option that will be added for everyone, defaulting to F5). This means you can install this new update without ruining your current settings.

~RPH NOTE: Having more than 64 weapons in your inventory will cause your game to crash due to an internal issue with RPH. This is known and a fix will be released with the next version of RPH.~

UPDATE NOTE: The menu has not undergone super extensive testing so if you have any issues with it please let me know. Version 2.0.5 is still available for download if the update doesn't work for you.

GTA V NOTE: Sometimes the GTA V folder gets set as read-only. This may prevent the plugin from writing to the ini file which would likely result in a crash. Please ensure your GTA V folder is not set to read-only before using this update.

Miner update version 3.1.0 adds a new ini option that will be added when you first load this version. It is called AddonComponents and is to be used if you have any addon weapon components (all default weapon components are already accounted for), so that they show up on the stowed weapon. You add them using the name (ie, COMPONENT_AT_AR_FLSH) and separate multiple values with a comma.

-----

This simple mod will stow your last used weapon on your back. Accepted weapons can be defined in the ini file, as well as the offset position and rotation. Version 2 adds optional support for AI peds, see below for configuration details. 


## Default ini values and explanation

```
[Main]
DeleteWeaponKey = Decimal  //The key for deleting the weapon off the player's back.
HideWhileInVehicle = true  //If true, the weapon will be invisible while the player is in a vehicle.
DisableFlashlight = false  //If true, the weapon will not show the flashlight component on the back of both the player and AI.
AcceptedWeapons = weapon_smg, weapon_pumpshotgun, weapon_pumpshotgun_mk2, weapon_carbinerifle, weapon_carbinerifle_mk2, weapon_specialcarbine, weapon_specialcarbine_mk2  //List of weapons that will show on the player's back.
OffsetPosition = 0.0,-0.17,-0.02  //Position of the weapon relative to the player's Spine3 bone.
Rotation = 0.0,165,0.0  //Rotation of the weapon for the player.
MenuKey = F5 //The key for opening the menu added with version 3.0.0

[AI]
EnableAI = true  //If true, enables support for the AI.
EnableBestWeapon = true  //If true, the best weapon in the ped's inventory will show on their back if they have not yet equipped an accepted weapon.
CopsOnly = false //If true, will only show weapons on the back of cops. Otherwise, will work on all peds in the game.
HideWhileInVehicle = true  //If true, the weapon will be invisible while the ped is in a vehicle.
AcceptedWeapons = weapon_smg, weapon_pumpshotgun, weapon_pumpshotgun_mk2, weapon_carbinerifle, weapon_carbinerifle_mk2, weapon_specialcarbine, weapon_specialcarbine_mk2  //List of weapons that will show on the peds' backs.
OffsetPosition = 0.0,-0.17,-0.02  //Position of the weapon relative to the ped's Spine3 bone.
Rotation = 0.0,165,0.0  //Rotation of the weapon for AI.
List of weapon names (you must use the "weapon_blahblah" format): https://wiki.rage.mp/index.php?title=Weapons

[Advanced]
AddonComponents = None //Here you can add any addon weapon components you have installed so that they will show up on the stowed weapon. You need to use the name of component, ie COMPONENT_AT_AR_FLSH. Multiple entries should be separated by a comma.
```

## Features

- Accepted weapons will show on your back
  - You must first equip the weapon, then switch to another or holster it
  - If you switch to another accepted weapon, that will replace current stowed weapon
- Stowed weapon will show all attachments and tint
  - Option to disable the flashlight since it activates when you use the flashlight on your equipped weapon
- Customizable accepted weapons, offset position, and rotation
- Option to hide weapon on back while in a vehicle (will be restored when you exit the vehicle)
- Key to delete the weapon from your back on the fly, default Decimal key (the one on your numpad) (keys reference - https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.keys?view=netframework-4.8)
- Customizable support for AI peds
- Menu for changing settings live
 

## Installation

- Installation is as simply as dragging the contents of the zip file to your GTA V folder.
- BackWeapon.dll and BackWeapon.pdb will end up in the plugins folder and RAGENativeUI.dll is in the main directory.
- Load with RPH. 

I know there already is a mod like this out there, but I wanted something that uses RPH and is customizable. So here is that.
