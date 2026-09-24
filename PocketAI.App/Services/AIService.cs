using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PocketAI.App.Services;


public sealed class AIService
{
    // ==========================================
    // DEPENDENCIES
    // ==========================================

    private readonly DataBaseManager
        dataBaseManager;


    private readonly FinancialSnapshotProvider
        financialSnapshotProvider;

    private PocketAIConversationState
    conversationState = new PocketAIConversationState();



    // ==========================================
    // CONSTRUCTOR
    // ==========================================

    public AIService(
        DataBaseManager dataBaseManager,
        FinancialSnapshotProvider financialSnapshotProvider)
    {
        this.dataBaseManager =
            dataBaseManager;


        this.financialSnapshotProvider =
            financialSnapshotProvider;
    }



    // ==========================================
    // ANALYZE CURRENT USER FINANCES
    // ==========================================

    public async Task<PocketAIAnalysisResult>
    AnalyzeCurrentFinancesAsync(
        string? question = null)
    {
        // ======================================
        // TRUSTED C# FINANCIAL SNAPSHOT
        // ======================================

        FinancialSnapshot snapshot =
            financialSnapshotProvider
                .GetSnapshot();


        // ======================================
        // LOAD SUPPORTING DATA
        // ======================================

        List<Expense> expenses =
            dataBaseManager
                .GetAllExpenses();


        List<BudgetLimit> budgetLimits =
            dataBaseManager
                .GetBudgetLimits();


        List<SavingsGoal> savingsGoals =
            dataBaseManager
                .GetSavingsGoals();


        List<RecurringExpenses> bills =
            dataBaseManager
                .GetRecuringExpenses();


        HashSet<int> paidBillIds =
            dataBaseManager
                .GetPaidRecurringExpenseIdsForMonth(
                    DateTime.Today);


        DateTime today =
            DateTime.Today;



        // ======================================
        // CURRENT MONTH TRANSACTIONS
        // ======================================

        List<Expense> currentMonthExpenses =
            expenses
                .Where(
                    expense =>
                        expense.Date.Year ==
                        today.Year
                        &&
                        expense.Date.Month ==
                        today.Month)
                .ToList();



        // ======================================
        // BUDGET LOOKUP
        // ======================================

        Dictionary<string, double>
            budgetLookup =
                budgetLimits
                    .GroupBy(
                        budget =>
                            budget.Category,
                        StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        group =>
                            group.Key,
                        group =>
                            group.Last()
                                .LimitAmount,
                        StringComparer.OrdinalIgnoreCase);



        // ======================================
        // CATEGORY SPENDING
        // ======================================

        var categorySpending =
            currentMonthExpenses
                .GroupBy(
                    expense =>
                        string.IsNullOrWhiteSpace(
                            expense.Category)

                            ? "Other"

                            : expense.Category)
                .Select(
                    group =>
                    {
                        double? budgetLimit =
                            budgetLookup
                                .TryGetValue(
                                    group.Key,
                                    out double limit)

                                ? limit

                                : null;


                        return new
                        {
                            category =
                                group.Key,

                            amount =
                                Math.Round(
                                    group.Sum(
                                        expense =>
                                            expense.Amount),
                                    2),

                            budgetLimit
                        };
                    })
                .ToList();



        // ======================================
        // SAVINGS GOALS
        // ======================================

        var savingsGoalData =
            savingsGoals
                .Select(
                    goal =>
                        new
                        {
                            name =
                                goal.Name,

                            currentAmount =
                                goal.CurrentAmount,

                            targetAmount =
                                goal.TargetAmount,

                            priorityRank =
                                goal.PriorityRank,

                            isCompleted =
                                goal.IsCompleted
                        })
                .ToList();



        // ======================================
        // RECURRING BILLS
        // ======================================

        var billData =
            bills
                .Select(
                    bill =>
                        new
                        {
                            name =
                                bill.Name,

                            amount =
                                bill.Amount,

                            dueDay =
                                bill.DueDay,

                            isActive =
                                bill.IsActive,

                            isPaidThisMonth =
                                paidBillIds
                                    .Contains(
                                        bill.Id)
                        })
                .ToList();



        // ======================================
        // BUILD PYTHON INPUT
        // ======================================

        var payload =
            new
            {
                question,

                conversationState =
                new
                {
                    lastIntent =
                        conversationState.LastIntent,

                    lastPurchaseAmount =
                        conversationState.LastPurchaseAmount,

                    lastPurchaseDescription =
                        conversationState.LastPurchaseDescription,

                    lastCategory =
                        conversationState.LastCategory,

                    lastBillName =
                        conversationState.LastBillName,

                    lastSavingsGoalName =
                        conversationState.LastSavingsGoalName,

                    previousQuestion =
                        conversationState.PreviousQuestion,

                    previousAnswer =
                        conversationState.PreviousAnswer
                },

                checkingBalance =
                    snapshot.CheckingBalance,

                savingsBalance =
                    snapshot.ProtectedSavingsBalance,

                expectedMonthlyIncome =
                    snapshot.ExpectedMonthlyIncome,

                currentMonthSpent =
                    snapshot.CurrentMonthSpent,

                upcomingBills =
                    snapshot.UpcomingBills,

                requiredSavingsThisMonth =
                    snapshot.RequiredSavingsThisMonth,

                acceptedExtraSavings =
                    snapshot.AcceptedExtraSavings,

                safeToSpendTotal =
                    snapshot.SafeToSpendTotal,

                safeToSpendToday =
                    snapshot.SafeToSpendToday,

                safeToSpendThisWeek =
                    snapshot.SafeToSpendThisWeek,

                obligationShortfall =
                    snapshot.ObligationShortfall,

                projectedAdditionalSpending =
                    snapshot.ProjectedAdditionalSpending,

                projectedMonthEndMoney =
                    snapshot
                        .ProjectedMonthEndSpendableCash,

                overBudgetCount =
                    snapshot.OverBudgetCount,

                budgetCount =
                    snapshot.BudgetCount,

                currentMonthTransactionCount =
                    snapshot
                        .CurrentMonthTransactionCount,

                activeRecurringBillCount =
                    snapshot
                        .ActiveRecurringBillCount,

                activeSavingsGoalCount =
                    snapshot
                        .ActiveSavingsGoalCount,

                dataConfidence =
                    snapshot.DataConfidence,

                financialHealthScore =
                    snapshot.FinancialHealthScore,

                categorySpending,

                savingsGoals =
                    savingsGoalData,

                bills =
                    billData
            };


        string inputJson =
            JsonSerializer.Serialize(
                payload);

        
        // ======================================
        // SEND TO PYTHON
        // ======================================

        string outputJson =
            await RunPythonAsync(
                inputJson);


        // ======================================
        // CHECK FOR PYTHON ERROR JSON
        // ======================================

        using JsonDocument document =
            JsonDocument.Parse(
                outputJson);


        JsonElement root =
            document.RootElement;


        if (root.TryGetProperty(
                "success",
                out JsonElement successElement)
            &&
            successElement.ValueKind ==
            JsonValueKind.False)
        {
            string error =
                root.TryGetProperty(
                    "error",
                    out JsonElement errorElement)

                    ? errorElement
                        .GetString()
                        ??
                        "Unknown Python error."

                    : "Unknown Python error.";


            throw new InvalidOperationException(
                error);
        }


        // ======================================
        // CONVERT PYTHON RESPONSE TO C#
        // ======================================

        PocketAIAnalysisResult? result =
            JsonSerializer.Deserialize<
                PocketAIAnalysisResult>(
                    outputJson);


        if (result == null)
        {
            throw new InvalidOperationException(
                "PocketAI AI returned an invalid response.");
        }

        if (!string.IsNullOrWhiteSpace(
                question)
            &&
            result.ConversationState != null)
        {
            conversationState =
                result.ConversationState;
        }
        Debug.WriteLine(
            $"PocketAI Python Engine {result.EngineVersion} responded successfully.");


        return result;
    }



    // ==========================================
    // RUN PYTHON
    // ==========================================

    private async Task<string>
        RunPythonAsync(
            string inputJson)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException(
                "The current PocketAI Python bridge is being developed for Windows first.");
        }


        string scriptPath =
            Path.Combine(
                AppContext.BaseDirectory,
                "PocketAI.AI",
                "run_ai.py");


        if (!File.Exists(
                scriptPath))
        {
            throw new FileNotFoundException(
                "PocketAI could not find run_ai.py.",
                scriptPath);
        }


        // ======================================
        // TRY WINDOWS PYTHON LAUNCHER FIRST
        // ======================================

        try
        {
            return await RunPythonProcessAsync(
                "py",
                scriptPath,
                inputJson);
        }

        catch (Win32Exception)
        {
            // py.exe is not installed.
            // Try python.exe instead.
        }


        return await RunPythonProcessAsync(
            "python",
            scriptPath,
            inputJson);
    }



    // ==========================================
    // START PYTHON PROCESS
    // ==========================================

    private static async Task<string>
        RunPythonProcessAsync(
            string executable,
            string scriptPath,
            string inputJson)
    {
        string workingDirectory =
            Path.GetDirectoryName(
                scriptPath)
            ??
            AppContext.BaseDirectory;


        ProcessStartInfo startInfo =
            new ProcessStartInfo
            {
                FileName =
                    executable,

                Arguments =
                    $"\"{scriptPath}\"",

                WorkingDirectory =
                    workingDirectory,

                RedirectStandardInput =
                    true,

                RedirectStandardOutput =
                    true,

                RedirectStandardError =
                    true,

                UseShellExecute =
                    false,

                CreateNoWindow =
                    true
            };


        using Process process =
            new Process
            {
                StartInfo =
                    startInfo
            };


        process.Start();


        Task<string> outputTask =
            process.StandardOutput
                .ReadToEndAsync();


        Task<string> errorTask =
            process.StandardError
                .ReadToEndAsync();


        await process.StandardInput
            .WriteAsync(
                inputJson);


        process.StandardInput.Close();


        await process.WaitForExitAsync();


        string output =
            await outputTask;


        string error =
            await errorTask;


        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(
                    error)

                    ? "PocketAI's Python engine stopped unexpectedly."

                    : error.Trim());
        }


        if (string.IsNullOrWhiteSpace(
                output))
        {
            throw new InvalidOperationException(
                "PocketAI's Python engine returned no response.");
        }


        return output.Trim();
    }
}



// ==========================================
// PYTHON ANALYSIS RESPONSE
// ==========================================

public sealed class PocketAIAnalysisResult
{
    [JsonPropertyName(
        "engine_version")]
    
    
    public string EngineVersion
    {
        get;
        set;
    } = "";


    [JsonPropertyName(
        "overall_status")]
    public string OverallStatus
    {
        get;
        set;
    } = "";


    [JsonPropertyName(
        "summary")]
    public string Summary
    {
        get;
        set;
    } = "";


    [JsonPropertyName(
        "confidence")]
    public string Confidence
    {
        get;
        set;
    } = "";


    [JsonPropertyName(
        "insights")]
    public List<PocketAIInsight> Insights
    {
        get;
        set;
    } =
        new List<PocketAIInsight>();


    [JsonPropertyName(
        "recommended_actions")]
    public List<PocketAIRecommendedAction>
        RecommendedActions
    {
        get;
        set;
    } =
        new List<PocketAIRecommendedAction>();

        [JsonPropertyName(
        "intent")]
    public string Intent
    {
        get;
        set;
    } = "";


    [JsonPropertyName(
        "intent_confidence")]
    public double IntentConfidence
    {
        get;
        set;
    }


    [JsonPropertyName(
        "answer")]
    public string Answer
    {
        get;
        set;
    } = "";

    [JsonPropertyName(
    "conversation_state")]
    public PocketAIConversationState
        ConversationState
    {
        get;
        set;
    } =
        new PocketAIConversationState();
}



// ==========================================
// PYTHON INSIGHT
// ==========================================

public sealed class PocketAIInsight
{
    [JsonPropertyName(
        "category")]
    public string Category
    {
        get;
        set;
    } = "";


    [JsonPropertyName(
        "severity")]
    public string Severity
    {
        get;
        set;
    } = "";


    [JsonPropertyName(
        "title")]
    public string Title
    {
        get;
        set;
    } = "";


    [JsonPropertyName(
        "message")]
    public string Message
    {
        get;
        set;
    } = "";


    [JsonPropertyName(
        "reason")]
    public string Reason
    {
        get;
        set;
    } = "";
}



// ==========================================
// PYTHON RECOMMENDED ACTION
// ==========================================

public sealed class PocketAIRecommendedAction
{
    [JsonPropertyName(
        "priority")]
    public int Priority
    {
        get;
        set;
    }


    [JsonPropertyName(
        "action")]
    public string Action
    {
        get;
        set;
    } = "";


    [JsonPropertyName(
        "reason")]
    public string Reason
    {
        get;
        set;
    } = "";

}

// ==========================================
// CONVERSATION STATE
// ==========================================

public sealed class PocketAIConversationState
{
    [JsonPropertyName(
        "last_intent")]
    public string LastIntent
    {
        get;
        set;
    } = "";


    [JsonPropertyName(
        "last_purchase_amount")]
    public double? LastPurchaseAmount
    {
        get;
        set;
    }


    [JsonPropertyName(
        "last_purchase_description")]
    public string LastPurchaseDescription
    {
        get;
        set;
    } = "";


    [JsonPropertyName(
        "last_category")]
    public string LastCategory
    {
        get;
        set;
    } = "";


    [JsonPropertyName(
        "last_bill_name")]
    public string LastBillName
    {
        get;
        set;
    } = "";


    [JsonPropertyName(
        "last_savings_goal_name")]
    public string LastSavingsGoalName
    {
        get;
        set;
    } = "";


    [JsonPropertyName(
        "previous_question")]
    public string PreviousQuestion
    {
        get;
        set;
    } = "";


    [JsonPropertyName(
        "previous_answer")]
    public string PreviousAnswer
    {
        get;
        set;
    } = "";
}