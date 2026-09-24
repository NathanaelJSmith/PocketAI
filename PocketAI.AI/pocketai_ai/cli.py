from __future__ import annotations

import json
import sys
from typing import Any

from .conversation import PocketAIConversationEngine
from .engine import PocketAIEngine
from .models import FinancialContext
from .models import(
    ConversationState,
    FinancialContext,
)


# ==========================================
# RUN POCKETAI
# ==========================================


def run_analysis(
    payload: dict[str, Any],
) -> dict[str, Any]:

    context = (
        FinancialContext.from_dict(
            payload
        )
    )


    question = str(
        payload.get(
            "question",
            "",
        )
        or
        ""
    ).strip()

    conversation_state = (
        ConversationState.from_dict(
            payload.get(
                "conversationState",
                {},
            )
        )
    )


    # ======================================
    # USER ASKED A QUESTION
    # ======================================

    if question:

        conversation_engine = (
            PocketAIConversationEngine()
        )


        result = (
            conversation_engine.answer(
                question,
                context,
                conversation_state,
            )
        )


    # ======================================
    # FINANCIAL ANALYSIS ONLY
    # ======================================

    else:

        engine = (
            PocketAIEngine()
        )


        result = (
            engine.analyze(
                context
            )
        )


    return result.to_dict()


# ==========================================
# COMMAND LINE ENTRY
# ==========================================


def main() -> None:

    try:

        raw_input = (
            sys.stdin.read()
        )


        if not raw_input.strip():

            raise ValueError(
                "No financial data was provided."
            )


        payload = (
            json.loads(
                raw_input
            )
        )


        if not isinstance(
            payload,
            dict,
        ):

            raise ValueError(
                "PocketAI expected a JSON object."
            )


        result = (
            run_analysis(
                payload
            )
        )


        print(
            json.dumps(
                result,
                indent=2,
            )
        )


    except Exception as error:

        error_result = {
            "success": False,
            "error": str(
                error
            ),
        }


        print(
            json.dumps(
                error_result,
                indent=2,
            )
        )


        sys.exit(
            1
        )


if __name__ == "__main__":
    main()