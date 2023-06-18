using OneOf;
using OnlineBanking.Domain.FinancialTransactionDetails;

namespace OnlineBanking.Domain;

public record FinancialTransaction(
    Guid Id,
    Guid? SenderId,
    Guid? ReceiverId,
    decimal Amount,
    string Currency,
    FinancialTransactionType Type,
    Details Details,
    string? Comment,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public FinancialTransactionStatus Status { get; set; } = FinancialTransactionStatus.Created;

    public static FinancialTransaction CreateDeposit(
        Guid id,
        Guid receiverId,
        decimal amount,
        string currency,
        DateTimeOffset createdAt,
        DepositDetails? details = null,
        string? comment = null)
    {
        return new(id, null, receiverId, amount, currency, FinancialTransactionType.Deposit, new(details), comment, createdAt, createdAt);
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
        return new(id, senderId, null, amount, currency, FinancialTransactionType.Withdrawal, new(details), comment, createdAt, createdAt);
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
        return new(id, senderId, receiverId, amount, currency, FinancialTransactionType.Transfer, new(details), comment, createdAt, createdAt);
    }

    public OneOf<FinancialTransactionStatus, DomainError> Start(User? receiver, User? sender)
    {
        return Type switch
        {
            FinancialTransactionType.Deposit => CompleteDeposit(receiver),
            FinancialTransactionType.Withdrawal or FinancialTransactionType.Transfer => StartWithdrawalOrTransfer(sender),
            _ => FinancialTransactionError.InvalidFinancialTransactionType()
        };

        OneOf<FinancialTransactionStatus, DomainError> CompleteDeposit(User? receiver)
        {
            if (receiver is null) return FinancialTransactionError.UnknownUser();
            if (receiver.DepositToCurrencyAccount(Amount, Currency).TryPickT1(out var error, out _))
            {
                Status = FinancialTransactionStatus.Failed;
                return error;
            }
            Status = FinancialTransactionStatus.Completed;
            return Status;
        }

        OneOf<FinancialTransactionStatus, DomainError> StartWithdrawalOrTransfer(User? sender)
        {
            if (sender is null) return FinancialTransactionError.UnknownUser();
            if (sender.HoldFromCurrencyAccount(Amount, Currency).TryPickT1(out var error, out _))
            {
                Status = FinancialTransactionStatus.Failed;
                return error;
            }
            Status = FinancialTransactionStatus.Processing;
            return Status;
        }
    }

    public OneOf<FinancialTransactionStatus, DomainError> Complete(User? receiver, User? sender)
    {
        return Type switch
        {
            FinancialTransactionType.Deposit => AlreadyCompleted(),
            FinancialTransactionType.Withdrawal => CompleteWithdrawal(sender),
            FinancialTransactionType.Transfer => CompleteTransfer(receiver, sender),
            _ => FinancialTransactionError.InvalidFinancialTransactionType()
        };

        OneOf<FinancialTransactionStatus, DomainError> CompleteWithdrawal(User? sender)
        {
            if (sender is null) return FinancialTransactionError.UnknownUser();
            if (sender.WithdrawFromCurrencyAccount(Amount, Currency).TryPickT1(out var error, out _))
            {
                Status = FinancialTransactionStatus.Failed;
                return error;
            }
            Status = FinancialTransactionStatus.Completed;
            return Status;
        }

        OneOf<FinancialTransactionStatus, DomainError> CompleteTransfer(User? receiver, User? sender)
        {
            if (sender is null || receiver is null) return FinancialTransactionError.UnknownUser();
            if (sender.WithdrawFromCurrencyAccount(Amount, Currency).TryPickT1(out var error, out _))
            {
                Status = FinancialTransactionStatus.Failed;
                return error;
            }

            if (receiver.DepositToCurrencyAccount(Amount, Currency).TryPickT1(out error, out _))
            {
                Status = FinancialTransactionStatus.Failed;
                return error;
            }

            Status = FinancialTransactionStatus.Completed;
            return Status;
        }

        FinancialTransactionStatus AlreadyCompleted() => Status;
    }

    public OneOf<FinancialTransactionStatus, DomainError> Cancel(User? receiver, User? sender)
    {
        return Type switch
        {
            FinancialTransactionType.Deposit => FinancialTransactionError.InvalidOperation("Deposit cancel is not supported"),
            FinancialTransactionType.Withdrawal => CancelWithdrawal(sender),
            FinancialTransactionType.Transfer => CancelTransfer(receiver, sender),
            _ => FinancialTransactionError.InvalidFinancialTransactionType()
        };

        OneOf<FinancialTransactionStatus, DomainError> CancelWithdrawal(User? sender)
        {
            if (sender is null) return FinancialTransactionError.UnknownUser();
            if (sender.UnHoldFromCurrencyAccount(Amount, Currency).TryPickT1(out var error, out _))
            {
                Status = FinancialTransactionStatus.Failed;
                return error;
            }
            Status = FinancialTransactionStatus.Canceled;
            return Status;
        }

        OneOf<FinancialTransactionStatus, DomainError> CancelTransfer(User? receiver, User? sender)
        {
            if (sender is null || receiver is null) return FinancialTransactionError.UnknownUser();
            if (sender.UnHoldFromCurrencyAccount(Amount, Currency).TryPickT1(out var error, out _))
            {
                Status = FinancialTransactionStatus.Failed;
                return error;
            }

            Status = FinancialTransactionStatus.Canceled;
            return Status;
        }
    }
}

