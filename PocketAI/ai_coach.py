import sys


def main():
    # Gets the financial prompt sent from C#
    prompt = sys.stdin.read()

    # Temporary test response so we know C# and Python are connected
    print("PocketAI Python Coach Response:")
    print()
    print("I received the financial summary from C#.")
    print("Based on the current data, focus on your biggest spending category and protect your savings goal.")
    print()
    print("Prompt received:")
    print("Protect your savings goal first, watch your biggest spending category, and stay under your daily safe to spend amount")


if __name__ == "__main__":
    main()
