using System;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One
{
    class ChampLoader
    {
        enum Developer
        {
            xcsoft,
            RL244,
            Fakker
        }

        internal static void Load(string champName)
        {
            switch (champName)
            {
                case "Karthus":
                    MadeBy(Developer.xcsoft);
                    champions.Karthus.Load();
                    break;
                default:
                    AIO_Func.sendDebugMsg("(ChampLoader) Error");
                    break;
            }
        }

        static void MadeBy(Developer Developer)
        {
            AIO_Func.sendDebugMsg(ObjectManager.Player.ChampionName + " Made By '" + Developer.ToString() + "'.");
            Notifications.AddNotification(ObjectManager.Player.ChampionName + " Made By '" + Developer.ToString() + "'.", 4000);
        }

        internal static bool champSupportedCheck(string checkNamespace)
        {
            try
            {
                AIO_Func.sendDebugMsg(Type.GetType(checkNamespace + ObjectManager.Player.ChampionName).Name + " is supported.");
                Notifications.AddNotification(Type.GetType(checkNamespace + ObjectManager.Player.ChampionName).Name + " is supported.", 4000);
                return true;
            }
            catch
            {
                AIO_Func.sendDebugMsg(ObjectManager.Player.ChampionName + " is not supported.");
                Notifications.AddNotification(ObjectManager.Player.ChampionName + " is not supported.", 4000);

                AIO_Menu.addItem("Sorry, " + ObjectManager.Player.ChampionName + " is not supported", null);
                return false;
            }
        }
    }
}
