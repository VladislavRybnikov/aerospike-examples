using System;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.Threading.Channels;
using Aerospike.Client;

namespace OnlineBanking.Persistance;

/// <summary>
/// Converts Aerospike's <see cref="RecordSequenceListener"/> to <see cref="IAsyncEnumerable{T}"/>
/// </summary>
public class RecordsAsyncResult : RecordSequenceListener, IAsyncEnumerator<Record>, IAsyncEnumerable<Record>
{
    private readonly Channel<Record> _channel;
    private bool _completed;
    private Exception? _exception;

    public RecordsAsyncResult()
    {
        _channel = Channel.CreateUnbounded<Record>();
        Current = null!;
    }

    public Record Current { get; private set; }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public IAsyncEnumerator<Record> GetAsyncEnumerator(CancellationToken cancellationToken = default) => this;

    public async ValueTask<bool> MoveNextAsync()
    {
        if (_exception != null) throw _exception;
        Record? record = null;
        try
        {
            if (_completed && !_channel.Reader.TryRead(out record)) return false;
            Current = record ?? await _channel.Reader.ReadAsync();
        }
        catch
        {
            return false;
        }
        return true;
    }

    public void OnFailure(AerospikeException exception)
    {
        _exception = exception;
        _completed = true;
    }

    public void OnRecord(Key key, Record record)
    {
        _channel.Writer.WriteAsync(record).GetAwaiter().GetResult();
    }

    public void OnSuccess()
    {
        _completed = true;
        _channel.Writer.Complete();
    }
}
