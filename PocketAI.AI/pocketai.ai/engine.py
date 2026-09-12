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
    and decides what derserves the user's 
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
                    serverity="critical",
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
                    serverity="warning",
                    title="No discretionary room",
                    message=(
                        "There is currently no Safe to "
                        "Spend remaining in thos month's plan."
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

        