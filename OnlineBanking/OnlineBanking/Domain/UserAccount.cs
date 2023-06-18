namespace OnlineBanking.Domain;

public record UserAccount(Guid id, string Number, AccountBalance Balance);