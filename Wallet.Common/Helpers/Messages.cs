namespace Wallet.Common.Helpers
{
    public static class Messages
    {
        //Common
        public const string UserNotFound = "User not found.";
        public const string Unauthorized = "Unauthorized.";
        public const string OperationFailed = "Something went wrong. Please try again later.";
        public static class Service
        {

            //Card
            public const string CardNotFound = "Card not found!";
            public const string CardsNotFound = "No cards were found!";
            public const string CardAlreadyExists = "This card has already been added.";

            //Category
            public const string CategoriesNotFound = "No categories found!";
            public const string CategoryNotFound = "Category not found!";
            public const string CategoryNameCannotBeEmpty = "Category name cannot be empty!";

            //Contact
            public const string ContactsNotFound = "Contacts list is empty";
            public const string ContactNotFound = "Contact user not found";
            public const string ContactAlreadyExists = "This contact already exists.";

            //CurrencyExchange
            public const string FailedDueToNetworkError = "Failed to retrieve exchange rate due to a network error.";
            public const string FailedToRetrieveRate = "Failed to retrieve exchange rate.";

            //MoneyRequest
            public const string RecipientNotFound = "Recipient not found.";
            public const string InvalidCurrency = "Invalid currency.";
            public const string MoneyRequestNotFound = "Money request not found.";
            public const string NoWalletsToSendFunds = "User has no wallets to send funds from.";
            public const string NoRecipientWalletInCurrency = "Recipient has no wallet in the requested currency.";

            //Transaction
            public const string TransactionNotFound = "Transaction not found.";
            public const string InvalidOrExpiredCode = "Invalid or expired verification code.";
            public const string InvalidOrExpiredTransactionToken = "Transaction token is invalid or has expired.";
            public const string TransactionConflict = "Transaction conflict detected. Please try again.";
            public const string RecipientWalletNotFound = "Recipient wallet does not exist.";
            public const string NotRecurringTransaction = "This transaction is not a recurring transaction.";


            //Cloudinary
            public const string NoFileUploaded = "No file uploaded.";
            public const string InvalidFileFormat = "Invalid file format. Only .jpg, .jpeg, and .png are allowed.";
            public const string ErrorUploadingFile = "Error uploading image.";
            public const string FailedToUpdateImage = "Failed to update profile picture.";

            //User
            public const string AdminCannotBeBlocked = "Admin users cannot be blocked.";
            public const string InvalidUserAction = "Invalid action. Use 'block', 'unblock', or 'makeadmin'.";
            public const string UserIsNotInRole = "User is not in the '{0}' role.";
            public const string UserAlreadyInRole = "User is already in the '{0}' role.";

            //Wallet
            public const string WalletNotFound = "Wallet not found.";
            public const string UserNotMemberOfWallet = "User is not a member of this wallet.";
            public const string OverdraftOperationNotAllowed = "Overdraft can only be enabled/disabled for personal wallets by the owner.";
            public const string NotJointWallet = "Can't add to wallet that is not joint";
            public const string FailedToUpdateWallet = "Failed to update the wallet.";

            //Email
            public const string VerificationCode = "Your Verification Code";
            public const string EmailMessage = "Hello {0},\n\nYour verification code is: {1}\n\nPlease enter this code to complete your transaction.";
        }

        public static class Controller
        {
            //Admin
            public const string ActionSuccessful = "Action '{0}' was successfully performed on the user.";
            public const string InterestRateSuccessful = "Default interest rate updated to {0}%";
            public const string InterestRateFailed = "Failed to update the default interest rate.";
            public const string OverdraftLimitSuccessful = "Default overdraft limit updated to {0}.";
            public const string OverdraftLimitFailed = "Failed to update the default overdraft limit.";
            public const string NegativeMonthsSuccessful = "Default consecutive negative months updated to {0} months.";
            public const string NegativeMonthsFailed = "Failed to update the default consecutive negative months.";

            //Card
            public const string CardAddedSuccessful = "Card has been added successfully.";
            public const string CardDeletedSuccessful = "Card has been deleted successfully";

            public const string InvalidCardNumber = "Invalid card number.";
            public const string UnknownCardNetwork = "Unknown card network.";
            public const string InvalidCardNumberLength = "Invalid card number length.";
            public const string CardNearingExpiration = "The card is nearing its expiration. Please provide a card with more than one month of validity remaining.";
            public const string InvalidCVV = "Invalid CVV.";
            public const string InvalidCardholderName = "Invalid cardholder name.";

            //Category
            public const string CategoryAddedSuccessful = "Category added successfully.";
            public const string CategoryDeletedSuccessful = "Category deleted successfully";

            //Contact
            public const string ContactAddedSuccessful = "Contact added successfully.";
            public const string ContactDeletedSuccessful = "Contact removed successfully";

            // Transaction
            public const string TransactionCreatedSuccessfully = "Transaction created successfully.";
            public const string VerificationRequired = "Transaction requires verification.";
            public const string VerificationFailed = "Verification failed. Invalid code.";
            public const string TransactionVerifiedSuccessfully = "Transaction verified and completed successfully.";
            public const string RecurringTransactionCancelledSuccessfully = "Recurring transaction canceled successfully.";
            public const string TransactionAddedToCategorySuccessfully = "Transaction successfully added to category.";
            public const string PageOrPageSizeInvalid = "Page and pageSize must be greater than 0.";

            // User
            public const string RegistrationSuccess = "User registered successfully. Confirmation codes sent.";
            public const string EmailConfirmationSuccess = "Email confirmed successfully.";
            public const string InvalidOrExpiredConfirmationCode = "Invalid or expired confirmation code.";
            public const string InvalidEmailOrPassword = "Invalid email or password.";
            public const string PhoneNumberVerificationSuccess = "Phone number verified successfully.";
            public const string InvalidVerificationCode = "Invalid verification code.";
            public const string ProfilePictureUpdatedSuccessfully = "Profile picture updated successfully.";
            public const string UserProfileUpdatedSuccessfully = "User profile updated successfully. Please verify your new phone number.";
            public const string TwoFactorAuthenticationEnabled = "2FA has been enabled.";
            public const string TwoFactorAuthenticationDisabled = "2FA has been disabled.";
            public const string InvalidUser = "Invalid user.";
            public const string TwoFactorRequired = "2FA is required to complete the login.";

            // Wallet
            public const string MemberAddedToWalletSuccess = "Member added successfully to the joint wallet.";
            public const string MemberRemovedFromWalletSuccess = "Member removed successfully from the joint wallet.";
            public const string WalletCreatedSuccessfully = "Wallet created successfully.";
            public const string OverdraftUpdatedSuccessfully = "Overdraft setting updated successfully.";
        }

    }
}
