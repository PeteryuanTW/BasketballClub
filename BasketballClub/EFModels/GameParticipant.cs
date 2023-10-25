using System;
using System.Collections.Generic;

namespace BasketballClub.EFModels
{
    public partial class GameParticipant
    {
        public string Id { get; set; } = null!;
        public string ParticipantName { get; set; } = null!;
        public int Amount { get; set; }
    }
}
