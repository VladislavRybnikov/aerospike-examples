namespace OnlineBanking.Domain;

public record FinancialTransaction(
    Guid Id,
    Guid? SenderId,
    Guid? ReceiverId,
    decimal Amount,
    FinancialTransactionType Type,
    FinancialTransactionStatus Status,
    string? Details,
    string? Comment)
{
    public static FinancialTransaction CreateDeposit(
        Guid id,
        Guid receiverId,
        decimal amount,
        string? details = null,
        string? comment = null)
    {
        return new(id, null, receiverId, amount, FinancialTransactionType.Deposit, FinancialTransactionStatus.Created, details, comment);
    }
}

