﻿using Wallet.Data.Models;

namespace Wallet.DTO.Response
{
    public class UserWithRolesDto
    {
        public AppUser User { get; set; }
        public List<string> Roles { get; set; }
    }
}
