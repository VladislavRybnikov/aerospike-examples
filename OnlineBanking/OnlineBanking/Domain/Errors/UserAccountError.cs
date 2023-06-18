using System;
namespace OnlineBanking.Domain;

public record UserAccountError : DomainError
{
    public UserAccountError(DomainErrorType type, string message, Dictionary<string, object?>? metadata = null)
        : base(type, message, metadata)
    {
    }

    public static UserAccountError AccountNotFound(string? currency = null)
    {
        return new(
            DomainErrorType.NotFound,
            "User account not found",
            new MetadataBuilder()
                .AddIfNotEmpty(currency)
                .Build());
    }

    public static UserAccountError InsufficientFunds(decimal? balance = null, decimal? amount = null)
    {
        return new(
            DomainErrorType.NotFound,
            "User account has insufficient amount of funds.",
            new MetadataBuilder()
                .AddIfNotNull(balance)
                .AddIfNotNull(amount)
                .Build());
    }
}

