namespace MoneyExperiment.Model
{
    using System.Collections.Generic;

    public class Budget
    {
        public List<string> UserInputItem { get; set; } = new List<string>();
        public List<double> UserInputCost { get; set; } = new List<double>();
        public double Amount { get; set; } = 0;
        public string Name { get; set; } = "Default Budget";
        public string SummaryPath { get; set; } = string.Empty;
        public string ItemsPath { get; set; } = string.Empty;
        public string CostsPath { get; set; } = string.Empty;
        public string BudgetPath { get; set; } = string.Empty;
        public string AllTransactionsPath { get; set; } = string.Empty;
        public List<string> AllUserTransactionFile { get; set; } = new List<string>();
        public List<string> TranasctionTime { get; set; } = new List<string>();
    }
}