namespace MoneyExperiment.Model
{
    public class Account
    {
        public double AccAmount { get; set; }
        public string AccName { get; set; } = "Default Account";
        public Budget Budget { get; set; } = new Budget();
        public string AccAmountFilePath { get; set; } = string.Empty;


        //     public AccountNameAndAmount NameAndAmount { get; set; } = new AccountNameAndAmount();

        //     public struct AccountNameAndAmount
        //     {
        //         public AccountNameAndAmount(double accAmount, string accName)
        //         {
        //             AccAmount = accAmount;
        //             AccName = accName;
        //         }

        //         public double AccAmount { get; set; }
        //         public string AccName { get; set; }
        //     }
    }
}