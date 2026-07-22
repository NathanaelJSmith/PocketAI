import sys


def get_value(prompt, label):
	for line in prompt.splitlines():
		#checks if the line starts with the laben we want
		if line.startswith(label):
			return line.replace(label, "").strip()
	return ""

def money_to_float(value):
	#converts money text like $700.00 into 700.00
	try:
		return float(value.replace("$", "").replace(",", ""))
	except:
		return 0.0

def main():
	#gets finance prompt from C#
	prompt = sys.stdin.read()

	money_left = money_to_float(get_value(prompt, "Money Left Before Savings:"))
	safe_to_spend = money_to_float(get_value(prompt, "Safe to Spend:"))
	daily_safe_to_spend = money_to_float(get_value(prompt, "Daily Safe To Spend:"))
	weekly_savings_needed = money_to_float(get_value(prompt, "Weekly Savings Needed:"))
	biggest_category = get_value(prompt, "Biggest Spending Category:")
	biggest_category_amount = money_to_float(get_value(prompt, "Biggest Category Amount:"))
	over_budget_categories = get_value(prompt, "Over-Budget Categories:")
	monthly_recurring_expenses = money_to_float(get_value(prompt, "Monthly Recurring Expenses:"))
	projected_end_money = money_to_float(get_value(prompt, "Projected End-of-Month Money:"))
	this_week_spending = money_to_float(get_value(prompt, "This Week Spending:"))
	last_week_spending = money_to_float(get_value(prompt, "Last Week Spending:"))
	this_month_spending = money_to_float(get_value(prompt, "This Month Spending:"))
	last_month_spending = money_to_float(get_value(prompt, "Last Month Spending:"))

	print("PocketAI Python Coach Response:")
	print()

	#1. Quick Summary
	print("1. Quick Summary")

	if money_left > 0:
		print(f"You currently have ${money_left:.2f} left after spending and savings planning.")
	elif money_left == 0:
		print("You are right at $0 left, so you need to be careful with any extrea spending.")
	else:
		print("You are negative after spending and savings planning, so you should slow down spending immediately.")

	print()

	#2. Biggest Concern
	print("2. Biggest Concern")

	if biggest_category:
		print(f"Your biggest spending category is {biggest_category} at ${biggest_category_amount:.2f}.")
	else:
		print("I could not find a biggest spending category yet.")

	if over_budget_categories != "" and over_budget_categories != "0":
		print(f"You also have {over_budget_categories} category/categories over budget.")
	elif safe_to_spend <= 0:
		print("Your safe-to-spend amount is to low, so extra purchases may hurt your plan.")
	elif weekly_savings_needed > 100:
		print("Your weekly savings needed is high, so savings needs to be a priority.")
	else:
		print("Your budget does not look out control right now, but you still need to stay consistent.")



	if projected_end_money < 0:
		print("Your biggest concern is your cash flow forecast. If your spending continues at this pace, you may end the month negative.")
	elif monthly_recurring_expenses > 0 and safe_to_spend < monthly_recurring_expenses:
		print("Your recurring expenses are taking up a lot of your available money. Keep enough money reserved for upcoming bills.")
	elif over_budget_categories != "" and over_budget_categories != "0":
		print(f"You have {over_budget_categories} category/categories over budget, so you should slow down spending in those areas.")
	elif this_week_spending > last_week_spending and last_week_spending > 0:
		print("Your spending is higher this week than last week. Watch your recent spending before it becomes a pattern.")
	elif this_month_spending > last_month_spending and last_month_spending > 0:
		print("Your spending is higher this month than last month. Look for what category increased the most.")
	else:
		print("Your budget does not look out of control right now, but you still need to stay consistent.")
        
	print()

	#3. What are you doing well
	print("3. What You Are Doing Well")

	if weekly_savings_needed > 0:
		print(f"You have savings plan started, and you know you need about ${weekly_savings_needed:.2f} per week.")
	else:
		print("You have a savings goal information set up, which is a good start.")

	print()

	#4. What to do next
	print("4. What You Should Do Next")
	if projected_end_money < 0:
		print("Cut non-essential spending immediately to avoid going negative at the end of the month.")
	elif safe_to_spend <= 0:
		print("Do not make extra purchases right now. Your safe-to-spend amount is too low.")
	elif monthly_recurring_expenses > 0:
		print(f"Keep at least ${monthly_recurring_expenses:.2f} protected for recurring expenses before spending extra money.")
    	

	if biggest_category.lower() in ["food", "restaurants", "eating out"]:
		print("Try settings food limit for the next 7 days. Eat at home more and avoid small random food purchases.")
	elif biggest_category.lower() in ["gas", "car", "transportation"]:
		print("Plan your driving for the week and avoid unnecessary trips if possible.")
	elif biggest_category.lower() in ["entertainment", "games", "fun"]:
		print("Cut entertainment spending first because it is usually easier to reduce than bills.")
	else:
		print("Review your bigget cateogry and choose one spending habit to cut this week.")

	print(f"Stay under daily safe-to-spend amount ${daily_safe_to_spend:.2f}.")
	print("Before buying somthing, ask: does this help my savings goal or hurt it?")

if __name__ == "__main__":
	main()
		



