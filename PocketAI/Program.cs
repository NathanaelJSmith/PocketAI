
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Linq;

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

    //Stores all the budget limits
    static List<BudgetLimit> budgetLimits = new List<BudgetLimit>();

    //Does the saving and loading data from Sqlite
    static DataBaseManager dataBaseManager = new DataBaseManager();



    static void Main()
    {

        //Creates the file when the app starts
        dataBaseManager.CreateTables();

        //Loads all saved expenses from database
        expenses = dataBaseManager.GetAllExpenses();

        //This sets the next ID based on the highest saved expense ID
        if (expenses.Count > 0)
        {
            nextExpenseId = expenses.Max(expense =>  expense.Id) + 1;
        }

        bool running = true;

        while (running)
        {
            Console.Clear();

            Console.WriteLine("---- PocketAI ----");
            Console.WriteLine("1. Add Expense");
            Console.WriteLine("2. View Expense");
            Console.WriteLine("3. View Total Expenses");
            Console.WriteLine("4. Delete Expense");
            Console.WriteLine("5. Edit Expenses");
            Console.WriteLine("6. Filtered by Categories");
            Console.WriteLine("7. Total by Category");
            Console.WriteLine("8. Category BreakDown");
            Console.WriteLine("9. Set Monthly Income");
            Console.WriteLine("10. View Monthly Income");
            Console.WriteLine("11. Set Savings Goal");
            Console.WriteLine("12. View Savings Goal");
            Console.WriteLine("13. View Savings Plan");
            Console.WriteLine("14. Add Budget Limit");
            Console.WriteLine("15. View Budget Limits");
            Console.WriteLine("16. Check Budget Limits");
            Console.WriteLine("17. AI Money Coach");
            Console.WriteLine("18. Exit");
            Console.WriteLine("Choose an option");

            string choice = Console.ReadLine();

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
                    SetMonthlyIncome();
                    break;
                case "10":
                    ViewMonthlyIncome();
                    break;
                case "11":
                    SetSavingsGoal();
                    break;
                case "12":
                    ViewSavingsGoal();
                    break;
                case "13":
                    ViewSavingsPlan();
                    break;
                case "14":
                    AddBudgetLimits();
                    break;
                case "15":
                    ViewBudgetLimits();
                    break;
                case "16":
                    CheckBudgetLimits();
                    break;
                case "17":
                    AIMoneyCoach();
                    break;
                case "18":
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

        Console.WriteLine("Expense deleted successfully.");
        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();
    }

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

        Console.WriteLine("Monthly income saved successfully.");
        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();
    }

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

        Console.WriteLine("Savings goal saved successfully.");
        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();




        {

        }
    }

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

        TimeSpan timeUnitDeadline = userSavingsGoal.DeadLine - today;
        double daysLeft = timeUnitDeadline.TotalDays;

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

        Console.WriteLine("TO reach your goal, you need to save about: ");
        Console.WriteLine($"Per month: {savePerMonth:C}");
        Console.WriteLine($"Per Week: {savePerWeek:C}");
        Console.WriteLine($"Per Day: {savePerDay}");

        Console.WriteLine();
        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();

    }

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
            existingLimit.LimitAmount = limitAmount;
            Console.WriteLine("Budget limit updated successfully/");
        }
        else
        {
            BudgetLimit newLimit = new BudgetLimit(category, limitAmount);
            budgetLimits.Add(newLimit);
            Console.WriteLine("Budget limit added successfully.");
        }

        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();
    }

    //Shows the user all of there budget limits
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

        if (expenses.Count == 0)
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

            foreach (Expense expense in expenses)
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

            if (amountLeft >= 0)
            {
                Console.WriteLine($"Remaining: {amountLeft:C}");
                Console.WriteLine("Status: Your are within this budget.");
            }
            else
            {
                Console.WriteLine($"Over Buget By: {Math.Abs(amountLeft):C}");
                Console.WriteLine("Statuc: Over Budget)");
            }

            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
        }
    }

    static void AIMoneyCoach()
    {
        Console.Clear();

        Console.WriteLine("===AI Money Coach ===");
        Console.WriteLine();

        if (userIncome == null)
        {
            Console.WriteLine("Set your monthly income first so I can give better advice.");
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
            return;
        }

        double totalSpent = 0;

        //Adds all expenses together 
        foreach (Expense expense in expenses)
        {
            totalSpent += expense.Amount;
        }

        double moneyLeft = userIncome.MonthlyAmount - totalSpent;

        Console.WriteLine($"Monthly Income: {userIncome.MonthlyAmount:C}");
        Console.WriteLine($"Total Spent: {totalSpent:C}");
        Console.WriteLine($"Money left before savings {moneyLeft:C}");
        Console.WriteLine();

        if (moneyLeft < 0)
        {
            Console.WriteLine("Warning: You spent more than your monthly income.");
            Console.WriteLine("You need to cute spending or increase your income.");
        }
        else if (moneyLeft < userIncome.MonthlyAmount * 0.20)
        {
            Console.WriteLine("Warning: You have less than 20% of your income left.");
            Console.WriteLine("Be carful. Your spending is getting tight.");
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

            DateTime today = DateTime.Today;

            double daysLeft = (userSavingsGoal.DeadLine - today).TotalDays;

            Console.WriteLine("Savings Goal Check:");
            Console.WriteLine($"Goal: {userSavingsGoal.Name}");
            Console.WriteLine($"Amount remaing: {amountRemaing:C}");

            if (amountRemaing < 0)
            {
                Console.WriteLine("You already reached your savings goal.");
            }
            else if (amountRemaing >= 0)
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
                    Console.WriteLine("You do not currently have enough left for this month to fullyreach the goal.");
                }
            }

            Console.WriteLine();
        }

        // Check budget limits if any exist
        if (budgetLimits.Count > 0)
        {
            Console.WriteLine("Budget Limit Check: ");

            foreach (BudgetLimit limit in budgetLimits)
            {
                double categoryTotal = 0;

                foreach (Expense expense in expenses)
                {
                    if (expense.Category.Equals(limit.Category, StringComparison.OrdinalIgnoreCase))
                    {
                        categoryTotal += expense.Amount;
                    }
                }

                if (categoryTotal > limit.LimitAmount)
                {
                    double overAmount = categoryTotal - limit.LimitAmount;
                    Console.WriteLine($"You are over budget in {limit.Category} by {overAmount:C}.");
                }
                else
                {
                    double remaining = limit.LimitAmount - categoryTotal;
                    Console.WriteLine($"You have {remaining:C} left in {limit.Category}");
                }
            }

            Console.WriteLine();

            if (expenses.Count > 0)
            {
                var highestCategory = expenses
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
                Console.WriteLine("This is the first place you should review if you need to save more.");
            }

            Console.WriteLine();
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
        }
        //
        
    }
}


