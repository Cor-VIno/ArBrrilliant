using System;

namespace JingHongLu.Player
{
    [Flags]
    public enum PlayerControlLockFlags
    {
        None = 0,
        Move = 1 << 0,
        Jump = 1 << 1,
        BasicSkill = 1 << 2,
        SwordArt = 1 << 3,
        Dash = 1 << 4,
        Aim = 1 << 5,

        Gameplay = Move | Jump | BasicSkill | Dash,
        All = Move | Jump | BasicSkill | SwordArt | Dash | Aim
    }
}
