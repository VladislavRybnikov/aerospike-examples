using System;
using System.Collections.Generic;
using System.Transactions;
using Aerospike.Client;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OnlineBanking.Domain;
using OnlineBanking.Domain.Repositories;

namespace OnlineBanking.Persistance;

public class AerospikeFinancialTransactionRepository : BaseAerospikeRepository<FinancialTransaction>, IFinancialTransactionRepository
{
    public AerospikeFinancialTransactionRepository(IOptions<AerospikeOptions> options) : base(options)
    {
    }

    protected override string Set => AerospikeOptions.Sets!.Transactions!;

    public async Task DeleteAsync(Guid id)
    {
        await AerospikeClient.Delete(null, CancellationToken.None, GetKey(id));
    }

    public async Task<IReadOnlyCollection<FinancialTransaction>> GetAllIncommingTransactions(Guid userId)
    {
        var filter = Filter.Equal(nameof(FinancialTransaction.ReceiverId), userId.ToString());

        var result = new List<FinancialTransaction>();
        await foreach (var model in QueryModelsAsync(s => s.SetFilter(filter)))
        {
            result.Add(model);
        }

        return result;
    }

    public async Task<IReadOnlyCollection<FinancialTransaction>> GetAllOutcommingTransactions(Guid userId)
    {
        var filter = Filter.Equal(nameof(FinancialTransaction.SenderId), userId.ToString());

        var result = new List<FinancialTransaction>();
        await foreach (var model in QueryModelsAsync(s => s.SetFilter(filter)))
        {
            result.Add(model);
        }

        return result;
    }

    public async Task<FinancialTransaction?> GetByIdAsync(Guid id)
    {
        var policy = new Policy() { socketTimeout = 300 };
        var record = await AerospikeClient.Get(policy, CancellationToken.None, GetKey(id));
        return ToModel(record);
    }

    public async Task InsertAsync(FinancialTransaction transaction)
    {
        var writePolicy = new WritePolicy() { sendKey = true, recordExistsAction = RecordExistsAction.CREATE_ONLY };
        await AerospikeClient.Put(writePolicy, CancellationToken.None, GetKey(transaction), GetBins(transaction));
    }

    public async Task UpdateAsync(FinancialTransaction transaction)
    {
        var writePolicy = new WritePolicy() { recordExistsAction = RecordExistsAction.UPDATE_ONLY };

        IEnumerable<Bin> BinsToUpdate()
        {
            yield return new Bin(nameof(FinancialTransaction.Status), transaction.Status.ToString());
            yield return new Bin(nameof(FinancialTransaction.UpdatedAt), JsonConvert.SerializeObject(transaction.UpdatedAt));
            if (transaction.Error != null)
            {
                yield return new Bin(nameof(FinancialTransaction.Error), JsonConvert.SerializeObject(transaction.Error));
            }
        }

        await AerospikeClient.Put(writePolicy, CancellationToken.None, GetKey(transaction), BinsToUpdate().ToArray());
    }

    protected override IEnumerable<Bin> GetFixedBins(FinancialTransaction transaction)
    {
        yield return new Bin(nameof(FinancialTransaction.Id), transaction.Id.ToString());
        yield return new Bin(nameof(FinancialTransaction.ReceiverId), transaction.ReceiverId.ToString());
        yield return new Bin(nameof(FinancialTransaction.SenderId), transaction.SenderId.ToString());
        yield return new Bin(nameof(FinancialTransaction.Status), transaction.Status.ToString());
        yield return new Bin(nameof(FinancialTransaction.CreatedAt), JsonConvert.SerializeObject(transaction.CreatedAt));
        yield return new Bin(nameof(FinancialTransaction.UpdatedAt), JsonConvert.SerializeObject(transaction.UpdatedAt));
        if (transaction.Error != null)
        {
            yield return new Bin(nameof(FinancialTransaction.Error), JsonConvert.SerializeObject(transaction.Error));
        }
    }

    protected override IEnumerable<string> GetJsonBins()
    {
        yield return nameof(FinancialTransaction.Error);
    }

    private Key GetKey(FinancialTransaction transaction) => GetKey(transaction.Id);
}

