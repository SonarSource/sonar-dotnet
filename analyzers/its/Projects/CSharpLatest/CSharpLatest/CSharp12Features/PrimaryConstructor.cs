namespace CSharpLatest.CSharp12Features;

internal class PrimaryConstructor
{
    public class BankAccount(string accountID, string owner)
    {
        public string AccountID { get; } = accountID;
        public string Owner { get; } = owner;
        public decimal CreditLimit { get; } = 0;
        public BankAccount(string accountID, string owner, decimal creditLimit) : this(accountID, owner)
        {
            CreditLimit = creditLimit;
        }

        public override string ToString() => $"Account ID: {AccountID}, Owner: {Owner}";
    }
}
