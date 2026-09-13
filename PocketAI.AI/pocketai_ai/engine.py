from __future__ import annotations

from .models import (
    AnalysisResult,
    FinancialContext,
    Insight,
    RecommendedAction,
)


class PocketAIEngine:
    """
    PocketAI's financial reasoning engine.

    IMPORTANT:

    This engine does NOT calculate the user's
    bank balances or Safe to Spend.

    The C# financial engine remains the
    financial source of truth.

    Python interprets those trusted numbers
    and decides what deserves the user's
    attention.
    """

    VERSION = "0.1.0"


    # ==========================================
    # MAIN ANALYSIS
    # ==========================================

    def analyze(
        self,
        context: FinancialContext,
    ) -> AnalysisResult:

        insights: list[Insight] = []

        actions: list[
            RecommendedAction
        ] = []


        # ======================================
        # MONTHLY SHORTFALL
        # ======================================

        if context.obligation_shortfall > 0:

            insights.append(
                Insight(
                    category="cash_flow",
                    severity="critical",
                    title="Monthly plan shortfall",
                    message=(
                        "Your current monthly plan "
                        f"is short by "
                        f"${context.obligation_shortfall:,.2f}."
                    ),
                    reason=(
                        "Your planned spending, bills, "
                        "and savings commitments exceed "
                        "the room currently available "
                        "in the monthly plan."
                    ),
                )
            )


            actions.append(
                RecommendedAction(
                    priority=1,
                    action=(
                        "Reduce discretionary spending "
                        "until the monthly shortfall is resolved."
                    ),
                    reason=(
                        "Required obligations should be "
                        "protected before optional spending."
                    ),
                )
            )


        # ======================================
        # SAFE TO SPEND
        # ======================================

        elif context.safe_to_spend_total <= 0:

            insights.append(
                Insight(
                    category="spending",
                    severity="warning",
                    title="No discretionary room",
                    message=(
                        "There is currently no Safe to "
                        "Spend remaining in this month's plan."
                    ),
                    reason=(
                        "Income has already been allocated "
                        "to spending, bills, or savings."
                    ),
                )
            )


            actions.append(
                RecommendedAction(
                    priority=1,
                    action=(
                        "Avoid additional discretionary "
                        "purchases for now."
                    ),
                    reason=(
                        "There is no remaining room in "
                        "the current monthly plan."
                    ),
                )
            )


        else:

            insights.append(
                Insight(
                    category="spending",
                    severity="success",
                    title="Safe to Spend available",
                    message=(
                        f"You currently have "
                        f"${context.safe_to_spend_total:,.2f} "
                        "Safe to Spend this month."
                    ),
                    reason=(
                        "PocketAI is using the amount "
                        "remaining after current spending, "
                        "bills, and savings commitments."
                    ),
                )
            )


        # ======================================
        # SPENDING PRESSURE
        # ======================================

        if context.expected_monthly_income > 0:

            spending_ratio = (
                context.current_month_spent
                /
                context.expected_monthly_income
            )


            if spending_ratio >= 0.80:

                insights.append(
                    Insight(
                        category="spending",
                        severity="warning",
                        title="High monthly spending",
                        message=(
                            f"You have already recorded "
                            f"${context.current_month_spent:,.2f} "
                            "of spending this month."
                        ),
                        reason=(
                            "Recorded spending is at least "
                            "80% of expected monthly income."
                        ),
                    )
                )


                actions.append(
                    RecommendedAction(
                        priority=2,
                        action=(
                            "Review discretionary purchases "
                            "before adding more spending."
                        ),
                        reason=(
                            "A large share of expected "
                            "income has already been spent."
                        ),
                    )
                )


        # ======================================
        # CATEGORY BUDGETS
        # ======================================

        for category in context.category_spending:

            if (
                category.budget_limit is None
                or
                category.budget_limit <= 0
            ):
                continue


            usage = (
                category.amount
                /
                category.budget_limit
            )


            if usage > 1:

                over_amount = (
                    category.amount
                    -
                    category.budget_limit
                )


                insights.append(
                    Insight(
                        category="budget",
                        severity="warning",
                        title=(
                            f"{category.category} is over budget"
                        ),
                        message=(
                            f"You are "
                            f"${over_amount:,.2f} over your "
                            f"{category.category} budget."
                        ),
                        reason=(
                            f"${category.amount:,.2f} has been "
                            f"spent against a "
                            f"${category.budget_limit:,.2f} limit."
                        ),
                    )
                )


                actions.append(
                    RecommendedAction(
                        priority=2,
                        action=(
                            f"Slow down spending in "
                            f"{category.category}."
                        ),
                        reason=(
                            "That category has already "
                            "passed its monthly limit."
                        ),
                    )
                )


            elif usage >= 0.80:

                remaining = (
                    category.budget_limit
                    -
                    category.amount
                )


                insights.append(
                    Insight(
                        category="budget",
                        severity="info",
                        title=(
                            f"{category.category} is close "
                            "to its budget"
                        ),
                        message=(
                            f"Only ${remaining:,.2f} remains "
                            f"in the {category.category} budget."
                        ),
                        reason=(
                            "At least 80% of this category's "
                            "monthly budget has been used."
                        ),
                    )
                )


        # ======================================
        # SAVINGS GOALS
        # ======================================

        active_goals = [
            goal
            for goal
            in context.savings_goals
            if not goal.is_completed
        ]


        if active_goals:

            highest_priority_goal = min(
                active_goals,
                key=lambda goal:
                    goal.priority_rank,
            )


            if (
                highest_priority_goal.remaining
                >
                0
            ):

                insights.append(
                    Insight(
                        category="savings",
                        severity="info",
                        title=(
                            "Top savings priority"
                        ),
                        message=(
                            f"{highest_priority_goal.name} "
                            f"still needs "
                            f"${highest_priority_goal.remaining:,.2f}."
                        ),
                        reason=(
                            f"It is currently Priority "
                            f"{highest_priority_goal.priority_rank}."
                        ),
                    )
                )


        # ======================================
        # REQUIRED SAVINGS
        # ======================================

        if (
            context.required_savings_this_month
            >
            0
        ):

            insights.append(
                Insight(
                    category="savings",
                    severity="info",
                    title="Required savings protected",
                    message=(
                        f"${context.required_savings_this_month:,.2f} "
                        "is currently required to keep "
                        "your savings goals on pace."
                    ),
                    reason=(
                        "This amount is part of the "
                        "monthly plan before optional "
                        "spending."
                    ),
                )
            )


        # ======================================
        # FINANCIAL HEALTH
        # ======================================

        if (
            context.financial_health_score
            is not None
        ):

            score = (
                context.financial_health_score
            )


            if score >= 70:

                severity = "success"

                description = (
                    "Your current financial health "
                    "is generally healthy."
                )

            elif score >= 50:

                severity = "warning"

                description = (
                    "Your financial health currently "
                    "needs some attention."
                )

            else:

                severity = "critical"

                description = (
                    "Your current financial position "
                    "is under significant pressure."
                )


            insights.append(
                Insight(
                    category="health",
                    severity=severity,
                    title=(
                        f"Financial health: "
                        f"{score}/100"
                    ),
                    message=description,
                    reason=(
                        "This score comes from "
                        "PocketAI's financial engine."
                    ),
                )
            )


        # ======================================
        # DATA CONFIDENCE
        # ======================================

        if context.data_confidence.lower() == "low":

            insights.append(
                Insight(
                    category="data_quality",
                    severity="info",
                    title="PocketAI needs more history",
                    message=(
                        "Recommendations are currently "
                        "based on limited financial history."
                    ),
                    reason=(
                        "More transactions and regular "
                        "usage will allow PocketAI to "
                        "identify stronger patterns."
                    ),
                )
            )


        # ======================================
        # REMOVE DUPLICATE ACTIONS
        # ======================================

        actions = self._remove_duplicate_actions(
            actions
        )


        actions.sort(
            key=lambda item:
                item.priority
        )


        # ======================================
        # OVERALL STATUS
        # ======================================

        overall_status = (
            self._determine_overall_status(
                insights
            )
        )


        summary = (
            self._build_summary(
                context,
                overall_status,
            )
        )


        return AnalysisResult(
            engine_version=self.VERSION,
            overall_status=overall_status,
            summary=summary,
            confidence=context.data_confidence,
            insights=insights,
            recommended_actions=actions,
        )


    # ==========================================
    # OVERALL STATUS
    # ==========================================

    def _determine_overall_status(
        self,
        insights: list[Insight],
    ) -> str:

        severities = {
            insight.severity
            for insight
            in insights
        }


        if "critical" in severities:
            return "critical"


        if "warning" in severities:
            return "warning"


        if "success" in severities:
            return "on_track"


        return "informational"


    # ==========================================
    # BUILD SUMMARY
    # ==========================================

    def _build_summary(
        self,
        context: FinancialContext,
        status: str,
    ) -> str:

        if status == "critical":

            return (
                "Your finances need immediate attention. "
                "PocketAI found a financial pressure "
                "that should be handled before optional "
                "spending."
            )


        if status == "warning":

            return (
                "Your overall plan is still manageable, "
                "but PocketAI found something worth "
                "watching closely."
            )


        if context.safe_to_spend_total > 0:

            return (
                f"You are currently on track with "
                f"${context.safe_to_spend_total:,.2f} "
                "Safe to Spend remaining this month."
            )


        return (
            "PocketAI has analyzed the financial "
            "information currently available."
        )


    # ==========================================
    # REMOVE DUPLICATE ACTIONS
    # ==========================================

    def _remove_duplicate_actions(
        self,
        actions: list[
            RecommendedAction
        ],
    ) -> list[
        RecommendedAction
    ]:

        unique_actions: list[
            RecommendedAction
        ] = []


        seen: set[str] = set()


        for action in actions:

            key = (
                action.action
                    .strip()
                    .lower()
            )


            if key in seen:
                continue


            seen.add(
                key
            )


            unique_actions.append(
                action
            )


        return unique_actions