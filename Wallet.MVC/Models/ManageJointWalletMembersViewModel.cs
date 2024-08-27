using Wallet.Data.Models;
using Wallet.DTO.Response;

namespace Wallet.MVC.Models
{
    public class ManageJointWalletMembersViewModel
    {
        public int WalletId { get; set; }
        public string WalletName { get; set; }
        public List<ContactResponseDTO> Contacts { get; set; } = new List<ContactResponseDTO>();
        public List<AppUser> Members { get; set; } = new List<AppUser>();
        public string OwnerId { get; set; }
    }
}
