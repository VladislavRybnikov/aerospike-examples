using System;
using Aerospike.Client;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using OnlineBanking.Domain;

namespace OnlineBanking.Persistance;

public class AerospikeFinancialTransactionRepository : IFinancialTransactionRepository
{
    private readonly AsyncClient _aerospikeClient;
    private readonly AerospikeOptions _aerospikeOptions;

    public AerospikeFinancialTransactionRepository(IOptions<AerospikeOptions> options)
    {
        var host = new Aerospike.Client.Host(options.Value.Host, options.Value.Port);
        _aerospikeClient = new AsyncClient(null, host);
        _aerospikeOptions = options.Value;
    }

    public async Task DeleteAsync(Guid id)
    {
        await _aerospikeClient.Delete(null, CancellationToken.None, GetKey(id));
    }

    public async Task<IReadOnlyCollection<FinancialTransaction>> GetAllIncommingTransactions(Guid userId)
    {
        var statement = new Statement();
        statement.SetNamespace(_aerospikeOptions.Namespace);
        statement.SetSetName(_aerospikeOptions.Sets!.Transactions);
        statement.SetFilter(Filter.Equal("ReceiverId", userId.ToString()));

        var recordsAsyncResult = new RecordsAsyncResult();
        _aerospikeClient.Query(null, recordsAsyncResult, statement);

        var result = new List<FinancialTransaction>();
        await foreach (var record in recordsAsyncResult)
        {
            result.Add(ToFinancialTransaction(record)!);
        }

        return result;
    }

    public async Task<FinancialTransaction?> GetByIdAsync(Guid id)
    {
        var policy = new Policy() { socketTimeout = 300 };
        var record = await _aerospikeClient.Get(policy, CancellationToken.None, GetKey(id));
        return ToFinancialTransaction(record);
    }

    public async Task InsertAsync(FinancialTransaction transaction)
    {
        var writePolicy = new WritePolicy() { sendKey = true, recordExistsAction = RecordExistsAction.CREATE_ONLY };
        await _aerospikeClient.Put(writePolicy, CancellationToken.None, GetKey(transaction), GetBins(transaction));
    }

    public async Task UpdateStatusAsync(Guid id, FinancialTransactionStatus status)
    {
        var writePolicy = new WritePolicy() { recordExistsAction = RecordExistsAction.UPDATE_ONLY };
        var statusBin = new Bin("Status", status.ToString());

        await _aerospikeClient.Put(writePolicy, CancellationToken.None, GetKey(id), statusBin);
    }

    private static Bin[] GetBins(FinancialTransaction transaction)
    {
        return GetBinsEnumerable().ToArray();

        IEnumerable<Bin> GetBinsEnumerable()
        {
            yield return new Bin("Id", transaction.Id.ToString());
            yield return new Bin("ReceiverId", transaction.ReceiverId.ToString());
            yield return new Bin("SenderId", transaction.SenderId.ToString());
            yield return new Bin("Status", transaction.Status.ToString());

            var data = JObject.FromObject(transaction);
            data.Remove("Id");
            data.Remove("ReceiverId");
            data.Remove("SenderId");
            data.Remove("Status");

            yield return new Bin("Data", data.ToString());
        }
    }

    private static FinancialTransaction? ToFinancialTransaction(Record record)
    {
        var data = JObject.Parse(record.bins["Data"].ToString()!);
        foreach (var bin in record.bins)
        {
            data.TryAdd(bin.Key, bin.Value.ToString());
        }
        return data.ToObject<FinancialTransaction>();
    }

    private Key GetKey(Guid transactionId) => new Key(_aerospikeOptions.Namespace, _aerospikeOptions.Sets!.Transactions, transactionId.ToString());
    private Key GetKey(FinancialTransaction transaction) => GetKey(transaction.Id);
}

