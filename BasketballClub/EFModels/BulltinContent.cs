using System;
using System.Collections.Generic;

namespace BasketballClub.EFModels
{
    public partial class BulltinContent
    {
        public string Id { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
        public int Priority { get; set; }
        public string Description { get; set; } = null!;
    }
}
