using System;
using System.Data.Common;
using System.Transactions;
using Aerospike.Client;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OnlineBanking.Domain;
using OnlineBanking.Domain.Repositories;
using User = OnlineBanking.Domain.User;

namespace OnlineBanking.Persistance;

public class AerospikeUserRepository : BaseAerospikeRepository<User>, IUserRepository
{
    public AerospikeUserRepository(IOptions<AerospikeOptions> options) : base(options)
    {
    }

    protected override string Set => AerospikeOptions.Sets!.Users!;

    public async Task<IReadOnlyCollection<User>> GetAllAsync()
    {
        var result = new List<User>();
        await foreach (var model in QueryModelsAsync())
        {
            result.Add(model);
        }

        return result;
    }

    public async Task<User?> GetByIdAsync(Guid userId)
    {
        var policy = new Policy() { socketTimeout = 300 };
        var record = await AerospikeClient.Get(policy, CancellationToken.None, GetKey(userId));
        return ToModel(record);
    }

    public async Task InsertAsync(User user)
    {
        var writePolicy = new WritePolicy() { sendKey = true, recordExistsAction = RecordExistsAction.CREATE_ONLY };
        await AerospikeClient.Put(writePolicy, CancellationToken.None, GetKey(user), GetBins(user));
    }

    public async Task UpdateAsync(User user)
    {
        var writePolicy = new WritePolicy() { recordExistsAction = RecordExistsAction.UPDATE_ONLY };

        IEnumerable<Bin> BinsToUpdate()
        {
            yield return new Bin(nameof(User.UpdatedAt), JsonConvert.SerializeObject(user.UpdatedAt));
            yield return GetDataBin(user);
        }

        await AerospikeClient.Put(writePolicy, CancellationToken.None, GetKey(user), BinsToUpdate().ToArray());
    }

    protected override IEnumerable<Bin> GetFixedBins(User user)
    {
        yield return new Bin(nameof(User.Id), user.Id.ToString());
        yield return new Bin(nameof(User.CreatedAt), JsonConvert.SerializeObject(user.CreatedAt));
        yield return new Bin(nameof(User.UpdatedAt), JsonConvert.SerializeObject(user.UpdatedAt));
    }

    private Key GetKey(User user) => GetKey(user.Id);
}

