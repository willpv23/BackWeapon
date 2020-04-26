# BackWeapon

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

[AI]
EnableAI = true  //If true, enables support for the AI.
EnableBestWeapon = true  //If true, the best weapon in the ped's inventory will show on their back if they have not yet equipped an accepted weapon.
CopsOnly = false //If true, will only show weapons on the back of cops. Otherwise, will work on all peds in the game.
HideWhileInVehicle = true  //If true, the weapon will be invisible while the ped is in a vehicle.
AcceptedWeapons = weapon_smg, weapon_pumpshotgun, weapon_pumpshotgun_mk2, weapon_carbinerifle, weapon_carbinerifle_mk2, weapon_specialcarbine, weapon_specialcarbine_mk2  //List of weapons that will show on the peds' backs.
OffsetPosition = 0.0,-0.17,-0.02  //Position of the weapon relative to the ped's Spine3 bone.
Rotation = 0.0,165,0.0  //Rotation of the weapon for AI.
List of weapon names (you must use the "weapon_blahblah" format): https://wiki.rage.mp/index.php?title=Weapons
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
 

## Installation

- Extract BackWeapon.dll and BackWeapon.ini to your Plugins folder
- Load with RPH
 

I know there already is a mod like this out there, but I wanted something that uses RPH and is customizable. So here is that.
