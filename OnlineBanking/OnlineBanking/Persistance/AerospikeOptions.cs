using System;
namespace OnlineBanking.Persistance;

public class AerospikeOptions
{
	public string? Host { get; set; }

	public int Port { get; set; }

	public string? Namespace { get; set; }

	public AerospikeSets? Sets { get; set; }
}

public class AerospikeSets
{
	public string? Transactions { get; set; }
}
