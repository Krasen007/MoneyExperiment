﻿namespace MoneyExperiment.Model
{
    public class Account
    {
        public double Amount { get; set; }
        public string Name { get; set; } = "Default Account";

        public Budget Budget { get; set; } = new Budget();
    }
}