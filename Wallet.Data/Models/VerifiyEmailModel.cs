﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Data.Models
{
    public class VerifyEmailModel
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }
}
