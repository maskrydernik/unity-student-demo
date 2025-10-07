using UnityEngine;

namespace MiniWoW
{
    public enum Faction
    {
        Player = 0,
        Friendly = 1,
        Enemy = 2
    }

    public static class FactionRules
    {
        public static bool IsFriendly(Faction a, Faction b)
        {
            if (a == b) return true;
            if ((a == Faction.Player && b == Faction.Friendly) || (a == Faction.Friendly && b == Faction.Player))
                return true;
            return false;
        }

        public static bool IsHostile(Faction a, Faction b) => !IsFriendly(a, b);
    }
}
