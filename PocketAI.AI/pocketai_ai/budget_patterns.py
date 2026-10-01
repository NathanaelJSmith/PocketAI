from __future__ import annotations

from collections import defaultdict
from datetime import date, datetime, timedelta

from .models import (
    FinancialContext,
    Insight,
)


class BudgetPressureAnalyzer:
    """
    Detects categories that repeatedly use a
    large portion of the user's current budget.

    Historical months are compared against the
    CURRENT budget limit because PocketAI does
    not yet store historical budget-limit changes.
    """

    PRESSURE_THRESHOLD = 0.80


    def analyze(
        self,
        context: FinancialContext,
    ) -> list[Insight]:

        if not context.budget_limits:
            return []

        today = date.today()

        first_this_month = today.replace(
            day=1
        )

        history_start = self._parse_date(
            context.transaction_history_start_date
        )

        if history_start is None:
            return []

        monthly_totals: dict[
            tuple[int, int, str],
            float,
        ] = defaultdict(float)

        for transaction in context.transaction_history:

            transaction_date = self._parse_date(
                transaction.date
            )

            if transaction_date is None:
                continue

            # Repeated-pattern analysis uses only
            # fully completed months.
            if transaction_date >= first_this_month:
                continue

            if transaction_date < history_start:
                continue

            category_key = (
                transaction.category
                    .strip()
                    .casefold()
            )

            if not category_key:
                category_key = "other"

            monthly_totals[
                (
                    transaction_date.year,
                    transaction_date.month,
                    category_key,
                )
            ] += transaction.amount

        complete_months = (
            self._recent_complete_months(
                today,
                history_start,
                maximum=3,
            )
        )

        # We need at least two fully-covered
        # historical months before calling
        # something "repeated."
        if len(complete_months) < 2:
            return []

        insights: list[Insight] = []

        for budget in context.budget_limits:

            if budget.limit_amount <= 0:
                continue

            category_key = (
                budget.category
                    .strip()
                    .casefold()
            )

            pressured_months = []

            for year, month in complete_months:

                amount = monthly_totals.get(
                    (
                        year,
                        month,
                        category_key,
                    ),
                    0.0,
                )

                usage = (
                    amount
                    /
                    budget.limit_amount
                )

                if usage >= self.PRESSURE_THRESHOLD:

                    pressured_months.append(
                        (
                            year,
                            month,
                            amount,
                            usage,
                        )
                    )

            if len(pressured_months) < 2:
                continue

            month_descriptions = []

            for (
                year,
                month,
                amount,
                usage,
            ) in pressured_months:

                month_name = date(
                    year,
                    month,
                    1,
                ).strftime("%B")

                month_descriptions.append(
                    f"{month_name}: "
                    f"${amount:,.2f} "
                    f"({usage * 100:.0f}%)"
                )

            current_spending = (
                self._find_current_category(
                    context,
                    budget.category,
                )
            )

            current_text = ""

            if current_spending is not None:

                current_usage = (
                    current_spending
                    /
                    budget.limit_amount
                )

                current_text = (
                    f" This month so far, you've "
                    f"recorded ${current_spending:,.2f}, "
                    f"or about "
                    f"{current_usage * 100:.0f}% "
                    f"of that budget."
                )

            insights.append(
                Insight(
                    category="budget_pressure",
                    severity="warning",
                    title=(
                        f"{budget.category} budget "
                        f"is repeatedly tight"
                    ),
                    message=(
                        f"{budget.category} reached at "
                        f"least 80% of your current "
                        f"${budget.limit_amount:,.2f} "
                        f"budget in "
                        f"{len(pressured_months)} of the "
                        f"last {len(complete_months)} "
                        f"complete months."
                        f"{current_text}"
                    ),
                    reason=(
                        "Historical usage: "
                        +
                        "; ".join(
                            month_descriptions
                        )
                        +
                        ". PocketAI is using your current "
                        "budget as the historical reference "
                        "because previous budget-limit "
                        "changes are not stored yet."
                    ),
                )
            )

        return insights


    def _recent_complete_months(
        self,
        today: date,
        history_start: date,
        maximum: int,
    ) -> list[tuple[int, int]]:

        months: list[
            tuple[int, int]
        ] = []

        first_this_month = (
            today.replace(day=1)
        )

        cursor = (
            first_this_month
            -
            timedelta(days=1)
        ).replace(day=1)

        while (
            len(months) < maximum
            and
            cursor >= history_start
        ):

            # Only use a month if PocketAI's
            # history includes its first day.
            if cursor >= history_start:

                months.append(
                    (
                        cursor.year,
                        cursor.month,
                    )
                )

            cursor = (
                cursor
                -
                timedelta(days=1)
            ).replace(day=1)

        return months


    def _find_current_category(
        self,
        context: FinancialContext,
        category_name: str,
    ) -> float | None:

        for category in context.category_spending:

            if (
                category.category.casefold()
                ==
                category_name.casefold()
            ):
                return category.amount

        return None


    def _parse_date(
        self,
        value: str,
    ) -> date | None:

        try:

            return datetime.strptime(
                value,
                "%Y-%m-%d",
            ).date()

        except (
            TypeError,
            ValueError,
        ):

            return None