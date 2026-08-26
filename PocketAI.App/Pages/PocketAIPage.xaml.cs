using System.Text.RegularExpressions;
using RoundRectangle = Microsoft.Maui.Controls.Shapes.RoundRectangle;

namespace PocketAI.App.Pages;

public partial class PocketAIPage : ContentPage
{
    // ==========================================
    // SERVICES
    // ==========================================

    private readonly DataBaseManager dataBaseManager;
    private readonly AnalyticsService analyticsService;


    // ==========================================
    // CURRENT FINANCIAL DATA
    // ==========================================

    private FinancialSummary? currentSummary;

    private double currentSafeToSpend;

    private double currentDailySafeToSpend;

    private double currentSavingsNeededThisMonth;

    private double currentProjectedEndOfMonthMoney;

    private string? lastReferencedCategory;

    private double? lastReferencedPurchaseAmount;

    private RecurringExpenses? lastReferencedBill;

    private SavingsGoal? lastReferencedSavingsGoal;


    private List<Expense> currentExpenses =
        new List<Expense>();


    private List<SavingsGoal> currentSavingsGoals =
        new List<SavingsGoal>();


    private List<BudgetLimit> currentBudgetLimits =
        new List<BudgetLimit>();


    private List<RecurringExpenses> currentRecurringExpenses =
        new List<RecurringExpenses>();


    // Prevents a typed question from also
    // creating the quick-question user bubble.
    private bool isProcessingTypedQuestion;

    // ==========================================
    // CHAT THEME REFERENCES
    // ==========================================

    // User message controls.
    private readonly List<Border> userMessageBubbles =
        new List<Border>();

    private readonly List<Label> userMessageLabels =
        new List<Label>();

    private readonly List<Label> userNameLabels =
        new List<Label>();


    // PocketAI message controls.
    private readonly List<Border> pocketAIIconBorders =
        new List<Border>();

    private readonly List<Label> pocketAIIconLabels =
        new List<Label>();

    private readonly List<Border> pocketAIMessageBubbles =
        new List<Border>();

    private readonly List<Label> pocketAIMessageLabels =
        new List<Label>();

private readonly List<Label> pocketAINameLabels =
    new List<Label>();



    // ==========================================
    // CONSTRUCTOR
    // ==========================================

    public PocketAIPage()
    {
        InitializeComponent();


        // PocketAI's MAUI database.
        string databasePath =
            Path.Combine(
                FileSystem.AppDataDirectory,
                "pocketai.db");


        dataBaseManager =
            new DataBaseManager(
                databasePath);


        analyticsService =
            new AnalyticsService();


        // Make sure all database tables exist.
        dataBaseManager.CreateTables();
    }



    // ==========================================
    // PAGE APPEARS
    // ==========================================

    protected override void OnAppearing()
    {
        base.OnAppearing();


        // Reload financial information.
        LoadFinancialSnapshot();


        // Repaint every existing conversation
        // message using the CURRENT app theme.
        RefreshConversationTheme();
    }



    // ==========================================
    // LOAD FINANCIAL SNAPSHOT
    // ==========================================

    private void LoadFinancialSnapshot()
    {
        // ======================================
        // EXPENSES
        // ======================================

        List<Expense> expenses =
            dataBaseManager
                .GetAllExpenses();


        currentExpenses =
            expenses;



        // ======================================
        // INCOME
        // ======================================

        Income? income =
            dataBaseManager
                .GetIncome();



        // ======================================
        // ACCOUNT BALANCES
        // ======================================

        AccountBalance? accountBalance =
            dataBaseManager
                .GetAccountBalance();



        // ======================================
        // SAVINGS GOALS
        // ======================================

        SavingsGoal? primarySavingsGoal =
            dataBaseManager
                .GetSavingsGoal();


        List<SavingsGoal> savingsGoals =
            dataBaseManager
                .GetSavingsGoals();


        currentSavingsGoals =
            savingsGoals;



        // ======================================
        // BUDGET LIMITS
        // ======================================

        List<BudgetLimit> budgetLimits =
            dataBaseManager
                .GetBudgetLimits();


        currentBudgetLimits =
            budgetLimits;



        // ======================================
        // RECURRING BILLS
        // ======================================

        List<RecurringExpenses> recurringExpenses =
            dataBaseManager
                .GetRecuringExpenses();


        currentRecurringExpenses =
            recurringExpenses;



        // ======================================
        // BUILD FINANCIAL SUMMARY
        // ======================================

        FinancialSummary summary =
            analyticsService
                .BuildFinancialSummary(
                    expenses,
                    income,
                    accountBalance,
                    primarySavingsGoal,
                    budgetLimits,
                    recurringExpenses);


        currentSummary =
            summary;



        // ======================================
        // DATE INFORMATION
        // ======================================

        DateTime today =
            DateTime.Today;


        int daysInMonth =
            DateTime.DaysInMonth(
                today.Year,
                today.Month);


        int daysLeftInMonth =
            daysInMonth -
            today.Day +
            1;



        // ======================================
        // SAVINGS NEEDED THIS MONTH
        // ======================================

        double savingsNeededThisMonth =
            0;


        foreach (SavingsGoal goal
                 in savingsGoals)
        {
            double amountRemaining =
                Math.Max(
                    goal.TargetAmount -
                    goal.CurrentAmount,
                    0);


            // Completed goals no longer
            // require additional savings.
            if (amountRemaining <= 0)
            {
                continue;
            }


            double daysUntilDeadline =
                (
                    goal.DeadLine.Date -
                    today.Date
                ).TotalDays;


            double goalSavingsNeeded =
                analyticsService
                    .GetSavingsNeededThisMonth(
                        amountRemaining,
                        daysUntilDeadline,
                        daysLeftInMonth);


            savingsNeededThisMonth +=
                goalSavingsNeeded;
        }


        currentSavingsNeededThisMonth =
            savingsNeededThisMonth;



        // ======================================
        // SAFE TO SPEND
        // ======================================

        double safeToSpend =
            analyticsService
                .GetSafeToSpend(
                    summary.MoneyLeft,
                    savingsNeededThisMonth);


        currentSafeToSpend =
            safeToSpend;



        // ======================================
        // DAILY SAFE TO SPEND
        // ======================================

        double dailySafeToSpend =
            analyticsService
                .GetDailySafeToSpend(
                    safeToSpend,
                    daysLeftInMonth);


        currentDailySafeToSpend =
            dailySafeToSpend;



        // ======================================
        // END-OF-MONTH PROJECTION
        // ======================================

        double averageDailySpending =
            analyticsService
                .GetAverageDailySpending(
                    summary.CurrentMonthSpent,
                    today.Day);


        double projectedAdditionalSpending =
            analyticsService
                .GetProjectedAdditionalSpending(
                    averageDailySpending,
                    daysLeftInMonth);


        double projectedEndOfMonthMoney =
            analyticsService
                .GetProjectedEndOfMonthMoney(
                    summary.MoneyLeft,
                    projectedAdditionalSpending);


        currentProjectedEndOfMonthMoney =
            projectedEndOfMonthMoney;



        // ======================================
        // FINANCIAL HEALTH
        // ======================================

        int financialHealthScore =
            analyticsService
                .GetFinancialHealthScore(
                    summary,
                    projectedEndOfMonthMoney,
                    safeToSpend);



        // ======================================
        // SNAPSHOT CARDS
        // ======================================

        SnapshotSafeToSpendLabel.Text =
            safeToSpend
                .ToString("C");


        SnapshotMoneyLeftLabel.Text =
            summary.MoneyLeft
                .ToString("C");


        SnapshotSpentLabel.Text =
            summary.CurrentMonthSpent
                .ToString("C");


        SnapshotHealthLabel.Text =
            $"{financialHealthScore} / 100";



        // ======================================
        // SAFE TO SPEND COLOR
        // ======================================

        if (safeToSpend < 0)
        {
            SnapshotSafeToSpendLabel.SetDynamicResource(
                Label.TextColorProperty,
                "DangerColor");
        }
        else
        {
            SnapshotSafeToSpendLabel.SetDynamicResource(
                Label.TextColorProperty,
                "TextPrimary");
        }



        // ======================================
        // MONEY LEFT COLOR
        // ======================================

        if (summary.MoneyLeft < 0)
        {
            SnapshotMoneyLeftLabel.SetDynamicResource(
                Label.TextColorProperty,
                "DangerColor");
        }
        else
        {
            SnapshotMoneyLeftLabel.SetDynamicResource(
                Label.TextColorProperty,
                "TextPrimary");
        }


        // ======================================
        // HEALTH SCORE COLOR
        // ======================================

        if (financialHealthScore >= 80)
        {
            SnapshotHealthLabel.SetDynamicResource(
                Label.TextColorProperty,
                "SuccessColor");
        }
        else if (financialHealthScore >= 60)
        {
            SnapshotHealthLabel.SetDynamicResource(
                Label.TextColorProperty,
                "WarningColor");
        }
        else
        {
            SnapshotHealthLabel.SetDynamicResource(
                Label.TextColorProperty,
                "DangerColor");
        }
    }



    // ==========================================
    // QUICK QUESTION:
    // CAN I AFFORD TO SPEND TODAY?
    // ==========================================

    private void SafeToSpendQuestionClicked(
        object? sender,
        EventArgs e)
    {
        if (!isProcessingTypedQuestion)
        {
            AddUserMessage(
                "Can I afford to spend today?");
        }



        // ======================================
        // NO FINANCIAL SUMMARY
        // ======================================

        if (currentSummary == null)
        {
            ShowAssistantResponse(
                "I don't have enough financial information yet. " +
                "Add your income, expenses, bills, and savings goals " +
                "so I can analyze your spending.");

            return;
        }



        // ======================================
        // NO INCOME
        // ======================================

        if (currentSummary.MonthlyIncome <= 0)
        {
            ShowAssistantResponse(
                "I can't accurately tell you how much is safe to spend yet " +
                "because you haven't entered monthly income. " +
                "Add your income in Accounts first.");

            return;
        }



        // ======================================
        // NEGATIVE SAFE TO SPEND
        // ======================================

        if (currentSafeToSpend < 0)
        {
            ShowAssistantResponse(
                $"I would avoid extra spending right now. " +
                $"Your current plan is {Math.Abs(currentSafeToSpend):C} short " +
                $"after accounting for your spending, recurring bills, " +
                $"and savings goals.");

            return;
        }



        // ======================================
        // NO DISCRETIONARY MONEY
        // ======================================

        if (currentSafeToSpend == 0)
        {
            ShowAssistantResponse(
                "I would avoid discretionary spending today. " +
                "Your current income is already fully committed to " +
                "spending, bills, and savings goals.");

            return;
        }



        // ======================================
        // SAFE TO SPEND
        // ======================================

        ShowAssistantResponse(
            $"Yes. Based on your current financial plan, about " +
            $"{currentDailySafeToSpend:C} is safe to spend today. " +
            $"You currently have {currentSafeToSpend:C} available " +
            $"for the rest of the month after accounting for your " +
            $"spending, recurring bills, and savings goals.");
    }



    // ==========================================
    // QUICK QUESTION:
    // WHERE AM I SPENDING THE MOST?
    // ==========================================

    private void SpendingQuestionClicked(
        object? sender,
        EventArgs e)
    {
        if (!isProcessingTypedQuestion)
        {
            AddUserMessage(
                "Where am I spending the most?");
        }



        // ======================================
        // CURRENT MONTH EXPENSES
        // ======================================

        List<Expense> currentMonthExpenses =
            analyticsService
                .GetCurrentMonthExpense(
                    currentExpenses);



        // ======================================
        // NO TRANSACTIONS
        // ======================================

        if (currentMonthExpenses.Count == 0)
        {
            ShowAssistantResponse(
                "You don't have any spending recorded for this month yet. " +
                "Add transactions and I'll be able to show you where " +
                "most of your money is going.");

            return;
        }



        // ======================================
        // BIGGEST CATEGORY
        // ======================================

        string biggestCategory =
            analyticsService
                .GetBiggestSpendingCategory(
                    currentMonthExpenses);

        lastReferencedCategory =
        biggestCategory;

        double biggestCategoryAmount =
            analyticsService
                .GetCategoryTotal(
                    currentMonthExpenses,
                    biggestCategory);


        double totalSpent =
            analyticsService
                .GetTotalSpent(
                    currentMonthExpenses);



        // ======================================
        // PERCENTAGE OF SPENDING
        // ======================================

        double percentageOfSpending =
            0;


        if (totalSpent > 0)
        {
            percentageOfSpending =
                biggestCategoryAmount /
                totalSpent *
                100;
        }



        // ======================================
        // RESPONSE
        // ======================================

        ShowAssistantResponse(
            $"Your biggest spending category this month is " +
            $"{biggestCategory}. " +
            $"You've spent {biggestCategoryAmount:C} there, " +
            $"which is about {percentageOfSpending:F0}% of your " +
            $"total monthly spending. " +
            $"You've spent {totalSpent:C} overall this month.");
    }



    // ==========================================
    // QUICK QUESTION:
    // HOW ARE MY SAVINGS GOALS?
    // ==========================================

    private void SavingsQuestionClicked(
        object? sender,
        EventArgs e)
    {
        if (!isProcessingTypedQuestion)
        {
            AddUserMessage(
                "How are my savings goals?");
        }



        // ======================================
        // NO SAVINGS GOALS
        // ======================================

        if (currentSavingsGoals.Count == 0)
        {
            ShowAssistantResponse(
                "You don't have any savings goals yet. " +
                "Add a goal and I can track your progress, " +
                "remaining amount, and how quickly you need to save.");

            return;
        }



        // ======================================
        // TOTAL SAVINGS
        // ======================================

        double totalTarget =
            currentSavingsGoals.Sum(
                goal =>
                    goal.TargetAmount);


        double totalSaved =
            currentSavingsGoals.Sum(
                goal =>
                    goal.CurrentAmount);


        double totalRemaining =
            Math.Max(
                totalTarget -
                totalSaved,
                0);


        double overallProgress =
            0;


        if (totalTarget > 0)
        {
            overallProgress =
                totalSaved /
                totalTarget *
                100;
        }



        // ======================================
        // COMPLETED GOALS
        // ======================================

        int completedGoals =
            currentSavingsGoals.Count(
                goal =>
                    goal.CurrentAmount >=
                    goal.TargetAmount);


        int activeGoals =
            currentSavingsGoals.Count -
            completedGoals;



        // ======================================
        // PRIMARY GOAL
        // ======================================

        SavingsGoal? primaryGoal =
            currentSavingsGoals
                .FirstOrDefault(
                    goal =>
                        goal.IsPrimary);

        if (primaryGoal != null)
        {
            lastReferencedSavingsGoal =
                primaryGoal;

            // The newest subject is now a savings goal,
            // not the previously discussed bill.
            lastReferencedBill =
                null;
        }

        // ======================================
        // BUILD RESPONSE
        // ======================================

        string response =
            $"You have {currentSavingsGoals.Count} savings goal";


        if (currentSavingsGoals.Count != 1)
        {
            response +=
                "s";
        }


        response +=
            $". You've saved {totalSaved:C} toward {totalTarget:C}, " +
            $"which is about {overallProgress:F0}% complete. ";



        // ======================================
        // COMPLETED GOALS
        // ======================================

        if (completedGoals > 0)
        {
            response +=
                $"{completedGoals} of your goals";


            if (completedGoals == 1)
            {
                response +=
                    " is complete. ";
            }

            else
            {
                response +=
                    " are complete. ";
            }
        }



        // ======================================
        // ACTIVE GOALS
        // ======================================

        if (activeGoals > 0)
        {
            response +=
                $"You still have {totalRemaining:C} left " +
                $"across your unfinished goals. ";
        }



        // ======================================
        // PRIMARY GOAL DETAILS
        // ======================================

        if (primaryGoal != null)
        {
            double primaryRemaining =
                Math.Max(
                    primaryGoal.TargetAmount -
                    primaryGoal.CurrentAmount,
                    0);


            double primaryProgress =
                0;


            if (primaryGoal.TargetAmount > 0)
            {
                primaryProgress =
                    primaryGoal.CurrentAmount /
                    primaryGoal.TargetAmount *
                    100;
            }



            // Goal is complete.
            if (primaryRemaining <= 0)
            {
                response +=
                    $"Your primary goal, {primaryGoal.Name}, " +
                    $"is fully funded.";
            }

            else
            {
                int daysRemaining =
                    (
                        primaryGoal.DeadLine.Date -
                        DateTime.Today
                    ).Days;


                response +=
                    $"Your primary goal is {primaryGoal.Name}. " +
                    $"It is {primaryProgress:F0}% complete with " +
                    $"{primaryRemaining:C} remaining";



                // Deadline is in future.
                if (daysRemaining > 0)
                {
                    double weeksRemaining =
                        Math.Max(
                            daysRemaining /
                            7.0,
                            1);


                    double neededPerWeek =
                        primaryRemaining /
                        weeksRemaining;


                    response +=
                        $". You have {daysRemaining} days until the deadline, " +
                        $"so you would need to average about " +
                        $"{neededPerWeek:C} per week to finish on time.";
                }



                // Deadline is today.
                else if (daysRemaining == 0)
                {
                    response +=
                        ". The deadline is today.";
                }



                // Deadline passed.
                else
                {
                    response +=
                        ". The deadline has already passed, " +
                        "so you may want to update the goal date " +
                        "or adjust the target.";
                }
            }
        }



        // ======================================
        // DISPLAY RESPONSE
        // ======================================

        ShowAssistantResponse(
            response);
    }



    // ==========================================
    // QUICK QUESTION:
    // WHAT SHOULD I FOCUS ON?
    // ==========================================

    private void FocusQuestionClicked(
        object? sender,
        EventArgs e)
    {
        if (!isProcessingTypedQuestion)
        {
            AddUserMessage(
                "What should I focus on?");
        }



        // ======================================
        // NO SUMMARY
        // ======================================

        if (currentSummary == null)
        {
            ShowAssistantResponse(
                "I don't have enough financial information yet " +
                "to determine what you should focus on.");

            return;
        }



        // ======================================
        // PRIORITY 1:
        // NO INCOME
        // ======================================

        if (currentSummary.MonthlyIncome <= 0)
        {
            ShowAssistantResponse(
                "Your first priority should be entering your monthly income. " +
                "Without income information, I can't accurately calculate " +
                "your Safe to Spend amount, cash flow, or financial health.");

            return;
        }



        // ======================================
        // PRIORITY 2:
        // NEGATIVE SAFE TO SPEND
        // ======================================

        if (currentSafeToSpend < 0)
        {
            double shortage =
                Math.Abs(
                    currentSafeToSpend);


            ShowAssistantResponse(
                $"Your biggest priority right now should be reducing " +
                $"spending or adjusting your financial plan. " +
                $"You're currently {shortage:C} short after accounting " +
                $"for your expenses, recurring bills, and savings goals. " +
                $"I would avoid unnecessary spending until that gap is reduced.");

            return;
        }



        // ======================================
        // CURRENT MONTH EXPENSES
        // ======================================

        List<Expense> currentMonthExpenses =
            analyticsService
                .GetCurrentMonthExpense(
                    currentExpenses);



        // ======================================
        // PRIORITY 3:
        // OVER-BUDGET CATEGORY
        // ======================================

        string? worstBudgetCategory =
            null;


        double biggestBudgetOverage =
            0;


        foreach (BudgetLimit budget
                 in currentBudgetLimits)
        {
            double categorySpent =
                analyticsService
                    .GetCategoryTotal(
                        currentMonthExpenses,
                        budget.Category);


            double amountOver =
                categorySpent -
                budget.LimitAmount;


            if (amountOver >
                biggestBudgetOverage)
            {
                biggestBudgetOverage =
                    amountOver;


                worstBudgetCategory =
                    budget.Category;
            }
        }



        if (worstBudgetCategory != null &&
            biggestBudgetOverage > 0)
        {
            ShowAssistantResponse(
                $"I would focus on your {worstBudgetCategory} spending first. " +
                $"You're currently {biggestBudgetOverage:C} over that " +
                $"category's budget. Reducing spending there will have " +
                $"the most immediate impact on getting your budget back on track.");

            return;
        }



        // ======================================
        // PRIORITY 4:
        // NEGATIVE MONTH-END PROJECTION
        // ======================================

        if (currentProjectedEndOfMonthMoney < 0)
        {
            double projectedShortage =
                Math.Abs(
                    currentProjectedEndOfMonthMoney);


            ShowAssistantResponse(
                $"Your spending pace should be your main focus. " +
                $"At your current rate, you're projected to finish " +
                $"the month about {projectedShortage:C} short. " +
                $"Try reducing discretionary spending for the rest " +
                $"of the month so your projected balance stays positive.");

            return;
        }



        // ======================================
        // PRIORITY 5:
        // UPCOMING BILL
        // ======================================

        RecurringExpenses? upcomingBill =
            currentRecurringExpenses
                .Where(
                    bill =>
                        bill.IsActive)
                .OrderBy(
                    bill =>
                        analyticsService
                            .GetDaysUntilDue(
                                bill.DueDay))
                .FirstOrDefault();



        if (upcomingBill != null)
        {
            int daysUntilBill =
                analyticsService
                    .GetDaysUntilDue(
                        upcomingBill.DueDay);


            if (daysUntilBill <= 7)
            {
                string dueText;


                if (daysUntilBill == 0)
                {
                    dueText =
                        "today";
                }

                else if (daysUntilBill == 1)
                {
                    dueText =
                        "tomorrow";
                }

                else
                {
                    dueText =
                        $"in {daysUntilBill} days";
                }


                ShowAssistantResponse(
                    $"Your next priority should be preparing for " +
                    $"{upcomingBill.Name}. " +
                    $"The {upcomingBill.Amount:C} bill is due {dueText}. " +
                    $"Make sure that money remains available before " +
                    $"making additional discretionary purchases.");

                return;
            }
        }



        // ======================================
        // PRIORITY 6:
        // SAVINGS GOALS
        // ======================================

        if (currentSavingsNeededThisMonth > 0)
        {
            ShowAssistantResponse(
                $"Your finances are currently stable enough that I would " +
                $"focus on your savings goals next. " +
                $"Based on your goal deadlines, you should try to put about " +
                $"{currentSavingsNeededThisMonth:C} toward savings this month. " +
                $"You still have {currentSafeToSpend:C} available after " +
                $"accounting for that savings plan.");

            return;
        }



        // ======================================
        // PRIORITY 7:
        // FINANCES LOOK HEALTHY
        // ======================================

        if (currentMonthExpenses.Count > 0)
        {
            string biggestCategory =
                analyticsService
                    .GetBiggestSpendingCategory(
                        currentMonthExpenses);


            double biggestCategoryAmount =
                analyticsService
                    .GetCategoryTotal(
                        currentMonthExpenses,
                        biggestCategory);


            ShowAssistantResponse(
                $"Nothing urgent stands out right now. " +
                $"You have {currentSafeToSpend:C} available within your " +
                $"current plan and your end-of-month projection is positive. " +
                $"If you want to improve further, keep an eye on " +
                $"{biggestCategory}, your largest spending category " +
                $"this month at {biggestCategoryAmount:C}.");
        }

        else
        {
            ShowAssistantResponse(
                $"Nothing urgent stands out right now. " +
                $"You currently have {currentSafeToSpend:C} available " +
                $"within your financial plan. " +
                $"Keep recording transactions so I can give you more " +
                $"detailed recommendations as your spending data grows.");
        }
    }



    // ==========================================
    // ASK BUTTON
    // ==========================================

    private void AskButtonClicked(
        object? sender,
        EventArgs e)
    {
        ProcessUserQuestion();
    }



    // ==========================================
    // ENTER KEY
    // ==========================================

    private void QuestionEntryCompleted(
        object? sender,
        EventArgs e)
    {
        ProcessUserQuestion();
    }



    // ==========================================
    // PROCESS TYPED QUESTIONS
    // ==========================================

    private void ProcessUserQuestion()
    {
        string question =
            QuestionEntry.Text?
                .Trim() ?? "";


        // Don't submit an empty question.
        if (string.IsNullOrWhiteSpace(
            question))
        {
            return;
        }


        // Display exactly what the user typed.
        AddUserMessage(
            question);


        // Makes matching easier.
        string lowerQuestion =
            question
                .ToLowerInvariant();


        // Prevent quick-question handlers
        // from creating duplicate user bubbles.
        isProcessingTypedQuestion =
            true;

        try
        {
            // ==================================
            // CATEGORY BUDGET REMAINING
            // ==================================

            string? spendingLimitCategory =
                FindMentionedCategory(
                    lowerQuestion);


            if (spendingLimitCategory != null &&
                QuestionContainsAny(
                    lowerQuestion,
                    "how much can i spend",
                    "how much can i still spend",
                    "how much do i have left",
                    "how much is left",
                    "remaining budget",
                    "budget remaining",
                    "how much room do i have"))
            {
                AnswerCategoryAvailableToSpendQuestion(
                    spendingLimitCategory);

                return;
            }
            // ==================================
            // AFFORDABILITY FOLLOW-UP
            // ==================================

            if (lastReferencedPurchaseAmount.HasValue &&
                QuestionContainsAny(
                    lowerQuestion,
                    "what about",
                    "how about",
                    "instead",
                    "what if"))
            {
                Match followUpAmount =
                    Regex.Match(
                        question,
                        @"\$?\s*(\d+(?:\.\d{1,2})?)");


                if (followUpAmount.Success)
                {
                    AnswerAffordabilityQuestion(
                        question);

                    return;
                }
            }
            // ==================================
            // REFER TO LAST PURCHASE
            // ==================================

            if (lastReferencedPurchaseAmount.HasValue &&
                QuestionContainsAny(
                    lowerQuestion,
                    "is that too much",
                    "is that to much",
                    "is it too much",
                    "is it to much",
                    "is that expensive",
                    "is it expensive",
                    "is that safe",
                    "is it safe",
                    "should i do it",
                    "should i buy it",
                    "can i do it",
                    "can i afford it"))
            {
                AnswerAffordabilityQuestion(
                    $"Can I afford ${lastReferencedPurchaseAmount.Value:F2}?");

                return;
            }
            // ==================================
            // AFFORDABILITY / SAFE TO SPEND
            // ==================================

            if (QuestionContainsAny(
                lowerQuestion,
                "afford",
                "can i spend",
                "should i spend",
                "can i buy",
                "should i buy",
                "safe to spend",
                "how much can i spend",
                "money to spend"))
            {
                AnswerAffordabilityQuestion(
                    question);

                return;
            }

            // ==================================
            // FOLLOW-UP ABOUT LAST CATEGORY
            // ==================================

            if (lastReferencedCategory != null &&
                QuestionContainsAny(
                    lowerQuestion,
                    "how much over",
                    "am i over",
                    "is that over",
                    "is it over",
                    "what is the budget for that",
                    "what's the budget for that",
                    "budget for it",
                    "budget for that",
                    "what is the remaining budget",
                    "what's the remaining budget",
                    "whats the remaining budget",
                    "remaining budget",
                    "budget remaining",
                    "how much is left in the budget",
                    "how much do i have left in the budget",
                    "how much do i have left"))
            {
                AnswerReferencedCategoryBudgetQuestion();

                return;
            }

            // ==================================
            // BUDGET RISK / CLOSE TO LIMIT
            // ==================================

            if (QuestionContainsAny(
                lowerQuestion,
                "which budget should i watch",
                "what budget should i watch",
                "closest to going over",
                "closest to my budget",
                "close to my budget",
                "close to any budget",
                "close to a budget",
                "budget is getting close",
                "budget getting close",
                "near my budget",
                "near a budget",
                "budget risk"))
            {
                AnswerBudgetRiskQuestion();

                return;
            }

            // ==================================
            // BUDGETS
            // ==================================

            if (QuestionContainsAny(
                lowerQuestion,
                "budget",
                "over budget",
                "budget limit",
                "blow my budget",
                "blew my budget"))
            {
                AnswerBudgetQuestion();

                return;
            }

            // ==================================
            // FOLLOW-UP ABOUT LAST BILL
            // ==================================

            if (lastReferencedBill != null &&
                QuestionContainsAny(
                    lowerQuestion,
                    "how much is that",
                    "how much is it",
                    "how much does that cost",
                    "how much does it cost",
                    "when is that due",
                    "when is it due",
                    "when is that due again",
                    "when is it due again",
                    "is that due soon",
                    "is it due soon"))
            {
                AnswerReferencedBillQuestion(
                    lowerQuestion);

                return;
            }


            // ==================================
            // FOLLOW-UP ABOUT SAVINGS GOAL
            // ==================================

            if (lastReferencedSavingsGoal != null &&
                QuestionContainsAny(
                    lowerQuestion,
                    "how much do i have left",
                    "how much is left",
                    "how much left",
                    "how much have i saved",
                    "how much did i save",
                    "when is it due",
                    "when is that due",
                    "when is the goal due",
                    "when is my goal due",
                    "how long do i have",
                    "how far along",
                    "what percent",
                    "what percentage",
                    "progress on it"))
            {
                AnswerReferencedSavingsGoalQuestion(
                    lowerQuestion);

                return;
            }

            
            // ==================================
            // BILLS
            // ==================================

            if (QuestionContainsAny(
                lowerQuestion,
                "bill",
                "bills",
                "due",
                "recurring",
                "payment",
                "payments"))
            {
                AnswerBillsQuestion();

                return;
            }



            // ==================================
            // SAVINGS
            // ==================================

            if (QuestionContainsAny(
                lowerQuestion,
                "saving",
                "savings",
                "save",
                "goal",
                "goals"))
            {
                SavingsQuestionClicked(
                    this,
                    EventArgs.Empty);

                return;
            }

            // ==================================
            // SPECIFIC SPENDING CATEGORY
            // ==================================

            string? mentionedCategory =
                FindMentionedCategory(
                    lowerQuestion);


            if (mentionedCategory != null &&
                QuestionContainsAny(
                    lowerQuestion,
                    "spend",
                    "spent",
                    "spending",
                    "how much",
                    "cost"))
            {
                AnswerCategorySpendingQuestion(
                    mentionedCategory);

                return;
            }

            // ==================================
            // SPENDING
            // ==================================

            if (QuestionContainsAny(
                lowerQuestion,
                "spending",
                "spend the most",
                "where is my money going",
                "where does my money go",
                "biggest category",
                "most money",
                "largest expense",
                "costs me the most"))
            {
                SpendingQuestionClicked(
                    this,
                    EventArgs.Empty);

                return;
            }
            // ==================================
            // CATEGORY SPENDING COMPARISON
            // ==================================

            string? comparisonCategory =
                FindMentionedCategory(
                    lowerQuestion);


            bool comparingWeeks =
                lowerQuestion.Contains("this week") &&
                lowerQuestion.Contains("last week");


            bool comparingMonths =
                lowerQuestion.Contains("this month") &&
                lowerQuestion.Contains("last month");


            if (comparisonCategory != null &&
                (comparingWeeks ||
                comparingMonths) &&
                QuestionContainsAny(
                    lowerQuestion,
                    "more",
                    "less",
                    "compare",
                    "compared",
                    "difference",
                    "change"))
            {
                AnswerCategorySpendingComparisonQuestion(
                    comparisonCategory,
                    lowerQuestion);

                return;
            }
            // ==================================
            // SPENDING PERIOD COMPARISON
            // ==================================

            if (QuestionContainsAny(
                lowerQuestion,
                "this week than last week",
                "this week compare to last week",
                "this week compared to last week",
                "compare this week",
                "this month than last month",
                "this month compare to last month",
                "this month compared to last month",
                "compare this month"))
            {
                AnswerSpendingComparisonQuestion(
                    lowerQuestion);

                return;
            }


            // ==================================
            // CATEGORY + TIME PERIOD SPENDING
            // ==================================

            string? categoryWithTimePeriod =
                FindMentionedCategory(
                    lowerQuestion);


            if (categoryWithTimePeriod != null &&
                QuestionContainsAny(
                    lowerQuestion,
                    "this week",
                    "last week",
                    "this month",
                    "last month") &&
                QuestionContainsAny(
                    lowerQuestion,
                    "spend",
                    "spent",
                    "spending",
                    "how much"))
            {
                AnswerCategoryTimePeriodQuestion(
                    categoryWithTimePeriod,
                    lowerQuestion);

                return;
            }
            // ==================================
            // SPENDING BY TIME PERIOD
            // ==================================

            if (QuestionContainsAny(
                lowerQuestion,
                "spend this week",
                "spent this week",
                "spending this week",
                "spend last week",
                "spent last week",
                "spending last week",
                "spend this month",
                "spent this month",
                "spending this month",
                "spent last month",
                "spend last month",
                "spending last month"))
            {
                AnswerSpendingTimePeriodQuestion(
                    lowerQuestion);

                return;
            }



            // ==================================
            // FOCUS / FINANCIAL PRIORITY
            // ==================================

            if (QuestionContainsAny(
                lowerQuestion,
                "focus",
                "priority",
                "what should i do",
                "attention",
                "worry about",
                "financially",
                "how am i doing"))
            {
                FocusQuestionClicked(
                    this,
                    EventArgs.Empty);

                return;
            }



            // ==================================
            // QUESTION NOT UNDERSTOOD YET
            // ==================================

            ShowAssistantResponse(
                "I don't understand that question yet. " +
                "I can currently help with spending, affordability, " +
                "budgets, bills, savings goals, and financial priorities.");
        }

        finally
        {
            // Always reset after processing.
            isProcessingTypedQuestion =
                false;


            // Clear the input box.
            QuestionEntry.Text =
                "";
        }
    }
    // ==========================================
    // ANSWER CATEGORY AVAILABLE TO SPEND
    // ==========================================

    private void AnswerCategoryAvailableToSpendQuestion(
        string category)
    {
        // Remember this category for
        // later follow-up questions.
        lastReferencedCategory =
            category;


        // ======================================
        // FIND CATEGORY BUDGET
        // ======================================

        BudgetLimit? budget =
            currentBudgetLimits
                .FirstOrDefault(
                    item =>
                        item.Category.Equals(
                            category,
                            StringComparison.OrdinalIgnoreCase));


        // ======================================
        // NO BUDGET CREATED
        // ======================================

        if (budget == null)
        {
            ShowAssistantResponse(
                $"You don't currently have a budget limit set for {category}. " +
                $"Your overall Safe to Spend is {currentSafeToSpend:C}, " +
                $"but I can't give you a category-specific spending limit " +
                $"until you create a {category} budget.");

            return;
        }


        // ======================================
        // CURRENT MONTH SPENDING
        // ======================================

        List<Expense> currentMonthExpenses =
            analyticsService
                .GetCurrentMonthExpense(
                    currentExpenses);


        double categorySpent =
            analyticsService
                .GetCategoryTotal(
                    currentMonthExpenses,
                    category);


        // ======================================
        // CATEGORY BUDGET REMAINING
        // ======================================

        double categoryRemaining =
            budget.LimitAmount -
            categorySpent;


        // ======================================
        // ALREADY OVER BUDGET
        // ======================================

        if (categoryRemaining < 0)
        {
            double amountOver =
                Math.Abs(
                    categoryRemaining);


            ShowAssistantResponse(
                $"I wouldn't recommend additional {category} spending right now. " +
                $"You've spent {categorySpent:C} against your " +
                $"{budget.LimitAmount:C} budget, so you're already " +
                $"{amountOver:C} over.");

            return;
        }


        // ======================================
        // EXACTLY AT BUDGET LIMIT
        // ======================================

        if (categoryRemaining == 0)
        {
            ShowAssistantResponse(
                $"You've already used your entire {category} budget. " +
                $"You've spent {categorySpent:C} out of {budget.LimitAmount:C}, " +
                $"so you currently have $0.00 remaining in that category.");

            return;
        }


        // ======================================
        // NO OVERALL SAFE-TO-SPEND MONEY
        // ======================================

        if (currentSafeToSpend <= 0)
        {
            ShowAssistantResponse(
                $"You still have {categoryRemaining:C} remaining in your " +
                $"{category} budget, but your overall Safe to Spend is " +
                $"{currentSafeToSpend:C}. I would avoid additional spending " +
                $"until your overall financial plan has room for it.");

            return;
        }


        // ======================================
        // ACTUAL SAFE AMOUNT
        // ======================================

        double amountSafeWithinBoth =
            Math.Min(
                categoryRemaining,
                currentSafeToSpend);


        // ======================================
        // RESPONSE
        // ======================================

        ShowAssistantResponse(
            $"You've spent {categorySpent:C} of your " +
            $"{budget.LimitAmount:C} {category} budget, leaving " +
            $"{categoryRemaining:C}. " +
            $"Your overall Safe to Spend is {currentSafeToSpend:C}, " +
            $"so up to {amountSafeWithinBoth:C} currently fits both " +
            $"your {category} budget and your overall financial plan.");
    }
    // ==========================================
    // ANSWER CATEGORY SPENDING COMPARISON
    // ==========================================

    private void AnswerCategorySpendingComparisonQuestion(
        string category,
        string question)
    {
        List<Expense> currentPeriodExpenses;

        List<Expense> previousPeriodExpenses;

        string currentPeriodName;

        string previousPeriodName;


        // ======================================
        // WEEK COMPARISON
        // ======================================

        if (question.Contains("week"))
        {
            currentPeriodExpenses =
                analyticsService
                    .GetCurrentWeekExpenses(
                        currentExpenses);


            previousPeriodExpenses =
                analyticsService
                    .GetLastWeekExpenses(
                        currentExpenses);


            currentPeriodName =
                "this week";


            previousPeriodName =
                "last week";
        }


        // ======================================
        // MONTH COMPARISON
        // ======================================

        else
        {
            currentPeriodExpenses =
                analyticsService
                    .GetCurrentMonthExpense(
                        currentExpenses);


            previousPeriodExpenses =
                analyticsService
                    .GetLastMonthExpense(
                        currentExpenses);


            currentPeriodName =
                "this month";


            previousPeriodName =
                "last month";
        }


        // Remember this category for
        // later follow-up questions.
        lastReferencedCategory =
            category;


        // ======================================
        // CATEGORY TOTALS
        // ======================================

        double currentSpent =
            analyticsService
                .GetCategoryTotal(
                    currentPeriodExpenses,
                    category);


        double previousSpent =
            analyticsService
                .GetCategoryTotal(
                    previousPeriodExpenses,
                    category);


        // ======================================
        // NO SPENDING IN EITHER PERIOD
        // ======================================

        if (currentSpent == 0 &&
            previousSpent == 0)
        {
            ShowAssistantResponse(
                $"You don't have any {category} spending recorded " +
                $"for either {currentPeriodName} or {previousPeriodName}.");

            return;
        }


        // ======================================
        // DIFFERENCE
        // ======================================

        double difference =
            currentSpent -
            previousSpent;


        double percentageChange =
            0;


        if (previousSpent > 0)
        {
            percentageChange =
                difference /
                previousSpent *
                100;
        }


        // ======================================
        // SPENDING INCREASED
        // ======================================

        if (difference > 0)
        {
            string response =
                $"You spent {currentSpent:C} on {category} {currentPeriodName}, " +
                $"compared with {previousSpent:C} {previousPeriodName}. " +
                $"That's {difference:C} more";


            if (previousSpent > 0)
            {
                response +=
                    $", an increase of about " +
                    $"{Math.Abs(percentageChange):F0}%";
            }


            response += ".";


            ShowAssistantResponse(
                response);

            return;
        }


        // ======================================
        // SPENDING DECREASED
        // ======================================

        if (difference < 0)
        {
            double amountLess =
                Math.Abs(
                    difference);


            string response =
                $"You spent {currentSpent:C} on {category} {currentPeriodName}, " +
                $"compared with {previousSpent:C} {previousPeriodName}. " +
                $"That's {amountLess:C} less";


            if (previousSpent > 0)
            {
                response +=
                    $", a decrease of about " +
                    $"{Math.Abs(percentageChange):F0}%";
            }


            response += ".";


            ShowAssistantResponse(
                response);

            return;
        }


        // ======================================
        // SAME AMOUNT
        // ======================================

        ShowAssistantResponse(
            $"Your {category} spending was exactly the same. " +
            $"You spent {currentSpent:C} {currentPeriodName} " +
            $"and {previousSpent:C} {previousPeriodName}.");
    }

    // ==========================================
    // ANSWER CATEGORY + TIME PERIOD QUESTION
    // ==========================================

    private void AnswerCategoryTimePeriodQuestion(
        string category,
        string question)
    {
        List<Expense> periodExpenses;

        string periodName;


        // ======================================
        // LAST WEEK
        // ======================================

        if (question.Contains(
            "last week"))
        {
            periodExpenses =
                analyticsService
                    .GetLastWeekExpenses(
                        currentExpenses);

            periodName =
                "last week";
        }


        // ======================================
        // THIS WEEK
        // ======================================

        else if (question.Contains(
            "this week"))
        {
            periodExpenses =
                analyticsService
                    .GetCurrentWeekExpenses(
                        currentExpenses);

            periodName =
                "this week";
        }


        // ======================================
        // LAST MONTH
        // ======================================

        else if (question.Contains(
            "last month"))
        {
            periodExpenses =
                analyticsService
                    .GetLastMonthExpense(
                        currentExpenses);

            periodName =
                "last month";
        }


        // ======================================
        // THIS MONTH
        // ======================================

        else
        {
            periodExpenses =
                analyticsService
                    .GetCurrentMonthExpense(
                        currentExpenses);

            periodName =
                "this month";
        }


        // ======================================
        // CATEGORY TOTAL
        // ======================================

        double categorySpent =
            analyticsService
                .GetCategoryTotal(
                    periodExpenses,
                    category);


        // Remember category for follow-up
        // budget questions.
        lastReferencedCategory =
            category;


        // ======================================
        // NO SPENDING
        // ======================================

        if (categorySpent <= 0)
        {
            ShowAssistantResponse(
                $"You don't have any {category} spending recorded for {periodName}.");

            return;
        }


        // ======================================
        // TOTAL PERIOD SPENDING
        // ======================================

        double totalSpent =
            analyticsService
                .GetTotalSpent(
                    periodExpenses);


        double percentage =
            0;


        if (totalSpent > 0)
        {
            percentage =
                categorySpent /
                totalSpent *
                100;
        }


        // ======================================
        // RESPONSE
        // ======================================

        ShowAssistantResponse(
            $"You spent {categorySpent:C} on {category} {periodName}. " +
            $"That was about {percentage:F0}% of your total " +
            $"{periodName} spending of {totalSpent:C}.");
    }

    // ==========================================
    // ANSWER SPENDING COMPARISON QUESTION
    // ==========================================

    private void AnswerSpendingComparisonQuestion(
        string question)
    {
        List<Expense> currentPeriodExpenses;

        List<Expense> previousPeriodExpenses;

        string currentPeriodName;

        string previousPeriodName;



        // ======================================
        // WEEK COMPARISON
        // ======================================

        if (question.Contains(
            "week"))
        {
            currentPeriodExpenses =
                analyticsService
                    .GetCurrentWeekExpenses(
                        currentExpenses);


            previousPeriodExpenses =
                analyticsService
                    .GetLastWeekExpenses(
                        currentExpenses);


            currentPeriodName =
                "this week";


            previousPeriodName =
                "last week";
        }



        // ======================================
        // MONTH COMPARISON
        // ======================================

        else
        {
            currentPeriodExpenses =
                analyticsService
                    .GetCurrentMonthExpense(
                        currentExpenses);


            previousPeriodExpenses =
                analyticsService
                    .GetLastMonthExpense(
                        currentExpenses);


            currentPeriodName =
                "this month";


            previousPeriodName =
                "last month";
        }



        // ======================================
        // TOTALS
        // ======================================

        double currentSpent =
            analyticsService
                .GetTotalSpent(
                    currentPeriodExpenses);


        double previousSpent =
            analyticsService
                .GetTotalSpent(
                    previousPeriodExpenses);



        // ======================================
        // NO DATA
        // ======================================

        if (currentSpent == 0 &&
            previousSpent == 0)
        {
            ShowAssistantResponse(
                $"You don't have spending recorded for either " +
                $"{currentPeriodName} or {previousPeriodName}.");

            return;
        }



        // ======================================
        // DIFFERENCE
        // ======================================

        double difference =
            currentSpent -
            previousSpent;



        // ======================================
        // PERCENT CHANGE
        // ======================================

        double percentageChange =
            0;


        if (previousSpent > 0)
        {
            percentageChange =
                difference /
                previousSpent *
                100;
        }



        // ======================================
        // SPENDING INCREASED
        // ======================================

        if (difference > 0)
        {
            string response =
                $"Yes. You spent {currentSpent:C} {currentPeriodName} " +
                $"compared with {previousSpent:C} {previousPeriodName}. " +
                $"That's {difference:C} more";


            if (previousSpent > 0)
            {
                response +=
                    $", an increase of about " +
                    $"{Math.Abs(percentageChange):F0}%";
            }


            response +=
                ".";


            ShowAssistantResponse(
                response);

            return;
        }



        // ======================================
        // SPENDING DECREASED
        // ======================================

        if (difference < 0)
        {
            double amountLess =
                Math.Abs(
                    difference);


            string response =
                $"No. You spent {currentSpent:C} {currentPeriodName} " +
                $"compared with {previousSpent:C} {previousPeriodName}. " +
                $"That's {amountLess:C} less";


            if (previousSpent > 0)
            {
                response +=
                    $", a decrease of about " +
                    $"{Math.Abs(percentageChange):F0}%";
            }


            response +=
                ".";


            ShowAssistantResponse(
                response);

            return;
        }



        // ======================================
        // SPENDING IS THE SAME
        // ======================================

        ShowAssistantResponse(
            $"Your spending is exactly the same. " +
            $"You spent {currentSpent:C} {currentPeriodName} " +
            $"and {previousSpent:C} {previousPeriodName}.");
    }
    // ==========================================
    // ANSWER SPENDING TIME PERIOD QUESTION
    // ==========================================

    private void AnswerSpendingTimePeriodQuestion(
        string question)
    {
        List<Expense> periodExpenses;

        string periodName;



        // ======================================
        // LAST WEEK
        // ======================================

        if (question.Contains(
            "last week"))
        {
            periodExpenses =
                analyticsService
                    .GetLastWeekExpenses(
                        currentExpenses);


            periodName =
                "last week";
        }



        // ======================================
        // THIS WEEK
        // ======================================

        else if (question.Contains(
            "this week"))
        {
            periodExpenses =
                analyticsService
                    .GetCurrentWeekExpenses(
                        currentExpenses);


            periodName =
                "this week";
        }



        // ======================================
        // LAST MONTH
        // ======================================

        else if (question.Contains(
            "last month"))
        {
            periodExpenses =
                analyticsService
                    .GetLastMonthExpense(
                        currentExpenses);


            periodName =
                "last month";
        }



        // ======================================
        // THIS MONTH
        // ======================================

        else
        {
            periodExpenses =
                analyticsService
                    .GetCurrentMonthExpense(
                        currentExpenses);


            periodName =
                "this month";
        }



        // ======================================
        // NO SPENDING
        // ======================================

        if (periodExpenses.Count == 0)
        {
            ShowAssistantResponse(
                $"You don't have any spending recorded for {periodName}.");

            return;
        }



        // ======================================
        // TOTAL SPENDING
        // ======================================

        double totalSpent =
            analyticsService
                .GetTotalSpent(
                    periodExpenses);



        // ======================================
        // BIGGEST CATEGORY
        // ======================================

        string biggestCategory =
            analyticsService
                .GetBiggestSpendingCategory(
                    periodExpenses);


        double biggestCategoryAmount =
            analyticsService
                .GetCategoryTotal(
                    periodExpenses,
                    biggestCategory);



        // Remember the category for
        // follow-up questions.
        lastReferencedCategory =
            biggestCategory;



        // ======================================
        // RESPONSE
        // ======================================

        ShowAssistantResponse(
            $"You spent {totalSpent:C} {periodName}. " +
            $"Your largest spending category was {biggestCategory} " +
            $"at {biggestCategoryAmount:C}.");
    }
    // ==========================================
    // ANSWER CATEGORY SPENDING QUESTION
    // ==========================================

    private void AnswerCategorySpendingQuestion(
        string category)
    {
        // Remember this category for
        // later follow-up questions.
        lastReferencedCategory =
            category;


        // ======================================
        // CURRENT MONTH TRANSACTIONS
        // ======================================

        List<Expense> currentMonthExpenses =
            analyticsService
                .GetCurrentMonthExpense(
                    currentExpenses);


        // ======================================
        // CATEGORY TOTAL
        // ======================================

        double categorySpent =
            analyticsService
                .GetCategoryTotal(
                    currentMonthExpenses,
                    category);


        // ======================================
        // NO SPENDING
        // ======================================

        if (categorySpent <= 0)
        {
            ShowAssistantResponse(
                $"You haven't recorded any {category} spending this month.");

            return;
        }


        // ======================================
        // TOTAL MONTHLY SPENDING
        // ======================================

        double totalSpent =
            analyticsService
                .GetTotalSpent(
                    currentMonthExpenses);


        double percentage =
            0;


        if (totalSpent > 0)
        {
            percentage =
                categorySpent /
                totalSpent *
                100;
        }


        // ======================================
        // CHECK FOR CATEGORY BUDGET
        // ======================================

        BudgetLimit? budget =
            currentBudgetLimits
                .FirstOrDefault(
                    item =>
                        item.Category.Equals(
                            category,
                            StringComparison.OrdinalIgnoreCase));


        // ======================================
        // NO BUDGET FOR CATEGORY
        // ======================================

        if (budget == null)
        {
            ShowAssistantResponse(
                $"You've spent {categorySpent:C} on {category} this month. " +
                $"That's about {percentage:F0}% of your total monthly spending. " +
                $"You don't currently have a budget limit set for this category.");

            return;
        }


        // ======================================
        // COMPARE AGAINST BUDGET
        // ======================================

        double budgetRemaining =
            budget.LimitAmount -
            categorySpent;


        if (budgetRemaining > 0)
        {
            ShowAssistantResponse(
                $"You've spent {categorySpent:C} on {category} this month, " +
                $"which is about {percentage:F0}% of your total spending. " +
                $"Your {category} budget is {budget.LimitAmount:C}, " +
                $"so you still have {budgetRemaining:C} remaining.");

            return;
        }


        if (budgetRemaining == 0)
        {
            ShowAssistantResponse(
                $"You've spent {categorySpent:C} on {category} this month " +
                $"and have used your entire {budget.LimitAmount:C} budget.");

            return;
        }


        double amountOver =
            Math.Abs(
                budgetRemaining);


        ShowAssistantResponse(
            $"You've spent {categorySpent:C} on {category} this month. " +
            $"Your budget is {budget.LimitAmount:C}, " +
            $"so you're currently {amountOver:C} over budget.");
    }
    // ==========================================
    // ANSWER SAVINGS GOAL FOLLOW-UP
    // ==========================================

    private void AnswerReferencedSavingsGoalQuestion(
        string question)
    {
        // ======================================
        // NO SAVINGS GOAL CONTEXT
        // ======================================

        if (lastReferencedSavingsGoal == null)
        {
            ShowAssistantResponse(
                "I'm not sure which savings goal you're referring to.");

            return;
        }


        SavingsGoal goal =
            lastReferencedSavingsGoal;



        // ======================================
        // CALCULATE GOAL INFORMATION
        // ======================================

        double amountRemaining =
            Math.Max(
                goal.TargetAmount -
                goal.CurrentAmount,
                0);


        double progress =
            0;


        if (goal.TargetAmount > 0)
        {
            progress =
                goal.CurrentAmount /
                goal.TargetAmount *
                100;
        }


        int daysRemaining =
            (
                goal.DeadLine.Date -
                DateTime.Today
            ).Days;



        // ======================================
        // HOW MUCH IS LEFT?
        // ======================================

        if (QuestionContainsAny(
            question,
            "how much do i have left",
            "how much is left",
            "how much left"))
        {
            if (amountRemaining <= 0)
            {
                ShowAssistantResponse(
                    $"{goal.Name} is already fully funded. " +
                    $"You've reached the {goal.TargetAmount:C} target.");
            }

            else
            {
                ShowAssistantResponse(
                    $"You have {amountRemaining:C} left to save for " +
                    $"{goal.Name}. You've currently saved " +
                    $"{goal.CurrentAmount:C} of your {goal.TargetAmount:C} goal.");
            }

            return;
        }



        // ======================================
        // HOW MUCH HAVE I SAVED?
        // ======================================

        if (QuestionContainsAny(
            question,
            "how much have i saved",
            "how much did i save"))
        {
            ShowAssistantResponse(
                $"You've saved {goal.CurrentAmount:C} toward {goal.Name}. " +
                $"Your target is {goal.TargetAmount:C}, so the goal is " +
                $"about {progress:F0}% complete.");

            return;
        }



        // ======================================
        // PROGRESS / PERCENTAGE
        // ======================================

        if (QuestionContainsAny(
            question,
            "how far along",
            "what percent",
            "what percentage",
            "progress on it"))
        {
            ShowAssistantResponse(
                $"{goal.Name} is about {progress:F0}% complete. " +
                $"You've saved {goal.CurrentAmount:C} out of " +
                $"{goal.TargetAmount:C}.");

            return;
        }



        // ======================================
        // DEADLINE
        // ======================================

        if (QuestionContainsAny(
            question,
            "when is it due",
            "when is that due",
            "when is the goal due",
            "when is my goal due",
            "how long do i have"))
        {
            if (daysRemaining > 1)
            {
                ShowAssistantResponse(
                    $"{goal.Name} is due on " +
                    $"{goal.DeadLine:MMMM d, yyyy}. " +
                    $"You have {daysRemaining} days remaining.");
            }

            else if (daysRemaining == 1)
            {
                ShowAssistantResponse(
                    $"{goal.Name} is due tomorrow.");
            }

            else if (daysRemaining == 0)
            {
                ShowAssistantResponse(
                    $"{goal.Name} is due today.");
            }

            else
            {
                ShowAssistantResponse(
                    $"The deadline for {goal.Name} was " +
                    $"{goal.DeadLine:MMMM d, yyyy}. " +
                    $"You still have {amountRemaining:C} remaining, " +
                    $"so you may want to update the deadline.");
            }

            return;
        }



        // ======================================
        // FALLBACK
        // ======================================

        ShowAssistantResponse(
            $"{goal.Name} is {progress:F0}% complete. " +
            $"You've saved {goal.CurrentAmount:C} of " +
            $"{goal.TargetAmount:C}, with {amountRemaining:C} remaining.");
    }

    // ==========================================
    // ANSWER AFFORDABILITY QUESTION
    // ==========================================

    private void AnswerAffordabilityQuestion(
        string question)
    {
        // ======================================
        // FIND PURCHASE AMOUNT
        // ======================================

        // First try to find an amount that
        // specifically uses a dollar sign.
        Match amountMatch =
            Regex.Match(
                question,
                @"\$\s*(\d+(?:\.\d{1,2})?)");



        // If no dollar sign was used,
        // try finding a normal number.
        if (!amountMatch.Success)
        {
            amountMatch =
                Regex.Match(
                    question,
                    @"\b(\d+(?:\.\d{1,2})?)\b");
        }



        // ======================================
        // NO PRICE PROVIDED
        // ======================================

        if (!amountMatch.Success)
        {
            ShowAssistantResponse(
                $"Based on your current finances, you have " +
                $"{currentDailySafeToSpend:C} safe to spend today " +
                $"and {currentSafeToSpend:C} available within your " +
                $"current monthly plan. Tell me the price of what " +
                $"you're considering and I can compare it directly.");

            return;
        }



        // ======================================
        // CONVERT PRICE
        // ======================================

        bool amountParsed =
            double.TryParse(
                amountMatch.Groups[1].Value,
                out double purchaseAmount);


        if (!amountParsed ||
            purchaseAmount <= 0)
        {
            ShowAssistantResponse(
                "I couldn't understand the purchase amount. " +
                "Try something like \"Can I afford a $50 dinner?\"");

            return;
        }

        lastReferencedPurchaseAmount =
        purchaseAmount;

        // ======================================
        // CHECK FINANCIAL INFORMATION
        // ======================================

        if (currentSummary == null ||
            currentSummary.MonthlyIncome <= 0)
        {
            ShowAssistantResponse(
                "I need your monthly income and financial information " +
                "before I can accurately judge whether that purchase " +
                "fits your budget.");

            return;
        }



        // ======================================
        // NO DISCRETIONARY MONEY
        // ======================================

        if (currentSafeToSpend <= 0)
        {
            ShowAssistantResponse(
                $"I wouldn't recommend the {purchaseAmount:C} purchase " +
                $"right now. Your current financial plan doesn't have " +
                $"discretionary money available after spending, bills, " +
                $"and savings commitments.");

            return;
        }



        // ======================================
        // PURCHASE EXCEEDS MONTHLY SAFE AMOUNT
        // ======================================

        if (purchaseAmount >
            currentSafeToSpend)
        {
            double amountOver =
                purchaseAmount -
                currentSafeToSpend;


            ShowAssistantResponse(
                $"I wouldn't recommend spending {purchaseAmount:C}. " +
                $"You currently have {currentSafeToSpend:C} safe to " +
                $"spend for the rest of the month, so that purchase " +
                $"would put you about {amountOver:C} beyond your current plan.");

            return;
        }



        // ======================================
        // FITS MONTHLY PLAN,
        // BUT ABOVE DAILY SAFE AMOUNT
        // ======================================

        if (purchaseAmount >
            currentDailySafeToSpend)
        {
            double remainingAfterPurchase =
                currentSafeToSpend -
                purchaseAmount;


            ShowAssistantResponse(
                $"You can technically afford the {purchaseAmount:C} " +
                $"purchase within your monthly plan, but it's above " +
                $"your current daily Safe to Spend amount of " +
                $"{currentDailySafeToSpend:C}. " +
                $"After the purchase, you would have about " +
                $"{remainingAfterPurchase:C} of Safe to Spend left " +
                $"for the rest of the month.");

            return;
        }



        // ======================================
        // PURCHASE FITS COMFORTABLY
        // ======================================

        double safeRemaining =
            currentSafeToSpend -
            purchaseAmount;


        ShowAssistantResponse(
            $"Yes. A {purchaseAmount:C} purchase fits within your " +
            $"current plan. Your Safe to Spend today is about " +
            $"{currentDailySafeToSpend:C}, and you would still have " +
            $"approximately {safeRemaining:C} available within your " +
            $"monthly plan afterward.");
    }



    // ==========================================
    // SHOW POCKETAI RESPONSE
    // ==========================================

    private void ShowAssistantResponse(
        string message)
    {
        // Keep the original label updated,
        // although its welcome container will
        // be hidden during a real conversation.
        AssistantMessageLabel.Text =
            message;


        // Add a permanent chat bubble.
        AddPocketAIMessage(
            message);
    }



    // ==========================================
    // ADD USER MESSAGE
    // ==========================================

    private void AddUserMessage(
        string message)
    {
        WelcomeAssistantMessage.IsVisible =
            false;


        // ======================================
        // USER MESSAGE BUBBLE
        // ======================================

        Border messageBubble =
            new Border
            {
                BackgroundColor =
                    GetThemeColor(
                        "ThemePrimary",
                        "#7C3AED"),

                StrokeThickness =
                    0,

                Padding =
                    new Thickness(
                        16,
                        12),

                HorizontalOptions =
                    LayoutOptions.End,

                MaximumWidthRequest =
                    600
            };


        messageBubble.StrokeShape =
            new RoundRectangle
            {
                CornerRadius =
                    new CornerRadius(
                        14)
            };


        // ======================================
        // MESSAGE TEXT
        // ======================================

        Label messageLabel =
            new Label
            {
                Text =
                    message,

                FontSize =
                    14,

                TextColor =
                    GetThemeColor(
                        "TextOnPrimary",
                        "#FFFFFF"),

                LineHeight =
                    1.3
            };


        messageBubble.Content =
            messageLabel;


        // ======================================
        // "YOU" LABEL
        // ======================================

        Label userLabel =
            new Label
            {
                Text =
                    "YOU",

                FontSize =
                    9,

                FontAttributes =
                    FontAttributes.Bold,

                TextColor =
                    GetThemeColor(
                        "TextSecondary",
                        "#6B7280"),

                HorizontalOptions =
                    LayoutOptions.End
            };


        // ======================================
        // GROUP MESSAGE
        // ======================================

        VerticalStackLayout messageGroup =
            new VerticalStackLayout
            {
                Spacing =
                    5,

                HorizontalOptions =
                    LayoutOptions.End
            };


        messageGroup.Children.Add(
            userLabel);


        messageGroup.Children.Add(
            messageBubble);


        ConversationContainer.Children.Add(
            messageGroup);


        // ======================================
        // REMEMBER THE CONTROLS
        // ======================================

        userMessageBubbles.Add(
            messageBubble);


        userMessageLabels.Add(
            messageLabel);


        userNameLabels.Add(
            userLabel);


        ScrollToLatestMessage(
            messageGroup);
    }

    // ==========================================
    // ANSWER FOLLOW-UP CATEGORY BUDGET QUESTION
    // ==========================================

    private void AnswerReferencedCategoryBudgetQuestion()
    {
        // ======================================
        // NO CATEGORY CONTEXT
        // ======================================

        if (string.IsNullOrWhiteSpace(
            lastReferencedCategory))
        {
            ShowAssistantResponse(
                "I'm not sure which spending category you're referring to.");

            return;
        }


        string category =
            lastReferencedCategory;



        // ======================================
        // FIND THE CATEGORY'S BUDGET
        // ======================================

        BudgetLimit? budget =
            currentBudgetLimits
                .FirstOrDefault(
                    item =>
                        item.Category.Equals(
                            category,
                            StringComparison.OrdinalIgnoreCase));


        // The user may be spending in a category
        // that doesn't have a budget yet.
        if (budget == null)
        {
            ShowAssistantResponse(
                $"You haven't created a budget limit for {category} yet. " +
                $"You can create one in the Budget page and I'll be able " +
                $"to track whether you're staying within it.");

            return;
        }



        // ======================================
        // CURRENT MONTH CATEGORY SPENDING
        // ======================================

        List<Expense> currentMonthExpenses =
            analyticsService
                .GetCurrentMonthExpense(
                    currentExpenses);


        double categorySpent =
            analyticsService
                .GetCategoryTotal(
                    currentMonthExpenses,
                    category);



        // ======================================
        // COMPARE SPENDING TO BUDGET
        // ======================================

        double difference =
            categorySpent -
            budget.LimitAmount;



        // ======================================
        // OVER BUDGET
        // ======================================

        if (difference > 0)
        {
            ShowAssistantResponse(
                $"Your {category} budget is {budget.LimitAmount:C}. " +
                $"You've spent {categorySpent:C} this month, " +
                $"so you're {difference:C} over budget.");

            return;
        }



        if (difference == 0)
        {
            ShowAssistantResponse(
                $"You've used your entire {category} budget. " +
                $"You've spent {categorySpent:C} out of " +
                $"{budget.LimitAmount:C}, so you have $0.00 remaining " +
                $"in your {category} budget this month.");

            return;
        }



        // ======================================
        // STILL UNDER BUDGET
        // ======================================

        double remaining =
            Math.Abs(
                difference);


        ShowAssistantResponse(
            $"You're still within your {category} budget. " +
            $"You've spent {categorySpent:C} out of " +
            $"{budget.LimitAmount:C}, leaving {remaining:C} available.");
    }
    
    // ==========================================
    // ANSWER BUDGET RISK QUESTION
    // ==========================================

    private void AnswerBudgetRiskQuestion()
    {
        // ======================================
        // NO BUDGETS
        // ======================================

        if (currentBudgetLimits.Count == 0)
        {
            ShowAssistantResponse(
                "You don't have any budget limits set yet. " +
                "Create category budgets and I can warn you " +
                "when you're getting close to them.");

            return;
        }


        // ======================================
        // CURRENT MONTH EXPENSES
        // ======================================

        List<Expense> currentMonthExpenses =
            analyticsService
                .GetCurrentMonthExpense(
                    currentExpenses);


        BudgetLimit? highestBudget =
            null;


        double highestSpent =
            0;


        double highestPercentage =
            0;


        // ======================================
        // FIND HIGHEST BUDGET USAGE
        // ======================================

        foreach (BudgetLimit budget
                in currentBudgetLimits)
        {
            // Avoid dividing by zero.
            if (budget.LimitAmount <= 0)
            {
                continue;
            }


            double spent =
                analyticsService
                    .GetCategoryTotal(
                        currentMonthExpenses,
                        budget.Category);


            double percentageUsed =
                spent /
                budget.LimitAmount *
                100;


            if (highestBudget == null ||
                percentageUsed >
                highestPercentage)
            {
                highestBudget =
                    budget;


                highestSpent =
                    spent;


                highestPercentage =
                    percentageUsed;
            }
        }


        // ======================================
        // NO VALID BUDGET
        // ======================================

        if (highestBudget == null)
        {
            ShowAssistantResponse(
                "I couldn't find a valid budget limit to analyze.");

            return;
        }


        // Remember this category so the user
        // can ask follow-up questions about it.
        lastReferencedCategory =
            highestBudget.Category;


        double remaining =
            highestBudget.LimitAmount -
            highestSpent;


        // ======================================
        // ALREADY OVER BUDGET
        // ======================================

        if (remaining < 0)
        {
            double amountOver =
                Math.Abs(
                    remaining);


            ShowAssistantResponse(
                $"Your {highestBudget.Category} budget needs the most " +
                $"attention right now. You've spent {highestSpent:C} " +
                $"against a {highestBudget.LimitAmount:C} budget, " +
                $"putting you {amountOver:C} over the limit.");

            return;
        }


        // ======================================
        // EXACTLY AT LIMIT
        // ======================================

        if (remaining <= 0.01)
        {
            ShowAssistantResponse(
                $"Your {highestBudget.Category} budget is the one to watch. " +
                $"You've used 100% of the budget: {highestSpent:C} out of " +
                $"{highestBudget.LimitAmount:C}. You have $0.00 remaining.");

            return;
        }


        // ======================================
        // 80% OR MORE USED
        // ======================================

        if (highestPercentage >= 80)
        {
            ShowAssistantResponse(
                $"Your {highestBudget.Category} budget is getting close " +
                $"to its limit. You've used about {highestPercentage:F0}% " +
                $"of it, spending {highestSpent:C} out of " +
                $"{highestBudget.LimitAmount:C}. " +
                $"You have {remaining:C} remaining.");

            return;
        }


        // ======================================
        // UNDER 80%
        // ======================================

        ShowAssistantResponse(
            $"None of your budgets are in a high-risk range right now. " +
            $"Your closest is {highestBudget.Category}, where you've used " +
            $"about {highestPercentage:F0}% of the " +
            $"{highestBudget.LimitAmount:C} budget. " +
            $"You still have {remaining:C} remaining.");
    }
    // ==========================================
    // ANSWER BUDGET QUESTIONS
    // ==========================================

    private void AnswerBudgetQuestion()
    {
        // ======================================
        // NO BUDGETS CREATED
        // ======================================

        if (currentBudgetLimits.Count == 0)
        {
            ShowAssistantResponse(
                "You don't have any budget limits set yet. " +
                "Create budgets for categories like Dining, Groceries, " +
                "Shopping, or Entertainment and I can track whether " +
                "you're staying within them.");

            return;
        }


        // ======================================
        // CURRENT MONTH SPENDING
        // ======================================

        List<Expense> currentMonthExpenses =
            analyticsService
                .GetCurrentMonthExpense(
                    currentExpenses);


        // ======================================
        // BUDGET STATUS LISTS
        // ======================================

        List<BudgetStatusItem> overBudgetCategories =
            new List<BudgetStatusItem>();


        List<BudgetStatusItem> atLimitCategories =
            new List<BudgetStatusItem>();


        // ======================================
        // CHECK EACH BUDGET
        // ======================================

        foreach (BudgetLimit budget
                in currentBudgetLimits)
        {
            double spent =
                analyticsService
                    .GetCategoryTotal(
                        currentMonthExpenses,
                        budget.Category);


            double amountOver =
                spent -
                budget.LimitAmount;


            // ==================================
            // OVER BUDGET
            // ==================================

            if (amountOver > 0.01)
            {
                overBudgetCategories.Add(
                    new BudgetStatusItem
                    {
                        Category =
                            budget.Category,

                        Limit =
                            budget.LimitAmount,

                        Spent =
                            spent,

                        AmountOver =
                            amountOver
                    });

                continue;
            }


            // ==================================
            // EXACTLY AT BUDGET LIMIT
            // ==================================

            if (Math.Abs(amountOver) <= 0.01)
            {
                atLimitCategories.Add(
                    new BudgetStatusItem
                    {
                        Category =
                            budget.Category,

                        Limit =
                            budget.LimitAmount,

                        Spent =
                            spent,

                        AmountOver =
                            0
                    });
            }
        }


        // ======================================
        // OVER-BUDGET CATEGORIES
        // ======================================

        if (overBudgetCategories.Count > 0)
        {
            BudgetStatusItem worstBudget =
                overBudgetCategories
                    .OrderByDescending(
                        item =>
                            item.AmountOver)
                    .First();


            string response;


            if (overBudgetCategories.Count == 1)
            {
                response =
                    "Yes. You're currently over budget in 1 category. ";
            }

            else
            {
                response =
                    $"Yes. You're currently over budget in " +
                    $"{overBudgetCategories.Count} categories. ";
            }


            response +=
                $"Your biggest issue is {worstBudget.Category}. " +
                $"You've spent {worstBudget.Spent:C} against a " +
                $"{worstBudget.Limit:C} budget, putting you " +
                $"{worstBudget.AmountOver:C} over the limit.";


            // Also warn if another category
            // has reached its exact limit.
            if (atLimitCategories.Count > 0)
            {
                response +=
                    $" You also have {atLimitCategories.Count} budget " +
                    $"{(atLimitCategories.Count == 1 ? "category" : "categories")} " +
                    $"that has reached its limit.";
            }


            ShowAssistantResponse(
                response);

            return;
        }


        // ======================================
        // AT BUDGET LIMIT
        // ======================================

        if (atLimitCategories.Count > 0)
        {
            BudgetStatusItem firstAtLimit =
                atLimitCategories[0];


            if (atLimitCategories.Count == 1)
            {
                ShowAssistantResponse(
                    $"You're not over budget yet, but your " +
                    $"{firstAtLimit.Category} budget has reached its limit. " +
                    $"You've spent {firstAtLimit.Spent:C} out of " +
                    $"{firstAtLimit.Limit:C}, leaving $0.00 remaining. " +
                    $"I would avoid additional spending in that category.");

                return;
            }


            ShowAssistantResponse(
                $"You're not over budget yet, but " +
                $"{atLimitCategories.Count} of your budget categories " +
                $"have reached their limits. " +
                $"Any additional spending in those categories would " +
                $"put you over budget.");

            return;
        }


        // ======================================
        // ALL BUDGETS ARE UNDER LIMIT
        // ======================================

        ShowAssistantResponse(
            $"You're currently under all {currentBudgetLimits.Count} " +
            $"of your budget limits this month. " +
            $"Keep tracking your transactions so I can warn you " +
            $"as categories get close to their limits.");
    }


    // ==========================================
    // ANSWER FOLLOW-UP BILL QUESTION
    // ==========================================

    private void AnswerReferencedBillQuestion(
        string question)
    {
        // ======================================
        // NO BILL CONTEXT
        // ======================================

        if (lastReferencedBill == null)
        {
            ShowAssistantResponse(
                "I'm not sure which bill you're referring to.");

            return;
        }


        RecurringExpenses bill =
            lastReferencedBill;



        // ======================================
        // USER ASKED ABOUT PRICE
        // ======================================

        if (QuestionContainsAny(
            question,
            "how much",
            "cost"))
        {
            ShowAssistantResponse(
                $"{bill.Name} costs {bill.Amount:C} per month.");

            return;
        }



        // ======================================
        // USER ASKED ABOUT DUE DATE
        // ======================================

        if (QuestionContainsAny(
            question,
            "when",
            "due",
            "due soon"))
        {
            int daysUntilDue =
                analyticsService
                    .GetDaysUntilDue(
                        bill.DueDay);


            string dueText;


            if (daysUntilDue == 0)
            {
                dueText =
                    "today";
            }

            else if (daysUntilDue == 1)
            {
                dueText =
                    "tomorrow";
            }

            else
            {
                dueText =
                    $"in {daysUntilDue} days";
            }


            ShowAssistantResponse(
                $"{bill.Name} is {bill.Amount:C} per month and is due {dueText}.");

            return;
        }



        // ======================================
        // FALLBACK
        // ======================================

        ShowAssistantResponse(
            $"{bill.Name} is {bill.Amount:C} per month.");
    }

    // ==========================================
    // ANSWER BILL QUESTIONS
    // ==========================================

    private void AnswerBillsQuestion()
    {
        // ======================================
        // ACTIVE BILLS ONLY
        // ======================================

        List<RecurringExpenses> activeBills =
            currentRecurringExpenses
                .Where(
                    bill =>
                        bill.IsActive)
                .OrderBy(
                    bill =>
                        analyticsService
                            .GetDaysUntilDue(
                                bill.DueDay))
                .ToList();



        // ======================================
        // NO ACTIVE BILLS
        // ======================================

        if (activeBills.Count == 0)
        {
            ShowAssistantResponse(
                "You don't currently have any active recurring bills recorded.");

            return;
        }



        // ======================================
        // NEXT BILL
        // ======================================

        RecurringExpenses nextBill =
            activeBills[0];

        lastReferencedBill =
        nextBill;

        lastReferencedSavingsGoal =
        null;

        int daysUntilNextBill =
            analyticsService
                .GetDaysUntilDue(
                    nextBill.DueDay);


        string nextBillDueText;


        if (daysUntilNextBill == 0)
        {
            nextBillDueText =
                "today";
        }

        else if (daysUntilNextBill == 1)
        {
            nextBillDueText =
                "tomorrow";
        }

        else
        {
            nextBillDueText =
                $"in {daysUntilNextBill} days";
        }



        // ======================================
        // MONTHLY BILL TOTAL
        // ======================================

        double monthlyBillTotal =
            activeBills.Sum(
                bill =>
                    bill.Amount);



        // ======================================
        // BILLS DUE WITHIN 7 DAYS
        // ======================================

        List<RecurringExpenses> billsDueSoon =
            activeBills
                .Where(
                    bill =>
                        analyticsService
                            .GetDaysUntilDue(
                                bill.DueDay) <= 7)
                .ToList();



        // ======================================
        // BUILD RESPONSE
        // ======================================

        string response =
            $"You have {activeBills.Count} active recurring bill";


        if (activeBills.Count != 1)
        {
            response +=
                "s";
        }


        response +=
            $" totaling {monthlyBillTotal:C} per month. ";


        response +=
            $"Your next bill is {nextBill.Name} for {nextBill.Amount:C}, " +
            $"due {nextBillDueText}. ";



        // ======================================
        // UPCOMING BILL WARNING
        // ======================================

        if (billsDueSoon.Count > 1)
        {
            double dueSoonTotal =
                billsDueSoon.Sum(
                    bill =>
                        bill.Amount);


            response +=
                $"You have {billsDueSoon.Count} bills due within the next 7 days, " +
                $"totaling {dueSoonTotal:C}.";
        }

        else if (billsDueSoon.Count == 1)
        {
            response +=
                $"That's your only bill due within the next 7 days.";
        }

        else
        {
            response +=
                "You don't have any bills due within the next 7 days.";
        }



        // ======================================
        // DISPLAY RESPONSE
        // ======================================

        ShowAssistantResponse(
            response);
    }

    // ==========================================
    // ADD POCKETAI MESSAGE
    // ==========================================

    private void AddPocketAIMessage(
        string message)
    {
        WelcomeAssistantMessage.IsVisible =
            false;


        // ======================================
        // POCKETAI ICON
        // ======================================

        Border iconBorder =
            new Border
            {
                WidthRequest =
                    36,

                HeightRequest =
                    36,

                BackgroundColor =
                    GetThemeColor(
                        "ThemePrimary",
                        "#7C3AED"),

                StrokeThickness =
                    0,

                VerticalOptions =
                    LayoutOptions.Start
            };


        iconBorder.StrokeShape =
            new RoundRectangle
            {
                CornerRadius =
                    new CornerRadius(
                        10)
            };


        Label iconLabel =
            new Label
            {
                Text =
                    "✦",

                FontSize =
                    17,

                TextColor =
                    GetThemeColor(
                        "TextOnPrimary",
                        "#FFFFFF"),

                HorizontalOptions =
                    LayoutOptions.Center,

                VerticalOptions =
                    LayoutOptions.Center
            };


        iconBorder.Content =
            iconLabel;


        // ======================================
        // POCKETAI MESSAGE BUBBLE
        // ======================================

        Border messageBubble =
            new Border
            {
                BackgroundColor =
                    GetThemeColor(
                        "SurfaceBackground",
                        "#F9FAFB"),

                Stroke =
                    GetThemeColor(
                        "BorderColor",
                        "#E5E7EB"),

                StrokeThickness =
                    1,

                Padding =
                    new Thickness(
                        16,
                        12),

                MaximumWidthRequest =
                    750
            };


        messageBubble.StrokeShape =
            new RoundRectangle
            {
                CornerRadius =
                    new CornerRadius(
                        14)
            };


        // ======================================
        // POCKETAI MESSAGE TEXT
        // ======================================

        Label messageLabel =
            new Label
            {
                Text =
                    message,

                FontSize =
                    14,

                TextColor =
                    GetThemeColor(
                        "TextPrimary",
                        "#111827"),

                LineHeight =
                    1.35
            };


        messageBubble.Content =
            messageLabel;


        // ======================================
        // ICON + MESSAGE GRID
        // ======================================

        Grid messageGrid =
            new Grid
            {
                ColumnSpacing =
                    12
            };


        messageGrid.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width =
                    GridLength.Auto
            });


        messageGrid.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width =
                    GridLength.Star
            });


        Grid.SetColumn(
            iconBorder,
            0);


        Grid.SetColumn(
            messageBubble,
            1);


        messageGrid.Children.Add(
            iconBorder);


        messageGrid.Children.Add(
            messageBubble);


        // ======================================
        // "POCKETAI" LABEL
        // ======================================

        Label aiLabel =
            new Label
            {
                Text =
                    "POCKETAI",

                FontSize =
                    9,

                FontAttributes =
                    FontAttributes.Bold,

                TextColor =
                    GetThemeColor(
                        "ThemePrimary",
                        "#7C3AED"),

                Margin =
                    new Thickness(
                        48,
                        0,
                        0,
                        0)
            };


        // ======================================
        // GROUP MESSAGE
        // ======================================

        VerticalStackLayout messageGroup =
            new VerticalStackLayout
            {
                Spacing =
                    5
            };


        messageGroup.Children.Add(
            aiLabel);


        messageGroup.Children.Add(
            messageGrid);


        ConversationContainer.Children.Add(
            messageGroup);


        // ======================================
        // REMEMBER THE CONTROLS
        // ======================================

        pocketAIIconBorders.Add(
            iconBorder);


        pocketAIIconLabels.Add(
            iconLabel);


        pocketAIMessageBubbles.Add(
            messageBubble);


        pocketAIMessageLabels.Add(
            messageLabel);


        pocketAINameLabels.Add(
            aiLabel);


        ScrollToLatestMessage(
            messageGroup);
    }

    // ==========================================
// REFRESH ENTIRE CONVERSATION THEME
// ==========================================

private void RefreshConversationTheme()
{
    // ======================================
    // CURRENT THEME COLORS
    // ======================================

    Color accentColor =
        GetThemeColor(
            "ThemePrimary",
            "#7C3AED");


    Color textOnAccent =
        GetThemeColor(
            "TextOnPrimary",
            "#FFFFFF");


    Color primaryText =
        GetThemeColor(
            "TextPrimary",
            "#111827");


    Color secondaryText =
        GetThemeColor(
            "TextSecondary",
            "#6B7280");


    Color surfaceBackground =
        GetThemeColor(
            "SurfaceBackground",
            "#F9FAFB");


    Color borderColor =
        GetThemeColor(
            "BorderColor",
            "#E5E7EB");


    // ======================================
    // USER MESSAGES
    // ======================================

    foreach (Border bubble
             in userMessageBubbles)
    {
        bubble.BackgroundColor =
            accentColor;
    }


    foreach (Label label
             in userMessageLabels)
    {
        label.TextColor =
            textOnAccent;
    }


    foreach (Label label
             in userNameLabels)
    {
        label.TextColor =
            secondaryText;
    }


    // ======================================
    // POCKETAI ICONS
    // ======================================

    foreach (Border icon
             in pocketAIIconBorders)
    {
        icon.BackgroundColor =
            accentColor;
    }


    foreach (Label iconLabel
             in pocketAIIconLabels)
    {
        iconLabel.TextColor =
            textOnAccent;
    }


    // ======================================
    // POCKETAI MESSAGE BUBBLES
    // ======================================

    foreach (Border bubble
             in pocketAIMessageBubbles)
    {
        bubble.BackgroundColor =
            surfaceBackground;


        bubble.Stroke =
            borderColor;
    }


    foreach (Label label
             in pocketAIMessageLabels)
    {
        label.TextColor =
            primaryText;
    }


    foreach (Label label
             in pocketAINameLabels)
    {
        label.TextColor =
            accentColor;
    }
}

    // ==========================================
    // SCROLL TO NEWEST MESSAGE
    // ==========================================

    private async void ScrollToLatestMessage(
        VisualElement message)
    {
        // Wait briefly for MAUI to finish
        // laying out the new message bubble.
        await Task.Delay(
            50);


        await ConversationScrollView
            .ScrollToAsync(
                message,
                ScrollToPosition.End,
                true);
    }

    // ==========================================
    // GET CURRENT THEME COLOR
    // ==========================================

    private static Color GetThemeColor(
        string resourceName,
        string fallbackColor)
    {
        if (Application.Current != null &&
            Application.Current.Resources[
                resourceName] is Color color)
        {
            return color;
        }


        return Color.FromArgb(
            fallbackColor);
    }
    // ==========================================
    // BUDGET STATUS HELPER
    // ==========================================

    private class BudgetStatusItem
    {
        public string Category
        {
            get;
            set;
        } = "";


        public double Limit
        {
            get;
            set;
        }


        public double Spent
        {
            get;
            set;
        }


        public double AmountOver
        {
            get;
            set;
        }
    }

    // ==========================================
    // CHECK QUESTION FOR MULTIPLE PHRASES
    // ==========================================

    private bool QuestionContainsAny(
        string question,
        params string[] phrases)
    {
        foreach (string phrase
                in phrases)
        {
            if (question.Contains(
                phrase))
            {
                return true;
            }
        }

        return false;
    }


    // ==========================================
    // FIND CATEGORY FROM COMMON WORDS
    // ==========================================

    private string? FindCategoryAlias(
        string question)
    {
        // Dining
        if (QuestionContainsAny(
            question,
            "eating out",
            "eat out",
            "restaurant",
            "restaurants",
            "fast food"))
        {
            return "Dining";
        }


        // Groceries
        if (QuestionContainsAny(
            question,
            "grocery",
            "groceries",
            "supermarket"))
        {
            return "Groceries";
        }


        // Utilities
        if (QuestionContainsAny(
            question,
            "utility",
            "utilities",
            "electric",
            "electricity",
            "water bill",
            "internet bill"))
        {
            return "Utilities";
        }


        // Transportation
        if (QuestionContainsAny(
            question,
            "gas",
            "fuel",
            "transportation",
            "uber",
            "lyft"))
        {
            return "Transportation";
        }


        // Entertainment
        if (QuestionContainsAny(
            question,
            "movie",
            "movies",
            "gaming",
            "games",
            "entertainment"))
        {
            return "Entertainment";
        }


        // Shopping
        if (QuestionContainsAny(
            question,
            "shopping",
            "clothes",
            "clothing"))
        {
            return "Shopping";
        }


        return null;
    }
    // ==========================================
    // FIND CATEGORY MENTIONED IN QUESTION
    // ==========================================

    private string? FindMentionedCategory(
        string question)
    {

        // First check common words and phrases.
        string? aliasCategory =
            FindCategoryAlias(
                question);


        if (aliasCategory != null)
        {
            return aliasCategory;
        }
        // ======================================
        // STANDARD POCKETAI CATEGORIES
        // ======================================

        // These categories should be understood
        // even if the user has not recorded any
        // transactions in them yet.
        List<string> standardCategories =
            new List<string>
            {
                "Housing",
                "Utilities",
                "Groceries",
                "Dining",
                "Transportation",
                "Shopping",
                "Entertainment",
                "Health",
                "Insurance",
                "Education",
                "Subscriptions",
                "Debt",
                "Other"
            };



        // ======================================
        // CATEGORIES ALREADY IN USER DATA
        // ======================================

        List<string> dataCategories =
            currentExpenses
                .Select(
                    expense =>
                        expense.Category)

                .Concat(
                    currentBudgetLimits
                        .Select(
                            budget =>
                                budget.Category))

                .Concat(
                    currentRecurringExpenses
                        .Select(
                            bill =>
                                bill.Category))

                .Where(
                    category =>
                        !string.IsNullOrWhiteSpace(
                            category))

                .ToList();



        // ======================================
        // COMBINE EVERYTHING
        // ======================================

        List<string> categories =
            standardCategories
                .Concat(
                    dataCategories)

                .Distinct(
                    StringComparer.OrdinalIgnoreCase)

                .ToList();



        // ======================================
        // LOOK FOR CATEGORY IN QUESTION
        // ======================================

        foreach (string category
                in categories)
        {
            if (question.Contains(
                category.ToLowerInvariant()))
            {
                return category;
            }
        }


        return null;
    }
}