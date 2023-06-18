using System;
namespace OnlineBanking.Domain.Repositories;

public interface IFinancialTransactionRepository
{
    Task InsertAsync(FinancialTransaction transaction);

    Task<FinancialTransaction?> GetByIdAsync(Guid id);

    Task UpdateAsync(FinancialTransaction transaction);

    Task<IReadOnlyCollection<FinancialTransaction>> GetAllIncommingTransactions(Guid userId);

    Task<IReadOnlyCollection<FinancialTransaction>> GetAllOutcommingTransactions(Guid userId);

    Task DeleteAsync(Guid id);
}

