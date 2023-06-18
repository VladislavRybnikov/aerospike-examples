using System;
using Aerospike.Client;
using Microsoft.Extensions.Options;
using Polly;

namespace OnlineBanking.Persistance;

public class AerospikeSetup
{
    private readonly AerospikeOptions _aerospikeOptions;
    private readonly ILogger _logger;

    public AerospikeSetup(
        IOptions<AerospikeOptions> options,
        ILogger<AerospikeSetup> logger)
    {
        _aerospikeOptions = options.Value;
        _logger = logger;
    }

    public void Setup()
    {
        try
        {
            Polly
                .Policy
                .Handle<Exception>()
                .Retry(3, (_, i) => TimeSpan.FromSeconds(Math.Pow(2, i)))
                .Execute(CreateIndicies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform Aerospike Setup. {Host}:{Port}", _aerospikeOptions.Host, _aerospikeOptions.Port);
            throw;
        }
    }

    private void CreateIndicies()
    {
        var host = new Aerospike.Client.Host(_aerospikeOptions.Host, _aerospikeOptions.Port);
        var client = new AsyncClient(null, host);

        client.CreateIndex(
            null,
            _aerospikeOptions.Namespace,
            _aerospikeOptions.Sets!.Transactions,
            "ReceiverId_Idx",
            "ReceiverId",
            IndexType.STRING).Wait();

        client.CreateIndex(
            null,
            _aerospikeOptions.Namespace,
            _aerospikeOptions.Sets!.Transactions,
            "SenderId_Idx",
            "SenderId",
            IndexType.STRING).Wait();
    }
}

