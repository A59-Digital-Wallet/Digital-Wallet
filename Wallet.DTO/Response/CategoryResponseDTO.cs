namespace Wallet.DTO.Response
{
    public class CategoryResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<TransactionDto>? Transactions { get; set; }
    }
}
