using ACE.Entity.Enum;

namespace ACE.Server.WorldObjects
{
    partial class Player
    {
        public bool IsPKType => PlayerKillerStatus == PlayerKillerStatus.PK || PlayerKillerStatus == PlayerKillerStatus.PKLite;

        public bool IsPK => PlayerKillerStatus == PlayerKillerStatus.PK;

        public bool IsPKL => PlayerKillerStatus == PlayerKillerStatus.PKLite;

        public bool IsNPK => PlayerKillerStatus == PlayerKillerStatus.NPK;
    }
}
