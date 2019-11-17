using System.Collections.Generic;

namespace MoneyExperiment.Model
{
    public class Account
    {
        ////public double NetWorth { get; set; }
        public List<Wallet> Wallet { get; set; } = new List<Wallet>();

        ////public Wallet Wallet { get; set; } = new Wallet();
        public Budget Budget { get; set; } = new Budget();
    }
}