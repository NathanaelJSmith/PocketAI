from __future__ import annotations

import calendar
import re
from datetime import date
from .patterns import PatternAnalyzer

from .engine import PocketAIEngine
from .models import (
    AnalysisResult,
    BillContext,
    CategorySpending,
    ConversationState,
    FinancialContext,
    SavingsGoalContext,
)


class PocketAIConversationEngine:
    VERSION = "0.3.0"


    def __init__(self) -> None:
        self.analysis_engine = (
            PocketAIEngine()
        )

        self.pattern_analyzer = (
            PatternAnalyzer()
        )


    # ==========================================
    # MAIN ENTRY
    # ==========================================

    def answer(
        self,
        question: str,
        context: FinancialContext,
        state: ConversationState,
    ) -> AnalysisResult:

        question = (
            question.strip()
        )


        analysis = (
            self.analysis_engine
                .analyze(
                    context
                )
        )


        intent, confidence = (
            self._detect_intent(
                question,
                state,
            )
        )


        if intent == "affordability":

            answer = (
                self._answer_affordability(
                    question,
                    context,
                    state,
                )
            )


        elif intent == "purchase_savings_impact":

            answer = (
                self._answer_purchase_savings_impact(
                    question,
                    context,
                    state,
                )
            )


        elif intent == "budget":

            answer = (
                self._answer_budget(
                    question,
                    context,
                    state,
                )
            )


        elif intent == "spending":

            answer = (
                self._answer_spending(
                    context
                )
            )


        elif intent == "savings":

            answer = (
                self._answer_savings(
                    question,
                    context,
                    state,
                )
            )


        elif intent == "bills":

            answer = (
                self._answer_bills(
                    question,
                    context,
                    state,
                )
            )

        elif intent == "patterns":

            answer = (
                self._answer_patterns(
                    context
                )
            )

        elif intent == "health":

            answer = (
                self._answer_health(
                    context
                )
            )


        elif intent == "focus":

            answer = (
                self._answer_focus(
                    analysis
                )
            )


        elif intent == "explanation":

            answer = (
                self._answer_safe_to_spend_explanation(
                    context
                )
            )


        else:

            answer = (
                self._answer_general(
                    analysis
                )
            )


        state.last_intent = (
            intent
        )


        state.previous_question = (
            question
        )


        state.previous_answer = (
            answer
        )


        analysis.engine_version = (
            self.VERSION
        )


        analysis.intent = (
            intent
        )


        analysis.intent_confidence = (
            confidence
        )


        analysis.answer = (
            self._add_confidence_note(
                answer,
                context,
            )
        )


        analysis.conversation_state = (
            state
        )


        return analysis



    # ==========================================
    # INTENT DETECTION
    # ==========================================

    def _detect_intent(
        self,
        question: str,
        state: ConversationState,
    ) -> tuple[str, float]:

        text = (
            question
                .lower()
                .strip()
        )


        amount = (
            self._extract_amount(
                question
            )
        )


        # ======================================
        # AFFORDABILITY FOLLOW-UP
        # ======================================

        if (
            state.last_purchase_amount
            is not None
            and
            amount is not None
            and
            self._contains_any(
                text,
                [
                    "what about",
                    "how about",
                    "instead",
                    "what if",
                ],
            )
        ):

            return (
                "affordability",
                0.99,
            )


        if (
            state.last_purchase_amount
            is not None
            and
            self._contains_any(
                text,
                [
                    "can i afford it",
                    "is that safe",
                    "is it safe",
                    "is that too much",
                    "is it too much",
                    "should i buy it",
                    "should i do it",
                    "can i do it",
                    "would that be okay",
                    "would it be okay",
                ],
            )
        ):

            return (
                "affordability",
                0.99,
            )


        # ======================================
        # PURCHASE → SAVINGS FOLLOW-UP
        # ======================================

        if (
            state.last_purchase_amount
            is not None
            and
            self._contains_any(
                text,
                [
                    "hurt my savings",
                    "affect my savings",
                    "hurt my goal",
                    "affect my goal",
                    "hurt my savings goal",
                    "affect my savings goal",
                    "what about my savings",
                    "what about my goal",
                ],
            )
        ):

            return (
                "purchase_savings_impact",
                0.99,
            )


        # ======================================
        # CONTEXTUAL CATEGORY FOLLOW-UP
        # ======================================

        if (
            state.last_intent == "budget"
            and
            state.last_category
            and
            self._contains_any(
                text,
                [
                    "how much is left",
                    "how much left",
                    "am i over",
                    "is that over",
                    "what is the budget",
                    "what's the budget",
                    "remaining budget",
                    "budget remaining",
                ],
            )
        ):

            return (
                "budget",
                0.98,
            )


        # ======================================
        # CONTEXTUAL BILL FOLLOW-UP
        # ======================================

        if (
            state.last_intent == "bills"
            and
            state.last_bill_name
            and
            self._contains_any(
                text,
                [
                    "when is that due",
                    "when is it due",
                    "how much is that",
                    "how much is it",
                    "is that due soon",
                    "is it due soon",
                ],
            )
        ):

            return (
                "bills",
                0.98,
            )


        # ======================================
        # CONTEXTUAL SAVINGS FOLLOW-UP
        # ======================================

        if (
            state.last_intent == "savings"
            and
            state.last_savings_goal_name
            and
            self._contains_any(
                text,
                [
                    "how much is left",
                    "how much left",
                    "how much have i saved",
                    "how far along",
                    "what percent",
                    "what percentage",
                    "progress",
                ],
            )
        ):

            return (
                "savings",
                0.98,
            )


        # ======================================
        # SAFE TO SPEND EXPLANATION
        # ======================================

        if (
            "safe to spend" in text
            and
            self._contains_any(
                text,
                [
                    "why",
                    "calculate",
                    "calculated",
                    "how does",
                    "how is",
                ],
            )
        ):

            return (
                "explanation",
                0.95,
            )


        # ======================================
        # AFFORDABILITY
        # ======================================

        if self._contains_any(
            text,
            [
                "can i afford",
                "can i buy",
                "should i buy",
                "can i spend",
                "should i spend",
                "is it safe to buy",
                "is it safe to spend",
                "could i afford",
                "do i have enough for",
            ],
        ):

            return (
                "affordability",
                0.95,
            )


        # ======================================
        # NORMAL INTENTS
        # ======================================

        rules = {
            "focus": [
                "what should i focus on",
                "what should i focus",
                "what should i do",
                "what is my priority",
                "what's my priority",
                "what should i worry about",
                "needs my attention",
            ],

            "budget": [
                "budget",
                "over budget",
                "budget limit",
                "budget left",
                "room left",
            ],

            "savings": [
                "saving",
                "savings",
                "save",
                "goal",
                "goals",
                "save for",
                "save first",
            ],

            "bills": [
                "bill",
                "bills",
                "due",
                "payment",
                "payments",
                "recurring",
            ],

            "spending": [
                "spending",
                "spent",
                "spend the most",
                "where is my money going",
                "where does my money go",
                "biggest category",
                "largest category",
                "largest expense",
            ],

            "patterns": [
                "anything unusual",
                "anything weird",
                "spending pattern",
                "spending patterns",
                "any patterns",
                "notice anything",
                "spending faster",
                "spending more",
                "changed in my spending",
                "changes in my spending",
            ],

            "health": [
                "financial health",
                "health score",
                "how am i doing financially",
                "how are my finances",
                "financially",
            ],
        }


        best_intent = (
            "general"
        )


        best_score = (
            0
        )


        for intent, phrases in rules.items():

            score = sum(
                1
                for phrase
                in phrases
                if phrase in text
            )


            if score > best_score:

                best_score = (
                    score
                )


                best_intent = (
                    intent
                )


        if best_score == 0:

            return (
                "general",
                0.40,
            )


        return (
            best_intent,
            min(
                0.55
                +
                best_score
                *
                0.15,
                0.95,
            ),
        )



    # ==========================================
    # AFFORDABILITY
    # ==========================================

    def _answer_affordability(
        self,
        question: str,
        context: FinancialContext,
        state: ConversationState,
    ) -> str:

        amount = (
            self._extract_amount(
                question
            )
        )


        if amount is None:

            amount = (
                state.last_purchase_amount
            )


        if amount is None:

            return (
                f"You currently have "
                f"${context.safe_to_spend_total:,.2f} "
                f"Safe to Spend this month. "
                f"Tell me the price of what you're "
                f"considering."
            )


        description = (
            self._extract_purchase_description(
                question
            )
        )


        if description:

            state.last_purchase_description = (
                description
            )


        state.last_purchase_amount = (
            amount
        )


        item_text = (
            f" {state.last_purchase_description}"
            if state.last_purchase_description
            else " purchase"
        )


        if context.obligation_shortfall > 0:

            return (
                f"I would not treat the "
                f"${amount:,.2f}{item_text} as safe "
                f"right now. Your monthly plan is "
                f"already short by "
                f"${context.obligation_shortfall:,.2f}."
            )


        if amount > context.checking_balance:

            shortage = (
                amount
                -
                context.checking_balance
            )


            return (
                f"You do not currently have enough "
                f"in Checking for the "
                f"${amount:,.2f}{item_text}. "
                f"Your Checking balance is "
                f"${context.checking_balance:,.2f}, "
                f"which is ${shortage:,.2f} short."
            )


        if amount > context.safe_to_spend_total:

            over_amount = (
                amount
                -
                context.safe_to_spend_total
            )


            return (
                f"You have enough in Checking, but "
                f"I would not consider the "
                f"${amount:,.2f}{item_text} safe "
                f"inside your monthly plan. "
                f"It is ${over_amount:,.2f} more than "
                f"your Safe to Spend of "
                f"${context.safe_to_spend_total:,.2f}."
            )


        remaining = (
            context.safe_to_spend_total
            -
            amount
        )


        percent = (
            amount
            /
            context.safe_to_spend_total
            *
            100
            if context.safe_to_spend_total > 0
            else 100
        )


        if percent <= 25:

            return (
                f"The ${amount:,.2f}{item_text} "
                f"currently fits comfortably inside "
                f"your plan. You would still have "
                f"about ${remaining:,.2f} "
                f"Safe to Spend afterward."
            )


        if percent <= 50:

            return (
                f"The ${amount:,.2f}{item_text} fits "
                f"inside your current plan, but it "
                f"would use about {percent:.0f}% of "
                f"your remaining Safe to Spend. "
                f"You would have about "
                f"${remaining:,.2f} left."
            )


        return (
            f"The ${amount:,.2f}{item_text} technically "
            f"fits inside your Safe to Spend, but it "
            f"would use about {percent:.0f}% of what "
            f"remains in your monthly plan. "
            f"You would have only "
            f"${remaining:,.2f} left afterward, so "
            f"I would treat it as a significant purchase."
        )



    # ==========================================
    # PURCHASE IMPACT ON SAVINGS
    # ==========================================

    def _answer_purchase_savings_impact(
        self,
        question: str,
        context: FinancialContext,
        state: ConversationState,
    ) -> str:

        amount = (
            self._extract_amount(
                question
            )
            or
            state.last_purchase_amount
        )


        if amount is None:

            return (
                "Tell me which purchase you mean and "
                "I can compare it with your savings plan."
            )


        state.last_purchase_amount = (
            amount
        )


        if amount > context.safe_to_spend_total:

            return (
                f"A ${amount:,.2f} purchase is larger "
                f"than your current Safe to Spend of "
                f"${context.safe_to_spend_total:,.2f}. "
                f"I would avoid it if protecting your "
                f"savings goals is the priority."
            )


        remaining = (
            context.safe_to_spend_total
            -
            amount
        )


        return (
            f"PocketAI has already protected "
            f"${context.required_savings_this_month:,.2f} "
            f"of required savings inside your monthly "
            f"plan. A ${amount:,.2f} purchase would "
            f"reduce your remaining Safe to Spend to "
            f"about ${remaining:,.2f}, but it should "
            f"not reduce your actual Savings balance "
            f"unless you choose to spend money from Savings."
        )



    # ==========================================
    # BUDGET
    # ==========================================

    def _answer_budget(
        self,
        question: str,
        context: FinancialContext,
        state: ConversationState,
    ) -> str:

        category = (
            self._find_category(
                question,
                context,
            )
        )


        if (
            category is None
            and
            state.last_category
        ):

            category = (
                self._find_category_by_name(
                    state.last_category,
                    context,
                )
            )


        if category is not None:

            state.last_category = (
                category.category
            )


            if (
                category.budget_limit is None
                or
                category.budget_limit <= 0
            ):

                return (
                    f"You have spent "
                    f"${category.amount:,.2f} on "
                    f"{category.category} this month, "
                    f"but there is no budget limit "
                    f"attached to it yet."
                )


            remaining = (
                category.budget_limit
                -
                category.amount
            )


            if remaining < 0:

                return (
                    f"You have spent "
                    f"${category.amount:,.2f} on "
                    f"{category.category} against a "
                    f"${category.budget_limit:,.2f} budget. "
                    f"You are currently "
                    f"${abs(remaining):,.2f} over."
                )


            usage = (
                category.amount
                /
                category.budget_limit
                *
                100
            )


            return (
                f"You have spent "
                f"${category.amount:,.2f} of your "
                f"${category.budget_limit:,.2f} "
                f"{category.category} budget. "
                f"That is about {usage:.0f}% used, "
                f"leaving ${remaining:,.2f}."
            )


        tracked = [
            item
            for item
            in context.category_spending
            if (
                item.budget_limit is not None
                and
                item.budget_limit > 0
            )
        ]


        if not tracked:

            return (
                "I do not have enough budget information "
                "yet to identify a category that needs attention."
            )


        highest = max(
            tracked,
            key=lambda item:
                item.amount
                /
                item.budget_limit,
        )


        state.last_category = (
            highest.category
        )


        remaining = (
            highest.budget_limit
            -
            highest.amount
        )


        return (
            f"{highest.category} is currently your "
            f"most-used tracked budget. "
            f"You have spent "
            f"${highest.amount:,.2f} of "
            f"${highest.budget_limit:,.2f}, "
            f"leaving ${remaining:,.2f}."
        )



    # ==========================================
    # SPENDING
    # ==========================================

    def _answer_spending(
        self,
        context: FinancialContext,
    ) -> str:

        if not context.category_spending:

            return (
                "You do not have enough spending "
                "recorded this month for me to identify "
                "your largest category yet."
            )


        biggest = max(
            context.category_spending,
            key=lambda item:
                item.amount,
        )


        total = sum(
            item.amount
            for item
            in context.category_spending
        )


        percentage = (
            biggest.amount
            /
            total
            *
            100
            if total > 0
            else 0
        )


        return (
            f"Your largest spending category this "
            f"month is {biggest.category} at "
            f"${biggest.amount:,.2f}. "
            f"That is about {percentage:.0f}% of your "
            f"${total:,.2f} recorded monthly spending."
        )



    # ==========================================
    # SAVINGS
    # ==========================================

    def _answer_savings(
        self,
        question: str,
        context: FinancialContext,
        state: ConversationState,
    ) -> str:

        active = [
            goal
            for goal
            in context.savings_goals
            if not goal.is_completed
        ]


        if not active:

            return (
                "You do not currently have any active "
                "savings goals."
            )


        goal = (
            self._find_savings_goal(
                question,
                active,
            )
        )


        if (
            goal is None
            and
            state.last_savings_goal_name
        ):

            goal = next(
                (
                    item
                    for item
                    in active
                    if item.name.lower()
                    ==
                    state.last_savings_goal_name.lower()
                ),
                None,
            )


        if goal is None:

            goal = min(
                active,
                key=lambda item:
                    item.priority_rank,
            )


        state.last_savings_goal_name = (
            goal.name
        )


        progress = (
            goal.progress
            *
            100
        )


        return (
            f"{goal.name} is currently Priority "
            f"{goal.priority_rank}. "
            f"You have saved "
            f"${goal.current_amount:,.2f} of "
            f"${goal.target_amount:,.2f}, "
            f"which is about {progress:.0f}% complete. "
            f"${goal.remaining:,.2f} remains."
        )



    # ==========================================
    # BILLS
    # ==========================================

    def _answer_bills(
        self,
        question: str,
        context: FinancialContext,
        state: ConversationState,
    ) -> str:

        bill = (
            self._find_bill(
                question,
                context,
            )
        )


        if (
            bill is None
            and
            state.last_bill_name
        ):

            bill = next(
                (
                    item
                    for item
                    in context.bills
                    if item.name.lower()
                    ==
                    state.last_bill_name.lower()
                ),
                None,
            )


        if bill is None:

            unpaid = [
                item
                for item
                in context.bills
                if (
                    item.is_active
                    and
                    not item.is_paid_this_month
                )
            ]


            if not unpaid:

                return (
                    "I do not see any active unpaid bills "
                    "that need attention right now."
                )


            bill = min(
                unpaid,
                key=lambda item:
                    self._days_until_due(
                        item.due_day
                    ),
            )


        state.last_bill_name = (
            bill.name
        )


        days_until = (
            self._days_until_due(
                bill.due_day
            )
        )


        if days_until == 0:

            due_text = "today"

        elif days_until == 1:

            due_text = "tomorrow"

        else:

            due_text = (
                f"in {days_until} days"
            )


        text = (
            question.lower()
        )


        if "how much" in text:

            return (
                f"{bill.name} is "
                f"${bill.amount:,.2f}."
            )


        if (
            "when" in text
            or
            "due" in text
        ):

            return (
                f"{bill.name} is due "
                f"{due_text} and is "
                f"${bill.amount:,.2f}."
            )


        return (
            f"Your next relevant bill is "
            f"{bill.name} for "
            f"${bill.amount:,.2f}, due "
            f"{due_text}."
        )

    # ==========================================
    # PATTERN QUESTIONS
    # ==========================================

    def _answer_patterns(
        self,
        context: FinancialContext,
    ) -> str:

        patterns = (
            self.pattern_analyzer
                .analyze(
                    context
                )
        )


        if not patterns:

            return (
                "I don't see a strong spending pattern "
                "that stands out yet. Keep recording "
                "transactions and I'll become more "
                "confident as your history grows."
            )


        # Limit the response so PocketAI does
        # not overwhelm the user.
        strongest_patterns = (
            patterns[:3]
        )


        parts = [
            (
                f"{pattern.message} "
                f"{pattern.reason}"
            )
            for pattern
            in strongest_patterns
        ]


        return (
            "Here's what stands out: "
            +
            " ".join(
                parts
            )
        )

    # ==========================================
    # FINANCIAL HEALTH
    # ==========================================

    def _answer_health(
        self,
        context: FinancialContext,
    ) -> str:

        score = (
            context.financial_health_score
        )


        if score is None:

            return (
                "I do not have enough financial history "
                "yet to give you a confident Financial "
                "Health score."
            )


        if score >= 70:

            status = (
                "generally healthy"
            )

        elif score >= 50:

            status = (
                "showing some areas that need attention"
            )

        else:

            status = (
                "under noticeable financial pressure"
            )


        return (
            f"Your current Financial Health score is "
            f"{score}/100, which means your finances "
            f"are {status}. "
            f"You currently have "
            f"${context.safe_to_spend_total:,.2f} "
            f"Safe to Spend remaining this month."
        )



    # ==========================================
    # FOCUS
    # ==========================================

    def _answer_focus(
        self,
        analysis: AnalysisResult,
    ) -> str:

        if analysis.recommended_actions:

            action = min(
                analysis.recommended_actions,
                key=lambda item:
                    item.priority,
            )


            return (
                f"{analysis.summary} "
                f"My top recommendation: "
                f"{action.action} "
                f"{action.reason}"
            )


        warning = next(
            (
                insight
                for insight
                in analysis.insights
                if insight.severity
                in (
                    "critical",
                    "warning",
                )
            ),
            None,
        )


        if warning is not None:

            return (
                f"{analysis.summary} "
                f"{warning.message} "
                f"{warning.reason}"
            )


        budget = next(
            (
                insight
                for insight
                in analysis.insights
                if insight.category
                ==
                "budget"
            ),
            None,
        )


        if budget is not None:

            return (
                f"{analysis.summary} "
                f"{budget.message}"
            )


        return (
            analysis.summary
        )



    # ==========================================
    # SAFE TO SPEND EXPLANATION
    # ==========================================

    def _answer_safe_to_spend_explanation(
        self,
        context: FinancialContext,
    ) -> str:

        return (
            f"Your Safe to Spend starts with "
            f"${context.expected_monthly_income:,.2f} "
            f"of expected monthly income, then subtracts "
            f"${context.current_month_spent:,.2f} "
            f"of spending, "
            f"${context.upcoming_bills:,.2f} "
            f"of active bills, "
            f"${context.required_savings_this_month:,.2f} "
            f"of required savings, and "
            f"${context.accepted_extra_savings:,.2f} "
            f"of accepted extra savings. "
            f"That leaves "
            f"${context.safe_to_spend_total:,.2f}."
        )



    # ==========================================
    # GENERAL
    # ==========================================

    def _answer_general(
        self,
        analysis: AnalysisResult,
    ) -> str:

        return (
            f"{analysis.summary} "
            f"You can ask me about affordability, "
            f"spending, budgets, savings goals, bills, "
            f"Safe to Spend, or Financial Health."
        )



    # ==========================================
    # HELPERS
    # ==========================================

    def _contains_any(
        self,
        text: str,
        phrases: list[str],
    ) -> bool:

        return any(
            phrase in text
            for phrase
            in phrases
        )


    def _extract_amount(
        self,
        question: str,
    ) -> float | None:

        match = re.search(
            r"\$?\s*(\d+(?:\.\d{1,2})?)",
            question,
        )


        if match is None:

            return None


        try:

            amount = float(
                match.group(1)
            )


            return (
                amount
                if amount > 0
                else None
            )

        except ValueError:

            return None


    def _extract_purchase_description(
        self,
        question: str,
    ) -> str:

        match = re.search(
            r"\$?\s*\d+(?:\.\d{1,2})?\s*(.*)",
            question,
            re.IGNORECASE,
        )


        if match is None:

            return ""


        description = (
            match.group(1)
                .strip(
                    " ?!.,"
                )
        )


        description = re.sub(
            r"^(for|on)\s+",
            "",
            description,
            flags=re.IGNORECASE,
        )


        description = re.sub(
            r"^(a|an|the)\s+",
            "",
            description,
            flags=re.IGNORECASE,
        )


        return (
            description.strip()
        )


    def _find_category(
        self,
        question: str,
        context: FinancialContext,
    ) -> CategorySpending | None:

        text = (
            question.lower()
        )


        return next(
            (
                item
                for item
                in context.category_spending
                if item.category.lower()
                in text
            ),
            None,
        )


    def _find_category_by_name(
        self,
        name: str,
        context: FinancialContext,
    ) -> CategorySpending | None:

        return next(
            (
                item
                for item
                in context.category_spending
                if item.category.lower()
                ==
                name.lower()
            ),
            None,
        )


    def _find_bill(
        self,
        question: str,
        context: FinancialContext,
    ) -> BillContext | None:

        text = (
            question.lower()
        )


        return next(
            (
                bill
                for bill
                in context.bills
                if bill.name.lower()
                in text
            ),
            None,
        )


    def _find_savings_goal(
        self,
        question: str,
        goals: list[
            SavingsGoalContext
        ],
    ) -> SavingsGoalContext | None:

        text = (
            question.lower()
        )


        return next(
            (
                goal
                for goal
                in goals
                if goal.name.lower()
                in text
            ),
            None,
        )


    def _days_until_due(
        self,
        due_day: int,
    ) -> int:

        today = (
            date.today()
        )


        current_month_days = (
            calendar.monthrange(
                today.year,
                today.month,
            )[1]
        )


        current_due_day = min(
            due_day,
            current_month_days,
        )


        if current_due_day >= today.day:

            return (
                current_due_day
                -
                today.day
            )


        if today.month == 12:

            next_year = (
                today.year
                +
                1
            )

            next_month = 1

        else:

            next_year = (
                today.year
            )

            next_month = (
                today.month
                +
                1
            )


        next_month_days = (
            calendar.monthrange(
                next_year,
                next_month,
            )[1]
        )


        next_due_day = min(
            due_day,
            next_month_days,
        )


        return (
            current_month_days
            -
            today.day
            +
            next_due_day
        )


    def _add_confidence_note(
        self,
        answer: str,
        context: FinancialContext,
    ) -> str:

        if (
            context.data_confidence.lower()
            !=
            "low"
        ):

            return answer


        return (
            answer
            +
            " I still have limited financial history, "
            "so this recommendation will improve as "
            "you record more activity."
        )