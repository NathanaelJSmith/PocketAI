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
    # ==========================================

    def _detect_weekly_acceleration(
        self,
        transactions: list[
            TransactionContext
        ],
    ) -> Insight | None:

        today = (
            date.today()
        )


        start_this_week = (
            today
            -
            timedelta(
                days=today.weekday()
            )
        )


        start_last_week = (
            start_this_week
            -
            timedelta(
                days=7
            )
        )


        this_week_total = sum(
            transaction.amount
            for transaction
            in transactions
            if (
                self._parse_date(
                    transaction.date
                )
                is not None
                and
                start_this_week
                <=
                self._parse_date(
                    transaction.date
                )
                <=
                today
            )
        )


        last_week_total = sum(
            transaction.amount
            for transaction
            in transactions
            if (
                self._parse_date(
                    transaction.date
                )
                is not None
                and
                start_last_week
                <=
                self._parse_date(
                    transaction.date
                )
                <
                start_this_week
            )
        )


        if last_week_total <= 0:

            return None


        difference = (
            this_week_total
            -
            last_week_total
        )


        percentage_change = (
            difference
            /
            last_week_total
        )


        # Ignore small changes.
        if (
            difference < 50
            or
            percentage_change < 0.25
        ):

            return None


        return Insight(
            category="pattern",
            severity="warning",
            title="Spending pace increased",
            message=(
                f"You have spent "
                f"${this_week_total:,.2f} this week, "
                f"compared with "
                f"${last_week_total:,.2f} last week."
            ),
            reason=(
                f"Your weekly spending increased by "
                f"about "
                f"{percentage_change * 100:.0f}%."
            ),
        )



    # ==========================================
    # CATEGORY MONTH-TO-MONTH CHANGE
    # ==========================================

    def _detect_category_growth(
        self,
        transactions: list[
            TransactionContext
        ],
    ) -> Insight | None:

        today = (
            date.today()
        )


        first_this_month = (
            today.replace(
                day=1
            )
        )


        if first_this_month.month == 1:

            first_last_month = (
                first_this_month.replace(
                    year=
                        first_this_month.year
                        -
                        1,
                    month=12,
                )
            )

        else:

            first_last_month = (
                first_this_month.replace(
                    month=
                        first_this_month.month
                        -
                        1
                )
            )


        current_totals: dict[
            str,
            float
        ] = defaultdict(
            float
        )


        previous_totals: dict[
            str,
            float
        ] = defaultdict(
            float
        )


        for transaction in transactions:

            transaction_date = (
                self._parse_date(
                    transaction.date
                )
            )


            if transaction_date is None:

                continue


            category = (
                transaction.category
                    .strip()
                or
                "Other"
            )


            if (
                transaction_date
                >=
                first_this_month
            ):

                current_totals[
                    category
                ] += transaction.amount


            elif (
                first_last_month
                <=
                transaction_date
                <
                first_this_month
            ):

                previous_totals[
                    category
                ] += transaction.amount


        strongest_category = (
            None
        )


        strongest_growth = (
            0.0
        )


        strongest_difference = (
            0.0
        )


        for (
            category,
            current_amount,
        ) in current_totals.items():

            previous_amount = (
                previous_totals.get(
                    category,
                    0,
                )
            )


            if previous_amount <= 0:

                continue


            difference = (
                current_amount
                -
                previous_amount
            )


            growth = (
                difference
                /
                previous_amount
            )


            if (
                difference >= 25
                and
                growth >= 0.30
                and
                growth > strongest_growth
            ):

                strongest_category = (
                    category
                )


                strongest_growth = (
                    growth
                )


                strongest_difference = (
                    difference
                )


        if strongest_category is None:

            return None


        current_amount = (
            current_totals[
                strongest_category
            ]
        )


        previous_amount = (
            previous_totals[
                strongest_category
            ]
        )


        return Insight(
            category="pattern",
            severity="info",
            title=(
                f"{strongest_category} spending increased"
            ),
            message=(
                f"You have spent "
                f"${current_amount:,.2f} on "
                f"{strongest_category} this month, "
                f"compared with "
                f"${previous_amount:,.2f} last month."
            ),
            reason=(
                f"That is about "
                f"{strongest_growth * 100:.0f}% higher, "
                f"an increase of "
                f"${strongest_difference:,.2f}."
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