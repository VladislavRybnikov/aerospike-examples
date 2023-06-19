using System;
namespace OnlineBanking.Domain.Repositories;

public interface IUserRepository
{
    Task InsertAsync(User user);

    Task<User?> GetByIdAsync(Guid userId);

    Task<IReadOnlyCollection<User>> GetAllAsync();

    Task UpdateAsync(User user);
}

