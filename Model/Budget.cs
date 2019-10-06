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

        public Budget()
        {
            SummaryPath = @"Database\" + this.Name + "\\Summary" + this.Name + ".txt";
            //ItemsPath = @"Database\" + this.Name + "\\Items" + this.Name + ".krs";
            //CostsPath = @"Database\" + this.Name + "\\Costs" + this.Name + ".krs";
            //BudgetPath = @"Database\" + this.Name + "\\Budget" + this.Name + ".krs";
        }
    }
}
