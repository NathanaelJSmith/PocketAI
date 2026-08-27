using System;
using System.Collections.Generic;


// ==========================================
// POCKETAI FINANCIAL SNAPSHOT PROVIDER
// ==========================================
//
// This class connects:
//
// DATABASE DATA
//
//       ↓
//
// FinancialCalculationService
//
//       ↓
//
// FinancialSnapshot
//
//
// Pages should ask this provider for the
// current snapshot instead of rebuilding
// financial calculations themselves.
//
// This keeps Home, Analytics, Savings,
// PocketAI, Budget, and other pages using
// the exact same financial definitions.
// ==========================================

public class FinancialSnapshotProvider
{
    // ==========================================
    // DEPENDENCIES
    // ==========================================

    private readonly DataBaseManager
        dataBaseManager;


    private readonly FinancialCalculationService
        financialCalculationService;



    // ==========================================
    // CONSTRUCTOR
    // ==========================================

    public FinancialSnapshotProvider(
        DataBaseManager dataBaseManager)
    {
        this.dataBaseManager =
            dataBaseManager;


        financialCalculationService =
            new FinancialCalculationService();
    }



    // ==========================================
    // GET CURRENT FINANCIAL SNAPSHOT
    // ==========================================

    public FinancialSnapshot GetSnapshot(
        double acceptedExtraSavings = 0)
    {
        // ======================================
        // LOAD TRANSACTIONS
        // ======================================

        List<Expense> expenses =
            dataBaseManager
                .GetAllExpenses();



        // ======================================
        // LOAD EXPECTED INCOME
        // ======================================

        Income? income =
            dataBaseManager
                .GetIncome();



        // ======================================
        // LOAD CURRENT ACCOUNT BALANCES
        // ======================================

        AccountBalance? accountBalance =
            dataBaseManager
                .GetAccountBalance();



        // ======================================
        // LOAD ALL SAVINGS GOALS
        // ======================================

        List<SavingsGoal> savingsGoals =
            dataBaseManager
                .GetSavingsGoals();



        // ======================================
        // LOAD BUDGET LIMITS
        // ======================================

        List<BudgetLimit> budgetLimits =
            dataBaseManager
                .GetBudgetLimits();



        // ======================================
        // LOAD RECURRING BILLS
        // ======================================

        List<RecurringExpenses>
            recurringExpenses =
                dataBaseManager
                    .GetRecuringExpenses();



        // ======================================
        // BUILD ONE TRUSTED SNAPSHOT
        // ======================================

        FinancialSnapshot snapshot =
            financialCalculationService
                .BuildSnapshot(
                    expenses,
                    income,
                    accountBalance,
                    savingsGoals,
                    budgetLimits,
                    recurringExpenses,
                    acceptedExtraSavings);



        return snapshot;
    }
}