using System.Collections.Generic;

namespace MoneyExperiment.Model
{
    public class Account
    {
        public List<Wallet> Wallet { get; set; } = new List<Wallet>();
        public Budget Budget { get; set; } = new Budget();
    }
}