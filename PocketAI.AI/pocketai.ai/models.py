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