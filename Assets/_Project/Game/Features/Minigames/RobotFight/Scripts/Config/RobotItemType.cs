namespace ChainSawLeg.Features.Minigames
{
    /// <summary>
    /// Kind of thrown attack: selects the item prefab and how many arena cells are affected.
    /// </summary>
    public enum RobotItemType
    {
        /// <summary>Single-cell hit with the regular throwable item.</summary>
        OneBlock,
        /// <summary>Two adjacent cells (target + right neighbour) hit with the wide item.</summary>
        TwoBlock,
        /// <summary>2x2 cell block hit with the heaviest item.</summary>
        FourBlock,
        /// <summary>Single-cell hit with the kickable block the player can bounce back at the robot.</summary>
        BeatableBlock,
        /// <summary>Always targets the player's current cell.</summary>
        TargetBlock
    }
}
