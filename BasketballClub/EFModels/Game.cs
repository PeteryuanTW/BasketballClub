using System;
using System.Collections.Generic;

namespace BasketballClub.EFModels
{
    public partial class Game
    {
        public string Id { get; set; } = null!;
        public string Host { get; set; } = null!;
        public string Place { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTine { get; set; }
    }
}
