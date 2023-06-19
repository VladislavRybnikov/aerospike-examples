using OneOf;
using OnlineBanking.Domain.FinancialTransactionDetails;

namespace OnlineBanking.Domain;

public record FinancialTransaction
{ 
    public Guid Id { get; set; }

    public Guid? SenderId { get; set; }

    public Guid? ReceiverId { get; set; }

    public decimal Amount { get; set; }

    public string? Currency { get; set; }

    public FinancialTransactionType Type { get; set; }

    public Details? Details { get; set; }

    public string? Comment { get; set; }

    public FinancialTransactionStatus Status { get; set; } = FinancialTransactionStatus.Created;

    public DateTime UpdatedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DomainError? Error { get; set; }

    public static FinancialTransaction CreateDeposit(
        Guid id,
        Guid receiverId,
        decimal amount,
        string currency,
        DateTimeOffset createdAt,
        DepositDetails? details = null,
        string? comment = null)
    {
        return new FinancialTransaction
        {
            Id = id,
            SenderId = null,
            ReceiverId = receiverId,
            Amount = amount,
            Currency = currency,
            Type = FinancialTransactionType.Deposit,
            Details = new(details),
            Comment = comment,
            CreatedAt = createdAt.UtcDateTime,
            UpdatedAt = createdAt.UtcDateTime
        };
    }

    public static FinancialTransaction CreateWithdrawal(
        Guid id,
        Guid senderId,
        decimal amount,
        string currency,
        DateTimeOffset createdAt,
        WithdrwalDetails? details = null,
        string? comment = null)
    {
        return new FinancialTransaction
        {
            Id = id,
            SenderId = senderId,
            ReceiverId = null,
            Amount = amount,
            Currency = currency,
            Type = FinancialTransactionType.Withdrawal,
            Details = new(details),
            Comment = comment,
            CreatedAt = createdAt.UtcDateTime,
            UpdatedAt = createdAt.UtcDateTime
        };
    }

    public static FinancialTransaction CreateTransfer(
        Guid id,
        Guid senderId,
        Guid receiverId,
        decimal amount,
        string currency,
        DateTimeOffset createdAt,
        TransferDetails? details = null,
        string? comment = null)
    {
        return new FinancialTransaction
        {
            Id = id,
            SenderId = senderId,
            ReceiverId = receiverId,
            Amount = amount,
            Currency = currency,
            Type = FinancialTransactionType.Transfer,
            Details = new(details),
            Comment = comment,
            CreatedAt = createdAt.UtcDateTime,
            UpdatedAt = createdAt.UtcDateTime
        };
    }

    public OneOf<FinancialTransactionStatus, DomainError> BeginProcessing(User? receiver, User? sender, DateTimeOffset updatedAt)
    {
        return (Type, Status) switch
        {
            (_, FinancialTransactionStatus.Processing) => StatusAlreadyProcessed(),
            (FinancialTransactionType.Deposit, FinancialTransactionStatus.Created) => CompleteDeposit(receiver),
            (FinancialTransactionType.Withdrawal or FinancialTransactionType.Transfer, FinancialTransactionStatus.Created) => BeginWithdrawalOrTransfer(sender),
            _ => FinancialTransactionError.InvalidOperation()
        };

        OneOf<FinancialTransactionStatus, DomainError> CompleteDeposit(User? receiver)
        {
            if (receiver is null) return FailTransaction(FinancialTransactionError.UnknownUser(), updatedAt);
            if (receiver.DepositToCurrencyAccount(Amount, Currency, updatedAt).TryPickT1(out var error, out _))
            {
                return FailTransaction(error, updatedAt);
            }
            return CompleteTransaction(updatedAt);
        }

        OneOf<FinancialTransactionStatus, DomainError> BeginWithdrawalOrTransfer(User? sender)
        {
            if (sender is null) return FinancialTransactionError.UnknownUser();
            if (sender.HoldFromCurrencyAccount(Amount, Currency, updatedAt).TryPickT1(out var error, out _))
            {
                return FailTransaction(error, updatedAt);
            }
            return BeginTransaction(updatedAt);
        }
    }

    public OneOf<FinancialTransactionStatus, DomainError> Complete(User? receiver, User? sender, DateTimeOffset updatedAt)
    {
        return (Type, Status) switch
        {
            (FinancialTransactionType.Deposit, FinancialTransactionStatus.Completed) => StatusAlreadyProcessed(),
            (FinancialTransactionType.Withdrawal, FinancialTransactionStatus.Processing) => CompleteWithdrawal(sender),
            (FinancialTransactionType.Transfer, FinancialTransactionStatus.Processing) => CompleteTransfer(receiver, sender),
            _ => FinancialTransactionError.InvalidOperation()
        };

        OneOf<FinancialTransactionStatus, DomainError> CompleteWithdrawal(User? sender)
        {
            if (sender is null) return FailTransaction(FinancialTransactionError.UnknownUser(), updatedAt);
            if (sender.WithdrawFromCurrencyAccount(Amount, Currency, updatedAt).TryPickT1(out var error, out _))
            {
                return FailTransaction(error, updatedAt);
            }
            return CompleteTransaction(updatedAt);
        }

        OneOf<FinancialTransactionStatus, DomainError> CompleteTransfer(User? receiver, User? sender)
        {
            if (sender is null || receiver is null) return FailTransaction(FinancialTransactionError.UnknownUser(), updatedAt);

            if (sender.WithdrawFromCurrencyAccount(Amount, Currency, updatedAt).TryPickT1(out var error, out _)
                || receiver.DepositToCurrencyAccount(Amount, Currency, updatedAt).TryPickT1(out error, out _))
            {
                return FailTransaction(error, updatedAt);
            }

            return CompleteTransaction(updatedAt);
        }
    }

    public OneOf<FinancialTransactionStatus, DomainError> Cancel(User? receiver, User? sender, DateTimeOffset updatedAt)
    {
        return (Type, Status) switch
        {
            (_, FinancialTransactionStatus.Canceled) => StatusAlreadyProcessed(),
            (FinancialTransactionType.Withdrawal, FinancialTransactionStatus.Created or FinancialTransactionStatus.Processing) => CancelWithdrawal(sender),
            (FinancialTransactionType.Transfer, FinancialTransactionStatus.Created or FinancialTransactionStatus.Processing) => CancelTransfer(receiver, sender),
            _ => FinancialTransactionError.InvalidOperation()
        };

        OneOf<FinancialTransactionStatus, DomainError> CancelWithdrawal(User? sender)
        {
            if (sender is null) return FailTransaction(FinancialTransactionError.UnknownUser(), updatedAt);

            if (sender.UnHoldFromCurrencyAccount(Amount, Currency, updatedAt).TryPickT1(out var error, out _))
                return FailTransaction(error, updatedAt);

            return CancelTransaction(updatedAt);
        }

        OneOf<FinancialTransactionStatus, DomainError> CancelTransfer(User? receiver, User? sender)
        {
            if (sender is null || receiver is null) return FinancialTransactionError.UnknownUser();

            if (sender.UnHoldFromCurrencyAccount(Amount, Currency, updatedAt).TryPickT1(out var error, out _))
                return FailTransaction(error, updatedAt);

            return CancelTransaction(updatedAt);
        }
    }

    private FinancialTransactionStatus UpdateStatus(FinancialTransactionStatus status, DateTimeOffset updatedAt)
    {
        UpdatedAt = updatedAt.UtcDateTime;
        Status = status;

        return Status;
    }

    private FinancialTransactionStatus BeginTransaction(DateTimeOffset updatedAt)
    {
        return UpdateStatus(FinancialTransactionStatus.Processing, updatedAt);
    }

    private FinancialTransactionStatus CancelTransaction(DateTimeOffset updatedAt)
    {
        return UpdateStatus(FinancialTransactionStatus.Canceled, updatedAt);
    }

    private FinancialTransactionStatus CompleteTransaction(DateTimeOffset updatedAt)
    {
        return UpdateStatus(FinancialTransactionStatus.Completed, updatedAt);
    }

    private DomainError FailTransaction(DomainError error, DateTimeOffset updatedAt)
    {
        UpdatedAt = updatedAt.UtcDateTime;
        Status = FinancialTransactionStatus.Failed;
        Error = error;

        return error;
    }


    private FinancialTransactionStatus StatusAlreadyProcessed() => Status;
}

