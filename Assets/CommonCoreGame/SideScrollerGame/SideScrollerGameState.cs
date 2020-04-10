//extra gamestate for SideScrollerGame

namespace CommonCore.State
{
    public partial class MetaState
    {
        public int Player1Credits { get; set; }
        public int Player2Credits { get; set; }
    }


    public partial class GameState
    {
        public bool Player2Active { get; set; }

        public string Player1Character { get; set; }
        public string Player2Character { get; set; }

        public int Player1Score { get; set; }
        public int Player2Score { get; set; }

        [Init]
        private void SideScrollerInit()
        {
            Player1Character = "Marco"; //these should really be empty or null by default but it'll probably break something so I won't change it now
            Player2Character = "Joakim";
        }
    }


}