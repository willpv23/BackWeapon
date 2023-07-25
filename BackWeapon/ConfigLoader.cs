using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Rage;
using Rage.Native;
using System.Drawing;

namespace BackWeapon
{
    class ConfigLoader
    {
        public static Dictionary<string, object> GetIniValues(bool log = true)
        {
            if (log)
            {
                Game.LogTrivial("Loading Configuration...");
            }
            Dictionary<string, object> iniValues = new Dictionary<string, object> { };
            InitializationFile ini = InitializeFile();

            string[] acceptedWeaponStrings = ini.ReadString("Main", "AcceptedWeapons", "WEAPON_CARBINERIFLE_MK2,WEAPON_SMG").Split(',');
            List<uint> acceptedWeaponHashes = new List<uint> { };
            foreach (var acceptedWeapon in acceptedWeaponStrings)
            {
                try
                {
                    acceptedWeaponHashes.Add(Game.GetHashKey(acceptedWeapon.Trim(' ')));
                }
                catch
                {
                    Game.LogTrivial($"{acceptedWeapon} is not a valid weapon");
                }
            }
            iniValues.Add("AcceptedWeapons", acceptedWeaponHashes);
            iniValues.Add("AcceptedWeaponStrings", acceptedWeaponStrings);

            Vector3 offsetPosition = ini.ReadVector3("Main", "OffsetPosition", new Vector3(0.0f, -0.19f, -0.02f));
            iniValues.Add("OffsetPosition", offsetPosition);

            Rotator Rotation = ini.ReadRotator("Main", "Rotation", new Rotator(0.0f, 165f, 0.0f));
            iniValues.Add("Rotation", Rotation);

            bool hideWhileInVehicle = ini.ReadBoolean("Main", "HideWhileInVehicle", true);
            iniValues.Add("HideWhileInVehicle", hideWhileInVehicle);

            bool disableFlashlight = ini.ReadBoolean("Main", "DisableFlashlight", false);
            iniValues.Add("DisableFlashlight", disableFlashlight);

            string deleteWeaponKeyString = ini.ReadString("Main", "DeleteWeaponKey", "Decimal");
            Keys deleteWeaponKey = (Keys)Enum.Parse(typeof(Keys), deleteWeaponKeyString);
            iniValues.Add("DeleteWeaponKey", deleteWeaponKey);

            bool enableAI = ini.ReadBoolean("AI", "EnableAI", true);
            iniValues.Add("EnableAI", enableAI);

            bool copsOnly = ini.ReadBoolean("AI", "CopsOnly", true);
            iniValues.Add("CopsOnly", copsOnly);

            string[] aiAcceptedWeaponStrings = ini.ReadString("AI", "AcceptedWeapons", "WEAPON_CARBINERIFLE_MK2,WEAPON_SMG").Split(',');
            List<uint> aiAcceptedWeaponHashes = new List<uint> { };
            foreach (var acceptedWeapon in aiAcceptedWeaponStrings)
            {
                try
                {
                    aiAcceptedWeaponHashes.Add(Game.GetHashKey(acceptedWeapon.Trim(' ')));
                }
                catch
                {
                    Game.LogTrivial($"{acceptedWeapon} is not a valid weapon");
                }
            }
            iniValues.Add("AIAcceptedWeapons", aiAcceptedWeaponHashes);
            iniValues.Add("AIAcceptedWeaponStrings", aiAcceptedWeaponStrings);


            Vector3 aiOffsetPosition = ini.ReadVector3("AI", "OffsetPosition", new Vector3(0.0f, -0.19f, -0.02f));
            iniValues.Add("AIOffsetPosition", aiOffsetPosition);

            Rotator aiRotation = ini.ReadRotator("AI", "Rotation", new Rotator(0.0f, 165f, 0.0f));
            iniValues.Add("AIRotation", aiRotation);

            bool aiHideWhileInVehicle = ini.ReadBoolean("AI", "HideWhileInVehicle", true);
            iniValues.Add("AIHideWhileInVehicle", aiHideWhileInVehicle);

            bool enableBestWeapon = ini.ReadBoolean("AI", "EnableBestWeapon");
            iniValues.Add("EnableBestWeapon", enableBestWeapon);

            string menuKeyString = ini.ReadString("Main", "MenuKey", "F5");
            Keys menuKey = (Keys)Enum.Parse(typeof(Keys), menuKeyString);
            iniValues.Add("MenuKey", menuKey);

            string[] addonComponentStrings = ini.ReadString("Advanced", "AddonComponents", "None").Split(',');
            List<uint> addonComponentHashes = new List<uint> { };
            foreach (var component in addonComponentStrings)
            {
                try
                {
                    addonComponentHashes.Add(Game.GetHashKey(component.Trim(' ')));
                }
                catch
                {
                    Game.LogTrivial($"{component} is not a valid weapon component");
                }
            }
            iniValues.Add("AddonComponents", addonComponentHashes);
            iniValues.Add("AddonComponentStrings", addonComponentStrings);

            string config = null;
            foreach (string key in iniValues.Keys.ToArray<string>())
                config += $"{key}: {iniValues[key]}; ";

            if (log)
            {
                Game.LogTrivial($"Loaded configuration: {config}");
            }

            return iniValues;
        }

        private static InitializationFile InitializeFile()
        {
            InitializationFile ini = new InitializationFile("Plugins/BackWeapon.ini");
            
            //Create missing entries
            if (!ini.DoesKeyExist("Main", "AcceptedWeapons"))
            {
                ini.Write("Main", "AcceptedWeapons", "weapon_smg, weapon_pumpshotgun, weapon_pumpshotgun_mk2, weapon_carbinerifle, weapon_carbinerifle_mk2, weapon_specialcarbine, weapon_specialcarbine_mk2");
            }
            if (!ini.DoesKeyExist("Main", "OffSetPosition"))
            {
                ini.Write("Main", "OffsetPosition", new Vector3(0.0f, -0.19f, -0.02f));
            }
            if (!ini.DoesKeyExist("Main", "Rotation"))
            {
                ini.Write("Main", "Rotation", new Rotator(0.0f, 165f, 0.0f));
            }
            if (!ini.DoesKeyExist("Main", "HideWhileInVehicle"))
            {
                ini.Write("Main", "HideWhileInVehicle", true);
            }
            if (!ini.DoesKeyExist("Main", "DisableFlashlight"))
            {
                ini.Write("Main", "DisableFlashlight", false);
            }
            if (!ini.DoesKeyExist("Main", "DeleteWeaponKey"))
            {
                ini.Write("Main", "DeleteWeaponKey", "Decimal");
            }
            if (!ini.DoesKeyExist("AI", "EnableAI"))
            {
                ini.Write("AI", "EnableAI", true);
            }
            if (!ini.DoesKeyExist("AI", "CopsOnly"))
            {
                ini.Write("AI", "CopsOnly", true);
            }
            if (!ini.DoesKeyExist("AI", "AcceptedWeapons"))
            {
                ini.Write("AI", "AcceptedWeapons", "WEAPON_CARBINERIFLE_MK2,WEAPON_SMG");
            }
            if (!ini.DoesKeyExist("AI", "OffsetPosition"))
            {
                ini.Write("AI", "OffsetPosition", new Vector3(0.0f, -0.19f, -0.02f));
            }
            if (!ini.DoesKeyExist("AI", "Rotation"))
            {
                ini.Write("AI", "Rotation", new Rotator(0.0f, 165f, 0.0f));
            }
            if (!ini.DoesKeyExist("AI", "HideWhileInVehicle"))
            {
                ini.Write("AI", "HideWhileInVehicle", true);
            }
            if (!ini.DoesKeyExist("AI", "EnableBestWeapon"))
            {
                ini.Write("AI", "EnableBestWeapon", true);
            }
            if (!ini.DoesKeyExist("Main", "MenuKey"))
            {
                ini.Write("Main", "MenuKey", "F5");
            }
            if (!ini.DoesKeyExist("Advanced", "AddonComponents"))
            {
                ini.Write("Advanced", "AddonComponents", "None");
            }
            //end

            ini.Create();
            return ini;
        }

        public static void UpdateIniFile(string sectionName, string keyName, object value)
        {
            Game.LogTrivial($"Updating ini value: [{sectionName}] {keyName} = {value.ToString()}");
            InitializationFile ini = new InitializationFile("Plugins/BackWeapon.ini");

            if (!ini.DoesSectionExist(sectionName))
            {
                Game.LogTrivial("Failed to update ini file: unknown section");
                return;
            }
            if (!ini.DoesKeyExist(sectionName, keyName))
            {
                Game.LogTrivial("Failed to update ini file: unknown key");
                return;
            }
            ini.Write(sectionName, keyName, value);
        }
    }
}
