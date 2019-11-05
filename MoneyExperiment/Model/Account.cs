namespace MoneyExperiment.Model
{
    public class Account
    {
        public double AccAmount { get; set; }
        public string AccName { get; set; } = "Default Account";

        public Budget Budget { get; set; } = new Budget();
    }
}