using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Common.Exceptions
{
    public class VerificationRequiredException : Exception
    {
        public string TransactionToken { get; }

        public VerificationRequiredException(string transactionToken)
        {
            TransactionToken = transactionToken;
        }
    }
}
