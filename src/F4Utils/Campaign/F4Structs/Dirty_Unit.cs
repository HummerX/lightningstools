namespace F4Utils.Campaign.F4Structs
{
    public enum Dirty_Unit
    {
        DIRTY_WAYPOINT = 0x00000001,
        DIRTY_ROSTER = 0x00000004,
        DIRTY_UNIT_FLAGS = 0x00000008,
        DIRTY_DESTINATION = 0x00000010,
        DIRTY_CARGO_ID = 0x00000020,
        DIRTY_TARGET_ID = 0x00000040,
        DIRTY_MOVED = 0x00000080,
        DIRTY_TACTIC = 0x00000200,
        DIRTY_REINFORCEMENT = 0x00000800,
        DIRTY_WP_LIST = 0x00002000,
    };
}