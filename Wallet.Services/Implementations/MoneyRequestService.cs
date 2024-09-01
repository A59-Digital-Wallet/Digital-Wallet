using Wallet.Common.Exceptions;
using Wallet.Common.Helpers;
using Wallet.Data.Models;
using Wallet.Data.Models.Enums;
using Wallet.Data.Repositories.Contracts;
using Wallet.DTO.Request;
using Wallet.DTO.Response;
using Wallet.Services.Contracts;

namespace Wallet.Services.Implementations
{

    public class MoneyRequestService : IMoneyRequestService
    {
        private readonly IMoneyRequestRepository _moneyRequestRepository;
        private readonly IUserService _userService;
        private readonly IWalletRepository _walletRepository;
        private readonly ITransactionService _transactionService;
        private readonly ICurrencyExchangeService _currencyExchangeService;

        public MoneyRequestService(IMoneyRequestRepository moneyRequestRepository, IUserService userService, IWalletRepository walletRepository, ITransactionService transactionService, ICurrencyExchangeService currencyExchangeService)
        {
            _moneyRequestRepository = moneyRequestRepository;
            _userService = userService;
            _walletRepository = walletRepository;
            _transactionService = transactionService;
            _currencyExchangeService = currencyExchangeService;
        }

        public async Task<MoneyRequestResponseDTO> CreateMoneyRequestAsync(MoneyRequestCreateDTO requestDto, string requesterId)
        {
            var recipient = await _userService.GetUserByIdAsync(requestDto.RecipientId);

            if (recipient == null)
            {
                throw new ArgumentException(Messages.Service.RecipientNotFound);
            }

            if (!Enum.TryParse(requestDto.RequestedCurrency, out Currency requestedCurrency))
            {
                throw new ArgumentException(Messages.Service.InvalidCurrency);
            }

            var moneyRequest = new MoneyRequest
            {
                RequesterId = requesterId,
                RecipientId = requestDto.RecipientId,
                Amount = requestDto.Amount,
                Description = requestDto.Description,
                RequestedCurrency = requestedCurrency, // Store the enum value in the entity
                Status = RequestStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _moneyRequestRepository.CreateAsync(moneyRequest);

            return new MoneyRequestResponseDTO
            {
                Id = moneyRequest.Id,
                RequesterId = moneyRequest.RequesterId,
                RecipientId = moneyRequest.RecipientId,
                Amount = moneyRequest.Amount,
                Description = moneyRequest.Description,
                RequestedCurrency = moneyRequest.RequestedCurrency.ToString(), // Convert enum back to string for response
                Status = moneyRequest.Status,
                CreatedAt = moneyRequest.CreatedAt,
                UpdatedAt = moneyRequest.UpdatedAt,
                UserName = requestDto.UserName
            };
        }

        public async Task<IEnumerable<MoneyRequestResponseDTO>> GetReceivedRequestsAsync(string recipientId)
        {
            var requests = await _moneyRequestRepository.GetReceivedRequestsAsync(recipientId);

            return requests.Select(request => new MoneyRequestResponseDTO
            {
                Id = request.Id,
                RequesterId = request.RequesterId,
                RecipientId = request.RecipientId,
                Amount = request.Amount,
                Description = request.Description,
                RequestedCurrency = request.RequestedCurrency.ToString(), // Convert enum back to string
                Status = request.Status,
                CreatedAt = request.CreatedAt,
                UpdatedAt = request.UpdatedAt,
                UserName = request.Requester.UserName
            }).ToList();
        }

        public async Task<MoneyRequestResponseDTO> GetMoneyRequestByIdAsync(int id)
        {
            var request = await _moneyRequestRepository.GetByIdAsync(id);

            if (request == null)
            {
                throw new ArgumentException(Messages.Service.MoneyRequestNotFound);
            }

            return new MoneyRequestResponseDTO
            {
                Id = request.Id,
                RequesterId = request.RequesterId,
                RecipientId = request.RecipientId,
                Amount = request.Amount,
                Description = request.Description,
                RequestedCurrency = request.RequestedCurrency.ToString(), // Convert enum back to string
                Status = request.Status,
                CreatedAt = request.CreatedAt,
                UpdatedAt = request.UpdatedAt,
                UserName = request.Requester.UserName
            };
        }

        public async Task UpdateMoneyRequestStatusAsync(int id, RequestStatus status)
        {
            var request = await _moneyRequestRepository.GetByIdAsync(id);

            if (request == null)
            {
                throw new ArgumentException(Messages.Service.MoneyRequestNotFound);
            }

            request.Status = status;
            request.UpdatedAt = DateTime.UtcNow;

            await _moneyRequestRepository.UpdateAsync(request);
        }

        public async Task ApproveMoneyRequestAsync(int requestId, string senderId)
        {
            var request = await _moneyRequestRepository.GetByIdAsync(requestId);

            if (request == null)
            {
                throw new ArgumentException(Messages.Service.MoneyRequestNotFound);
            }

            if (request.RecipientId != senderId)
            {
                throw new UnauthorizedAccessException(Messages.Unauthorized);
            }

            await UpdateMoneyRequestStatusAsync(requestId, RequestStatus.Approved);

            var senderWallets = await _walletRepository.GetUserWalletsAsync(senderId);

            // Find the wallet with the same currency as the requested currency or the wallet with the most balance
            var senderWallet = senderWallets.FirstOrDefault(w => w.Currency == request.RequestedCurrency) ??
                               senderWallets.OrderByDescending(w => w.Balance).FirstOrDefault();

            if (senderWallet == null)
            {
                throw new InvalidOperationException(Messages.Service.NoWalletsToSendFunds);
            }

            var recipientWallets = await _walletRepository.GetUserWalletsAsync(request.RequesterId);
            var recipientWallet = recipientWallets.FirstOrDefault(w => w.Currency == request.RequestedCurrency);

            if (recipientWallet == null)
            {
                throw new InvalidOperationException(Messages.Service.NoRecipientWalletInCurrency);
            }

            decimal finalAmount = request.Amount;
            if (senderWallet.Currency != request.RequestedCurrency)
            {
                finalAmount = await _currencyExchangeService.ConvertAsync(request.Amount, request.RequestedCurrency, senderWallet.Currency);
            }

            // Create the transaction
            var transactionRequest = new TransactionRequestModel
            {
                WalletId = senderWallet.Id,
                Amount = finalAmount,
                Description = request.Description,
                TransactionType = TransactionType.Transfer,
                RecepientWalletId = recipientWallet.Id,
                IsRecurring = false
            };

            try
            {
                await _transactionService.CreateTransactionAsync(transactionRequest, senderId);
            }
            catch (VerificationRequiredException ex)
            {
                // Handle the high-value transaction by rethrowing the exception with the token
                throw new VerificationRequiredException(ex.TransactionToken, senderWallet.Id, finalAmount, transactionRequest.Description, recipientWallet.Id);
            }
        }
    }

}
