using System;
namespace OnlineBanking.Domain;

public interface IFinancialTransactionRepository
{
    Task InsertAsync(FinancialTransaction transaction);

    Task<FinancialTransaction?> GetByIdAsync(Guid id);

    Task UpdateStatusAsync(Guid id, FinancialTransactionStatus status);

    Task<IReadOnlyCollection<FinancialTransaction>> GetAllIncommingTransactions(Guid userId);

    Task DeleteAsync(Guid id);
}

