using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Rage;
using Rage.Native;

namespace BackWeapon
{
    public class EntryPoint
    {
        static bool enableAI = (bool) ConfigLoader.GetIniValues()["EnableAI"];

        private static void Main()
        {
            while (Game.IsLoading)
            {
                GameFiber.Yield();
            }
            Game.LogTrivial("Enabling Player Loop...");
            var PlayerProcessFiber = new GameFiber(BackWeapon.PlayerLoop);
            PlayerProcessFiber.Start();
            if (enableAI)
            {
                Game.LogTrivial("Enabling AI Loop...");
                var AIProcessFiber = new GameFiber(BackWeapon.AIPedsLoop);
                AIProcessFiber.Start();
            }
            Game.LogTrivial("Stow That Weapon (BackWeapon.dll) by willpv23 has been loaded!");
            GameFiber.Hibernate();
        }
    }
}
