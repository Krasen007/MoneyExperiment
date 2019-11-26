/*
    Money Experiment Experimental console budgeting app.
    Built on .net core. Use it to sync between PCs.
    Copyright (C) 2019  Krasen Ivanov

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

namespace MoneyExperiment.Model
{
    public class Wallet
    {
        public double WalletAmount { get; set; }
        public string WalletName { get; set; } = "Default Wallet";
        public string AmountAndNameFilePath { get; set; } = string.Empty;
    }
}