using System;
using OneOf;

namespace OnlineBanking.Domain.FinancialTransactionDetails;

public class Details
{
	// for serialization
	public Details() { }

	public Details(OneOf<DepositDetails?, WithdrwalDetails?, TransferDetails?> oneOf)
	{
		oneOf.Switch(
			deposit => Deposit = deposit,
			withdrawal => Withdrwal = withdrawal,
			transfer => Transfer = transfer
			);
	}

	public DepositDetails? Deposit { get; set; }
    public WithdrwalDetails? Withdrwal { get; set; }
    public TransferDetails? Transfer { get; set; }
}

