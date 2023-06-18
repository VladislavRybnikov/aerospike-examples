using System;
using OnlineBanking.Domain;

namespace OnlineBanking.Domain;

public record FinancialTransactionError : DomainError
{
    public FinancialTransactionError(DomainErrorType type, string message, Dictionary<string, object?>? metadata = null) : base(type, message, metadata)
    {
    }

    public static FinancialTransactionError UnknownUser()
	{
		return new(DomainErrorType.NotFound, "Unknonwn user.");
	}

    public static FinancialTransactionError InvalidFinancialTransactionType()
    { 
        return new(DomainErrorType.BadRequest, "Invalid transaction type.");
    }

    public static FinancialTransactionError InvalidOperation(string? details = null)
    {
        return new(
            DomainErrorType.BadRequest,
            "Invalid operation.",
            new MetadataBuilder()
                .AddIfNotEmpty(details)
                .Build());
    }
}