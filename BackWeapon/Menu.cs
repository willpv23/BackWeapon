using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RAGENativeUI;
using RAGENativeUI.Elements;
using Rage;
using Rage.Native;
using System.Windows.Forms;

namespace BackWeapon
{
    class Menu
    {
        private static GameFiber MenusProcessFiber;
        private static UIMenu mainMenu;
        private static UIMenu positionConfirm;
        private static UIMenu acceptedWeaponMenu;
        private static UIMenu aiMenu;
        private static UIMenu aiAcceptedWeaponMenu;

        private static UIMenuItem changePosition;
        private static UIMenuItem acceptPosition;
        private static UIMenuItem declinePosition;
        private static UIMenuItem changeDeleteWeaponKey;
        private static UIMenuItem changeMenuKey;
        private static UIMenuCheckboxItem changeHideInVehicle;
        private static UIMenuCheckboxItem changeDisableFlashlight;
        private static UIMenuItem acceptedWeaponMenuItem;
        private static List<UIMenuItem> acceptedWeaponItems = new List<UIMenuItem>();
        private static UIMenuItem aiMenuItem;
        private static UIMenuCheckboxItem changeEnableAI;
        private static UIMenuCheckboxItem changeEnableBestWeapon;
        private static UIMenuCheckboxItem changeCopsOnly;
        private static UIMenuCheckboxItem changeAIHideInVehicle;
        private static UIMenuItem changeAIPosition;
        private static UIMenuItem aiAcceptedWeaponMenuItem;
        private static List<UIMenuItem> aiAcceptedWeaponItems = new List<UIMenuItem>();
        private static UIMenuItem reloadIni;


        private static MenuPool menuPool;
        private static Keys menuKey = (Keys)BackWeapon.iniValues["MenuKey"];
        private static Keys currentDeleteWeaponKey = (Keys)BackWeapon.iniValues["DeleteWeaponKey"];
        private static bool hideWhileInVehicle = (bool)BackWeapon.iniValues["HideWhileInVehicle"];
        private static bool disableFlashlight = (bool)BackWeapon.iniValues["DisableFlashlight"];
        private static string[] acceptedWeapons = (string[])BackWeapon.iniValues["AcceptedWeaponStrings"];
        private static bool enableAI = (bool)BackWeapon.iniValues["EnableAI"];
        private static bool enableBestWeapon = (bool)BackWeapon.iniValues["EnableBestWeapon"];
        private static bool copsOnly = (bool)BackWeapon.iniValues["CopsOnly"];
        private static bool aiHideWhileInVehicle = (bool)BackWeapon.iniValues["AIHideWhileInVehicle"];
        private static string[] aiAcceptedWeapons = (string[])BackWeapon.iniValues["AIAcceptedWeaponStrings"];
        private static Vector3 currentPosition;
        private static Rotator currentRotation;

        public static void LoadMenu()
        {
            Game.LogTrivial("Menus Loading...");
            MenusProcessFiber = new GameFiber(ProcessLoop);
            menuPool = new MenuPool();
            mainMenu = new UIMenu("Stow That Weapon", "Main Menu");
            positionConfirm = new UIMenu("Stow That Weapon", "Keep settings?");
            acceptedWeaponMenu = new UIMenu("Stow That Weapon", "Accepted Weapons");
            aiMenu = new UIMenu("Stow That Weapon", "AI Settings");
            aiAcceptedWeaponMenu = new UIMenu("Stow That Weapon", "AI Accepted Weapons");



            // Add our main menu to the MenuPool
            menuPool.Add(mainMenu);
            menuPool.Add(positionConfirm);
            menuPool.Add(acceptedWeaponMenu);
            menuPool.Add(aiMenu);
            menuPool.Add(aiAcceptedWeaponMenu);

            //Create menu items
            changePosition = new UIMenuItem("Change Weapon Position");
            changeDeleteWeaponKey = new UIMenuItem("Change Delete Weapon Key", $"Current key: {currentDeleteWeaponKey}");
            changeMenuKey = new UIMenuItem("Change Menu Key", $"Current key: {menuKey}");
            changeHideInVehicle = new UIMenuCheckboxItem("Hide While In Vehicle", hideWhileInVehicle, "If checked, the stowed weapon will not appear when in a vehicle.");
            changeDisableFlashlight = new UIMenuCheckboxItem("Disable Flashlight", disableFlashlight, "If checked, the flashlight will not be added to the stowed weapon.");
            acceptedWeaponMenuItem = new UIMenuItem("Accepted Weapons", "View a list of accepted weapons. These are edited manually in the ini file.");
            aiMenuItem = new UIMenuItem("AI Settings");
            reloadIni = new UIMenuItem("Reload ini File", "Reloads the ini file for changes made outside this menu. Most settings will update without needing to reload the plugin (except EnableAI).");
            mainMenu.AddItem(changePosition);
            mainMenu.AddItem(changeDeleteWeaponKey);
            mainMenu.AddItem(changeMenuKey);
            mainMenu.AddItem(changeHideInVehicle);
            mainMenu.AddItem(changeDisableFlashlight);
            mainMenu.AddItem(acceptedWeaponMenuItem);
            mainMenu.AddItem(reloadIni);
            mainMenu.AddItem(aiMenuItem);

            acceptPosition = new UIMenuItem("Save & Reload ini");
            declinePosition = new UIMenuItem("Cancel");
            positionConfirm.AddItem(acceptPosition);
            positionConfirm.AddItem(declinePosition);

            foreach (string weapon in acceptedWeapons)
            {
                acceptedWeaponItems.Add(new UIMenuItem(weapon));
            }
            acceptedWeaponMenu.AddItems(acceptedWeaponItems);

            changeEnableAI = new UIMenuCheckboxItem("Enable AI", enableAI, "Change requires reloading plugin to take effect.");
            changeEnableBestWeapon = new UIMenuCheckboxItem("Enable Best Weapon", enableBestWeapon, "Show ped's best weapon in their inventory.");
            changeCopsOnly = new UIMenuCheckboxItem("Cops Only", copsOnly, "Only show on AI cops, if disabled shows on all peds.");
            changeAIHideInVehicle = new UIMenuCheckboxItem("Hide While In Vehicle", aiHideWhileInVehicle, "If checked, the stowed weapon will not appear when the ped is in a vehicle.");
            changeAIPosition = new UIMenuItem("Copy Player Position", "Copies the AI weapon placement from the player settings.");
            aiAcceptedWeaponMenuItem = new UIMenuItem("AI Accepted Weapons", "View a list of AI accepted weapons. These are edited manually in the ini file.");
            aiMenu.AddItem(changeEnableAI);
            aiMenu.AddItem(changeEnableBestWeapon);
            aiMenu.AddItem(changeCopsOnly);
            aiMenu.AddItem(changeAIHideInVehicle);
            aiMenu.AddItem(changeAIPosition);
            aiMenu.AddItem(aiAcceptedWeaponMenuItem);

            foreach (string weapon in aiAcceptedWeapons)
            {
                aiAcceptedWeaponItems.Add(new UIMenuItem(weapon));
            }
            aiAcceptedWeaponMenu.AddItems(aiAcceptedWeaponItems);


            mainMenu.RefreshIndex();

            mainMenu.OnItemSelect += OnItemSelect;
            mainMenu.OnIndexChange += OnItemChange;
            positionConfirm.OnItemSelect += OnItemSelect;
            positionConfirm.OnIndexChange += OnItemChange;
            aiMenu.OnItemSelect += OnItemSelect;
            aiMenu.OnIndexChange += OnItemChange;

            MenusProcessFiber.Start();
            GameFiber.Hibernate();
        }

        public static void OnItemChange(UIMenu sender, int index)
        {
            sender.MenuItems[index].LeftBadge = UIMenuItem.BadgeStyle.None;
        }

        public static void OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (sender != mainMenu && sender != positionConfirm && sender != acceptedWeaponMenu && sender != aiMenu) return; // We only want to detect changes from our menu.

            if (selectedItem == changePosition)
            {
                GameFiber.StartNew(delegate
                {
                    currentPosition = BackWeapon.offsetPosition;
                    currentRotation = BackWeapon.Rotation;
                    float xaxis = currentPosition.X;
                    float yaxis = currentPosition.Y;
                    float zaxis = currentPosition.Z;
                    float pitch = currentRotation.Pitch;
                    float yaw = currentRotation.Yaw;
                    float roll = currentRotation.Roll;

                    Ped ped = Game.LocalPlayer.Character;
                    Weapon sampleWeapon = null;
                    sampleWeapon = new Weapon("WEAPON_CARBINERIFLE_MK2", ped.Position, 30);
                    sampleWeapon.Model.LoadAndWait();
                    sampleWeapon.AttachTo(ped, ped.GetBoneIndex(PedBoneId.Spine3), currentPosition, currentRotation);

                    sender.Close();
                    Game.DisplayHelp($"Use the arrow keys and NumPad to adjust the position.~n~Press ~INPUT_FRONTEND_RDOWN~ to exit.", true);
                    int count = 0;
                    while (true)
                    {
                        GameFiber.Yield();
                        Game.DisableControlAction(32, GameControl.Phone, true);
                        if (Game.IsKeyDown(Keys.Down))
                        {
                            xaxis -= .01f;
                        }
                        if (Game.IsKeyDown(Keys.Up))
                        {
                            xaxis += .01f;
                        }
                        if (Game.IsKeyDown(Keys.NumPad8))
                        {
                            yaxis += .01f;
                        }
                        if (Game.IsKeyDown(Keys.NumPad2))
                        {
                            yaxis -= .01f;
                        }
                        if (Game.IsKeyDown(Keys.Left))
                        {
                            zaxis -= .01f;
                        }
                        if (Game.IsKeyDown(Keys.Right))
                        {
                            zaxis += .01f;
                        }
                        if (Game.IsKeyDown(Keys.NumPad4))
                        {
                            roll -= 1f;
                        }

                        if (Game.IsKeyDown(Keys.NumPad6))
                        {
                            roll += 1f;
                        }

                        if (Game.IsKeyDown(Keys.NumPad7))
                        {
                            yaw -= 1f;
                        }

                        if (Game.IsKeyDown(Keys.NumPad9))
                        {
                            yaw += 1f;
                        }
                        if (Game.IsKeyDown(Keys.NumPad1))
                        {
                            pitch -= 1f;
                        }

                        if (Game.IsKeyDown(Keys.NumPad3))
                        {
                            pitch += 1f;
                        }

                        if (Game.IsKeyDownRightNow(Keys.Down))
                        {
                            count += 1;
                            if (count >= 30)
                            {
                                xaxis -= .01f;
                            }
                        }
                        else if (Game.IsKeyDownRightNow(Keys.Up))
                        {
                            count += 1;
                            if (count >= 30)
                            {
                                xaxis += .01f;
                            }
                        }
                        else if (Game.IsKeyDownRightNow(Keys.NumPad8))
                        {
                            count += 1;
                            if (count >= 30)
                            {
                                yaxis += .01f;
                            }
                        }
                        else if (Game.IsKeyDownRightNow(Keys.NumPad2))
                        {
                            count += 1;
                            if (count >= 30)
                            {
                                yaxis -= .01f;
                            }
                        }
                        else if (Game.IsKeyDownRightNow(Keys.Left))
                        {
                            count += 1;
                            if (count >= 30)
                            {
                                zaxis -= .01f;
                            }
                        }
                        else if (Game.IsKeyDownRightNow(Keys.Right))
                        {
                            count += 1;
                            if (count >= 30)
                            {
                                zaxis += .01f;
                            }
                        }
                        else if (Game.IsKeyDownRightNow(Keys.NumPad4))
                        {
                            count += 1;
                            if (count >= 30)
                            {
                                roll -= 1f;
                            }
                        }
                        else if (Game.IsKeyDownRightNow(Keys.NumPad6))
                        {
                            count += 1;
                            if (count >= 30)
                            {
                                roll += 1f;
                            }
                        }
                        else if (Game.IsKeyDownRightNow(Keys.NumPad7))
                        {
                            count += 1;
                            if (count >= 30)
                            {
                                yaw -= 1f;
                            }
                        }
                        else if (Game.IsKeyDownRightNow(Keys.NumPad9))
                        {
                            count += 1;
                            if (count >= 30)
                            {
                                yaw += 1f;
                            }
                        }
                        else if (Game.IsKeyDownRightNow(Keys.NumPad1))
                        {
                            count += 1;
                            if (count >= 30)
                            {
                                pitch -= 1f;
                            }
                        }
                        else if (Game.IsKeyDownRightNow(Keys.NumPad3))
                        {
                            count += 1;
                            if (count >= 30)
                            {
                                pitch += 1f;
                            }
                        }
                        else
                        {
                            count = 0;
                        }

                        if (Game.IsKeyDown(Keys.Enter))
                        {
                            break;
                        }
                        currentPosition = new Vector3(xaxis, yaxis, zaxis);
                        currentRotation = new Rotator(pitch, roll, yaw);
                        sampleWeapon.AttachTo(ped, ped.GetBoneIndex(PedBoneId.Spine3), currentPosition, currentRotation);
                        Game.DisplaySubtitle($"X: {xaxis} Y: {yaxis} Z: {zaxis}~n~Pitch: {pitch} Roll: {roll} Yaw {yaw}");
                    }
                    sampleWeapon.Delete();
                    Game.HideHelp();
                    positionConfirm.Visible = true;
                    //go to new menu to accept or decline
                });
            }

            if (selectedItem == acceptPosition)
            {
                GameFiber.StartNew(delegate
                {
                    //send settings to config, save to file, reload ini
                    ConfigLoader.UpdateIniFile("Main", "OffsetPosition", currentPosition);
                    ConfigLoader.UpdateIniFile("Main", "Rotation", currentRotation);
                    BackWeapon.UpdateConfig();
                    Game.DisplayNotification("Player weapon position updated.");
                    sender.Close();
                    mainMenu.Visible = true;
                });
            }

            if (selectedItem == declinePosition)
            {
                GameFiber.StartNew(delegate
                {
                    //clear settings and close menu
                    sender.Close();
                    mainMenu.Visible = true;
                });
            }

            if (selectedItem == changeDeleteWeaponKey)
            {
                GameFiber.StartNew(delegate
                {
                    sender.Visible = !sender.Visible;
                    Game.DisplayHelp($"Current binding: {currentDeleteWeaponKey}~n~Press the new key now to rebind or ~INPUT_FRONTEND_RRIGHT~ to cancel.", true);
                    while (true)
                    {
                        GameFiber.Yield();
                        if (Game.IsKeyDown(Keys.Back))
                        {
                            Game.HideHelp();
                            sender.Visible = !sender.Visible;
                            sender.CurrentSelection = 1;
                            return;
                        }
                        KeyboardState keyboardState = Game.GetKeyboardState();
                        if (keyboardState.PressedKeys.Count() > 0)
                        {
                            currentDeleteWeaponKey = keyboardState.PressedKeys.First();
                            break;
                        }
                    }
                    Game.HideHelp();
                    ConfigLoader.UpdateIniFile("Main", "DeleteWeaponKey", currentDeleteWeaponKey);
                    BackWeapon.UpdateConfig();
                    Game.DisplayNotification($"Delete Weapon Key changed to {currentDeleteWeaponKey}");
                    changeDeleteWeaponKey.Description = $"Current key: {currentDeleteWeaponKey}";
                    mainMenu.Visible = true;
                    mainMenu.CurrentSelection = 1;
                });
            }

            if (selectedItem == changeMenuKey)
            {
                GameFiber.StartNew(delegate
                {
                    sender.Visible = !sender.Visible;
                    Game.DisplayHelp($"Current binding: {menuKey}~n~Press the new key now to rebind or ~INPUT_FRONTEND_RRIGHT~ to cancel.", true);
                    while (true)
                    {
                        GameFiber.Yield();
                        if (Game.IsKeyDown(Keys.Back))
                        {
                            Game.HideHelp();
                            sender.Visible = !sender.Visible;
                            sender.CurrentSelection = 2;
                            return;
                        }
                        KeyboardState keyboardState = Game.GetKeyboardState();
                        if (keyboardState.PressedKeys.Count() > 0)
                        {
                            menuKey = keyboardState.PressedKeys.First();
                            break;
                        }
                    }
                    Game.HideHelp();
                    ConfigLoader.UpdateIniFile("Main", "MenuKey", menuKey);
                    BackWeapon.UpdateConfig();
                    Game.DisplayNotification($"Menu Key changed to {menuKey}");
                    changeMenuKey.Description = $"Current key: {menuKey}";
                    mainMenu.Visible = true;
                    mainMenu.CurrentSelection = 2;
                });
            }

            if (selectedItem == changeHideInVehicle)
            {
                hideWhileInVehicle = !hideWhileInVehicle;
                ConfigLoader.UpdateIniFile("Main", "HideWhileInVehicle", hideWhileInVehicle);
                BackWeapon.UpdateConfig();
                mainMenu.RefreshIndex();
                mainMenu.CurrentSelection = 3;
            }

            if (selectedItem == changeDisableFlashlight)
            {
                disableFlashlight = !disableFlashlight;
                ConfigLoader.UpdateIniFile("Main", "DisableFlashlight", disableFlashlight);
                BackWeapon.UpdateConfig();
                mainMenu.RefreshIndex();
                mainMenu.CurrentSelection = 4;
            }

            if (selectedItem == acceptedWeaponMenuItem)
            {
                GameFiber.StartNew(delegate
                {
                    sender.Close();
                    acceptedWeaponMenu.Visible = !acceptedWeaponMenu.Visible;
                    while (true)
                    {
                        GameFiber.Yield();
                        if (Game.IsKeyDown(Keys.Back))
                        {
                            acceptedWeaponMenu.Close();
                            mainMenu.Visible = !mainMenu.Visible;
                            mainMenu.CurrentSelection = 5;
                            return;
                        }
                    }
                });
            }

            if (selectedItem == reloadIni)
            {
                BackWeapon.UpdateConfig();
                menuKey = (Keys)BackWeapon.iniValues["MenuKey"];
                changeMenuKey.Description = $"Current key: {menuKey}";

                currentDeleteWeaponKey = (Keys)BackWeapon.iniValues["DeleteWeaponKey"];
                changeDeleteWeaponKey.Description = $"Current key: {currentDeleteWeaponKey}";

                hideWhileInVehicle = (bool)BackWeapon.iniValues["HideWhileInVehicle"];
                changeHideInVehicle.Checked = hideWhileInVehicle;

                disableFlashlight = (bool)BackWeapon.iniValues["DisableFlashlight"];
                changeDisableFlashlight.Checked = disableFlashlight;

                acceptedWeapons = (string[])BackWeapon.iniValues["AcceptedWeaponStrings"];
                acceptedWeaponItems.Clear();
                acceptedWeaponMenu.Clear();
                foreach (string weapon in acceptedWeapons)
                {
                    acceptedWeaponItems.Add(new UIMenuItem(weapon));
                }
                acceptedWeaponMenu.AddItems(acceptedWeaponItems);

                enableAI = (bool)BackWeapon.iniValues["EnableAI"];
                changeEnableAI.Checked = enableAI;

                enableBestWeapon = (bool)BackWeapon.iniValues["EnableBestWeapon"];
                changeEnableBestWeapon.Checked = enableBestWeapon;

                copsOnly = (bool)BackWeapon.iniValues["CopsOnly"];
                changeCopsOnly.Checked = copsOnly;

                aiHideWhileInVehicle = (bool)BackWeapon.iniValues["AIHideWhileInVehicle"];
                changeAIHideInVehicle.Checked = aiHideWhileInVehicle;

                aiAcceptedWeapons = (string[])BackWeapon.iniValues["AIAcceptedWeaponStrings"];
                aiAcceptedWeaponItems.Clear();
                aiAcceptedWeaponMenu.Clear();
                foreach (string weapon in aiAcceptedWeapons)
                {
                    aiAcceptedWeaponItems.Add(new UIMenuItem(weapon));
                }
                aiAcceptedWeaponMenu.AddItems(aiAcceptedWeaponItems);


                Game.DisplayNotification("Configuration reloaded.");
                mainMenu.RefreshIndex();
                mainMenu.CurrentSelection = 6;



            }

            if (selectedItem == aiMenuItem)
            {
                GameFiber.StartNew(delegate
                {
                    sender.Close();
                    aiMenu.Visible = !aiMenu.Visible;
                    while (true)
                    {
                        GameFiber.Yield();
                        if (Game.IsKeyDown(Keys.Back) && aiMenu.Visible)
                        {
                            aiMenu.Close();
                            mainMenu.Visible = !mainMenu.Visible;
                            mainMenu.CurrentSelection = 7;
                            return;
                        }
                    }
                });
            }

            if (selectedItem == changeEnableAI)
            {
                enableAI = !enableAI;
                ConfigLoader.UpdateIniFile("AI", "EnableAI", enableAI);
                BackWeapon.UpdateConfig();
                aiMenu.RefreshIndex();
                aiMenu.CurrentSelection = 0;
            }

            if (selectedItem == changeEnableBestWeapon)
            {
                enableBestWeapon = !enableBestWeapon;
                ConfigLoader.UpdateIniFile("AI", "EnableBestWeapon", enableBestWeapon);
                BackWeapon.UpdateConfig();
                aiMenu.RefreshIndex();
                aiMenu.CurrentSelection = 1;
            }

            if (selectedItem == changeCopsOnly)
            {
                copsOnly = !copsOnly;
                ConfigLoader.UpdateIniFile("AI", "CopsOnly", copsOnly);
                BackWeapon.UpdateConfig();
                aiMenu.RefreshIndex();
                aiMenu.CurrentSelection = 2;
            }

            if (selectedItem == changeAIHideInVehicle)
            {
                aiHideWhileInVehicle = !aiHideWhileInVehicle;
                ConfigLoader.UpdateIniFile("AI", "HideWhileInVehicle", aiHideWhileInVehicle);
                BackWeapon.UpdateConfig();
                aiMenu.RefreshIndex();
                aiMenu.CurrentSelection = 3;
            }

            if (selectedItem == changeAIPosition)
            {
                currentPosition = BackWeapon.offsetPosition;
                currentRotation = BackWeapon.Rotation;
                ConfigLoader.UpdateIniFile("AI", "OffsetPosition", currentPosition);
                ConfigLoader.UpdateIniFile("AI", "Rotation", currentRotation);
                BackWeapon.UpdateConfig();
                Game.DisplayNotification("AI Offset Position and Rotation copied from player settings.");
            }

            if (selectedItem == aiAcceptedWeaponMenuItem)
            {
                GameFiber.StartNew(delegate
                {
                    sender.Close();
                    aiAcceptedWeaponMenu.Visible = true;
                    while (true)
                    {
                        GameFiber.Yield();
                        if (Game.IsKeyDown(Keys.Back))
                        {
                            aiAcceptedWeaponMenu.Close();
                            aiMenu.Visible = true;
                            aiMenu.CurrentSelection = 5;
                            return;
                        }
                    }
                });
            }
        }

        public static void ProcessLoop()
        {
            Game.LogTrivial("Menus ready to use.");
            while (true)
            {
                GameFiber.Yield();

                //change key to be ini setting
                if (Game.IsKeyDown(menuKey) && !menuPool.IsAnyMenuOpen()) // Our menu on/off switch.
                {
                    mainMenu.Visible = !mainMenu.Visible;
                }

                menuPool.ProcessMenus();       // Process all our menus: draw the menu and process the key strokes and the mouse. 
            }

        }
    }
}
