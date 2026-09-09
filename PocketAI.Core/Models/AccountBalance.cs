// Represents the user's real account balances.
public class AccountBalance
{
    public double CheckingBalance
    {
        get;
        set;
    }


    public double SavingsBalance
    {
        get;
        set;
    }


    // ==========================================
    // LEGACY CASH FIELD
    // ==========================================
    //
    // PocketAI no longer uses physical cash.
    //
    // This remains temporarily so older database
    // code can still compile safely.
    //
    // It always behaves as $0.
    // ==========================================

    public double CashBalance
    {
        get => 0;

        set
        {
            // Intentionally ignored.
        }
    }


    // ==========================================
    // NEW CONSTRUCTOR
    // ==========================================

    public AccountBalance(
        double checkingBalance,
        double savingsBalance)
    {
        CheckingBalance =
            checkingBalance;


        SavingsBalance =
            savingsBalance;
    }


    // ==========================================
    // LEGACY CONSTRUCTOR
    // ==========================================
    //
    // Keeps older PocketAI code compatible while
    // Cash is removed from the app.
    // ==========================================

    public AccountBalance(
        double checkingBalance,
        double savingsBalance,
        double cashBalance)
        : this(
            checkingBalance,
            savingsBalance)
    {
    }


    // ==========================================
    // TOTAL ACCOUNT BALANCE
    // ==========================================

    public double GetTotalBalance()
    {
        return
            CheckingBalance +
            SavingsBalance;
    }
}
