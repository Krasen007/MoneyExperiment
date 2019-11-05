namespace MoneyExperiment.Model
{
    using System.Collections.Generic;

    public class Budget
    {
        public List<string> UserInputItem { get; set; } = new List<string>();
        public List<double> UserInputCost { get; set; } = new List<double>();
        public double Amount { get; set; }
        public string Name { get; set; } = "Default Budget";
        public string SummaryFilePath { get; set; } = string.Empty;
        public string ItemsFilePath { get; set; } = string.Empty;
        public string CostsFilePath { get; set; } = string.Empty;
        public string BudgetFilePath { get; set; } = string.Empty;
        public string AllTransactionsFilePath { get; set; } = string.Empty;
        public List<string> AllUserTransactionFile { get; set; } = new List<string>();
        public List<string> TranasctionTime { get; set; } = new List<string>();
    }
}