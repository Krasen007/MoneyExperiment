namespace MoneyExperiment.Model
{
    public class Account
    {
        ////public double NetWorth { get; set; }
        public Wallet Wallet { get; set; } = new Wallet();
        public Budget Budget { get; set; } = new Budget();
    }
}