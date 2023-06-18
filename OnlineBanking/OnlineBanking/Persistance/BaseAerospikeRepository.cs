using System;
using Aerospike.Client;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        var data = JObject.Parse(record.bins[DataBin].ToString()!);
        var jsonBins = GetJsonBins().ToHashSet();
        var dateTimeBins = GetDateTimeBins().ToHashSet();

        foreach (var bin in record.bins)
        {
            if (jsonBins.Contains(bin.Key) && bin.Value.ToString() is { } value)
            {
                data.TryAdd(bin.Key, JObject.Parse(value));
            }
            else if (dateTimeBins.Contains(bin.Key) && bin.Value.ToString() is { } dateValue)
            {
                data.TryAdd(bin.Key, JToken.Parse(dateValue));
            }
            else
            {
                data.TryAdd(bin.Key, bin.Value.ToString());
            }
        }
        return data.ToObject<T>();
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

