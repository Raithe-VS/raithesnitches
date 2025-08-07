
using System;

namespace raithesnitches.src.Violations
{
    [Flags]
    public enum EnumViolationType
    {
        Trespassed = 1 << 0,   // 1
        Escaped = 1 << 1,   // 2

        BlockUsed = 1 << 2,   // 4
        BlockPlaced = 1 << 3,   // 8
        BlockBroke = 1 << 4,   // 16

        ReinforcementBroke = 1 << 5,   // 32
        ReinforcementPlaced = 1 << 6,   // 64

        EntityHit = 1 << 7,   // 128
        EntityKilled = 1 << 8,    // 256
        PlayerSpawned = 1 << 9,  // 512
        
        CollectibleTaken = 1 << 10  // 1024

        
    }
}

