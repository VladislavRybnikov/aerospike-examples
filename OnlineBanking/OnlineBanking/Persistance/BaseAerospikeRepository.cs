using System;
using Aerospike.Client;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OneOf.Types;
using OnlineBanking.Domain;

namespace OnlineBanking.Persistance;

public abstract class BaseAerospikeRepository<T> where T : class
{
    public const string DataBin = "Data";

    protected readonly AsyncClient AerospikeClient;
    protected readonly AerospikeOptions AerospikeOptions;

    public BaseAerospikeRepository(IOptions<AerospikeOptions> options)
    {
        var host = new Aerospike.Client.Host(options.Value.Host, options.Value.Port);
        AerospikeClient = new AsyncClient(null, host);
        AerospikeOptions = options.Value;
    }

    protected RecordsAsyncResult QueryRecordsAsync(Action<Statement>? statementSetup = null)
    {
        var statement = new Statement();
        statement.SetNamespace(AerospikeOptions.Namespace);
        statement.SetSetName(Set);
        statementSetup?.Invoke(statement);

        var recordsAsyncResult = new RecordsAsyncResult();
        AerospikeClient.Query(null, recordsAsyncResult, statement);

        return recordsAsyncResult;
    }

    protected async IAsyncEnumerable<T> QueryModelsAsync(Action<Statement>? statementSetup = null)
    {
        var recordsAsyncResult = QueryRecordsAsync(statementSetup);
        await foreach (var record in recordsAsyncResult)
        {
            yield return ToModel(record)!;
        }
    }

    protected Bin[] GetBins(T model)
    {
        return GetFixedBins(model)
            .Append(GetDataBin(model))
            .ToArray();
    }

    protected Bin GetDataBin(T model)
    {
        var fixedBins = GetFixedBins(model);
        var data = JObject.FromObject(model);
        foreach (var bin in fixedBins)
        {
            data.Remove(bin.name);
        }

        return new Bin(DataBin, data.ToString());
    }

    protected T? ToModel(Record record)
    {
        if (record == null || record.bins == null || record.bins.Count == 0) return null;
        JObject? data = null;

        try
        {
            data = JObject.Parse(record.bins[DataBin].ToString()!);
            var jsonBins = GetJsonBins().ToHashSet();
            var dateTimeBins = GetDateTimeBins().ToHashSet();

            foreach (var bin in record.bins)
            {
                Func<string, JToken>? modification = null;
                if (jsonBins.Contains(bin.Key))
                {
                    modification = JObject.Parse;
                }
                else if (dateTimeBins.Contains(bin.Key))
                {
                    modification = JToken.Parse;
                }
                TryAddSafe(data, bin, modification);
            }

            return data.ToObject<T>();
        }
        catch
        {
            return null;
        }

        static void TryAddSafe(JObject data, KeyValuePair<string, object> bin, Func<string, JToken>? modification = null)
        {
            var stringValue = bin.Value.ToString();
            if (modification != null)
            {
                if (!string.IsNullOrEmpty(stringValue))
                {
                    try
                    {
                        data.TryAdd(bin.Key, modification(stringValue));
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }
            else
            {
                data.TryAdd(bin.Key, stringValue);
            }
        }
    }

    protected Key GetKey(Guid id) => new Key(AerospikeOptions.Namespace, Set, id.ToString());

    protected abstract string Set { get; }

    protected abstract IEnumerable<Bin> GetFixedBins(T model);

    protected virtual IEnumerable<string> GetJsonBins()
    {
        yield break;
    }

    protected virtual IEnumerable<string> GetDateTimeBins()
    {
        yield return "CreatedAt";
        yield return "UpdatedAt";
    }
}

