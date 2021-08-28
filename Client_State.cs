namespace AtlasSimulator
{
    public partial class Client
    {
        public bool Connected;
        public bool LoggedIn;
        public bool InGame;
        public int PacketSequence;
        public int SessionId;
        public bool CharacterSelected;
        public bool GameOpenSent;
        public bool OverviewRequested;
        private bool PlayerInitSent;
        private ushort PositionStatus;
        private float ZoneX;
        private float ZoneY;
        private float ZoneZ;
        private ushort ZoneId;
        private ushort PositionHeading;
        private float PositionSpeed;

        private void ChantTimerCallback(object state)
        {
            // Unsure how to map these, currently only 1 is working (endo)
            SendUseSkill(0, 16, 1);
            SendUseSkill(0, 14, 1);
            SendUseSkill(0, 12, 1);
            SendUseSkill(0, 13, 1);
        }
    }
}