using System;
using System.Collections.Generic;
using System.Linq;


// ==========================================
// POCKETAI SAVINGS ALLOCATION SERVICE
// ==========================================
//
// This service contains the trusted C# math
// behind PocketAI's Priority Savings system.
//
// Responsibilities:
//
// - Calculate how much money is realistically
//   available for savings.
//
// - Keep a financial buffer instead of
//   recommending every leftover dollar.
//
// - Make the buffer more conservative when
//   the user's finances are under pressure.
//
// - Divide available savings between goals.
//
// - Respect Priority levels.
//
// - Allow multiple goals to share the same
//   Priority level.
//
// - Give Essential goals additional weight.
//
// - Never recommend more money than a goal
//   actually needs.
//
// - Redistribute unused money when one goal
//   becomes fully funded.
//
// AI can later EXPLAIN these calculations,
// but C# remains the source of truth.
// ==========================================

public class SavingsAllocationService
{
    // ==========================================
    // ESSENTIAL GOAL MULTIPLIER
    // ==========================================
    //
    // Essential goals receive a 50% increase
    // to their normal Priority weight.
    //
    // Example:
    //
    // Priority weight = 2
    //
    // Optional:
    // 2
    //
    // Essential:
    // 2 × 1.5 = 3
    //
    // Keeping this value in one place makes
    // it easy to tune PocketAI later.
    // ==========================================

    private const double EssentialWeightMultiplier =
        1.5;



    // ==========================================
    // CALCULATE DYNAMIC BUFFER PERCENTAGE
    // ==========================================
    //
    // PocketAI should not use the same exact
    // savings buffer for every financial
    // situation.
    //
    // The buffer becomes more conservative
    // when:
    //
    // - projected month-end money is low
    // - recurring bills consume lots of income
    // - the user is over budget
    //
    // Returned value is decimal form.
    //
    // Example:
    //
    // 0.30 = 30%
    // ==========================================

    public double CalculateRecommendedBufferPercentage(
        double projectedEndOfMonthMoney,
        double monthlyIncome,
        double monthlyRecurringExpenses,
        int overBudgetCount)
    {
        // ======================================
        // NO POSITIVE MONTH-END SURPLUS
        // ======================================
        //
        // If PocketAI expects the user to have
        // no money left, none should be allocated
        // toward new savings recommendations.
        // ======================================

        if (projectedEndOfMonthMoney <= 0)
        {
            return 1.0;
        }



        double bufferPercentage;



        // ======================================
        // NO RELIABLE MONTHLY INCOME
        // ======================================

        if (monthlyIncome <= 0)
        {
            // Be conservative when PocketAI
            // cannot compare the surplus against
            // reliable monthly income.
            bufferPercentage =
                0.60;
        }



        // ======================================
        // MONTHLY INCOME IS AVAILABLE
        // ======================================

        else
        {
            // ==================================
            // SURPLUS RATIO
            // ==================================
            //
            // Example:
            //
            // Monthly income = $2,200
            //
            // Projected leftover = $680
            //
            // $680 / $2,200 ≈ 31%
            // ==================================

            double surplusRatio =
                projectedEndOfMonthMoney /
                monthlyIncome;



            // ==================================
            // VERY TIGHT MONTH
            // ==================================
            //
            // User is projected to have only
            // 10% or less of monthly income left.
            // ==================================

            if (surplusRatio <= 0.10)
            {
                bufferPercentage =
                    0.60;
            }



            // ==================================
            // TIGHT MONTH
            // ==================================

            else if (surplusRatio <= 0.20)
            {
                bufferPercentage =
                    0.45;
            }



            // ==================================
            // HEALTHIER MONTH
            // ==================================

            else if (surplusRatio <= 0.35)
            {
                bufferPercentage =
                    0.30;
            }



            // ==================================
            // STRONG MONTH-END SURPLUS
            // ==================================

            else
            {
                bufferPercentage =
                    0.25;
            }



            // ==================================
            // RECURRING BILL PRESSURE
            // ==================================
            //
            // The more monthly income already
            // committed to recurring bills,
            // the more money PocketAI should
            // protect as a buffer.
            // ==================================

            double recurringRatio =
                monthlyRecurringExpenses /
                monthlyIncome;



            // Half or more of income is already
            // committed to recurring expenses.
            if (recurringRatio >= 0.50)
            {
                bufferPercentage +=
                    0.10;
            }



            // A significant amount of income is
            // committed to recurring expenses.
            else if (recurringRatio >= 0.35)
            {
                bufferPercentage +=
                    0.05;
            }
        }



        // ======================================
        // OVER-BUDGET PROTECTION
        // ======================================
        //
        // If the user is exceeding budgets,
        // PocketAI becomes more conservative.
        // ======================================

        if (overBudgetCount >= 2)
        {
            bufferPercentage +=
                0.10;
        }


        else if (overBudgetCount == 1)
        {
            bufferPercentage +=
                0.05;
        }



        // ======================================
        // SAFETY LIMITS
        // ======================================
        //
        // PocketAI will currently:
        //
        // - protect at least 25%
        // - protect at most 75%
        //
        // of a positive projected surplus.
        //
        // We can make these limits more
        // personalized later.
        // ======================================

        return Math.Clamp(
            bufferPercentage,
            0.25,
            0.75);
    }



    // ==========================================
    // CALCULATE RECOMMENDED CASH BUFFER
    // ==========================================

    public double CalculateRecommendedBuffer(
        double projectedEndOfMonthMoney,
        double monthlyIncome,
        double monthlyRecurringExpenses,
        int overBudgetCount)
    {
        if (projectedEndOfMonthMoney <= 0)
        {
            return 0;
        }



        double bufferPercentage =
            CalculateRecommendedBufferPercentage(
                projectedEndOfMonthMoney,
                monthlyIncome,
                monthlyRecurringExpenses,
                overBudgetCount);



        double recommendedBuffer =
            projectedEndOfMonthMoney *
            bufferPercentage;



        return Math.Round(
            recommendedBuffer,
            2);
    }



    // ==========================================
    // CALCULATE AVAILABLE FOR SAVINGS
    // ==========================================
    //
    // Example:
    //
    // Projected end-of-month money:
    // $680
    //
    // Recommended buffer:
    // $204
    //
    // Available for Savings:
    // $476
    // ==========================================

    public double CalculateAvailableForSavings(
        double projectedEndOfMonthMoney,
        double monthlyIncome,
        double monthlyRecurringExpenses,
        int overBudgetCount)
    {
        if (projectedEndOfMonthMoney <= 0)
        {
            return 0;
        }



        double recommendedBuffer =
            CalculateRecommendedBuffer(
                projectedEndOfMonthMoney,
                monthlyIncome,
                monthlyRecurringExpenses,
                overBudgetCount);



        double availableForSavings =
            projectedEndOfMonthMoney -
            recommendedBuffer;



        return Math.Round(
            Math.Max(
                availableForSavings,
                0),
            2);
    }



    // ==========================================
    // CALCULATE RECOMMENDED GOAL ALLOCATION
    // ==========================================
    //
    // This method takes the money PocketAI
    // believes is available for savings and
    // divides it among the user's active goals.
    //
    // Example:
    //
    // Available to save:
    // $500
    //
    // Emergency Fund
    // Priority 1
    // Essential
    //
    // Vacation
    // Priority 1
    // Optional
    //
    // Because both goals share Priority 1,
    // they begin with equal Priority weight.
    //
    // Essential then gives Emergency Fund
    // extra weight.
    //
    // Result:
    //
    // Emergency Fund:
    // 60% = $300
    //
    // Vacation:
    // 40% = $200
    // ==========================================

    public SavingsAllocationPlan
        CalculateRecommendedAllocation(
            List<SavingsGoal> savingsGoals,
            double availableToSave)
    {
        // ======================================
        // CREATE EMPTY PLAN
        // ======================================

        SavingsAllocationPlan plan =
            new SavingsAllocationPlan
            {
                AvailableToSave =
                    Math.Max(
                        availableToSave,
                        0)
            };



        // ======================================
        // NOTHING AVAILABLE TO SAVE
        // ======================================

        if (availableToSave <= 0)
        {
            plan.TotalAllocated =
                0;


            plan.UnallocatedAmount =
                0;


            return plan;
        }



        // ======================================
        // FIND ACTIVE GOALS
        // ======================================
        //
        // Completed goals should NOT receive
        // additional recommendations.
        // ======================================

        List<SavingsGoal> activeGoals =
            savingsGoals
                .Where(
                    goal =>
                        goal.TargetAmount >
                        goal.CurrentAmount)
                .OrderBy(
                    goal =>
                        goal.PriorityRank <= 0

                            ? int.MaxValue

                            : goal.PriorityRank)
                .ThenBy(
                    goal =>
                        goal.Id)
                .ToList();



        // ======================================
        // NO GOALS NEED MONEY
        // ======================================

        if (activeGoals.Count == 0)
        {
            plan.TotalAllocated =
                0;


            plan.UnallocatedAmount =
                availableToSave;


            return plan;
        }



        // ======================================
        // FIND DISTINCT PRIORITY TIERS
        // ======================================
        //
        // IMPORTANT:
        //
        // Priority is a TIER.
        //
        // Multiple goals may have the same
        // Priority.
        //
        // Example:
        //
        // Emergency Fund → Priority 1
        // Tuition        → Priority 1
        // Vacation       → Priority 2
        //
        // Distinct tiers:
        //
        // 1, 2
        //
        // Not:
        //
        // 1, 2, 3
        // ======================================

        List<int> priorityLevels =
            activeGoals
                .Where(
                    goal =>
                        goal.PriorityRank > 0)
                .Select(
                    goal =>
                        goal.PriorityRank)
                .Distinct()
                .OrderBy(
                    rank =>
                        rank)
                .ToList();



        // ======================================
        // CHECK FOR UNRANKED GOALS
        // ======================================
        //
        // PriorityRank <= 0 is treated as
        // a final lowest-priority tier.
        //
        // This is primarily a fallback for
        // old or incomplete data.
        // ======================================

        bool hasUnrankedGoals =
            activeGoals.Any(
                goal =>
                    goal.PriorityRank <= 0);



        int totalPriorityTiers =
            priorityLevels.Count
            +
            (
                hasUnrankedGoals
                    ? 1
                    : 0
            );



        // Safety fallback.
        if (totalPriorityTiers <= 0)
        {
            totalPriorityTiers =
                1;
        }



        // ======================================
        // CREATE PRIORITY WEIGHTS
        // ======================================

        Dictionary<SavingsGoal, double>
            priorityWeights =
                new Dictionary<
                    SavingsGoal,
                    double>();



        foreach (SavingsGoal goal
                 in activeGoals)
        {
            // ==================================
            // BASE PRIORITY WEIGHT
            // ==================================

            double baseWeight;



            // ==================================
            // UNRANKED GOAL
            // ==================================

            if (goal.PriorityRank <= 0)
            {
                // Lowest possible tier.
                baseWeight =
                    1;
            }



            // ==================================
            // RANKED GOAL
            // ==================================

            else
            {
                int tierIndex =
                    priorityLevels.IndexOf(
                        goal.PriorityRank);



                // Example with three tiers:
                //
                // Priority 1 → 3
                // Priority 2 → 2
                // Priority 3 → 1
                //
                // Two goals sharing Priority 1
                // both receive weight 3.
                baseWeight =
                    totalPriorityTiers -
                    tierIndex;
            }



            // ==================================
            // ESSENTIAL PROTECTION
            // ==================================
            //
            // Essential goals receive a 50%
            // increase in weight.
            // ==================================

            double finalWeight =
                goal.IsEssential

                    ? baseWeight *
                      EssentialWeightMultiplier

                    : baseWeight;



            priorityWeights[
                goal] =
                finalWeight;
        }



        // ======================================
        // TRACK ALLOCATED MONEY
        // ======================================

        Dictionary<SavingsGoal, double>
            allocatedAmounts =
                new Dictionary<
                    SavingsGoal,
                    double>();



        foreach (SavingsGoal goal
                 in activeGoals)
        {
            allocatedAmounts[
                goal] =
                0;
        }



        double moneyRemaining =
            availableToSave;



        List<SavingsGoal>
            goalsStillNeedingMoney =
                new List<SavingsGoal>(
                    activeGoals);



        // ======================================
        // DISTRIBUTE AVAILABLE MONEY
        // ======================================
        //
        // This may require multiple rounds.
        //
        // Example:
        //
        // Emergency Fund gets a calculated
        // recommendation of $300.
        //
        // But it only needs $50 to finish.
        //
        // PocketAI gives it $50.
        //
        // The unused $250 is then redistributed
        // among the other incomplete goals.
        // ======================================

        while (
            moneyRemaining > 0.0001
            &&
            goalsStillNeedingMoney.Count > 0)
        {
            // ==================================
            // TOTAL WEIGHT THIS ROUND
            // ==================================

            double totalWeight =
                goalsStillNeedingMoney
                    .Sum(
                        goal =>
                            priorityWeights[
                                goal]);



            if (totalWeight <= 0)
            {
                break;
            }



            // ==================================
            // MONEY AT START OF ROUND
            // ==================================
            //
            // Every goal's percentage must be
            // based on the same amount.
            // ==================================

            double roundMoney =
                moneyRemaining;



            double allocatedThisRound =
                0;



            // ==================================
            // ALLOCATE TO EACH ACTIVE GOAL
            // ==================================

            foreach (
                SavingsGoal goal
                in goalsStillNeedingMoney)
            {
                double weight =
                    priorityWeights[
                        goal];



                // Goal's percentage of this
                // allocation round.
                double percentageOfRound =
                    weight /
                    totalWeight;



                double recommendedShare =
                    roundMoney *
                    percentageOfRound;



                // ==================================
                // ORIGINAL REMAINING BALANCE
                // ==================================

                double originalRemaining =
                    Math.Max(
                        goal.TargetAmount -
                        goal.CurrentAmount,
                        0);



                // ==================================
                // ALREADY ALLOCATED THIS PLAN
                // ==================================

                double alreadyAllocated =
                    allocatedAmounts[
                        goal];



                // ==================================
                // WHAT GOAL STILL NEEDS
                // ==================================

                double goalStillNeeds =
                    Math.Max(
                        originalRemaining -
                        alreadyAllocated,
                        0);



                // ==================================
                // NEVER OVERFUND
                // ==================================

                double amountForGoal =
                    Math.Min(
                        recommendedShare,
                        goalStillNeeds);



                allocatedAmounts[
                    goal]
                    +=
                    amountForGoal;



                allocatedThisRound
                    +=
                    amountForGoal;
            }



            // ==================================
            // REMOVE ALLOCATED MONEY
            // ==================================

            moneyRemaining
                -=
                allocatedThisRound;



            // ==================================
            // FLOATING-POINT SAFETY
            // ======================================
            //
            // Prevent a potential endless loop
            // when the remaining amount becomes
            // extremely tiny.
            // ==================================

            if (allocatedThisRound <= 0.0001)
            {
                break;
            }



            // ==================================
            // REMOVE FULLY FUNDED GOALS
            // ==================================

            goalsStillNeedingMoney =
                goalsStillNeedingMoney
                    .Where(
                        goal =>
                        {
                            double remaining =
                                Math.Max(
                                    goal.TargetAmount
                                    -
                                    goal.CurrentAmount
                                    -
                                    allocatedAmounts[
                                        goal],
                                    0);



                            return
                                remaining >
                                0.0001;
                        })
                    .ToList();
        }



        // ======================================
        // BUILD DISPLAY RESULTS
        // ======================================

        foreach (SavingsGoal goal
                 in activeGoals)
        {
            double amount =
                Math.Round(
                    allocatedAmounts[
                        goal],
                    2);



            double remainingBefore =
                Math.Max(
                    goal.TargetAmount -
                    goal.CurrentAmount,
                    0);



            double remainingAfter =
                Math.Max(
                    remainingBefore -
                    amount,
                    0);



            double percentage =
                availableToSave > 0

                    ? amount /
                      availableToSave *
                      100

                    : 0;



            SavingsAllocationItem item =
                new SavingsAllocationItem
                {
                    GoalId =
                        goal.Id,


                    GoalName =
                        goal.Name,


                    PriorityRank =
                        goal.PriorityRank,


                    IsEssential =
                        goal.IsEssential,


                    PriorityWeight =
                        priorityWeights[
                            goal],


                    RemainingBeforeContribution =
                        Math.Round(
                            remainingBefore,
                            2),


                    RecommendedAmount =
                        amount,


                    RecommendedPercentage =
                        Math.Round(
                            percentage,
                            2),


                    RemainingAfterContribution =
                        Math.Round(
                            remainingAfter,
                            2)
                };



            plan.Allocations.Add(
                item);
        }



        // ======================================
        // TOTAL ACTUALLY ALLOCATED
        // ======================================

        plan.TotalAllocated =
            Math.Round(
                plan.Allocations.Sum(
                    item =>
                        item.RecommendedAmount),
                2);



        // ======================================
        // UNALLOCATED MONEY
        // ======================================
        //
        // This occurs when the user can afford
        // to save more than all active goals
        // actually need.
        //
        // Example:
        //
        // Available = $500
        //
        // All remaining goals only need $350
        //
        // TotalAllocated = $350
        //
        // UnallocatedAmount = $150
        // ======================================

        plan.UnallocatedAmount =
            Math.Max(
                Math.Round(
                    availableToSave -
                    plan.TotalAllocated,
                    2),
                0);



        return plan;
    }
}