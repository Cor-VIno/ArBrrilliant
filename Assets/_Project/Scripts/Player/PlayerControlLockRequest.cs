namespace JingHongLu.Player
{
    public readonly struct PlayerControlLockRequest
    {
        public readonly object Source;
        public readonly PlayerControlLockFlags Flags;

        public PlayerControlLockRequest(object source, PlayerControlLockFlags flags)
        {
            Source = source;
            Flags = flags;
        }
    }
}
