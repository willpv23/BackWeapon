using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Rage;
using Rage.Native;

[assembly: Rage.Attributes.Plugin("BackWeapon", Description = "Plugin to show weapon on back.", Author = "willpv23", PrefersSingleInstance = true)]


namespace BackWeapon
{
    class BackWeapon
    {
        public static Dictionary<string, object> iniValues = ConfigLoader.GetIniValues();

        static List<uint> acceptedWeapons = (List<uint>) iniValues["AcceptedWeapons"];
        public static Vector3 offsetPosition = (Vector3) iniValues["OffsetPosition"];
        public static Rotator Rotation = (Rotator) iniValues["Rotation"];
        static bool hideWhileInVehicle = (bool) iniValues["HideWhileInVehicle"];
        static bool disableFlashlight = (bool) iniValues["DisableFlashlight"];
        static Keys deleteWeaponKey = (Keys) iniValues["DeleteWeaponKey"];
        static bool copsOnly = (bool) iniValues["CopsOnly"];
        static List<uint> aiAcceptedWeapons = (List<uint>) iniValues["AIAcceptedWeapons"];
        static Vector3 aiOffsetPosition = (Vector3) iniValues["AIOffsetPosition"];
        static Rotator aiRotation = (Rotator) iniValues["AIRotation"];
        static bool aiHideWhileInVehicle = (bool) iniValues["AIHideWhileInVehicle"];
        static bool enableBestWeapon = (bool) iniValues["EnableBestWeapon"];
        static List<uint> addonComponents = (List<uint>)iniValues["AddonComponents"];

        public static void PlayerLoop()
        {
            Weapon weaponOnBack = null;
            while (true)
            {
                Ped ped = Game.LocalPlayer.Character;
                weaponOnBack = Process(weaponOnBack, ped);
                weaponOnBack = CheckInventory(weaponOnBack, ped);
                if (hideWhileInVehicle)
                {
                    weaponOnBack = CheckVehicle(weaponOnBack, ped);
                }
                if (weaponOnBack != null && Game.IsKeyDown(deleteWeaponKey))
                {
                    weaponOnBack.Delete();
                    weaponOnBack = null;
                }
                GameFiber.Yield();
            }
        }

        public static void AIPedsLoop()
        {
            Dictionary<Ped, uint> allPedsCurrentWeapon = new Dictionary<Ped, uint>();
            Dictionary<Ped, Weapon> allPedsWeaponOnBack = new Dictionary<Ped, Weapon>();
            Weapon weaponOnBack = null;
            while (true)
            {
                foreach (Ped ped in World.EnumeratePeds().ToArray<Ped>())
                {
                    if (!ped || !ped.Exists())
                        continue;
                    if (!ped.IsHuman)
                        continue;
                    if (ped.IsPlayer)
                        continue;
                    if (copsOnly && ped.RelationshipGroup != RelationshipGroup.Cop)
                        continue;
                    var task = NativeFunction.Natives.GET_IS_TASK_ACTIVE<bool>(ped, 56);
                    if (!task)
                    {
                        //show the best weapon on ped's back if nothing there
                        if (enableBestWeapon)
                        {
                            uint bestWeapon = NativeFunction.Natives.GET_BEST_PED_WEAPON<uint>(ped, 0);
                            if (!allPedsWeaponOnBack.ContainsKey(ped) && !allPedsCurrentWeapon.ContainsKey(ped) && aiAcceptedWeapons.Contains((uint)bestWeapon))
                            {
                                Game.LogTrivial("Adding best weapon to ped from inventory");
                                weaponOnBack = AIProcess(bestWeapon, ped);
                                allPedsWeaponOnBack.Add(ped, weaponOnBack);
                                weaponOnBack = null;
                            }
                        }
                        var currentWeapon = ped.Inventory.EquippedWeapon;
                        var currentWeaponObject = ped.Inventory.EquippedWeaponObject;
                        if (currentWeapon != null && currentWeaponObject != null)
                        {
                            if (aiAcceptedWeapons.Contains((uint)currentWeapon.Hash) && !allPedsCurrentWeapon.ContainsKey(ped))
                            {
                                allPedsCurrentWeapon.Add(ped, (uint)currentWeapon.Hash);
                                Game.LogTrivial("Ped with accepted weapon added");
                            }
                            if (allPedsWeaponOnBack.ContainsKey(ped) && allPedsWeaponOnBack[ped].Asset.Hash == currentWeaponObject.Asset.Hash)
                            {
                                allPedsWeaponOnBack[ped].Delete();
                                allPedsWeaponOnBack.Remove(ped);
                                Game.LogTrivial("Weapon deleted because it was equipped");
                            }
                            if (allPedsCurrentWeapon.ContainsKey(ped) && currentWeaponObject.Asset.Hash != allPedsCurrentWeapon[ped])
                            {
                                if (allPedsWeaponOnBack.ContainsKey(ped))
                                {
                                    allPedsWeaponOnBack[ped].Delete();
                                    allPedsWeaponOnBack.Remove(ped);
                                    Game.LogTrivial("Weapon deleted because another weapon needs to take its place");
                                }
                                Game.LogTrivial("Creating weapon on back of ped because ped equipped new weapon");
                                weaponOnBack = AIProcess(allPedsCurrentWeapon[ped], ped);
                                allPedsWeaponOnBack.Add(ped, weaponOnBack);
                                allPedsCurrentWeapon.Remove(ped);
                                weaponOnBack = null;
                            }
                        }
                        else
                        {
                            if (allPedsCurrentWeapon.ContainsKey(ped))
                            {
                                weaponOnBack = AIProcess(allPedsCurrentWeapon[ped], ped);
                                allPedsWeaponOnBack.Add(ped, weaponOnBack);
                                allPedsCurrentWeapon.Remove(ped);
                                weaponOnBack = null;
                                Game.LogTrivial("Weapon created on back of ped because ped put weapon away");
                            }
                        }
                    }
                }
                foreach (Ped ped in allPedsCurrentWeapon.Keys.ToArray<Ped>())
                {
                    if (!ped.Exists())
                    {
                        allPedsCurrentWeapon.Remove(ped);
                        Game.LogTrivial("Ped with weapon equipped removed because it is no longer valid");
                    }
                }
                foreach (Ped ped in allPedsWeaponOnBack.Keys.ToArray<Ped>())
                {
                    if (!ped.Exists())
                    {
                        if (allPedsWeaponOnBack[ped].Exists())
                            allPedsWeaponOnBack[ped].Delete();
                        allPedsWeaponOnBack.Remove(ped);
                        Game.LogTrivial("Ped with weapon on back removed because it is no longer valid");
                        continue;
                    }
                    allPedsWeaponOnBack[ped] = CheckInventory(allPedsWeaponOnBack[ped], ped);
                    if (allPedsWeaponOnBack[ped] == null)
                    {
                        allPedsWeaponOnBack.Remove(ped);
                        Game.LogTrivial("Weapon deleted because it is not in ped's inventory");
                        continue;
                    }
                    if (aiHideWhileInVehicle)
                        allPedsWeaponOnBack[ped] = CheckVehicle(allPedsWeaponOnBack[ped], ped);
                }
                GameFiber.Sleep(100);
                GameFiber.Yield();
            }
        }

        private static Weapon AIProcess(uint weapontoadd,Ped ped)
        {
            Weapon weaponOnBack = null;
            weaponOnBack = new Weapon(weapontoadd, ped.Position, 30);
            weaponOnBack.Model.LoadAndWait();
            List<uint> aiComponentHashes = GetComponentHashes(weapontoadd, ped);
            foreach (uint hash in aiComponentHashes)
            {
                NativeFunction.Natives.GIVE_WEAPON_COMPONENT_TO_WEAPON_OBJECT(weaponOnBack, hash);
                //Game.LogTrivial($"Adding {hash} to weapon on back");
            }
            int tint = NativeFunction.Natives.GET_PED_WEAPON_TINT_INDEX<int>(ped, weapontoadd);
            NativeFunction.Natives.SET_WEAPON_OBJECT_TINT_INDEX(weaponOnBack, tint);
            weaponOnBack.AttachTo(ped, ped.GetBoneIndex(PedBoneId.Spine3), aiOffsetPosition, aiRotation);
            return weaponOnBack;
        }

        private static Weapon Process(Weapon weaponOnBack, Ped ped)
        {
            var task = NativeFunction.Natives.GET_IS_TASK_ACTIVE<bool>(ped, 56);
            if (!task)
            {
                var currentWeapon = ped.Inventory.EquippedWeapon;
                var currentWeaponObject = ped.Inventory.EquippedWeaponObject;
                if (currentWeapon != null && currentWeaponObject != null)
                {
                    if (acceptedWeapons.Contains((uint)currentWeapon.Hash))
                    {
                        if (weaponOnBack != null)
                        {
                            if (weaponOnBack.Asset.Hash == currentWeaponObject.Asset.Hash)
                            {
                                weaponOnBack.Delete();
                                weaponOnBack = null;
                            }
                        }
                        try
                        {
                            while (ped.Inventory.EquippedWeapon == currentWeapon)
                            {
                                if (weaponOnBack != null && Game.IsKeyDown(deleteWeaponKey))
                                {
                                    weaponOnBack.Delete();
                                    weaponOnBack = null;
                                }
                                GameFiber.Yield();
                            }
                        }
                        catch
                        {
                            return weaponOnBack;
                        }
                        if (weaponOnBack != null)
                        {
                            weaponOnBack.Delete();
                            weaponOnBack = null;
                        }
                        weaponOnBack = new Weapon((uint)currentWeapon.Hash, ped.Position, 30);
                        weaponOnBack.Model.LoadAndWait();
                        List<uint> componentHashes = GetComponentHashes((uint)currentWeapon.Hash, ped);
                        foreach (uint hash in componentHashes)
                        {
                            NativeFunction.Natives.GIVE_WEAPON_COMPONENT_TO_WEAPON_OBJECT(weaponOnBack, hash);
                        }
                        int tint = NativeFunction.Natives.GET_PED_WEAPON_TINT_INDEX<int>(ped, (uint)currentWeapon.Hash);
                        NativeFunction.Natives.SET_WEAPON_OBJECT_TINT_INDEX(weaponOnBack, tint);
                        weaponOnBack.AttachTo(ped, ped.GetBoneIndex(PedBoneId.Spine3), offsetPosition, Rotation);
                    }
                }
            }
            return weaponOnBack;
        }

        public static List<uint> GetComponentHashes(uint currentWeaponHash, Ped ped)
        {
            List<uint> allComponentHashes = new List<uint> { 0xD7391086, 0x9B76C72C, 0x487AAE09, 0x85A64DF9, 0x377CD377, 0xD89B9658, 0x4EAD7533, 0x4032B5E7, 0x77B8AB2F, 0x7A6A7B7B, 0x161E9241, 0xF3462F33, 0xC613F685,0xEED9FD63,0x50910C31,0x9761D9DC,0x7DECFE30,0x3F4E8AA6,0x8B808BB,0xE28BABEF,0x7AF3F785,0x9137A500,0x5B3E7DB6,0xE7939662,0xFED0FD71,0xED265A1C,0x359B7AAE,0x65EA7EBB,0xD7391086,0x721B079,0xD67B4F2D,0xC304849A,0xC6654D72,0x31C4B22A,0x249A17D5,0x9B76C72C,0x2297BE19,0xD9D3AC92,0xA73D4664,0x77B8AB2F,0x16EE3040,0x9493B80D,0xE9867CE3,0xF8802ED9,0x7B0033B3,0x8033ECAF,0xD4A969A,0x64F9C62B,0x7A6A7B7B,0xBA23D8BE,0xC6D8E476,0xEFBF25,0x10F42E8F,0xDC8BA3F,0x420FD713,0x49B2945,0x27077CCB,0xC03FED9F,0xB5DE24,0xA7FF1B8,0xF2E24289,0x11317F27,0x17C30C42,0x257927AE,0x37304B1C,0x48DAEE71,0x20ED9B5B,0xD951E867,0x1466CE6,0xCE8C0772,0x902DA26E,0xE6AD5F79,0x8D107402,0xC111EB26,0x4A4965F3,0x47DE9258,0xAA8283BF,0xF7BEEDD,0x8A612EF6,0x76FA8829,0xA93C6CAC,0x9C905354,0x4DFA3621,0x42E91FFF,0x54A8437D,0x68C2746,0x2366E467,0x441882E6,0xE7EE68EA,0x29366D21,0x3ADE514B,0xE64513E9,0xCD7AEB9A,0xFA7B27A6,0xE285CA9A,0x2B904B19,0x22C24F9C,0x8D0D5ECD,0x1F07150A,0x94F42D62,0x5ED6C128,0x25CAAEAF,0x2BBD7A3A,0x85FEA109,0x4F37DF2A,0x8ED4BB70,0x43FD595B,0x21E34793,0x5C6C749C,0x15F7A390,0x968E24DB,0x17BFA99,0xF2685C72,0xDD2231E6,0xBB43EE76,0x4D901310,0x5F31B653,0x697E19A0,0x930CB951,0xB4FC92B0,0x1A1F1260,0xE4E00B70,0x2C298B2B,0xDFB79725,0x6BD7228C,0x9DDBCF8C,0xB319A52C,0xC6836E12,0x43B1B173,0x4ABDA3FA,0x45A3B6BB,0x33BA12E8,0xD7DBF707,0xCB48AEF0,0x10E6BA2B,0x9D2FBF29,0x487AAE09,0x26574997,0x350966FB,0x79C77076,0x7BC4CDDC,0x3CC6BA57,0x27872C90,0x8D1307B0,0xBB46E417,0x278C78AF,0x84C8B2D3,0x937ED0B7,0x4C24806E,0xB9835B2E,0x7FEA36EC,0xD99222E5,0x3A1BD6FA,0xB5A715F,0x9FDB5652,0xE502AB6B,0x3DECC7DA,0xB99402D4,0xC867A07B,0xDE11CBCF,0xEC9068CC,0x2E7957A,0x347EF8AC,0x4DB62ABE,0xD9103EE1,0xA564D78B,0xC4979067,0x3815A945,0x4B4B4FB0,0xEC729200,0x48F64B22,0x35992468,0x24B782A5,0xA2E67F01,0x2218FD68,0x45C5C3C5,0x399D558F,0x476E85FF,0xB92C6979,0xA9E9CAF4,0x4317F19E,0x334A5203,0x6EB8C8DB,0xC164F53,0xAA2C45B4,0xE608B35E,0xA2D79DDB,0x85A64DF9,0x94E81BC7,0x86BD7F72,0x837445AA,0xCD940141,0x9F8A1BF5,0x4E65B425,0xE9582927,0x3BE4465D,0x3F3C8181,0xAC42DF71,0x5F7DCE4D,0xE3BD9E44,0x17148F9B,0x24D22B16,0xF2BEC6F0,0x85627D,0xDC2919C5,0xE184247B,0xD8EF9356,0xEF29BFCA,0x67AEB165,0x46411A1D,0x324F2D5F,0x971CF6FD,0x88C7DA53,0xBE5EEA16,0xB1214F9B,0xDBF0A53D,0x4EAD7533,0x9FBE33EC,0x91109691,0xBA62E935,0xA0D89C42,0xD89B9658,0xFA8FA10F,0x8EC1C979,0x377CD377,0xC6C7E581,0x7C8BD10E,0x6B59AEAA,0x730154F2,0xC5A12F80,0xB3688B0F,0xA857BC78,0x18929DA,0xEFB00628,0x822060A9,0xA99CF95A,0xFAA7F5ED,0x43621710,0xC7ADD105,0x659AC11B,0x3BF26DC7,0x9D65907A,0xAE4055B7,0xB905ED6B,0xA6C448E8,0x9486246C,0x8A390FD2,0x2337FC5,0xEFFFDB5E,0xDDBDB6DA,0xCB631225,0xA87D541E,0xC5E9AE52,0x16C69281,0xDE1FA12C,0x8765C68A,0xDE011286,0x51351635,0x503DEA90,0xC66B6542,0xE73653A9,0xF97F783B,0xD40BB53B,0x431B238B,0x34CF86F4,0xB4C306DD,0xEE677A25,0xDF90DC78,0xA4C31EE,0x89CFB0F7,0x7B82145C,0x899CAF75,0x5218C819,0x8610343F,0xD12ACA6F,0xEF2C78C1,0xFB70D853,0xA7DD1E58,0x63E0A098,0x43A49D26,0x5646C26A,0x911B24AF,0x37E5444B,0x538B7B97,0x25789F72,0xC5495F2D,0xCF8B73B1,0xA9BB2811,0xFC674D54,0x7C7FCD9B,0xA5C38392,0xB9B15DB0,0x4C7A391E,0x5DD5DBD5,0x1757F566,0x3D25C2A7,0x255D5D57,0x44032F11,0x833637FF,0x8B3C480B,0x4BDD6F16,0x406A7908,0x2F3856A4,0xE50C424D,0xD37D1F2F,0x86268483,0xF420E076,0xAAE14DF8,0x9893A95D,0x6B13CD3E,0xDA55CD3F,0x513F0A63,0x59FF9BF8,0xC607740E,0xF434EF84,0x82158B47,0x3C00AFED,0xD6DABABE,0xE1FFB34A,0xD6C59CD6,0x92FECCDD,0x492B257C,0x17DF42E9,0xF6649745,0xC326BDBA,0x29882423,0x57EF1CC8,0xC34EF234,0xB5E2575B,0x4A768CB5,0xCCE06BBD,0xBE94CF26,0x7609BE11,0x48AF6351,0x9186750A,0x84555AA8,0x1B4C088B,0xE046DFC,0x28B536E,0xD703C94D,0x1CE5A6A5,0xEAC8C270,0x9BC64089,0xD2443DDC,0xBC54DA77,0x4032B5E7,0x476F52F4,0x94E12DCE,0xE6CFD1AA,0xD77A22D2,0x6DD7A86E,0xF46FD079,0xE14A9ED3,0x5B1C713C,0x381B5D89,0x68373DDC,0x9094FBA0,0x7320F4B2,0x60CF500F,0xFE668B3F,0xF3757559,0x193B40E8,0x107D2F6C,0xC4E91841,0x9BB1C5D3,0x3B61040B,0xB7A316DA,0xFA1E1A28,0x2CD8FF9D,0xEC0F617,0xF835D6D4,0x3BE948F6,0x89EBDAA7,0x82C10383,0xB68010B0,0x2E43DA41,0x6927E1A1,0x909630B7,0x108AB09E,0xF8337D02,0xC5BEDD65,0xE9712475,0x13AA78E7,0x26591E50,0x302731EC,0xAC722A78,0xBEA4CEDD,0xCD776C82,0xABC5ACC7,0x6C32D2EB,0xD83B4141,0xCCFD2AC5,0x1C221B1A,0x161E9241,0x11AE5C97 };
            allComponentHashes.AddRange(addonComponents);
            List<uint> componentHashes = new List<uint> { };
            if (disableFlashlight)
            {
                List<uint> flashlightHashes = new List<uint> { 0x359B7AAE, 0x4A4965F3, 0x43FD595B, 0x7BC4CDDC };
                foreach (uint hash in flashlightHashes)
                {
                    allComponentHashes.Remove(hash);
                }
            }
            foreach (uint hash in allComponentHashes)
            {
                if (NativeFunction.Natives.HAS_PED_GOT_WEAPON_COMPONENT<bool>(ped, currentWeaponHash, hash))
                {
                    componentHashes.Add(hash);
                }
            }
            return componentHashes;
        }

        private static Weapon CheckInventory(Weapon weaponOnBack, Ped ped)
        {
            if (weaponOnBack != null)
            {
                if (!ped.Inventory.Weapons.Contains(weaponOnBack.Asset.Hash))
                {
                    weaponOnBack.Delete();
                    weaponOnBack = null;
                }
            }
            return weaponOnBack;
        }

        private static Weapon CheckVehicle(Weapon weaponOnBack, Ped ped)
        {
            if (weaponOnBack != null)
            {
                if (ped.CurrentVehicle != null && weaponOnBack.IsVisible)
                {
                    NativeFunction.Natives.SET_ENTITY_VISIBLE(weaponOnBack, false, 0);
                    //Game.Log("Weapon set invisible because car")
                }
                if (ped.CurrentVehicle == null && !weaponOnBack.IsVisible)
                {
                    NativeFunction.Natives.SET_ENTITY_VISIBLE(weaponOnBack, true, 0);
                    //Game.Log("Weapon set visible because no car")
                }
            }
            return weaponOnBack;
        }

        public static void UpdateConfig()
        {
            iniValues = ConfigLoader.GetIniValues();

            acceptedWeapons = (List<uint>)iniValues["AcceptedWeapons"];
            offsetPosition = (Vector3)iniValues["OffsetPosition"];
            Rotation = (Rotator)iniValues["Rotation"];
            hideWhileInVehicle = (bool)iniValues["HideWhileInVehicle"];
            disableFlashlight = (bool)iniValues["DisableFlashlight"];
            deleteWeaponKey = (Keys)iniValues["DeleteWeaponKey"];
            copsOnly = (bool)iniValues["CopsOnly"];
            aiAcceptedWeapons = (List<uint>)iniValues["AIAcceptedWeapons"];
            aiOffsetPosition = (Vector3)iniValues["AIOffsetPosition"];
            aiRotation = (Rotator)iniValues["AIRotation"];
            aiHideWhileInVehicle = (bool)iniValues["AIHideWhileInVehicle"];
            enableBestWeapon = (bool)iniValues["EnableBestWeapon"];
        }
    }
}

