namespace MoneyExperiment.Model
{
    public class Wallet
    {
        public double WalletAmount { get; set; }
        public string WalletName { get; set; } = "Default Wallet";
        public string AmountFilePath { get; set; } = string.Empty;
    }
}
