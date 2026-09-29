
from __future__ import annotations

from datetime import date

from .models import FinancialContext, Insight


class SavingsDeadlineAnalyzer:
    """
    Analyzes savings-goal deadlines.

    Does not change savings balances, goal amounts,
    goal priorities, or financial calculations.
    """

    def analyze(
        self,
        context: FinancialContext,
    ) -> list[Insight]:

        insights: list[Insight] = []

        today = date.today()

        for goal in context.savings_goals:

            # Completed/archived goals do not need
            # deadline warnings.
            if goal.is_completed:
                continue

            try:
                deadline = date.fromisoformat(
                    goal.deadline
                )
            except (ValueError, TypeError):
                continue

            remaining = goal.remaining

            days_remaining = (
                deadline - today
            ).days

            # ==================================
            # FULLY FUNDED, NOT FINISHED
            # ==================================

            if remaining <= 0:

                insights.append(
                    Insight(
                        category="savings_deadline",
                        severity="success",
                        title=f"{goal.name} is fully funded",
                        message=(
                            f"You've reached your "
                            f"{goal.name} savings target."
                        ),
                        reason=(
                            "The goal hasn't been marked "
                            "complete yet. You can finish "
                            "it or increase its target."
                        ),
                    )
                )

                continue

            # ==================================
            # MISSED DEADLINE
            # ==================================

            if days_remaining < 0:

                insights.append(
                    Insight(
                        category="savings_deadline",
                        severity="warning",
                        title=f"{goal.name} deadline passed",
                        message=(
                            f"The deadline for {goal.name} "
                            f"has passed, with "
                            f"${remaining:,.2f} still needed."
                        ),
                        reason=(
                            "Review the goal's deadline "
                            "or adjust the target to match "
                            "your current plan."
                        ),
                    )
                )

                continue

            # ==================================
            # DEADLINE TODAY
            # ==================================

            if days_remaining == 0:

                insights.append(
                    Insight(
                        category="savings_deadline",
                        severity="warning",
                        title=f"{goal.name} is due today",
                        message=(
                            f"Your {goal.name} deadline "
                            f"is today, and "
                            f"${remaining:,.2f} remains."
                        ),
                        reason=(
                            "You can review the goal and "
                            "decide whether to contribute "
                            "more or change its deadline."
                        ),
                    )
                )

                continue

            # ==================================
            # NEXT 7 DAYS
            # ==================================

            if days_remaining <= 7:

                daily_needed = (
                    remaining / days_remaining
                )

                insights.append(
                    Insight(
                        category="savings_deadline",
                        severity="warning",
                        title=f"{goal.name} deadline approaching",
                        message=(
                            f"{goal.name} is due in "
                            f"{days_remaining} days, with "
                            f"${remaining:,.2f} remaining."
                        ),
                        reason=(
                            f"Reaching the target on time "
                            f"would require approximately "
                            f"${daily_needed:,.2f} per day. "
                            "This is a required contribution "
                            "estimate, not a prediction."
                        ),
                    )
                )

                continue

            # ==================================
            # NEXT 30 DAYS
            # ==================================

            if days_remaining <= 30:

                weekly_needed = (
                    remaining / days_remaining * 7
                )

                insights.append(
                    Insight(
                        category="savings_deadline",
                        severity="info",
                        title=f"{goal.name} deadline coming up",
                        message=(
                            f"{goal.name} is due in "
                            f"{days_remaining} days, with "
                            f"${remaining:,.2f} remaining."
                        ),
                        reason=(
                            f"Reaching the target on time "
                            f"would require approximately "
                            f"${weekly_needed:,.2f} per week."
                        ),
                    )
                )

        return insights
