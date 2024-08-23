using Wallet.DTO.Response;

namespace Wallet.MVC.Models
{
    public class AdminPanelViewModel
    {
        // Properties for Overdraft Settings
        public decimal CurrentInterestRate { get; set; }
        public decimal CurrentOverdraftLimit { get; set; }
        public int CurrentConsecutiveNegativeMonths { get; set; }

        // Properties for User Search
        public List<UserWithRolesDto> UsersWithRoles { get; set; } = new List<UserWithRolesDto>();
        public int PageNumber { get; set; } = 1;
        public int TotalPages { get; set; } = 0;
        public string SearchTerm { get; set; } = string.Empty;
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

}
