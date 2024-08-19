using Microsoft.AspNetCore.Mvc.Rendering;

namespace Wallet.MVC.Models
{
    public class AddUserToJointWalletViewModel
    {
       
            public int WalletId { get; set; }
            public List<SelectListItem>? Contacts { get; set; }
            public List<string> SelectedUserIds { get; set; } // IDs of selected users from contacts

            // New fields for permissions
            public Dictionary<string, bool> CanSpend { get; set; }
            public Dictionary<string, bool> CanAddFunds { get; set; }
        

    }

}
