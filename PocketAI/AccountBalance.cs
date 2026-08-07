using System;
using System.Collections.Generic;
using System.Text;

//Represents the account balance of the user
public class AccountBalance
{
    public double CheckingBalance { get; set; }

    public double SavingsBalance { get; set; }

    public double CashBalance { get; set; }

    //Builds a new account balance object
   public AccountBalance(double checkingBalance, double savingsBalance, double cashBalance)
    {
        CheckingBalance = checkingBalance;
        SavingsBalance = savingsBalance;
        CashBalance = cashBalance;
    }
    //Calculates the total money available
    public double GetTotalBalance()
    {
        return CheckingBalance + SavingsBalance + CashBalance;
    }
}
