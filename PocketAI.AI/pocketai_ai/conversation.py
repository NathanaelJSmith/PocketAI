from __future__ import annotations

import calendar
import re

from datetime import date, timedelta

from .engine import PocketAIEngine
from .patterns import PatternAnalyzer

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

    # ==========================================
    # CONSTRUCTOR
    # ==========================================

    def __init__(
        self,
    ) -> None:

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

        # ======================================
        # AFFORDABILITY
        # ======================================

        if intent == "affordability":

            answer = (
                self._answer_affordability(
                    question,
                    context,
                    state,
                )
            )

        # ======================================
        # PURCHASE SAVINGS IMPACT
        # ======================================

        elif intent == "purchase_savings_impact":

            answer = (
                self._answer_purchase_savings_impact(
                    question,
                    context,
                    state,
                )
            )

        # ======================================
        # REPEATED BUDGET PRESSURE
        # ======================================

        elif intent == "budget_pressure":

            answer = (
                self._answer_budget_pressure(
                    question,
                    context,
                    analysis,
                )
            )

        # ======================================
        # NORMAL BUDGET QUESTION
        # ======================================

        elif intent == "budget":

            answer = (
                self._answer_budget(
                    question,
                    context,
                    state,
                )
            )
        elif intent == "projection":

            answer = (
                self._answer_projection(
                    context
                )
            )

        # ======================================
        # SPENDING TREND
        # ======================================

        elif intent == "spending_trend":

            category = (
                self._find_trend_category(
                    question,
                    context,
                )
            )

            if category:

                intent = (
                    "category_trend"
                )

                state.last_category = (
                    category
                )

                answer = (
                    self._answer_category_trend(
                        context,
                        category,
                    )
                )

            else:

                answer = (
                    self._answer_spending_trend(
                        context
                    )
                )

        # ======================================
        # NORMAL SPENDING
        # ======================================

        elif intent == "spending":

            answer = (
                self._answer_spending(
                    context
                )
            )

        # ======================================
        # SAVINGS DEADLINES
        # ======================================

        elif intent == "savings_deadlines":

            answer = (
                self._answer_savings_deadlines(
                    analysis
                )
            )

        # ======================================
        # SAVINGS
        # ======================================

        elif intent == "savings":

            answer = (
                self._answer_savings(
                    question,
                    context,
                    state,
                )
            )

        # ======================================
        # BILLS
        # ======================================

        elif intent == "bills":

            answer = (
                self._answer_bills(
                    question,
                    context,
                    state,
                )
            )

        # ======================================
        # PATTERNS
        # ======================================

        elif intent == "patterns":

            answer = (
                self._answer_patterns(
                    context
                )
            )

        # ======================================
        # FINANCIAL HEALTH
        # ======================================

        elif intent == "health":

            answer = (
                self._answer_health(
                    context
                )
            )

        # ======================================
        # FOCUS
        # ======================================

        elif intent == "focus":

            answer = (
                self._answer_focus(
                    analysis
                )
            )

        # ======================================
        # SAFE TO SPEND EXPLANATION
        # ======================================

        elif intent == "explanation":

            answer = (
                self._answer_safe_to_spend_explanation(
                    context
                )
            )

        # ======================================
        # GENERAL
        # ======================================

        else:

            answer = (
                self._answer_general(
                    analysis
                )
            )

        # ======================================
        # SAVE CONVERSATION MEMORY
        # ======================================

        state.last_intent = (
            intent
        )

        state.previous_question = (
            question
        )

        state.previous_answer = (
            answer
        )

        # ======================================
        # FINAL RESPONSE INFORMATION
        # ======================================

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
        # PURCHASE -> SAVINGS FOLLOW-UP
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
        # SPENDING TREND QUESTIONS
        # ======================================

        trend_phrases = [
            "am i spending more",
            "am i spending less",
            "spending more than usual",
            "spending less than usual",
            "spending faster",
            "spending increased",
            "spending decreased",
            "spending changed",
            "spending change",
            "spending trend",
            "spending compared",
        ]

        if (
            any(
                phrase in text
                for phrase
                in trend_phrases
            )
            or
            (
                "spend" in text
                and
                "changed" in text
            )
        ):

            return (
                "spending_trend",
                0.95,
            )

        # ======================================
        # SAVINGS DEADLINE QUESTIONS
        # ======================================

        if self._contains_any(
            text,
            [
                "savings deadline",
                "goal deadline",
                "goals close to",
                "goals due",
                "savings goals due",
                "am i on track with my savings",
                "will i reach my savings",
                "will i reach my goal",
            ],
        ):

            return (
                "savings_deadlines",
                0.95,
            )

        # ======================================
        # REPEATED BUDGET PRESSURE
        # ======================================

        if self._contains_any(
            text,
            [
                "keep going over",
                "keep getting close",
                "usually over budget",
                "repeated budget",
                "budget pressure",
                "over budget often",
                "consistently over budget",
                "budget every month",
                "keep hitting my budget",
                "always close to my budget",
            ],
        ):

            return (
                "budget_pressure",
                0.96,
            )

        # ======================================
        # MONTH-END PROJECTION QUESTIONS
        # ======================================

        if self._contains_any(
            text,
            [
                "end of the month",
                "end of month",
                "month end",
                "month-end",
                "projected spending",
                "spending projection",
                "how much will i have left",
                "where will i end up",
                "finish the month",
                "on pace this month",
            ],
        ):

            return (
                "projection",
                0.96,
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

            # Patterns comes before spending so
            # a broad pattern question wins a tie.
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
            self._extract_amount(question)
            or state.last_purchase_amount
        )

        if amount is None:
            return (
                f"You have ${context.safe_to_spend_total:,.2f} "
                f"Safe to Spend this month. "
                "How much is the purchase?"
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

        state.last_purchase_amount = amount

        item = (
            state.last_purchase_description
            or
            "purchase"
        )

        if context.obligation_shortfall > 0:
            return (
                f"I wouldn't call the ${amount:,.2f} "
                f"{item} safe right now. "
                f"Your monthly plan is already short by "
                f"${context.obligation_shortfall:,.2f}."
            )

        if amount > context.checking_balance:

            shortage = (
                amount
                -
                context.checking_balance
            )

            return (
                f"You don't have enough in Checking for it. "
                f"You're about ${shortage:,.2f} short."
            )

        if amount > context.safe_to_spend_total:

            over = (
                amount
                -
                context.safe_to_spend_total
            )

            return (
                f"You can cover it from Checking, but it's "
                f"${over:,.2f} above your Safe to Spend. "
                "I'd wait or cut spending somewhere else first."
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
                f"Yes, the ${amount:,.2f} {item} fits "
                f"comfortably in your plan. "
                f"You'd still have ${remaining:,.2f} "
                "Safe to Spend."
            )

        if percent <= 50:
            return (
                f"Yes, it fits, but it would use about "
                f"{percent:.0f}% of your remaining money. "
                f"You'd have ${remaining:,.2f} left."
            )

        return (
            f"It fits, but it's a big purchase for your "
            f"current budget. It would use about "
            f"{percent:.0f}% of your Safe to Spend and "
            f"leave ${remaining:,.2f}."
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
            self._extract_amount(question)
            or
            state.last_purchase_amount
        )

        if amount is None:
            return (
                "Tell me the purchase amount and I'll "
                "compare it with your savings plan."
            )

        state.last_purchase_amount = amount

        if amount > context.safe_to_spend_total:

            difference = (
                amount
                -
                context.safe_to_spend_total
            )

            return (
                f"That purchase is ${difference:,.2f} above "
                f"your Safe to Spend. I'd avoid it if keeping "
                "your savings plan on track is the priority."
            )

        remaining = (
            context.safe_to_spend_total
            -
            amount
        )

        return (
            f"It shouldn't take money from Savings if you "
            f"pay from Checking. Your planned savings are "
            f"already protected, and you'd have "
            f"${remaining:,.2f} Safe to Spend left."
        )

    # ==========================================
    # REPEATED BUDGET PRESSURE
    # ==========================================

    def _answer_budget_pressure(
        self,
        question: str,
        context: FinancialContext,
        analysis: AnalysisResult,
    ) -> str:

        pressure_insights = [
            insight
            for insight in analysis.insights
            if insight.category == "budget_pressure"
        ]

        question_lower = (
            question.casefold()
        )

        mentioned_category = None

        for budget in context.budget_limits:

            if (
                budget.category.casefold()
                in question_lower
            ):
                mentioned_category = (
                    budget.category
                )
                break

        if mentioned_category:

            pressure_insights = [
                insight
                for insight in pressure_insights
                if (
                    mentioned_category.casefold()
                    in insight.title.casefold()
                    or
                    mentioned_category.casefold()
                    in insight.message.casefold()
                )
            ]

        if not pressure_insights:

            if mentioned_category:
                return (
                    f"I don't see repeated budget pressure "
                    f"in {mentioned_category} yet. "
                    "Keep recording expenses and I'll watch it."
                )

            return (
                "I don't see any category repeatedly pushing "
                "its budget yet."
            )

        insight = pressure_insights[0]

        return (
            f"{insight.message} "
            "If this keeps happening, consider either cutting "
            "that spending or setting a more realistic budget."
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
                    f"You've spent ${category.amount:,.2f} on "
                    f"{category.category} this month, but you "
                    "haven't set a budget for it."
                )

            remaining = (
                category.budget_limit
                -
                category.amount
            )

            if remaining < 0:
                return (
                    f"{category.category} is "
                    f"${abs(remaining):,.2f} over budget. "
                    f"You've spent ${category.amount:,.2f} "
                    f"of your ${category.budget_limit:,.2f} limit."
                )

            usage = (
                category.amount
                /
                category.budget_limit
                *
                100
            )

            return (
                f"You've used about {usage:.0f}% of your "
                f"{category.category} budget. "
                f"${remaining:,.2f} remains."
            )

        tracked = [
            item
            for item in context.category_spending
            if (
                item.budget_limit is not None
                and
                item.budget_limit > 0
            )
        ]

        if not tracked:
            return (
                "You don't have enough budget information "
                "recorded yet."
            )

        highest = max(
            tracked,
            key=lambda item:
                item.amount / item.budget_limit,
        )

        state.last_category = (
            highest.category
        )

        remaining = (
            highest.budget_limit
            -
            highest.amount
        )

        if remaining < 0:
            return (
                f"{highest.category} needs the most attention. "
                f"It's ${abs(remaining):,.2f} over budget."
            )

        return (
            f"{highest.category} is your most-used budget. "
            f"You have ${remaining:,.2f} left."
        )

    # ==========================================
    # MONTH-END PROJECTION
    # ==========================================

    def _answer_projection(
        self,
        context: FinancialContext,
    ) -> str:

        if (
            context.current_month_transaction_count == 0
            or
            context.projected_additional_spending <= 0
        ):

            return (
                "I don't have enough spending this month "
                "to make a useful projection yet."
            )

        if context.projected_month_end_money < 0:

            shortfall = abs(
                context.projected_month_end_money
            )

            return (
                f"At your current pace, you could end the "
                f"month about ${shortfall:,.2f} short. "
                "I'd cut back on optional spending for now."
            )

        if (
            context.projected_additional_spending
            >
            context.safe_to_spend_total
        ):

            difference = (
                context.projected_additional_spending
                -
                context.safe_to_spend_total
            )

            return (
                f"Your spending pace is a little high. "
                f"You're projected to spend about "
                f"${difference:,.2f} more than your "
                f"remaining Safe to Spend."
            )

        return (
            f"You're currently on pace to finish the month "
            f"with about "
            f"${context.projected_month_end_money:,.2f} left. "
            f"PocketAI expects about "
            f"${context.projected_additional_spending:,.2f} "
            f"of additional spending."
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
                "You don't have enough spending recorded "
                "this month yet."
            )

        biggest = max(
            context.category_spending,
            key=lambda item:
                item.amount,
        )

        total = sum(
            item.amount
            for item in context.category_spending
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
            f"You're spending the most on "
            f"{biggest.category}: ${biggest.amount:,.2f}. "
            f"That's about {percentage:.0f}% of your "
            "recorded spending this month."
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
            for goal in context.savings_goals
            if not goal.is_completed
        ]

        if not active:
            return (
                "You don't currently have an active "
                "savings goal."
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
                    for item in active
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
            f"{goal.name} is {progress:.0f}% complete. "
            f"You've saved ${goal.current_amount:,.2f} and "
            f"still need ${goal.remaining:,.2f}."
        )

    # ==========================================
    # SAVINGS DEADLINE QUESTIONS
    # ==========================================

    def _answer_savings_deadlines(
        self,
        analysis: AnalysisResult,
    ) -> str:

        deadline_insights = [
            insight
            for insight in analysis.insights
            if insight.category == "savings_deadline"
        ]

        if not deadline_insights:
            return (
                "Your savings deadlines look okay right now."
            )

        insight = (
            deadline_insights[0]
        )

        reason = (
            insight.reason.split(".")[0]
        )

        return (
            f"{insight.message} "
            f"{reason}."
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

            due_text = (
                "today"
            )

        elif days_until == 1:

            due_text = (
                "tomorrow"
            )

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
    # FIND CATEGORY IN TREND QUESTION
    # ==========================================

    def _find_trend_category(
        self,
        question: str,
        context: FinancialContext,
    ) -> str | None:

        category_names = {
            item.category
            for item
            in context.category_spending
        }

        category_names.update(
            item.category
            for item
            in context.transaction_history
        )

        question_lower = (
            question.casefold()
        )

        for category in sorted(
            category_names,
            key=len,
            reverse=True,
        ):

            if (
                category.strip()
                and
                category.casefold()
                in question_lower
            ):

                return category

        return None

    # ==========================================
    # OVERALL SPENDING TREND
    # ==========================================

    def _answer_spending_trend(
        self,
        context: FinancialContext,
    ) -> str:

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

        end_last_period = (
            start_last_week
            +
            timedelta(
                days=today.weekday()
            )
        )

        this_week = []

        last_week = []

        for transaction in context.transaction_history:

            if transaction.amount <= 0:
                continue

            try:

                transaction_date = (
                    date.fromisoformat(
                        transaction.date
                    )
                )

            except (
                ValueError,
                TypeError,
            ):

                continue

            if (
                start_this_week
                <=
                transaction_date
                <=
                today
            ):

                this_week.append(
                    transaction
                )

            elif (
                start_last_week
                <=
                transaction_date
                <=
                end_last_period
            ):

                last_week.append(
                    transaction
                )

        if (
            len(this_week) < 2
            or
            len(last_week) < 2
        ):

            return (
                "I don't have enough transactions in both "
                "comparison periods to determine whether "
                "your spending has increased reliably yet."
            )

        current_total = sum(
            transaction.amount
            for transaction
            in this_week
        )

        previous_total = sum(
            transaction.amount
            for transaction
            in last_week
        )

        if previous_total <= 0:

            return (
                "I don't have a usable previous-week "
                "spending total to compare against."
            )

        difference = (
            current_total
            -
            previous_total
        )

        percentage = (
            difference
            /
            previous_total
            *
            100
        )

        # ======================================
        # SPENDING INCREASED
        # ======================================

        if difference > 0:

            answer = (
                f"Yes. You've spent ${current_total:,.2f} "
                f"this week versus ${previous_total:,.2f} "
                f"at the same point last week. "
                f"That's ${difference:,.2f} more."
            )

        elif difference < 0:

            answer = (
                f"You're spending less this week. "
                f"You've spent ${current_total:,.2f} versus "
                f"${previous_total:,.2f} last week."
            )

        else:

            return (
                f"Your spending is about the same as last week "
                f"at ${current_total:,.2f}."
            )


        if (
            difference > 0
            and
            current_total > 0
        ):

            largest = max(
                this_week,
                key=lambda transaction:
                    transaction.amount,
            )

            share = (
                largest.amount
                /
                current_total
            )

            if share >= 0.50:

                answer += (
                    f" Your ${largest.amount:,.2f} "
                    f"{largest.name} purchase caused most "
                    "of the increase."
                )


        return answer

    # ==========================================
    # CATEGORY-SPECIFIC SPENDING TREND
    # ==========================================

    def _answer_category_trend(
        self,
        context: FinancialContext,
        category: str,
    ) -> str:

        today = (
            date.today()
        )

        first_this_month = (
            today.replace(
                day=1
            )
        )

        last_day_previous_month = (
            first_this_month
            -
            timedelta(
                days=1
            )
        )

        first_last_month = (
            last_day_previous_month.replace(
                day=1
            )
        )

        comparison_days = min(
            today.day,
            last_day_previous_month.day,
        )

        end_this_period = (
            first_this_month
            +
            timedelta(
                days=
                    comparison_days
                    -
                    1
            )
        )

        end_previous_period = (
            first_last_month
            +
            timedelta(
                days=
                    comparison_days
                    -
                    1
            )
        )

        current_total = (
            0.0
        )

        previous_total = (
            0.0
        )

        current_count = (
            0
        )

        previous_count = (
            0
        )

        for transaction in context.transaction_history:

            if (
                transaction.category.casefold()
                !=
                category.casefold()
            ):

                continue

            if transaction.amount <= 0:

                continue

            try:

                transaction_date = (
                    date.fromisoformat(
                        transaction.date
                    )
                )

            except (
                ValueError,
                TypeError,
            ):

                continue

            if (
                first_this_month
                <=
                transaction_date
                <=
                end_this_period
            ):

                current_total += (
                    transaction.amount
                )

                current_count += (
                    1
                )

            elif (
                first_last_month
                <=
                transaction_date
                <=
                end_previous_period
            ):

                previous_total += (
                    transaction.amount
                )

                previous_count += (
                    1
                )

        if (
            current_count < 1
            or
            previous_count < 1
        ):

            return (
                f"I don't have enough {category} transactions "
                f"in both months to calculate a reliable "
                f"month-to-month change."
            )

        if previous_total <= 0:

            return (
                f"I don't have a usable previous-month "
                f"{category} total for comparison."
            )

        difference = (
            current_total
            -
            previous_total
        )

        percentage = (
            difference
            /
            previous_total
            *
            100
        )

        if difference > 0:

            return (
                f"Your {category} spending increased from "
                f"${previous_total:,.2f} to "
                f"${current_total:,.2f}. "
                f"That's ${difference:,.2f} more."
            )

        if difference < 0:

            return (
                f"Your {category} spending dropped from "
                f"${previous_total:,.2f} to "
                f"${current_total:,.2f}. "
                f"That's ${abs(difference):,.2f} less."
            )

        return (
            f"Your {category} spending hasn't changed much. "
            f"It's ${current_total:,.2f} in both periods."
        )

    # ==========================================
    # PATTERN QUESTIONS
    # ==========================================

    def _answer_patterns(
        self,
        context: FinancialContext,
    ) -> str:

        if not context.transaction_history:
            return (
                "I need more transaction history before "
                "I can spot spending patterns."
            )

        patterns = (
            self.pattern_analyzer
                .analyze(
                    context
                )
        )

        if not patterns:
            return (
                "Nothing unusual stands out right now. "
                "Keep recording your expenses and I'll "
                "continue watching for changes."
            )

        strongest = (
            patterns[:3]
        )

        messages = [
            pattern.message
            for pattern in strongest
        ]

        return (
            "Here's what stands out: "
            +
            " ".join(
                messages
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
                "I need more financial history before "
                "I can give you a useful health score."
            )

        if score >= 70:
            status = "looking healthy"

        elif score >= 50:
            status = "okay, but needs some attention"

        else:
            status = "under some financial pressure"

        return (
            f"Your Financial Health score is {score}/100 "
            f"and is {status}. "
            f"You have ${context.safe_to_spend_total:,.2f} "
            "Safe to Spend."
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
                f"Your top priority: {action.action} "
                f"{action.reason}"
            )

        warning = next(
            (
                insight
                for insight in analysis.insights
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
                f"{warning.message} "
                f"That's the main thing I'd watch right now."
            )

        return (
            "Nothing urgent needs attention right now. "
            "Keep following your current plan."
        )

    # ==========================================
    # SAFE TO SPEND EXPLANATION
    # ==========================================

    def _answer_safe_to_spend_explanation(
        self,
        context: FinancialContext,
    ) -> str:

        return (
            f"You have ${context.safe_to_spend_total:,.2f} "
            f"Safe to Spend this month. "
            "That's what's left after your current spending, "
            "bills, and planned savings are accounted for."
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
            f"spending, spending trends, budgets, "
            f"repeated budget pressure, savings goals, "
            f"savings deadlines, bills, Safe to Spend, "
            f"or Financial Health."
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

    # ==========================================
    # EXTRACT MONEY AMOUNT
    # ==========================================

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

    # ==========================================
    # EXTRACT PURCHASE DESCRIPTION
    # ==========================================

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

    # ==========================================
    # FIND CURRENT CATEGORY
    # ==========================================

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

    # ==========================================
    # FIND CATEGORY BY NAME
    # ==========================================

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

    # ==========================================
    # FIND BILL
    # ==========================================

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

    # ==========================================
    # FIND SAVINGS GOAL
    # ==========================================

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

    # ==========================================
    # DAYS UNTIL BILL IS DUE
    # ==========================================

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

            next_month = (
                1
            )

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

    # ==========================================
    # CONFIDENCE NOTE
    # ==========================================

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
            " I need a little more history to be fully confident."
        )