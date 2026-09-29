from __future__ import annotations

from collections import defaultdict
from datetime import date, datetime, timedelta
from statistics import median

from .models import (
    FinancialContext,
    Insight,
    TransactionContext,
)


class PatternAnalyzer:
    """
    Detects behavioral patterns from transaction
    history.

    This does not change financial balances or
    Safe to Spend.

    It only interprets historical behavior.
    """


    # ==========================================
    # MAIN ANALYSIS
    # ==========================================

    def analyze(
        self,
        context: FinancialContext,
    ) -> list[Insight]:

        transactions = (
            self._valid_transactions(
                context.transaction_history
            )
        )


        if not transactions:

            return []


        insights: list[Insight] = []


        weekly_pattern = (
            self._detect_weekly_acceleration(
                transactions
            )
        )


        if weekly_pattern is not None:

            insights.append(
                weekly_pattern
            )


        category_pattern = (
            self._detect_category_growth(
                transactions
            )
        )


        if category_pattern is not None:

            insights.append(
                category_pattern
            )


        unusual_transaction = (
            self._detect_unusual_transaction(
                transactions
            )
        )


        if unusual_transaction is not None:

            insights.append(
                unusual_transaction
            )


        return insights



    # ==========================================
    # WEEK-TO-WEEK SPENDING
    # Compare equivalent weekdays
    # ==========================================

    def _detect_weekly_acceleration(
        self,
        transactions: list[TransactionContext],
    ) -> Insight | None:

        today = date.today()

        days_elapsed = today.weekday() + 1

        # Avoid conclusions from only one or two days.
        if days_elapsed < 3:
            return None

        start_this_week = today - timedelta(
            days=today.weekday()
        )

        start_last_week = start_this_week - timedelta(
            days=7
        )

        # Compare the same number of days.
        end_last_week = start_last_week + timedelta(
            days=days_elapsed - 1
        )

        this_week_total = 0.0
        last_week_total = 0.0

        this_week_count = 0
        last_week_count = 0

        for transaction in transactions:

            transaction_date = self._parse_date(
                transaction.date
            )

            if transaction_date is None:
                continue

            if start_this_week <= transaction_date <= today:

                this_week_total += transaction.amount
                this_week_count += 1

            elif start_last_week <= transaction_date <= end_last_week:

                last_week_total += transaction.amount
                last_week_count += 1

        # We need a meaningful comparison period.
        if this_week_count < 2 or last_week_count < 2:
            return None

        if last_week_total <= 0:
            return None

        difference = this_week_total - last_week_total

        percentage_change = difference / last_week_total

        # Require a meaningful increase in both
        # dollars and percentage.
        if difference < 50 or percentage_change < 0.25:
            return None

        return Insight(
            category="pattern",
            severity="warning",
            title="Spending pace increased",
            message=(
                f"You recorded ${this_week_total:,.2f} "
                f"of spending so far this week, compared "
                f"with ${last_week_total:,.2f} over the "
                f"same weekdays last week."
            ),
            reason=(
                f"That's an increase of "
                f"${difference:,.2f}, or about "
                f"{percentage_change * 100:.0f}%. "
                f"This comparison uses equivalent "
                f"time periods."
            ),
        )



    # ==========================================
    # CATEGORY MONTH-TO-MONTH CHANGE
    # Compare equivalent calendar days
    # ==========================================

    def _detect_category_growth(
        self,
        transactions: list[TransactionContext],
    ) -> Insight | None:

        today = date.today()

        first_this_month = today.replace(day=1)

        last_day_previous_month = (
            first_this_month - timedelta(days=1)
        )

        first_last_month = (
            last_day_previous_month.replace(day=1)
        )

        # Both periods must contain the same number
        # of calendar days.
        comparison_days = min(
            today.day,
            last_day_previous_month.day,
        )

        # Avoid conclusions too early in the month.
        if comparison_days < 7:
            return None

        end_this_period = (
            first_this_month
            + timedelta(days=comparison_days - 1)
        )

        end_previous_period = (
            first_last_month
            + timedelta(days=comparison_days - 1)
        )

        current_totals = defaultdict(float)
        previous_totals = defaultdict(float)

        current_counts = defaultdict(int)
        previous_counts = defaultdict(int)

        category_names = {}

        for transaction in transactions:

            transaction_date = self._parse_date(
                transaction.date
            )

            if transaction_date is None:
                continue

            category_name = (
                transaction.category.strip() or "Other"
            )

            # Treat Dining and dining as one category.
            category_key = category_name.casefold()

            category_names[category_key] = category_name

            if (
                first_this_month
                <= transaction_date
                <= end_this_period
            ):

                current_totals[category_key] += (
                    transaction.amount
                )

                current_counts[category_key] += 1

            elif (
                first_last_month
                <= transaction_date
                <= end_previous_period
            ):

                previous_totals[category_key] += (
                    transaction.amount
                )

                previous_counts[category_key] += 1

        strongest_category = None

        strongest_difference = 0.0
        strongest_growth = 0.0

        for category_key, current_amount in current_totals.items():

            previous_amount = previous_totals.get(
                category_key,
                0.0,
            )

            # Require actual recorded history
            # in both periods.
            if (
                current_counts[category_key] < 2
                or previous_counts[category_key] < 2
                or previous_amount < 25
            ):
                continue

            difference = current_amount - previous_amount

            growth = difference / previous_amount

            # Meaningful increase:
            # at least $25 and at least 30%.
            if difference < 25 or growth < 0.30:
                continue

            # Select the largest dollar increase.
            if difference > strongest_difference:

                strongest_category = category_key
                strongest_difference = difference
                strongest_growth = growth

        if strongest_category is None:
            return None

        category_name = category_names[
            strongest_category
        ]

        current_amount = current_totals[
            strongest_category
        ]

        previous_amount = previous_totals[
            strongest_category
        ]

        return Insight(
            category="pattern",
            severity="info",
            title=f"{category_name} spending increased",
            message=(
                f"You recorded ${current_amount:,.2f} "
                f"in {category_name} over the first "
                f"{comparison_days} days of this month, "
                f"compared with ${previous_amount:,.2f} "
                f"over the same number of days last month."
            ),
            reason=(
                f"That's an increase of "
                f"${strongest_difference:,.2f}, or about "
                f"{strongest_growth * 100:.0f}%. "
                f"The comparison uses equivalent "
                f"calendar periods."
            ),
        )



    # ==========================================
    # UNUSUALLY LARGE TRANSACTION
    # ==========================================

    def _detect_unusual_transaction(
        self,
        transactions: list[
            TransactionContext
        ],
    ) -> Insight | None:

        if len(
            transactions
        ) < 8:

            return None


        amounts = [
            transaction.amount
            for transaction
            in transactions
            if transaction.amount > 0
        ]


        if len(
            amounts
        ) < 8:

            return None


        typical_amount = (
            median(
                amounts
            )
        )


        if typical_amount <= 0:

            return None


        today = (
            date.today()
        )


        first_this_month = (
            today.replace(
                day=1
            )
        )


        current_month_transactions = [
            transaction
            for transaction
            in transactions
            if (
                self._parse_date(
                    transaction.date
                )
                is not None
                and
                self._parse_date(
                    transaction.date
                )
                >=
                first_this_month
            )
        ]


        if not current_month_transactions:

            return None


        largest = max(
            current_month_transactions,
            key=lambda transaction:
                transaction.amount,
        )


        # Require both:
        #
        # 1. at least twice the user's median
        # 2. at least $50
        #
        # This prevents tiny purchases from
        # being labeled unusual.

        if (
            largest.amount < 50
            or
            largest.amount
            <
            typical_amount * 2
        ):

            return None


        multiple = (
            largest.amount
            /
            typical_amount
        )


        return Insight(
            category="pattern",
            severity="info",
            title="Larger-than-usual transaction",
            message=(
                f"Your ${largest.amount:,.2f} "
                f"{largest.name} transaction is larger "
                f"than your typical recent purchase."
            ),
            reason=(
                f"Your median transaction over the "
                f"available history is about "
                f"${typical_amount:,.2f}, making this "
                f"purchase about {multiple:.1f} times "
                f"your typical transaction size."
            ),
        )



    # ==========================================
    # VALID TRANSACTIONS
    # ==========================================

    def _valid_transactions(
        self,
        transactions: list[
            TransactionContext
        ],
    ) -> list[
        TransactionContext
    ]:

        return [
            transaction
            for transaction
            in transactions
            if (
                transaction.amount > 0
                and
                self._parse_date(
                    transaction.date
                )
                is not None
            )
        ]



    # ==========================================
    # DATE PARSER
    # ==========================================

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