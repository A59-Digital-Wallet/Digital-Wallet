using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.DTO.Request
{
    public class VerifyTransactionRequestModel
    {
        public string Token { get; set; } // Token received from the initial transaction request
        public string VerificationCode { get; set; } // Code the user received via SMS
    }

}
