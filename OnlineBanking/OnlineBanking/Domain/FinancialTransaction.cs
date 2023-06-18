using OneOf;
using OnlineBanking.Domain.FinancialTransactionDetails;

namespace OnlineBanking.Domain;

public record FinancialTransaction(
    Guid Id,
    Guid? SenderId,
    Guid? ReceiverId,
    decimal Amount,
    FinancialTransactionType Type,
    FinancialTransactionStatus Status,
    Details Details,
    string? Comment)
{
    public static FinancialTransaction CreateDeposit(
        Guid id,
        Guid receiverId,
        decimal amount,
        DepositDetails? details = null,
        string? comment = null)
    {
        return new(id, null, receiverId, amount, FinancialTransactionType.Deposit, FinancialTransactionStatus.Created, new(details), comment);
    }

    public static FinancialTransaction CreateWithdrawal(
        Guid id,
        Guid senderId,
        decimal amount,
        WithdrwalDetails? details = null,
        string? comment = null)
    {
        return new(id, senderId, null, amount, FinancialTransactionType.Withdrawal, FinancialTransactionStatus.Created, new(details), comment);
    }

    public static FinancialTransaction CreateTransfer(
        Guid id,
        Guid senderId,
        Guid receiverId,
        decimal amount,
        TransferDetails? details = null,
        string? comment = null)
    {
        return new(id, senderId, receiverId, amount, FinancialTransactionType.Transfer, FinancialTransactionStatus.Created, new(details), comment);
    }
}

