using System;
using System.Collections.Generic;

namespace BasketballClub.EFModels
{
    public partial class UserInfo
    {
        public int EmployeeId { get; set; }
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public int UserRole { get; set; }
        public string SessionKey { get; set; } = null!;
    }
}
