using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Linq;
using System.Diagnostics;
using System.Transactions;
using Microsoft.VisualBasic;

class Progam
{
    //This list stores all the expenses while the app is running
    static List<Expense> expenses = new List<Expense>();

    //Gives each new expense an Id
    static int nextExpenseId = 1;

    //Stores the users income information
    static Income userIncome = null;

    //Stores the users savings goals
    static SavingsGoal userSavingsGoal = null;

    //Stores the user's account balances
    static AccountBalance userAccountBalance = null;

    //Stores all the budget limits
    static List<BudgetLimit> budgetLimits = new List<BudgetLimit>();

    //Does the saving and loading data from Sqlite
    static DataBaseManager dataBaseManager = new DataBaseManager();

    static AIService aiService = new AIService(
        @"C:\Users\Owner\Documents\GitHub\PocketAI\PocketAI"
    );

    static AnalyticsService analyticsService = new AnalyticsService();



    static void Main()
    {

        //Creates the file when the app starts
        dataBaseManager.CreateTables();

        //Loads all saved expenses from database
        expenses = dataBaseManager.GetAllExpenses();

        //This sets the next ID based on the highest saved expense ID
        if (expenses.Count > 0)
        {
            nextExpenseId = expenses.Max(expense => expense.Id) + 1;
        }

        //Loads the income from the database
        userIncome = dataBaseManager.GetIncome();

        //Loads the savings goal from the database
        userSavingsGoal = dataBaseManager.GetSavingsGoal();

        //Loads saved budget limits from the database
        budgetLimits = dataBaseManager.GetBudgetLimits();

        //Loads saved account balance from the database
        userAccountBalance = dataBaseManager.GetAccountBalance();

        bool running = true;

        while (running)
        {

            /*
             * Going to Orginize the Menu next 
             */
            Console.Clear();

            Console.WriteLine("==== PocketAI ====");
            Console.WriteLine();

            Console.WriteLine("--- Expenses ---");
            Console.WriteLine("1. Add Expense");
            Console.WriteLine("2. View Expense");
            Console.WriteLine("3. View Total Expenses");
            Console.WriteLine("4. Delete Expense");
            Console.WriteLine("5. Edit Expenses");
            Console.WriteLine("6. Filtered by Categories");
            Console.WriteLine("7. Total by Category");
            Console.WriteLine("8. Category BreakDown");
            Console.WriteLine("9. View Current Month Spending");
            Console.WriteLine("10. Add Recurring Expense");
            Console.WriteLine();

            Console.WriteLine("--- Income ---");
            Console.WriteLine("11. Set Monthly Income");
            Console.WriteLine("12. View Monthly Income");
            Console.WriteLine();

            Console.WriteLine("--- Savings ---");
            Console.WriteLine("13. Set Savings Goal");
            Console.WriteLine("14. View Savings Goal");
            Console.WriteLine("15. View Savings Plan");
            Console.WriteLine("16. Add Money to Savings Goal");
            Console.WriteLine("17. With draw From Savings Goal");
            Console.WriteLine();

            Console.WriteLine("--- Budget Limits ---");
            Console.WriteLine("18. Add Budget Limit");
            Console.WriteLine("19. View Budget Limits");
            Console.WriteLine("20. Check Budget Limits");
            Console.WriteLine("21. Delete Budget Limit");
            Console.WriteLine("22. Edit Budget Limit");
            Console.WriteLine();

            Console.WriteLine("--- Account Balance ---");
            Console.WriteLine("23. Set Account Balance");
            Console.WriteLine("24. View Account Balance");
            Console.WriteLine();

            Console.WriteLine("--- PocketAI Coach ---");
            Console.WriteLine("25. AI Money Coach");
            Console.WriteLine("26. View Financial Sumamry");
            Console.WriteLine("27. View Safe-to-Spend Amount");
            Console.WriteLine("28. View Daily Safe-to-Spend");
            Console.WriteLine("29. View Weekly Safe-to-Spend Limit");
            Console.WriteLine("30. View AI Prompt");
            Console.WriteLine("31. View Python AI Advice");
            Console.WriteLine("32. View AI Advice History");
            Console.WriteLine("33. View Monthly Report");
            Console.WriteLine("34. View AI Advice History Summary");
            Console.WriteLine("35. View AI Advice by ID");
            Console.WriteLine("36. Delete AI Advice.");
            Console.WriteLine("37. Search AI Advice History.");
            Console.WriteLine();

            Console.WriteLine("--- Recently Added Going to Orginize later ---");
            Console.WriteLine("38. View Recurring Expenses");
            Console.WriteLine("39. View Upcoming Bills");
            Console.WriteLine("40. Weekly Spending Report");
            Console.WriteLine("41. Weekly Spending Comparison");
            Console.WriteLine("42. Monthly Spending Comparison");
            Console.WriteLine("43. Cash Flow Forecast");

            Console.WriteLine("44. Exit");
            Console.WriteLine();
            Console.WriteLine("Choose an option");

            string choice = Console.ReadLine();

            //Choices for the user
            switch (choice)
            {
                case "1":
                    AddExpense();
                    break;

                case "2":
                    ViewExpenses();
                    break;

                case "3":
                    ViewTotalSpent();
                    break;

                case "4":
                    DeleteExpense();
                    break;

                case "5":
                    EditExpense();
                    break;

                case "6":
                    FilteredbyCategory();
                    break;

                case "7":
                    TotalByCategory();
                    break;

                case "8":
                    CategoryBreakdown();
                    break;

                case "9":
                    ViewCurrentMonthSpending();
                    break;
                case "10":
                    AddRecurringExpenseMenu();
                    break;

                case "11":
                    SetMonthlyIncome();
                    break;

                case "12":
                    ViewMonthlyIncome();
                    break;

                case "13":
                    SetSavingsGoal();
                    break;

                case "14":
                    ViewSavingsGoal();
                    break;

                case "15":
                    ViewSavingsPlan();
                    break;

                case "16":
                    AddMoneyTosavingsGoal();
                    break;

                case "17":
                    WithDrawFromSavingsGoal();
                    break;

                case "18":
                    AddBudgetLimits();
                    break;

                case "19":
                    ViewBudgetLimits();
                    break;

                case "20":
                    CheckBudgetLimits();
                    break;

                case "21":
                    DeleteBudgetLimit();
                    break;

                case "22":
                    EditBudgetLimits();
                    break;

                case "23":
                    SetAccountBalance();
                    break;

                case "24":
                    ViewAccountBalance();
                    break;

                case "25":
                    AIMoneyCoach();
                    break;

                case "26":
                    ViewFinancialSummary();
                    break;

                case "27":
                    ViewSafeToSpend();
                    break;

                case "28":
                    ViewDailySafeToSpend();
                    break;

                case "29":
                    ViewWeeklySafeToSpend();
                    break;

                case "30":
                    ViewAIPrompt();
                    break;

                case "31":
                    ViewPythonAIAdivce();
                    break;

                case "32":
                    ViewAIAdviceHistory();
                    break;
                case "33":
                    ViewMonthlyReport();
                    break;
                case "34":
                    ViewAIAdviceHistorySummary();
                    break;
                case "35":
                    VewAIAdviceById();
                    break;
                case "36":
                    DeleteAIAdvice();
                    break;
                case "37":
                    SearchAIAdviceHistory();
                    break;
                case "38":
                        ViewRecurringExpenses();
                    break;
                case "39":
                    ViewUpComingBill();
                    break;
                case "40":
                    ViewWeeklyReport();
                    break;
                case "41":
                    ViewWeeklyComparison();
                    break;
                case "42":
                    ViewMonthComparison();
                    break;
                    case "43":
                    ViewCashFlowForecast();
                    break;
                case "44":
                    running = false;
                    break;

                default:
                    Console.WriteLine("Invalid option. Press Enter to try again.");
                    Console.ReadLine();
                    break;
            }
        }
    }



    //Method to enter expense 
    static void AddExpense()
    {
        Console.Clear();

        Console.WriteLine("=== Add Expense ===");

        Console.Write("Expense name: ");
        string name = Console.ReadLine();

        Console.Write("Amount: ");
        double amount = double.Parse(Console.ReadLine());

        Console.Write("Category: ");
        string category = Console.ReadLine();

        Console.Write("Date (example: 5/9/2026): ");
        DateTime date = DateTime.Parse(Console.ReadLine());

        // Creates a new expense object
        Expense newExpense = new Expense(nextExpenseId, name, amount, category, date);

        // Adds the expense to the list
        expenses.Add(newExpense);

        //Saves the expense permanently in the database
        dataBaseManager.AddExpense(newExpense);

        //Moves the ID number for the next expense 
        nextExpenseId++;

        Console.WriteLine("Expense added successfully.");
        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();
    }

    //Method to view the expense 
    static void ViewExpenses()
    {
        Console.Clear();

        Console.WriteLine("=== All Expense ===");

        if (expenses.Count == 0)
        {
            Console.WriteLine("There are no expenses found.");
        }
        else
        {
            //Shows the expenses 
            foreach (Expense expense in expenses)
            {
                Console.WriteLine("-------------------");
                Console.WriteLine($"Id: {expense.Id}" + " | " +
                    $"Name: {expense.Name}" + " | " +
                    $"Amount: {expense.Amount:C}" + " | " +
                    $"Category: {expense.Category}" + " | " +
                    $"Date: {expense.Date}");

            }
        }

        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();
    }

    //Method to view the total of your expenses 
    static void ViewTotalSpent()
    {
        Console.Clear();

        Console.WriteLine("=== Total Spent ===");

        //Start total at 0
        double total = 0;

        foreach (Expense expense in expenses)
        {
            total += expense.Amount;
        }

        Console.WriteLine($"Total spent: {total:C}");
        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();

    }

    //Shows total spending for the current month only 
    static void ViewCurrentMonthSpending()
    {
        Console.Clear();

        Console.WriteLine("=== Current Month Spending ===");

        List<Expense> currentMonthExpenses = analyticsService.GetCurrentMonthExpense(expenses);

        if (currentMonthExpenses.Count == 0)
        {
            Console.WriteLine("No expenses found for the current month.");
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        double total = 0;

        foreach (Expense expense in currentMonthExpenses)
        {
            total += expense.Amount;
        }

        Console.WriteLine($"Current Month Total Spent: {total:C}");
        Console.WriteLine("Press Enter to Continue.");
        Console.ReadLine();
    }

    //Method that lets the user set their account balance
    static void SetAccountBalance()
    {
        Console.Clear();

        Console.WriteLine("=== Set Account Balance");

        Console.WriteLine("Checking balance: ");
        bool checkIsValid = double.TryParse(Console.ReadLine(), out double checkingBalance);

        if (!checkIsValid)
        {
            Console.WriteLine("Invalid checking balance. Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        Console.WriteLine("Savings balance: ");
        bool savingsIsValid = double.TryParse(Console.ReadLine(), out double savingsBalance);

        if (!savingsIsValid)
        {
            Console.WriteLine("Invalid savings balance. Press Enter to Continue.");
            Console.ReadLine();
            return;
        }

        Console.WriteLine("Cash Balance: ");
        bool cashIsValid = double.TryParse(Console.ReadLine(), out double cashBalance);

        if (!cashIsValid)
        {
            Console.WriteLine("Invalid cash Balance. Press Eter to continue.");
            Console.ReadLine();
            return;
        }

        //Stores the account balance while the app is running 
        userAccountBalance = new AccountBalance(checkingBalance, savingsBalance, cashBalance);

        //Saves the account balance permanently in the database
        dataBaseManager.SaveAccountBalance(userAccountBalance);

        Console.WriteLine("Account balance saved successfully.");
        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();

    }

    //Method that shows the user account balance
    static void ViewAccountBalance()
    {
        Console.Clear();

        Console.WriteLine("=== View Account Balance: ");

        if (userAccountBalance == null)
        {
            Console.WriteLine("No account balance has been seet yet.");
            Console.WriteLine("Go to option 19. to set Account Balance.");
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        Console.WriteLine($"Checking: {userAccountBalance.CheckingBalance:C}");
        Console.WriteLine($"Savings: {userAccountBalance.SavingsBalance:C}");
        Console.WriteLine($"Cash: {userAccountBalance.CashBalance:C}");
        Console.WriteLine("------------------------");
        Console.WriteLine($"Total Balance: {userAccountBalance.GetTotalBalance():C}");

        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();

    }

    //Adds money to users current savings goal
    static void AddMoneyTosavingsGoal()
    {
        Console.Clear();

        Console.WriteLine("=== Add Money to Savings Goal ===");

        if (userSavingsGoal == null)
        {
            Console.WriteLine("No savings goal has been added yet.");
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        Console.WriteLine($"Goal: {userSavingsGoal.Name}");
        Console.WriteLine($"Current Saved: {userSavingsGoal.CurrentAmount:C}");
        Console.WriteLine($"Target Amount: {userSavingsGoal.TargetAmount}");
        Console.WriteLine();

        Console.WriteLine("Amount to add: ");
        bool amountIsValid = double.TryParse(Console.ReadLine(), out double amountToAdd);

        if (!amountIsValid)
        {
            Console.WriteLine("Invalid amount. Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        if (amountToAdd <= 0)
        {
            Console.WriteLine("Amount must be greater than 0. Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        //Adds money to the current saved amount
        userSavingsGoal.CurrentAmount += amountToAdd;

        //Prevents the saved amount from going way past the target without warning
        if (userSavingsGoal.CurrentAmount >= userSavingsGoal.TargetAmount)
        {
            Console.WriteLine();
            Console.WriteLine("Congratualtions! You reached or passed your savings goal.");
        }

        //Saved the updated savings goal permanently 
        dataBaseManager.SaveSavingsGoal(userSavingsGoal);

        Console.WriteLine();
        Console.WriteLine($"New Saved Amount: {userSavingsGoal.CurrentAmount:C}");
        Console.WriteLine("Savings goal updated successfully.");
        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();
    }
    static void WithDrawFromSavingsGoal()
    {
        Console.Clear();

        Console.WriteLine("=== Withdraw From Savings Goal ===");

        if (userSavingsGoal == null)
        {
            Console.WriteLine("No savings goal set up yet.");
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        Console.WriteLine($"Goal: {userSavingsGoal.Name}");
        Console.WriteLine($"Current Saved: {userSavingsGoal.CurrentAmount}");
        Console.WriteLine($"Target Amount: {userSavingsGoal.TargetAmount}");

        Console.WriteLine("Amount to withdraw: ");
        bool amountIsValid = double.TryParse(Console.ReadLine(), out double amountToWithdraw);

        if (!amountIsValid)
        {
            Console.WriteLine("Invalid amount. Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        if (amountToWithdraw <= 0)
        {
            Console.WriteLine("Amount must be greater than 0. Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        if (amountToWithdraw > userSavingsGoal.CurrentAmount)
        {
            Console.WriteLine("You cannot withdraw more than your current saved amount.");
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        //subtracts money grom the current saved amount 
        userSavingsGoal.CurrentAmount -= amountToWithdraw;

        //Saves the updated savings goal permenantly.
        dataBaseManager.SaveSavingsGoal(userSavingsGoal);

        Console.WriteLine();
        Console.WriteLine($"New Savings Amount: {userSavingsGoal.CurrentAmount:C}");
        Console.WriteLine("Savings goal updated successfully.");
        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();
    }
    //Method to be able to Delete expense
    static void DeleteExpense()
    {
        Console.Clear();

        Console.WriteLine("=== Delete Expense ===");

        //If there are no expenses 
        if (expenses.Count == 0)
        {
            Console.WriteLine("No expenses found");
            Console.WriteLine("Press Enter to continue");
            Console.ReadLine();
            return;
        }

        //Shows the user what expense exists.
        foreach (Expense expense in expenses)
        {
            Console.WriteLine($"{expense.Id}. {expense.Name} - {expense.Amount:C} - {expense.Category}");
        }

        Console.WriteLine("To Delete the Expense select the Id: ");
        bool idIsValid = int.TryParse(Console.ReadLine(), out int id);

        if (!idIsValid)
        {
            Console.WriteLine("Invalid ID. Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        //Finds the expense with the matching Id
        Expense expensesToDelete = expenses.Find(expense => expense.Id == id);

        if (expensesToDelete == null)
        {
            Console.WriteLine("Expense not found. Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        //Removes the expense from the list
        expenses.Remove(expensesToDelete);

        dataBaseManager.DeleteExpenseById(id);

        Console.WriteLine("Expense deleted successfully.");
        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();
    }

    //Method to edit the expense
    static void EditExpense()
    {
        Console.Clear();
        Console.WriteLine("=== Edit Expense ===");

        if (expenses.Count == 0)
        {
            Console.WriteLine("No expenses to edit.");
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
            return;

        }

        //Shows all expenses for user
        foreach (Expense expense in expenses)
        {
            Console.WriteLine($"{expense.Id}. {expense.Name} - {expense.Amount} - {expense.Category} - {expense.Date.ToShortDateString()}");

        }

        Console.WriteLine("Enter the ID of the expense to edit: ");
        bool idIsValid = int.TryParse(Console.ReadLine(), out int id);

        if (!idIsValid)
        {
            Console.WriteLine("Invalid ID. Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        Expense expenseToEdit = expenses.Find(expense => expense.Id == id);

        if (expenseToEdit == null)
        {
            Console.WriteLine("Expense not found. Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        Console.WriteLine();
        Console.WriteLine("Leave blank and press Enter to keep the current value.");
        Console.WriteLine();

        Console.WriteLine($"New name ({expenseToEdit.Name}): ");
        string newName = Console.ReadLine();

        if (newName != "")
        {
            expenseToEdit.Name = newName;
        }

        Console.WriteLine($"New Amount ({expenseToEdit.Amount:C})");
        string amountInput = Console.ReadLine();

        if (amountInput != "")
        {
            bool amountIsValid = double.TryParse(amountInput, out double newAmount);

            if (!amountIsValid)
            {
                Console.Write("Invalid amount. Press Enter to continue.");
                Console.ReadLine();
                return;
            }

            expenseToEdit.Amount = newAmount;
        }

        Console.WriteLine($"New category ({expenseToEdit.Category})");
        string newCategory = Console.ReadLine();

        if (newCategory != "")
        {
            expenseToEdit.Category = newCategory;
        }

        Console.WriteLine($"New date ({expenseToEdit.Date.ToShortTimeString()})");
        string dateInput = Console.ReadLine();

        if (dateInput != "")
        {
            bool dateIsValid = DateTime.TryParse(dateInput, out DateTime newDate);

            if (!dateIsValid)
            {
                Console.WriteLine("Invalid date. Press Enter to continue.");
                return;
            }

            expenseToEdit.Date = newDate;
        }

        dataBaseManager.UpdateExpense(expenseToEdit);
        Console.WriteLine("Expense updated successfully");
        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();



    }

    //Shows the expenses that match a category 
    static void FilteredbyCategory()
    {
        Console.Clear();

        Console.WriteLine("=== Filtered by Category ===");

        if (expenses.Count == 0)
        {
            Console.WriteLine("No expenses found.");
            Console.WriteLine("Press Enter to continue");
            Console.ReadLine();
            return;
        }

        Console.WriteLine("Enter category)");
        string category = Console.ReadLine();

        bool foundExpense = false;

        foreach (Expense expense in expenses)
        {
            //Compares the categorys without caring about uppercase/lowercase
            if (expense.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("--------------");
                Console.WriteLine($"ID: {expense.Id}");
                Console.WriteLine($"Name: {expense.Name}");
                Console.WriteLine($"Amount: {expense.Amount:C}");
                Console.WriteLine($"Category: {expense.Category}");
                Console.WriteLine($"Date: {expense.Date.ToShortDateString()}");

                foundExpense = true;
            }
        }

        if (!foundExpense)
        {
            Console.WriteLine("No expenses found in that category");
        }

        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();
    }

    //Adds expenses from one category together
    static void TotalByCategory()
    {
        Console.Clear();

        Console.WriteLine("=== Total by Category ===");

        if (expenses.Count == 0)
        {
            Console.WriteLine("No expenses found.");
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        Console.WriteLine("Enter category: ");
        string category = Console.ReadLine();

        double total = 0;

        foreach (Expense expense in expenses)
        {
            //Adds only the expense that make the category
            if (expense.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            {
                total += expense.Amount;
            }
        }

        Console.WriteLine($"Total spent on {category}: {total:C}");
        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();


    }

    // Shows total spending for each category
    static void CategoryBreakdown()
    {
        Console.Clear();

        Console.WriteLine("=== Category Breakdown ===");

        if (expenses.Count == 0)
        {
            Console.WriteLine("No expenses found.");
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        // Groups expenses by category and adds each group together
        var categoryTotals = expenses
            .GroupBy(expense => expense.Category)
            .Select(group => new
            {
                Category = group.Key,
                Total = group.Sum(expense => expense.Amount)
            });

        foreach (var category in categoryTotals)
        {
            Console.WriteLine($"{category.Category}: {category.Total:C}");
        }

        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();
    }

    //Method to set the users monthly income
    static void SetMonthlyIncome()
    {
        Console.Clear();

        Console.WriteLine("=== Set Monthly Income");

        Console.WriteLine("Income source/job");
        string source = Console.ReadLine();

        Console.WriteLine("Monthly Income amount: ");
        bool incomeIsValid = double.TryParse(Console.ReadLine(), out double monthlyAmount);

        if (!incomeIsValid)
        {
            Console.WriteLine("Invalid income amount. Press Enter to continue.");
            Console.ReadLine();
        }

        //Saves the income information user has given
        userIncome = new Income(source, monthlyAmount);

        dataBaseManager.SaveIncome(userIncome);

        Console.WriteLine("Monthly income saved successfully.");
        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();
    }

    //Method to view the users monthly income
    static void ViewMonthlyIncome()
    {
        Console.Clear();

        Console.WriteLine("=== View Monthly Income ===");

        if (userIncome == null)
        {
            Console.WriteLine("No income been set yet.");
        }
        else
        {
            Console.WriteLine($"Souce: {userIncome.Source}");
            Console.WriteLine($"Monthy Income: {userIncome.MonthlyAmount:C}");
        }

        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();
    }

    //Method to set the users savings goal
    static void SetSavingsGoal()
    {
        Console.Clear();

        Console.WriteLine("=== Set Savings Goal ===");

        Console.WriteLine("Write Goal Name: ");
        string name = Console.ReadLine();

        Console.WriteLine("Enter Target Amount: ");
        bool isValidTargetAmount = double.TryParse(Console.ReadLine(), out double targetAmount);

        if (!isValidTargetAmount)
        {
            Console.WriteLine("Invalid Target amount.");
            Console.WriteLine("Press Enter to Continue.");
            Console.ReadLine();
            return;
        }

        Console.WriteLine("Enter current amount: ");
        bool isValidCurrentAmount = double.TryParse(Console.ReadLine(), out double currentAmount);

        if (!isValidCurrentAmount)
        {
            Console.WriteLine("Invaid current amount. Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        Console.WriteLine("Deadline (example: 8/1/2022)");
        bool isValidDeadLine = DateTime.TryParse(Console.ReadLine(), out DateTime deadLine);

        if (!isValidDeadLine)
        {
            Console.WriteLine("Invalid deadline. Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        //Saves the users goals for savings
        userSavingsGoal = new SavingsGoal(name, targetAmount, currentAmount, deadLine);

        //Saves the user's goal permanently in the database
        dataBaseManager.SaveSavingsGoal(userSavingsGoal);

        Console.WriteLine("Savings goal saved successfully.");
        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();




        {

        }
    }

    //Method to view the users savings goal and progress towards it
    static void ViewSavingsGoal()
    {
        Console.Clear();

        Console.WriteLine("=== View Savings Goal ===");

        if (userSavingsGoal == null)
        {
            Console.WriteLine("No savings goal has been set yet.");
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        double amountRemaining = userSavingsGoal.TargetAmount - userSavingsGoal.CurrentAmount;

        double progressPercent = 0;

        if (userSavingsGoal.TargetAmount > 0)
        {
            progressPercent = userSavingsGoal.CurrentAmount / userSavingsGoal.TargetAmount * 100;
        }

        Console.WriteLine($"Goal: {userSavingsGoal.Name}");
        Console.WriteLine($"Target Amount: {userSavingsGoal.TargetAmount:C}");
        Console.WriteLine($"Current Saved: {userSavingsGoal.CurrentAmount:C}");
        Console.WriteLine($"Amount Remaining: {amountRemaining:C}");
        Console.WriteLine($"Progress: {progressPercent:F1}%");
        Console.WriteLine($"Deadline: {userSavingsGoal.DeadLine.ToShortDateString()}");

        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();
    }

    //Method that gives the user a savings plan based on there current progress, target amount, and deadline.
    static void ViewSavingsPlan()
    {
        Console.Clear();

        Console.WriteLine("=== Savings Plan ===");

        if (userSavingsGoal == null)
        {
            Console.WriteLine("No savings goal has been set yet.");
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        double amountRemaining = userSavingsGoal.TargetAmount - userSavingsGoal.CurrentAmount;

        if (amountRemaining <= 0)
        {
            Console.WriteLine("You already reached your savings goal.");
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        DateTime today = DateTime.Today;

        TimeSpan timeUntilDeadline = userSavingsGoal.DeadLine - today;
        double daysLeft = timeUntilDeadline.TotalDays;

        if (daysLeft <= 0)
        {
            Console.WriteLine("Your deadline has already passed.");
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        double monthsLeft = daysLeft / 30;
        double weeksLeft = daysLeft / 7;

        double savePerMonth = amountRemaining / monthsLeft;
        double savePerWeek = amountRemaining / weeksLeft;
        double savePerDay = amountRemaining / daysLeft;

        Console.WriteLine($"Goal: {userSavingsGoal.Name}");
        Console.WriteLine($"Amount Remaing: {amountRemaining:C}");
        Console.WriteLine($"Days Left: {daysLeft:F0}");
        Console.WriteLine();

        Console.WriteLine("To reach your goal, you need to save about: ");
        Console.WriteLine($"Per month: {savePerMonth:C}");
        Console.WriteLine($"Per Week: {savePerWeek:C}");
        Console.WriteLine($"Per Day: {Math.Ceiling(savePerDay):C}");

        Console.WriteLine();
        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();

    }

    //Method to add budget limits for different categories and set the amount they want to limit themselves to spend in that category
    static void AddBudgetLimits()
    {
        Console.Clear();

        Console.WriteLine("=== Add Budget Limit ===");

        Console.WriteLine("Add Budget category: ");
        string category = Console.ReadLine();

        Console.WriteLine("Monthly limit amount: ");
        bool isValidlimit = double.TryParse(Console.ReadLine(), out double limitAmount);

        if (!isValidlimit)
        {
            Console.WriteLine("Invalid limit amount. Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        BudgetLimit existingLimit = budgetLimits.Find(limit => limit.Category.Equals(category, StringComparison.OrdinalIgnoreCase));

        if (existingLimit != null)
        {
            //Updates the limit whille the app is running 
            existingLimit.LimitAmount = limitAmount;

            //Saves the updated limit permanently in the database
            dataBaseManager.SaveBudgetLimit(existingLimit);

            Console.WriteLine("Budget limit updated successfully/");
        }
        else
        {
            //Creates a new budget limit while the app is running
            BudgetLimit newLimit = new BudgetLimit(category, limitAmount);
            budgetLimits.Add(newLimit);

            //Saves the new limit permanently in the database
            dataBaseManager.SaveBudgetLimit(newLimit);

            Console.WriteLine("Budget limit added successfully.");
        }

        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();
    }

    //Method that shows the user all of there budget limits
    static void ViewBudgetLimits()
    {
        Console.Clear();

        Console.WriteLine("=== Budget Limits ===");

        if (budgetLimits.Count == 0)
        {
            Console.WriteLine("No budget limts set yet.");
        }
        else
        {
            foreach (BudgetLimit limit in budgetLimits)
            {
                Console.WriteLine($"{limit.Category}: {limit.LimitAmount:C}");
            }
        }

        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();
    }

    //Method that allows user to delete there BudgetLimits
    static void DeleteBudgetLimit()
    {
        Console.Clear();

        Console.WriteLine("=== Budget Limits ===");

        if (budgetLimits.Count == 0)
        {
            Console.WriteLine("No budget limits found.");
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        //Lists the budget limits
        foreach (BudgetLimit limit in budgetLimits)
        {
            Console.WriteLine($"{limit.Category}: {limit.LimitAmount:C}");
        }

        Console.WriteLine();
        Console.WriteLine("Enter the category to delete: ");
        string category = Console.ReadLine();

        //Finds the budget limit with the matching category
        BudgetLimit limitToDelete = budgetLimits.Find(limit => limit.Category.Equals(category, StringComparison.OrdinalIgnoreCase));

        if (limitToDelete == null)
        {
            Console.WriteLine("Budget limit not found or miss spelled.");
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        //Removes the budget limit while the app is running
        budgetLimits.Remove(limitToDelete);

        //Deleted the budgetlimit from the database 

        dataBaseManager.DeleteBudgetLimitsByCategory(limitToDelete.Category);

        Console.WriteLine("Budget limit deleted successfully.");
        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();
        return;

    }

    //Method that allows user to Edit there Budget Limits 
    static void EditBudgetLimits()
    {
        Console.Clear();

        Console.WriteLine("=== Edit Budget Limits");

        if (budgetLimits.Count == 0)
        {
            Console.WriteLine("No budget limits found.");
            Console.WriteLine("Press Enter to Continue.");
            Console.ReadLine();
            return;
        }

        //Shows all current budget limits
        foreach (BudgetLimit limit in budgetLimits)
        {
            Console.WriteLine($"{limit.Category}: {limit.LimitAmount:C}");
        }

        Console.WriteLine();
        Console.WriteLine("Enter the category to edit: ");
        string category = Console.ReadLine();

        BudgetLimit limitToEdit = budgetLimits.Find(limit => limit.Category.Equals(category, StringComparison.OrdinalIgnoreCase));

        if (limitToEdit == null)
        {
            Console.WriteLine("Budget limit not found or misspelled.");
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        Console.WriteLine($"New monthly limit amount ({limitToEdit.LimitAmount:C})");
        bool limitIsValid = double.TryParse(Console.ReadLine(), out double newLimitAmount);

        if (!limitIsValid)
        {
            Console.WriteLine("Limit amount must be greater than 0. Press enter to continue.");
            Console.ReadLine();
            return;
        }

        if (newLimitAmount <= 0)
        {
            Console.WriteLine("Limit must be greater than 0. Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        limitToEdit.LimitAmount = newLimitAmount;

        dataBaseManager.SaveBudgetLimit(limitToEdit);

        Console.WriteLine("Budget limit updated successfully.");
        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();

    }

    //Method that compares each of the users Budget limit
    static void CheckBudgetLimits()
    {
        Console.Clear();


        if (budgetLimits.Count == 0)
        {
            Console.WriteLine("No budget limits set yet.");
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        List<Expense> currentMonthExpenses = analyticsService.GetCurrentMonthExpense(expenses);
        if (currentMonthExpenses.Count == 0)
        {
            Console.WriteLine("No expenses found.");
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        //Adds all expenses that match this budget category 
        foreach (BudgetLimit limit in budgetLimits)
        {
            double categoryTotal = 0;

            foreach (Expense expense in currentMonthExpenses)
            {
                if (expense.Category.Equals(limit.Category, StringComparison.OrdinalIgnoreCase))
                {
                    categoryTotal += expense.Amount;
                }
            }
            double amountLeft = limit.LimitAmount - categoryTotal;

            Console.WriteLine("-----------------------");
            Console.WriteLine($"Category: {limit.Category}");
            Console.WriteLine($"Limit: {limit.LimitAmount:C}");
            Console.WriteLine($"Spent: {categoryTotal:C}");

            double percentUsed = 0;

            //Calculates what percent of the budget has been used z
            if (limit.LimitAmount > 0)
            {
                percentUsed = categoryTotal / limit.LimitAmount * 100;
            }

            Console.WriteLine($"Used: {percentUsed:F1}%");

            if (amountLeft >= 0)
            {
                Console.WriteLine($"Remaining: {amountLeft:C}");

                if (percentUsed < 70)
                {
                    Console.WriteLine("Status: Good. You are safely within this budget.");
                }

                else if (percentUsed < 90)
                {
                    Console.WriteLine("Status: Be carful. You have used most of this budget.");
                }
                else
                {
                    Console.WriteLine("Status: Warning. You are very close to going over budget.");
                }
            }
            else
            {
                Console.WriteLine($"Over Budget By: {Math.Abs(amountLeft):C}");
                Console.WriteLine("Status: Over budget. You need to cut back in this category.");
            }

            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
        }
    }

    //Method that gives the user advice based on their income, expenses, savings goals, and budget limits(Fake AI Going to implemt AI after fully done with C#)
    static void AIMoneyCoach()
    {
        Console.Clear();

        Console.WriteLine("=== AI Money Coach ===");
        Console.WriteLine();

        if (userIncome == null)
        {
            Console.WriteLine("Set your monthly income first so I can give better advice.");
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        //Adds current month expenses together
        List<Expense> currentMonthExpenses = analyticsService.GetCurrentMonthExpense(expenses);

        double totalSpent = 0;

        //Adds current month expenses together 
        foreach (Expense expense in currentMonthExpenses)
        {
            totalSpent += expense.Amount;
        }

        double moneyLeft = userIncome.MonthlyAmount - totalSpent;

        Console.WriteLine($"Monthly Income: {userIncome.MonthlyAmount:C}");
        Console.WriteLine($"Current Month Spent: {totalSpent:C}");
        Console.WriteLine($"Money left before savings {moneyLeft:C}");
        Console.WriteLine();

        //Calculates safe to spend money after protecting savings goal
        double savingsNeeded = 0;

        if (userSavingsGoal != null)
        {
            double amountRemaining = userSavingsGoal.TargetAmount - userSavingsGoal.CurrentAmount;

            if (amountRemaining > 0)
            {
                savingsNeeded = amountRemaining;
            }
        }

        double safeToSpend = moneyLeft - savingsNeeded;

        DateTime today = DateTime.Today;

        //Gets the number of days in the current month
        int daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);

        //Calculates how many days are left in the month including today
        int daysLeftInMonth = daysInMonth - today.Day + 1;

        //Converts days left into weeks left
        double weeksLeftInMonth = Math.Ceiling(daysLeftInMonth / 7.0);

        double dailySafeToSpend = 0;
        double weeklySafeToSpend = 0;

        if (daysLeftInMonth > 0)
        {
            dailySafeToSpend = safeToSpend / daysLeftInMonth;
        }

        if (weeksLeftInMonth > 0)
        {
            weeklySafeToSpend = safeToSpend / weeksLeftInMonth;
        }

        Console.WriteLine("Safe-to-Spend Check:");
        Console.WriteLine($"Savings needed: {savingsNeeded:C}");

        if (safeToSpend >= 0)
        {
            Console.WriteLine($"Safe to Spend: {safeToSpend:C}");
            Console.WriteLine($"Daily Safe to Spend: {dailySafeToSpend:C}");
            Console.WriteLine($"Weekly Safe to Spend: {weeklySafeToSpend:C}");
        }
        else
        {
            Console.WriteLine($"ShortFall: {Math.Abs(safeToSpend):C}");
            Console.WriteLine("You do not currently have enough left to fully protect your savings goal.");
        }

        Console.WriteLine("Press Enter to see general Spending Advice.");
        Console.ReadLine();

        //Gives general spending advice 
        if (moneyLeft < 0)
        {
            Console.WriteLine("Warning: You spent more than your monthly income.");
            Console.WriteLine("You need to cut spending or increase your income.");
        }
        else if (moneyLeft < userIncome.MonthlyAmount * 0.20)
        {
            Console.WriteLine("Warning: You have less than 20% of your income left.");
            Console.WriteLine("Be careful. Your spending is getting tight.");
        }
        else
        {
            Console.WriteLine("Good Job. You still have a good amount of income left.");
        }

        Console.WriteLine();

        //Checks savings 

        if (userSavingsGoal != null)
        {
            double amountRemaing = userSavingsGoal.TargetAmount - userSavingsGoal.CurrentAmount;
            double daysLeft = (userSavingsGoal.DeadLine - DateTime.Today).TotalDays;

            Console.WriteLine("Savings Goal Check:");
            Console.WriteLine($"Goal: {userSavingsGoal.Name}");
            Console.WriteLine($"Amount remaing: {amountRemaing:C}");

            if (amountRemaing <= 0)
            {
                Console.WriteLine("You already reached your savings goal.");
            }
            else if (daysLeft <= 0)
            {
                Console.WriteLine("Your savings goal deadline has passed.");
            }
            else
            {
                double weeksLeft = daysLeft / 7;
                double savePerWeek = amountRemaing / weeksLeft;

                Console.WriteLine($"You need to save about {savePerWeek:C} per week.");

                if (moneyLeft >= amountRemaing)
                {
                    Console.WriteLine("You currently have enough left this month to reach the goal.");
                }
                else
                {
                    Console.WriteLine("You do not currently have enough left for this month to fully reach the goal.");
                }
            }

            Console.WriteLine();
        }

        // Checks Monthly budget limits 
        if (budgetLimits.Count > 0)
        {
            Console.WriteLine("Budget Limit Check: ");

            foreach (BudgetLimit limit in budgetLimits)
            {
                double categoryTotal = 0;

                // Adds current month expenses that match this budget category 
                foreach (Expense expense in currentMonthExpenses)
                {
                    if (expense.Category.Equals(limit.Category, StringComparison.OrdinalIgnoreCase))
                    {
                        categoryTotal += expense.Amount;
                    }
                }

                double amountLeft = limit.LimitAmount - categoryTotal;

                double percentUsed = 0;

                if (limit.LimitAmount > 0)
                {
                    percentUsed = categoryTotal / limit.LimitAmount * 100;
                }

                Console.WriteLine("-----------------");
                Console.WriteLine($"Category: {limit.Category}");
                Console.WriteLine($"Limit: {limit.LimitAmount:C}");
                Console.WriteLine($"Spent: {categoryTotal:C}");
                Console.WriteLine($"Used: {percentUsed:F1}%");

                if (amountLeft >= 0)
                {
                    Console.WriteLine($"Remaining: {amountLeft:C}");

                    if (percentUsed < 70)
                    {
                        Console.WriteLine("Status: Good. You are safely within this budget.");
                    }
                    else if (percentUsed < 90)
                    {
                        Console.WriteLine("Status: Be careful. You have used most of this budget.");
                    }
                    else
                    {
                        Console.WriteLine("Status: Warning. You are very close to going over budget.");
                    }
                }
                else
                {
                    Console.WriteLine($"Over Budget By: {Math.Abs(amountLeft):C}");
                    Console.WriteLine("Status: Over Budget. You need to cut back in this category.");
                }

                Console.WriteLine();
            }
        }

        //Added this Pause because it would skip to biggest spending category.
        Console.WriteLine("Press Enter to continue to category advice");
        Console.ReadLine();

        // Finds the biggest spending category for the current month 
        if (currentMonthExpenses.Count > 0)
        {
            var highestCategory = currentMonthExpenses
                .GroupBy(expense => expense.Category)
                .Select(group => new
                {
                    Category = group.Key,
                    Total = group.Sum(expense => expense.Amount)
                })
                .OrderByDescending(group => group.Total)
                .First();

            Console.WriteLine("Biggest Spending Category:");
            Console.WriteLine($"{highestCategory.Category}: {highestCategory.Total:C}");
            Console.WriteLine();

            GiveCategoryAdvice(highestCategory.Category, highestCategory.Total);
        }
        else
        {
            Console.WriteLine("No expenses found for the current month.");
            Console.WriteLine("Add expenses so PocketAI can give better advice.");
        }

        Console.WriteLine();
        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();
    }

    //Builds a financial summary from the user's current data
    static FinancialSummary BuildFinancialSummary()
    {
        List<RecurringExpenses> recurringExpenses = dataBaseManager.GetRecuringExpenses();

        return analyticsService.BuildFinancialSummary(
            expenses,
            userIncome,
            userAccountBalance,
            userSavingsGoal,
            budgetLimits,
            recurringExpenses
        );
    }

    //Builds a clean text prompt that can later be sent to AI
    static string BuildAIPrompt()
    {
        FinancialSummary summary = BuildFinancialSummary();

        double savingsNeeded = 0;

        //Uses remaining savings goal amount if a goal exists
        if (userSavingsGoal != null && summary.SavingsAmountRemaining > 0)
        {
            savingsNeeded = summary.SavingsAmountRemaining;
        }

        //Calculates safe-to-spend amount
        double safeToSpend = analyticsService.GetSafeToSpend(summary.MoneyLeft, savingsNeeded);

        DateTime today = DateTime.Today;

        //Gets how many days are left in the current month
        int daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
        int daysLeftInMonth = daysInMonth - today.Day + 1;

        //Converts days left into weeks
        double weeksLeftInMonth = Math.Ceiling(daysLeftInMonth / 7.0); //Math.Ceiling so it rounds up during the last week of the month and doesn't just show decimals.

        //Calculates daily safe to spend 
        double dailySafeToSpend = analyticsService.GetDailySafeToSpend(safeToSpend, daysInMonth);
        
        //Calculates the weekly safe-to-spend amount
        double weeklySafeToSpend = analyticsService.GetWeeklySafeToSpend(safeToSpend, weeksLeftInMonth);
        

        string prompt = "";

        prompt += "You are PocketAI, a helpful money coach.\n";
        prompt += "Use the user's financial summary to give clear practical budgeting advice.\n\n";

        //Financial Summary
        prompt += "Financial Summary:\n";
        prompt += $"Monthly Income: {summary.MonthlyIncome:C}\n";
        prompt += $"Current Month Spent: {summary.CurrentMonthSpent:C}\n";
        prompt += $"Money Left Before Savings: {summary.MoneyLeft:C}\n";
        prompt += $"Monthly Recurring Expenses: {summary.MonthlyRecurringExpenses}\n";

        //Upcoming Bills/ Recurring Expenses
        prompt += "\nUpcoming Bills:\n";
        List <RecurringExpenses> upcomingBills = dataBaseManager.GetRecuringExpenses();
        if (upcomingBills.Count > 0)
        {
            foreach (RecurringExpenses bill in upcomingBills)
            {
                int daysUntilDue = GetDaysUntilDue(bill.DueDay);

                prompt += $"{bill.Name}: {bill.Amount:C}, due in {daysUntilDue} days(s)\n";
            }
        }
        else
        {
            prompt += "No upcoming recurring bills.";
        }

        //Adds Weekly spending information for AI
        List<Expense> weeklyExpenses = analyticsService.GetCurrentWeekExpenses(expenses);;

        double weeklySpent = 0;

        foreach(Expense expense in weeklyExpenses)
        {
            weeklySpent += expense.Amount;
        }

        prompt += "\nWeekly Spending:\n";
        prompt += $"Spent This Week: {weeklySpent:C}\n";
        prompt += $"Daily Average Spending: {(weeklySpent / 7):C}\n";

        if (weeklyExpenses.Count > 0)
        {
            var weeklyCategory = weeklyExpenses
            .GroupBy(expense => expense.Category)
            .Select(group => new
            {
                Category = group.Key,
                Total = group.Sum(expense => expense.Amount)
            })
            .OrderByDescending(group => group.Total)
            .First();

            prompt += $"Biggest Weekly Category {weeklyCategory.Category:C}\n";
            prompt += $"Weekly Category Amount {weeklyCategory.Total:C}\n";
        }

        prompt += "\n";

        //Weekly Spending Comparison
        List<Expense> currentWeekExpenses = analyticsService.GetCurrentWeekExpenses(expenses);;
        List<Expense> lastWeekExpenses = analyticsService.GetLastWeekExpenses(expenses);;

        double currentWeekTotal = currentWeekExpenses.Sum(expense => expense.Amount);
        double lastWeekTotal = lastWeekExpenses.Sum(expense => expense.Amount);

        double spendingDifference = analyticsService.GetSpendingDifference(currentWeekTotal, lastWeekTotal);

        prompt += "\nWeekly Comparison:\n";
        prompt += $"This Week Spending: {currentWeekTotal:C}\n";
        prompt += $"Last Week Spending: {lastWeekTotal:C}\n";

        if (spendingDifference > 0)
        {
            prompt = $"Spending Increased By: {spendingDifference:C}\n";
        }
        else if (spendingDifference < 0)
        {
            prompt += $"Spending Decreased By: {Math.Abs(spendingDifference):C}";
        }
        else
        {
            prompt += $"Spending stayed the same as last week.\n";
        }

        //Monthly spending Comparison
        List<Expense> currentMonthExpenses = analyticsService.GetCurrentMonthExpense(expenses);
        List<Expense> lastMonthExpenses = analyticsService.GetLastMonthExpense(expenses);

        double currentMonthTotal = currentMonthExpenses.Sum(expense => expense.Amount);
        double lastMonthTotal = lastMonthExpenses.Sum(expense => expense.Amount);

        double monthlyDifference = analyticsService.GetSpendingDifference(currentMonthTotal, lastMonthTotal);

        prompt += "\nMonthly Spending Comparison:\n";
        prompt += $"This Month Spending: {currentMonthTotal:C}\n";
        prompt += $"Last Month Spending: {lastMonthTotal:C}\n";
        
        if (monthlyDifference > 0)
        {
            prompt += $"Spending increased by {monthlyDifference:C}\n";
        }
        else if (monthlyDifference < 0)
        {
            prompt += $"Spending Decreased By: {Math.Abs(monthlyDifference):C}\n";
        }
        else
        {
            prompt += "Spending was the same as last month.\n";
        }

        prompt += "\n";

        //Adds cash flow forecast information for AI
        int daysPassed = today.Day;
        int daysLeft = daysLeftInMonth;

        double averageDailySpending =
        analyticsService.GetAverageDailySpending(summary.CurrentMonthSpent, daysPassed);

        double projectedAdditionalSpending = analyticsService.GetProjectedAdditionalSpending(averageDailySpending, daysLeft);

        double projectedEndOfMonthMoney =
        analyticsService.GetProjectedEndOfMonthMoney(summary.MoneyLeft, projectedAdditionalSpending);

        prompt += "\nCash Flow Forecast\n";
        prompt += $"Average Daily Spending: {averageDailySpending:C}\n";
        prompt += $"Days Left This Month: {daysLeft}\n";
        prompt += $"Projected Additional Spending: {projectedAdditionalSpending:C}\n";
        prompt += $"Projected End-of-Month Balance: {projectedEndOfMonthMoney:C}\n";

        if (projectedEndOfMonthMoney > 0)
        {
            prompt += "Forecast Status: User is projected to end the month with money left.\n";
        }
        else if (projectedEndOfMonthMoney == 0)
        {
            prompt += "Forecast Status: User is projected to break even this month.\n";
        }
        else
        {
            prompt += "Forecast Status: User is projected to go negative if spending continues at this pace.\n";
        }

prompt += "\n";

        prompt += $"Savings Needed: {savingsNeeded:C}\n";
        prompt += $"Safe to Spend: {safeToSpend:C}\n";
        prompt += $"Daily Safe To Spend: {dailySafeToSpend:C} \n";
        prompt += $"Weekly Safe To Spend: {weeklySafeToSpend:C} \n";
        prompt += $"Total Account Balance: {summary.TotalAccountBalance:C} \n\n";

        if (userSavingsGoal != null)
        {
            prompt += "Savings Goal:\n";
            prompt += $"Goal Name: {summary.SavingsGoalName}\n";
            prompt += $"Target Amount: {summary.SavingsTargetAmount:C}\n";
            prompt += $"Current Saved: {summary.CurrentSavedAmount:C}\n";
            prompt += $"Amount Remaining: {summary.SavingsAmountRemaining:C}\n";
            prompt += $"Days Until Goal Deadline: {summary.DaysLeft:F0}\n";
            prompt += $"Weekly Savings Needed: {summary.WeeklySavingsNeeded:C}\n\n";
        }

        if (summary.BiggestSpendingCategory != null)
        {
            prompt += "Spending:\n";
            prompt += $"Biggest Spending Category: {summary.BiggestSpendingCategory}\n";
            prompt += $"Biggest Category Amount: {summary.BiggestCategoryAmount:C}\n\n";
        }

        prompt += $"OverBudget Categories: {summary.OverBudgetCount}\n\n";

        prompt += "Give the user advice in this format:\n";
        prompt += "1. Quick Summary\n";
        prompt += "2. Biggest concern\n";
        prompt += "3. What they are doing well\n";
        prompt += "4. What they should do next\n";

        return prompt;
    }

    // Sends the AI prompt to the Python script and gets advice back
    static string GetPythonAIAdvice(string prompt)
    {
        // Sets up Python process information
        ProcessStartInfo startInfo = new ProcessStartInfo();

        // Use "python" first. If it does not work, change this to "py"
        startInfo.FileName = "py";

        // Name of the Python file
        startInfo.Arguments = "ai_coach.py";

        startInfo.WorkingDirectory = @"C:\Users\Owner\Documents\GitHub\PocketAI\PocketAI";

        // Allows C# to send text into Python
        startInfo.RedirectStandardInput = true;

        // Allows C# to read Python's response
        startInfo.RedirectStandardOutput = true;

        // Allows C# to read Python errors
        startInfo.RedirectStandardError = true;

        // Prevents opening a separate window
        startInfo.UseShellExecute = false;

        // Keeps the process hidden
        startInfo.CreateNoWindow = true;

        // Starts Python
        using Process process = new Process();
        process.StartInfo = startInfo;
        process.Start();

        // Sends the prompt into Python
        process.StandardInput.Write(prompt);
        process.StandardInput.Close();

        // Reads what Python printed
        string output = process.StandardOutput.ReadToEnd();

        // Reads errors if Python failed
        string error = process.StandardError.ReadToEnd();

        process.WaitForExit();

        if (!string.IsNullOrWhiteSpace(error))
        {
            return "Python error:\n" + error;
        }

        return output;
    }

    //Method that allows user to View AI advice from Python AI Coach
    static void ViewPythonAIAdivce()
    {
        Console.Clear();

        Console.WriteLine("=== Python AI Adivce ===");

        Console.WriteLine();

        //Build the financial prompt from C# data
        string prompt = BuildAIPrompt();

        //Send the prompt to Python and gets the response
        string advice = aiService.GetPythonAIAdvice(prompt);

        //Save the AI advice to the database
        dataBaseManager.SaveAIAdvice(prompt, advice);

        Console.WriteLine(advice);

        Console.WriteLine();
        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();
    }

    //AI Prompt to connection Python to C#
    static void ViewAIPrompt()
    {
        Console.Clear();
        Console.WriteLine("=== Ai Prompt Preview ===");
        Console.WriteLine();

        string prompt = BuildAIPrompt();

        Console.WriteLine(prompt);

        Console.WriteLine();
        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();
    }

    //Method that allows user to look at AIHistory
    static void ViewAIAdviceHistory()
    {
        Console.Clear();

        Console.WriteLine("=== AI Advice History ===");
        Console.WriteLine();

        List<AIAdvice> adviceHistory = dataBaseManager.GetAIAdviceHistory();

        if (adviceHistory.Count == 0)
        {
            Console.WriteLine("No AI advice history saved yet.");
        }
        else
        {
            foreach (AIAdvice advice in adviceHistory)
            {
                Console.WriteLine($"ID: {advice.Id}");
                Console.WriteLine($"Date: {advice.DateCreated}");
                Console.WriteLine();
                Console.WriteLine("Advice:");
                Console.WriteLine(advice.AdviceText);
                Console.WriteLine("----------------------------------");
            }
        }

        Console.WriteLine();
        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();
    }

    //Views AI advice history by Id
    static void VewAIAdviceById()
    {
        Console.Clear();

        Console.WriteLine("=== View AI Advice by ID ===");
        Console.WriteLine();

        Console.WriteLine("Enter AI Advice ID: ");
        string input = Console.ReadLine();

        if (!int.TryParse(input, out int id))
        {
            Console.WriteLine("Invalid ID.");
            Console.WriteLine("Press enter to continue.");
            Console.ReadLine();
            return;
        }

        AIAdvice? advice = dataBaseManager.GetAIAdviceById(id);

        if (advice == null)
        {
            Console.WriteLine("No AI advice found with that ID.");
        }
        else
        {
            Console.WriteLine($"ID: {advice.Id}");
            Console.WriteLine($"Date: {advice.DateCreated}");
            Console.WriteLine();

            Console.WriteLine("Advice:");
            Console.WriteLine(advice.AdviceText);
        }

        Console.WriteLine();
        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();

    }

    //Allows user to search there AI advice history with a keyword
    static void SearchAIAdviceHistory()
    {
        Console.Clear();
        Console.WriteLine("=== Search AI Advice ===");
        Console.WriteLine();

        Console.WriteLine("Enter seach keyword: ");
        string keyword = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(keyword))
        {
            Console.WriteLine("Search keyword cannot be found.");
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        List<AIAdvice> results = dataBaseManager.SearchAIAdvice(keyword);

        if (results.Count == 0)
        {
            Console.WriteLine("No saved AI advice matched that keyword");
        }
        else
        {
            foreach (AIAdvice advice in results)
            {
                string preview = advice.AdviceText;

                if (preview.Length > 150)
                {
                    preview = preview.Substring(0, 150) + "...";
                }

                Console.WriteLine($"ID: {advice.Id}");
                Console.WriteLine($"Date: {advice.DateCreated}");
                Console.WriteLine($"Preview: {preview}");
                Console.WriteLine("-------------------------------");




            }
        }

        Console.WriteLine();
        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();
    }

    //Deleted useres saved AI advice 
    static void DeleteAIAdvice()
    {
        Console.Clear();

        Console.WriteLine("=== Delete AI Advice ===");
        Console.WriteLine();

        Console.Write("Enter AI advice ID to delete: ");
        string input = Console.ReadLine();

        if (!int.TryParse(input, out int id))
        {
            Console.WriteLine("Invalid ID.");
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        AIAdvice? advice = dataBaseManager.GetAIAdviceById(id);

        if (advice == null)
        {
            Console.WriteLine("No AI advice found with that ID.");
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"ID: {advice.Id}");
        Console.WriteLine($"Date: {advice.DateCreated}");
        Console.WriteLine();

        string preview = advice.AdviceText;

        if (preview.Length > 200)
        {
            preview = preview.Substring(0, 200) + "...";
        }

        Console.WriteLine("Preview:");
        Console.WriteLine(preview);
        Console.WriteLine();

        Console.Write("Are you sure you want to delete this AI advice? yes/no: ");
        string confirm = Console.ReadLine().ToLower();

        if (confirm != "yes")
        {
            Console.WriteLine("Delete canceled.");
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        bool deleted = dataBaseManager.DeleteAIAdviceById(id);

        if (deleted)
        {
            Console.WriteLine("AI advice deleted successfully.");
        }
        else
        {
            Console.WriteLine("No AI advice found with that ID.");
        }

        Console.WriteLine();
        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();
    }

    //Method that shows the Financial Sumamry
    static void ViewFinancialSummary()
    {
        Console.Clear();
        Console.WriteLine("=== View Financial Summary ===");

        FinancialSummary summary = BuildFinancialSummary();

        Console.WriteLine($"Monthly Income: {summary.MonthlyIncome:C}");
        Console.WriteLine($"Current Month Spent: {summary.CurrentMonthSpent:C}");
        Console.WriteLine($"Money Left: {summary.MoneyLeft:C}");
        Console.WriteLine($"Total Account Balance: {summary.TotalAccountBalance:C}");
        Console.WriteLine();

        if (userSavingsGoal != null)
        {
            Console.WriteLine("Savings Goal");
            Console.WriteLine($"Goal Name: {summary.SavingsGoalName}");
            Console.WriteLine($"Target Amount: {summary.SavingsTargetAmount:C}");
            Console.WriteLine($"Current Saved: {summary.CurrentSavedAmount:C}");
            Console.WriteLine($"Amount Remaining: {summary.SavingsAmountRemaining:C}");
            Console.WriteLine($"Days Left: {summary.DaysLeft:F0}");
            Console.WriteLine($"Weekly Savings Needed: {summary.WeeklySavingsNeeded:C}");
            Console.WriteLine();
        }

        if (summary.BiggestCategoryAmount != null)
        {
            Console.WriteLine("Spending:");
            Console.WriteLine($"Biggest Category: {summary.BiggestSpendingCategory}");
            Console.WriteLine($"Biggest Category Amount: {summary.BiggestCategoryAmount:C}");
        }

        Console.WriteLine($"Over Budget Categories: {summary.OverBudgetCount}");

        Console.WriteLine();
        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();
    }

    static void ViewAIAdviceHistorySummary()
    {
        Console.Clear();
        Console.WriteLine("=== AI Advice History Summary ===");
        Console.WriteLine();
        List<AIAdvice> adviceHistory = dataBaseManager.GetAIAdviceHistory();
        if (adviceHistory.Count == 0)
        {
            Console.WriteLine("No AI advice history saved yet.");
        }
        else
        {
            foreach (AIAdvice advice in adviceHistory)
            {
                string preview = advice.AdviceText;

                if (preview.Length > 120)
                {
                    preview = preview.Substring(0, 120) + "...";
                }

                Console.WriteLine($"ID: {advice.Id}");
                Console.WriteLine($"Date: {advice.DateCreated}");
                Console.WriteLine($"Preview: {preview}");
                Console.WriteLine("------------------------------------------------------------");
            }
        }
        Console.WriteLine();
        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();

    }

    //Method that shows how much money is left to spend after each protection savings
    static void ViewSafeToSpend()
    {
        Console.Clear();

        Console.WriteLine("=== Safe-to-Spend Amount ===");

        if (userIncome == null)
        {
            Console.WriteLine("Set your monthly income first.");
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        FinancialSummary summary = BuildFinancialSummary();

        double savingsNeeded = 0;

        //Uses the remaining savings goal amount if a goal exists
        if (userSavingsGoal != null && summary.SavingsAmountRemaining > 0)
        {
            savingsNeeded = summary.SavingsAmountRemaining;
        }

        //Calculates money that is safe to spend 
        double safeToSpend = summary.MoneyLeft - savingsNeeded;

        //Left off here (need to add Monthly Income, current month spent, money left before savings, and savings needed)
        Console.WriteLine($"Monthy Income: {summary.MonthlyIncome:C}");
        Console.WriteLine($"Currently Monthly Spent: {summary.CurrentMonthSpent:C}");
        Console.WriteLine($"Money Left Before Saving: {summary.MoneyLeft:C} ");
        Console.WriteLine($"Savings Needed: {savingsNeeded:C}");
        Console.WriteLine("----------------");

        if (safeToSpend >= 0)
        {
            Console.WriteLine($"Safe to Spend: {safeToSpend:C}");
            Console.WriteLine($"This is the amount you can spend after protecting your savings goal.");
        }
        else
        {
            Console.WriteLine($"Shortfall: {Math.Abs(safeToSpend):C}");
            Console.WriteLine("You do not currently have enough left to fully protect your savings goal.");
        }


        Console.WriteLine();
        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();
    }

    //Method that allows user to see what they can safely spend per day for the rest of the month. 
    static void ViewDailySafeToSpend()
    {
        Console.Clear();

        Console.WriteLine("=== View Daily Safe to Spend ===");
        if (userIncome == null)
        {
            Console.WriteLine("Set your monthly income first.");
            Console.WriteLine("Press Enter to Continue.");
            Console.ReadLine();
            return;
        }

        FinancialSummary summary = BuildFinancialSummary();

        double savingsNeeded = 0;

        //Uses remainging savings gaol amount if a goal exists
        if (userSavingsGoal != null && summary.SavingsAmountRemaining > 0)
        {
            savingsNeeded = summary.SavingsAmountRemaining;
        }

        //Calculates safe-to-spend amount that is safe to spend
        double safeToSpend = summary.MoneyLeft - savingsNeeded;

        DateTime today = DateTime.Today;

        //Gets total days in current month
        int daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);

        //Calculates how many days are left in the month
        int daysLeftInMonth = daysInMonth - today.Day + 1;

        double dailySafeToSpend = 0;

        if (daysLeftInMonth > 0)
        {
            dailySafeToSpend = safeToSpend / daysLeftInMonth;
        }

        Console.WriteLine($"Safe to Spend: {safeToSpend:C}");
        Console.WriteLine($"Days Left This Month: {daysLeftInMonth}");
        Console.WriteLine("---------------------");

        if (safeToSpend >= 0)
        {
            Console.WriteLine($"Daily Safe-to-Spend: {dailySafeToSpend:C}");
            Console.WriteLine("This is about how much you can safely spend each day for the rest of the month.");
        }
        else
        {
            Console.WriteLine($"ShortFall: {Math.Abs(safeToSpend):C}");
            Console.WriteLine("You do not have a safe daily spending amount right now.");
        }

        Console.WriteLine();
        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();
    }

    //Method that shows user what they can safely spend per week for the rest of the month. 
    static void ViewWeeklySafeToSpend()
    {
        Console.Clear();

        Console.WriteLine("=== Weekly Safe to Spend Limit ===");

        if (userIncome == null)
        {
            Console.WriteLine("Set youu monthly income first.");
            Console.WriteLine("Press Enter to continue.");
            return;
        }

        FinancialSummary summary = BuildFinancialSummary();

        double savingsNeeded = 0;

        //Uses remainging savings goal amount if goal exists
        if (userSavingsGoal != null && summary.SavingsAmountRemaining > 0) ;
        {
            savingsNeeded = summary.SavingsAmountRemaining;
        }

        double safeToSpend = summary.MoneyLeft - savingsNeeded;

        DateTime today = DateTime.Today;

        //Gets Last day of the current month 
        int daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);

        int daysLeftInMonth = daysInMonth - today.Day + 1;

        //Converts days left into weeks left
        double weeksLeftInMonth = Math.Ceiling(daysLeftInMonth / 7.0);

        double weeklySafeToSpend = 0;

        if (weeksLeftInMonth > 0)
        {
            weeklySafeToSpend = safeToSpend / weeksLeftInMonth;
        }

        Console.WriteLine($"Safe to Spend: {safeToSpend:C}");
        Console.WriteLine($"Days Left of this Month: {daysLeftInMonth}");
        Console.WriteLine($"Weeks Left of this Month: {weeksLeftInMonth}");
        Console.WriteLine("------------------------");

        if (safeToSpend >= 0)
        {
            Console.WriteLine($"Weekly Safe-to-Spend: {weeklySafeToSpend:C}");
            Console.WriteLine($"This is about how much you can safely spend each week for the rest of the month.");
        }
        else
        {
            Console.WriteLine($"ShortFall: {Math.Abs(safeToSpend):C}");
            Console.WriteLine("You do not havve a safe weekly spending amount right now.");
        }

        Console.WriteLine();
        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();
    }

    //method that shows a monthlyReport
    static void ViewMonthlyReport()
    {
        Console.Clear();

        Console.WriteLine("=== Monthly Report ===");
        Console.WriteLine();

        FinancialSummary summary = BuildFinancialSummary();

        double savingsNeeded = 0;

        //Uses remaining savings goal amount if a goal exists
        if (userSavingsGoal != null && summary.SavingsAmountRemaining > 0)
        {
            savingsNeeded = summary.SavingsAmountRemaining;
        }

        //Calculates safe to spend amount
        double safeToSpend = summary.MoneyLeft - savingsNeeded;

        DateTime today = DateTime.Today;

        //Gets how many days are in the current month 
        int daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
        int daysLeftInMonth = daysInMonth - today.Day + 1;

        //Converts days left into weeks left 
        double weeksLeftInMonth = Math.Ceiling(daysLeftInMonth / 7.0);

        double dailySafeToSpend = 0;
        double weeklySafeToSpend = 0;

        if (daysLeftInMonth > 0)
        {
            //Divides safe to spend money by days left 
            dailySafeToSpend = safeToSpend / daysLeftInMonth;
        }

        if (weeksLeftInMonth > 0)
        {
            //Divdes safe to spend money by weeks left
            weeklySafeToSpend = safeToSpend / weeksLeftInMonth;
        }



        Console.WriteLine($"Monthly Income: {summary.MonthlyIncome:C}");
        Console.WriteLine($"Current Month Spent: {summary.CurrentMonthSpent:C}");
        Console.WriteLine($"Money Left Before Savings: {summary.MoneyLeft:C}");
        Console.WriteLine();

        Console.WriteLine("--- Savings ---");
        Console.WriteLine($"Savings Goal: {summary.SavingsGoalName}");
        Console.WriteLine($"Target Amount: {summary.SavingsTargetAmount:C}");
        Console.WriteLine($"Current Saved: {summary.CurrentSavedAmount:C}");
        Console.WriteLine($"Savings Needed: {summary.SavingsAmountRemaining:C}");
        Console.WriteLine($"Days Left: {summary.DaysLeft:F0}");
        Console.WriteLine($"Weekly Savings Needed: {summary.WeeklySavingsNeeded:C}");
        Console.WriteLine();

        Console.WriteLine("--- Spending ---");
        Console.WriteLine($"Top Spending Category: {summary.BiggestSpendingCategory}");
        Console.WriteLine($"Top Category Amount: {summary.BiggestCategoryAmount:C}");
        Console.WriteLine($"Over Budget Categories: {summary.OverBudgetCount}");

        Console.WriteLine();
        Console.WriteLine("Press Enter to Continue.");
        Console.ReadLine();

    }
    
    //Shows weekly spending report
    static void ViewWeeklyReport()
    {
        Console.Clear();

        Console.WriteLine("=== Weekly Spending Report ===");
        Console.WriteLine();

        List<Expense> weeklyExpenses = analyticsService.GetCurrentWeekExpenses(expenses);

    double totalSpent = 0;

    foreach (Expense expense in weeklyExpenses)
    {
        totalSpent += expense.Amount;
    }

    Console.WriteLine($"Spent This Week: {totalSpent:C}");

    if (weeklyExpenses.Count > 0)
    {
        var highestCategory = weeklyExpenses
            .GroupBy(expense => expense.Category)
            .Select(group => new
            {
                Category = group.Key,
                Total = group.Sum(expense => expense.Amount)
            })
            .OrderByDescending(group => group.Total)
            .First();

        Console.WriteLine($"Biggest Category: {highestCategory.Category}");
        Console.WriteLine($"Category Amount: {highestCategory.Total:C}");
    }
    else
    {
        Console.WriteLine("No expenses recorded this week.");
    }

    double dailyAverage = totalSpent / 7;

    Console.WriteLine($"Daily Average Spending: {dailyAverage:C}");

    Console.WriteLine();
    Console.WriteLine("Press Enter to continue.");
    Console.ReadLine();
    }

    //Shows a simple end of the month flow forecast 
    static void ViewCashFlowForecast()
    {
        Console.Clear();

        Console.WriteLine("=== Cash Flow Forecast ===");
        Console.WriteLine();

        FinancialSummary summary = BuildFinancialSummary();

        DateTime today = DateTime.Today;

        int daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
        int daysPassed = today.Day;
        int DaysLeft = daysInMonth - today.Day + 1;

        double averageDailySpending = analyticsService.GetAverageDailySpending(summary.CurrentMonthSpent, daysPassed);

        double projectedAdditionalSpending = analyticsService.GetProjectedAdditionalSpending(averageDailySpending, DaysLeft);

        double projectedEndOfMonthMoney = analyticsService.GetProjectedEndOfMonthMoney(summary.MoneyLeft, projectedAdditionalSpending);

        Console.WriteLine($"Monthly Income: {summary.MonthlyIncome:C}");
        Console.WriteLine($"Current Month Spent: {summary.CurrentMonthSpent:C}");
        Console.WriteLine($"Monthly Recurring Expenses: {summary.MonthlyRecurringExpenses:C}");
        Console.WriteLine($"Average Daily Spending: {averageDailySpending:C}");
        Console.WriteLine($"Days Left This Month: {DaysLeft}");
        Console.WriteLine();

        Console.WriteLine($"Projected Additional Spending: {projectedAdditionalSpending:C}");
        Console.WriteLine($"Projected End-of-Month Money: {projectedEndOfMonthMoney:C}");

        if (projectedEndOfMonthMoney > 0)
        {
            Console.WriteLine("Forecast: You are projected to break even this month");
        }
        else if (projectedEndOfMonthMoney < 0)
        {
            Console.WriteLine("Forecast: You are projected to break even this month.");
        }
        else
        {
            Console.WriteLine("Forcast: You are projected to go negative if spending continues at this pace.");
        }

        Console.WriteLine();
        Console.WriteLine("Press Enter to Continue.");
        Console.ReadLine();

    }
    
    //Shows spending compared to last month
    static void ViewMonthComparison()
    {
        Console.Clear();
        
        Console.WriteLine("=== Monthly Spending Comparison ===");
        Console.WriteLine();

        List<Expense> currentMonth = analyticsService.GetCurrentMonthExpense(expenses);
        List<Expense> lastMonth = analyticsService.GetLastMonthExpense(expenses);

        double currentTotal = currentMonth.Sum(expense => expense.Amount);
        double lastTotal = lastMonth.Sum(expense => expense.Amount);

        double difference = currentTotal - lastTotal;

        Console.WriteLine($"This Month: {currentTotal:C}");
        Console.WriteLine($"Last Month: {lastTotal:C}");

        if (difference > 0)
        {
            Console.WriteLine($"You spent {difference:C} more than last month.");
        }
        else if (difference < 0)
        {
            Console.WriteLine($"You spent {Math.Abs(difference):C} less than last month");
        }
        else
        {
            Console.WriteLine("Your spending is the same as last month.");
        }

        Console.WriteLine();
        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();

    }
    
    //Shows spending trends compared to last week
    static void ViewWeeklyComparison()
    {
        Console.Clear();

        Console.WriteLine("=== Spending Trends ===");
        Console.WriteLine();

        List<Expense> currentWeek = analyticsService.GetCurrentWeekExpenses(expenses);;
        List<Expense> lastWeek = analyticsService.GetLastWeekExpenses(expenses);;

        double currentWeekTotal = currentWeek.Sum(expense => expense.Amount);
        double lastWeekTotal = lastWeek.Sum(expense => expense.Amount);

        double difference = analyticsService.GetSpendingDifference(currentWeekTotal, lastWeekTotal);

        double percentageChange = analyticsService.GetSpendingPercentageChange(currentWeekTotal, lastWeekTotal);

        Console.WriteLine($"This Week: {currentWeekTotal:C}");
        Console.WriteLine($"Last Week: {lastWeekTotal:C}");
        Console.WriteLine();

        /**
        I stopped here. Need to update with percentage 
        **/
        if (difference > 0)
        {
            Console.WriteLine($"You spent {difference:C} more than last week.");

            if (lastWeekTotal > 0)
            {
                Console.WriteLine($"That is an increase of {percentageChange:F1}%");
            }
        } 
        else if (difference < 0)
        {
            Console.WriteLine($"You spent {Math.Abs(difference):C} less than last week");

            if (lastWeekTotal > 0)
            {
                Console.WriteLine($"That is a decrease of {Math.Abs(percentageChange):F1}%");
            }
        }
        else
        {
            Console.WriteLine("Your spending is the same last week.");
        }

        Console.WriteLine();

        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();

        
    }
    //Method that allows user to Add Recurring Expenses like subscriptions
    static void AddRecurringExpenseMenu()
    {
        Console.Clear();

        Console.WriteLine("=== Add Recuring Expense ===");

        Console.WriteLine("Name: ");
        string name = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(name))
        {
            Console.WriteLine("Name cannont be empty.");
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        Console.WriteLine("Category: ");
        string category = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(category))
        {
            Console.WriteLine("Category cannot be empty.");
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        Console.WriteLine("Monthly Amount: ");
        string amountInput = Console.ReadLine();

        if (!double.TryParse(amountInput, out double amount) || amount <= 0)
        {
            Console.WriteLine("Invalid amount. Please enter a number greater than 0.");
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
        }

        Console.WriteLine("Due Day (1-31): ");
        string dueDayInput = Console.ReadLine();

        if (!int.TryParse(dueDayInput, out int dueDay) || dueDay > 31)
        {
            Console.WriteLine("Invalud due date. Please enter one number from 1 to 31");
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
        }



        RecurringExpenses expense = new RecurringExpenses(
            0,
            name,
            category,
            amount,
            dueDay,
            true
            );

        dataBaseManager.AddRecurringExpense(expense);

        Console.WriteLine();
        Console.WriteLine("Reccuring expense added successfully.");
        Console.WriteLine();
        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();
    }

    //Method that allows user to view all recurring expenses
    static void ViewRecurringExpenses()
    {
        Console.Clear();

        Console.WriteLine("=== View Recurring Expenses");

        List<RecurringExpenses> expenses = dataBaseManager.GetRecuringExpenses();

        if (expenses.Count == 0)
        {
            Console.WriteLine("No recurring expenses ");
        }
        else
        {
            double total = 0;

            foreach (RecurringExpenses expense in expenses)
            {
                Console.WriteLine($"Id: {expense.Id}");
                Console.WriteLine($"Name: {expense.Name}");
                Console.WriteLine($"Category: {expense.Category}");
                Console.WriteLine($"Amount: {expense.Amount:C}");
                Console.WriteLine($"Due Day: {expense.DueDay}th");

                total += expense.Amount;
            }

            Console.WriteLine($"Monthly Recurring Total: {total:C}");
        }

        Console.WriteLine();
        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();

    }

    //Calculates how many days until a recurring expense is due
    static int GetDaysUntilDue(int dueDay)

    {
        DateTime today = DateTime.Today;

        DateTime dueDate;

        //If the due day already passed this month, move to next month
        if (today.Day >= dueDay)
        {
            DateTime nextMonth = today.AddMonths(1);

            dueDate = new DateTime(
                nextMonth.Year,
                nextMonth.Month,
                dueDay
            );
        }
        else
        {
            dueDate = new DateTime(
                today.Year,
                today.Month,
                dueDay
            );
        }
        return (dueDate - today).Days;
    }

    //Shows upcoming bills
    static void ViewUpComingBill()
    {
        Console.Clear();

        Console.WriteLine("=== Upcoming Bills");
        Console.WriteLine();

        List<RecurringExpenses> expenses = dataBaseManager.GetRecuringExpenses();

        if(expenses.Count == 0)
        {
            Console.WriteLine("No Recurring Expenses");
        }
        else
        {
            foreach (RecurringExpenses expense in expenses)
            {
                int daysUntilDue = GetDaysUntilDue(expense.DueDay);

                Console.WriteLine($"Name: {expense.Name}");
                Console.WriteLine($"Category: {expense.Category}");
                Console.WriteLine($"Amount: {expense.Amount:C}");
                Console.WriteLine($"Due Day: {expense.DueDay}th");
                Console.WriteLine($"Due In: {daysUntilDue} days(s)");
                Console.WriteLine("---------------------------------");
            }
        }

        Console.WriteLine();
        Console.WriteLine("Press Enter to Continue.");
        Console.ReadLine();
    }
    //Method that gives simple advice on a spending category(Fake AI Going to implemt AI in Python after fully done with C#)
    static void GiveCategoryAdvice(string category, double total)
    {
        Console.Clear();

        string lowerCategory = category.ToLower();

        if (lowerCategory.Contains("food") || lowerCategory.Contains("resturant") || lowerCategory.Contains("eating"))
        {
            Console.WriteLine("Food is your highest spending area.");
            Console.WriteLine("Try meal prepping, limiting eating out, or setting a weekly food limit.");
            Console.WriteLine("Even cutting this category by 10-20% could help your savings goal.");
        }
        else if (lowerCategory.Contains("gas") || lowerCategory.Contains("car") || lowerCategory.Contains("transport"))
        {
            Console.WriteLine("Transportation is your highest spending area.");
            Console.WriteLine("Try planning trips better, combining errands, or tracking gas spending weekly.");
            Console.WriteLine("This category may be necessary, but still needs a clear limit.");
        }
        else if (lowerCategory.Contains("rent") || lowerCategory.Contains("housing") || lowerCategory.Contains("mortgage"))
        {
            Console.WriteLine("Housing is your highest spending area");
            Console.WriteLine("This may be a fixed expense, so focus on lowering flexible expenses like food or entertainment");
            Console.WriteLine("MAke sure your housing cost is not taking to much of your monthly income.");
        }
        else if (lowerCategory.Contains("entertainment") || lowerCategory.Contains("fun") || lowerCategory.Contains("games"))
        {
            Console.WriteLine("Subscriptions are your highest spending area.");
            Console.WriteLine("Review every subscription and cancel anything you do not use often");
            Console.WriteLine("Small monthly charges can quietly hurt your savings goal.");

        }
        else
        {
            Console.WriteLine($"Your highest spending category is {category}");
            Console.WriteLine("Review this category and ask if every expense was necessary");
            Console.WriteLine("If you want to save more, this is a good place to start.");
        }

        Console.WriteLine();
    }

}


