namespace MoneyExperiment.Model
{
    using System.Collections.Generic;
    public class Budget
    {
        public List<string> UserInputItem { get; set; }
        public List<double> UserInputCost { get; set; }
        public double BudgetAmount { get; set; }
        public string BudgetName { get; set; }

        public Budget()
        {
            this.UserInputItem = new List<string>();
            this.UserInputCost = new List<double>();
            this.BudgetAmount = 0;
            this.BudgetName = string.Empty;
        }
    }
}
