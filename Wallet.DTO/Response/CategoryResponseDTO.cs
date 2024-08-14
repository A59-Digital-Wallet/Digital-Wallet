using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.DTO.Response
{
    public class CategoryResponseDTO
    {
        public string Name { get; set; }
        public ICollection<TransactionDto>? Transactions { get; set; }
    }
}
