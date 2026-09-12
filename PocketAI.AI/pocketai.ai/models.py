from __future__ import annotations

from dataclasses import asdict, dataclass, field
from math import isfinite
from typing import Any

# ==========================================
# SAFE NUMBER HELPERS
# ==========================================

def safe_float(
        value: Any,
        default: float = 0.0,
) -> float:
    try:
        number = float(value)

        if not isfinite(number):
            return default

        return number

    except (TypeError, ValueError):
        return default


def safe_int(
        value: Any,
        default: int = 0,
) -> int:
    try:
        return int(value)

    except (TypeError, ValueError):
        return default

# ==========================================
# CATEGORY SPENDING
# ==========================================

@dataclass
class CategorySpending:
    category: str
    amount: float
    budget_limit: float | None = None

    @classmethod
    def from_dict(
        cls,
        data: dict[str, Any],
    ) -> "CategorySpending":
        
        budget_value = data.get(
            "budgetLimit"
        )

        budget_limit = (
            safe_float(budget_value)
            if budget_value is not None
            else None
        )

        return cls(
            category=str(
                data.get(
                    "category",
                    "Other",
                )
            ),
            amount=max(
                safe_float(
                    data.get(
                        "amount",
                        0,
                    )
                ),
                0,
            ),
            budget_limit=budget_limit,
        )

# ==========================================
# SAVINGS GOAL
# ==========================================

@dataclass
class SavingsGoalContext:
    name: str
    current_amount: float
    target_amount: float
    priority_rank: int
    is_completed: bool = False

    @property
    def remaining(self) -> float:
        return max(
            self.target_amount
            - self.current_amount,
            0,
        )

    @property
    def progress(self) -> float:
        if self.target_amount <= 0:
            return 0

        return min(
            max(
                self.current_amount
                / self.target_amount,
                0,
            ),
            1,
        )

    @classmethod
    def from_dict(
        cls, 
        data: dict[str, Any],
    ) -> "SavingsGoalContext":

        return cls(
            name=str(
                data.get(
                    "name",
                    "Savings Goal",
                )
            ),
            current_amount=max(
                safe_float(
                    data.get(
                        "currentAmount",
                        0,
                    )
                ),
                0,
            ),
            target_amount=max(
                safe_float(
                    data.get(
                        "targetAmount",
                        0,
                    )
                ),
                0,
            ),
            priority_rank=max(
                safe_int(
                    data.get(
                        "priorityRank",
                        1,
                    ),
                    1,
                ),
                1,
            ),
            is_completed=bool(
                data.get(
                    "isCompleted",
                    False,
                )
            ),
        )

# ==========================================
# BILL
# ==========================================

@dataclass
class BillContext:
    name: str
    amount: float
    due_day: int
    is_active: bool
    is_paid_this_month: bool

    @classmethod
    def from_dict(
        cls,
        data: dict[str, Any],
    ) -> "BillContext":

        return cls(
            name=str(
                data.get(
                    "name",
                    "Bill",
                )
            ),
            amount=max(
                safe_float(
                    data.get(
                        "amount",
                        0,
                    )
                ),
                0,
            ),
            due_day=min(
                max(
                    safe_int(
                        data.get(
                            "dueDay",
                            1,
                        ),
                        1,
                    ),
                    1,
                ),
                31,
            ),
            is_active=bool(
                data.get(
                    "isActive",
                    True,
                )
            ),
            is_paid_month=bool(
                data.get(
                    "isPaidThisMonth",
                    False,
                )
            ),
        )

# ==========================================
# COMPLETE FINANCIAL CONTEXT
# ==========================================

@dataclass
class FinancialContext:
    checking_balance: float = 0
    savings_account: float = 0

    expected_monthly_income: float = 0

    current_month_spent: float = 0

    upcoming_bills: float = 0

    required_savings_this_month = float = 0

    accepted_extra_savings: float = 0

    safe_to_spend_total: float = 0

    safe_to_spend_today: float = 0

    safe_to_spend_week: float = 0

    obligation_shortfall: float = 0

    projected_additional_spending: float = 0

    projected_month_and_money: float = 0

    over_budget_count: int = 0

    budget_count: int = 0

    current_month_transaction_count: int = 0

    active_recurring_bill_count: int = 0

    active_savings_goal_count: int = 0

    data_confidence: str = "Low"

    financial_health_score: int | None = None

    category_spending: list[
        CategorySpending
    ] = field(
        default_factory=list
    )

    savings_goals: list[
        SavingsGoalContext
    ] = field(
        default_factory=list
    )

    bills: list[
        BillContext
    ] = field(
        default_factory=list
    )

    @classmethod
    def from_dict(
        cls,
        data: dict[str, Any],
    ) -> "FinancialContext":

        health_score_raw = data.get(
            "financialHealthScore"
        )

        health_score = (
            safe_int(
                health_score_raw
            )
            if health_score_raw
            is not None
            else None
        )

        return cls(
            checking_balance=safe_float(
                data.get(
                    "checkingBalance",
                    0,
                )
            ),

            savings_balance=max(
                safe_float(
                    data.get(
                        "savingsBalance",
                        0,
                    )
                ),
                0,
            ),

            expected_monthly_income=max(
                safe_float(
                    data.get(
                        "expectedMonthlyIncome",
                        0,
                    )
                ),
                0,
            ),

            current_month_spent=max(
                safe_float(
                    data.get(
                        "currentMonthSpent",
                        0,
                    )
                ),
                0,
            ),

            upcoming_bills=max(
                safe_float(
                    data.get(
                        "upcomingBills",
                        0,
                    )
                ),
                0,
            ),

            required_savings_this_month=max(
                safe_float(
                    data.get(
                        "requiredSavingsThisMonth",
                        0,
                    )
                ),
                0,
            ),

            accepted_extra_savings=max(
                safe_float(
                    data.get(
                        "acceptedExtraSavings",
                        0,
                    )
                ),
                0,
            ),

            safe_to_spend_total=max(
                safe_float(
                    data.get(
                        "safeToSpendTotal",
                        0,
                    )
                ),
                0,
            ),

            safe_to_spend_today=max(
                safe_float(
                    data.get(
                        "safeToSpendToday",
                        0,
                    )
                ),
                0,
            ),

            safe_to_spend_this_week=max(
                safe_float(
                    data.get(
                        "safeToSpendThisWeek",
                        0,
                    )
                ),
                0,
            ),

            obligation_shortfall=max(
                safe_float(
                    data.get(
                        "obligationShortfall",
                        0,
                    )
                ),
                0,
            ),

            projected_additional_spending=max(
                safe_float(
                    data.get(
                        "projectedAdditionalSpending",
                        0,
                    )
                ),
                0,
            ),

            projected_month_end_money=safe_float(
                data.get(
                    "projectedMonthEndMoney",
                    0,
                )
            ),

            over_budget_count=max(
                safe_int(
                    data.get(
                        "overBudgetCount",
                        0,
                    )
                ),
                0,
            ),

            budget_count=max(
                safe_int(
                    data.get(
                        "budgetCount",
                        0,
                    )
                ),
                0,
            ),

            current_month_transaction_count=max(
                safe_int(
                    data.get(
                        "currentMonthTransactionCount",
                        0,
                    )
                ),
                0,
            ),

            active_recurring_bill_count=max(
                safe_int(
                    data.get(
                        "activeRecurringBillCount",
                        0,
                    )
                ),
                0,
            ),

            active_savings_goal_count=max(
                safe_int(
                    data.get(
                        "activeSavingsGoalCount",
                        0,
                    )
                ),
                0,
            ),

            data_confidence=str(
                data.get(
                    "dataConfidence",
                    "Low",
                )
            ),

            financial_health_score=health_score,

            category_spending=[
                CategorySpending.from_dict(
                    item
                )
                for item
                in data.get(
                    "categorySpending",
                    [],
                )
                if isinstance(
                    item,
                    dict,
                )
            ],

            savings_goals=[
                SavingsGoalContext.from_dict(
                    item
                )
                for item
                in data.get(
                    "savingsGoals",
                    [],
                )
                if isinstance(
                    item,
                    dict,
                )
            ],

            bills=[
                BillContext.from_dict(
                    item
                )
                for item
                in data.get(
                    "bills",
                    [],
                )
                if isinstance(
                    item,
                    dict,
                )
            ],
        )

# ==========================================
# AI INSIGHT
# ==========================================

@dataclass
class Insight:
    category: str

    serverity: str

    title: str

    message: str

    reason: str

# ==========================================
# RECOMMENDED ACTION
# ==========================================

@dataclass
class RecommendedAction:
    priority: int

    action: str

    reason: setattr


# ==========================================
# AI ANALYSIS RESULT
# ==========================================

@dataclass
class AnalysisResult:
    engine_version: str

    overall_status: str

    summary: str

    confidence: str

    insights: list[Insight]

    recommended_actions: list[
        RecommendedAction
    ]

    def to_dict(
            self
    ) -> dict[str, Any]:

        return asdict(
            self
        )
