using System.Collections.Generic;


// ==========================================
// COMPLETE SAVINGS ALLOCATION PLAN
// ==========================================

public class SavingsAllocationPlan
{
    // Amount the user can afford to save.
    //
    // Example:
    // $500
    public double AvailableToSave
    {
        get;
        set;
    }


    // Amount PocketAI actually recommends
    // distributing across savings goals.
    public double TotalAllocated
    {
        get;
        set;
    }


    // Money that was not needed by any
    // remaining savings goal.
    //
    // Example:
    //
    // User can save $500
    // Goals only need $400
    //
    // UnallocatedAmount = $100
    public double UnallocatedAmount
    {
        get;
        set;
    }


    // Individual recommendation for
    // every active savings goal.
    public List<SavingsAllocationItem> Allocations
    {
        get;
        set;
    }


    public SavingsAllocationPlan()
    {
        Allocations =
            new List<SavingsAllocationItem>();
    }
}



// ==========================================
// ONE GOAL'S RECOMMENDED ALLOCATION
// ==========================================

public class SavingsAllocationItem
{
    // Database ID of the savings goal.
    public int GoalId
    {
        get;
        set;
    }


    // Goal name.
    public string GoalName
    {
        get;
        set;
    }


    // User's financial priority.
    public int PriorityRank
    {
        get;
        set;
    }


    // Whether this is an essential goal.
    //
    // We are storing this now so the
    // smarter algorithm can use it later.
    public bool IsEssential
    {
        get;
        set;
    }


    // Base mathematical priority weight.
    //
    // Example with three goals:
    //
    // Priority 1 = 3
    // Priority 2 = 2
    // Priority 3 = 1
    public double PriorityWeight
    {
        get;
        set;
    }


    // Amount still needed BEFORE
    // this recommendation.
    public double RemainingBeforeContribution
    {
        get;
        set;
    }


    // Dollar amount PocketAI recommends.
    //
    // Example:
    // $250
    public double RecommendedAmount
    {
        get;
        set;
    }


    // Percentage of the user's available
    // savings amount.
    //
    // Example:
    // 50%
    public double RecommendedPercentage
    {
        get;
        set;
    }


    // Amount still needed AFTER applying
    // the recommendation.
    public double RemainingAfterContribution
    {
        get;
        set;
    }


    // True when this recommendation would
    // completely finish the goal.
    public bool IsFullyFundedAfterRecommendation =>
        RemainingAfterContribution <= 0.009;


    public SavingsAllocationItem()
    {
        GoalName =
            "";
    }
}