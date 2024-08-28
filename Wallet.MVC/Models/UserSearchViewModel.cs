using Wallet.DTO.Response;

namespace Wallet.MVC.Models
{
    public class UserSearchViewModel
    {
        public List<UserWithRolesDto> UsersWithRoles { get; set; } = new List<UserWithRolesDto>();
        public string UserRole { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public string SearchTerm { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}
