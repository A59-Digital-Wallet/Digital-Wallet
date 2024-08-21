using Wallet.DTO.Response;

namespace Wallet.MVC.Models
{
    public class CategoryViewModel
    {
        public int Id { get; set; } 
        public string Name { get; set; }
        public ICollection<TransactionViewModel>? Transactions { get; set; }
    }
}
