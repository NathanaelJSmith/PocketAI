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
	financial_health_score_text = get_value(prompt, "Financial Health Score:")

	try:
		financial_health_score = int(financial_health_score_text.replace("/100", "").strip())
	except: 
		financial_health_score = 0

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
	if financial_health_score < 40:
		print(
			"Your overall financial health is currently at risk. "
			"Focus on reducing spending, protecting bill money "
			"and improving your monthly cash flow."
		)
	elif financial_health_score < 60:
		print(
			"Your overall financial health is fair, but you should "
			"focus on improving your cash flow and reducing spending."
		)
	elif projected_end_money < 0:
		print(
			"Your financial health needs attention. "
        	"Your current budget or cash flow has areas that should be improved."
		)
	elif safe_to_spend <= 0:
		print(
			"Your safe to spend amount is too low, so extra purchases "
			"could interfere with your bills or savings goal."
		)
	elif over_budget_categories != "" and over_budget_categories != "0":
		print(
			f"You currently have {over_budget_categories} category/categories "
			"over budget."
		)
	elif this_week_spending > last_week_spending and last_week_spending > 0:
		difference = this_week_spending - last_week_spending
		print(
			f"You spent ${difference:.2f} more this week than last week."
		)
	elif this_month_spending > last_month_spending and last_month_spending > 0:
		difference = this_month_spending - last_month_spending
		print(
			f"You spent ${difference:.2f} more this month than last month"
		)
	elif monthly_recurring_expenses > 0:
		print(
			f"You have ${monthly_recurring_expenses:.2f} recurring expenses. "
			"Keep enough money reserved for those payments."
		)
	elif biggest_category:
		print(
			f"Your biggest spending category is {biggest_category} "
			f"at ${biggest_category_amount:.2f}."
		)
	else:
		print(
			"Your budget does not appear out of control, "
			"but you still need to stay consistent."
		)
		
	print()

	#3. What are you doing well
	print("3. What You Are Doing Well")
	if financial_health_score >= 90:
		print(
			"Your financial health is excellent! Keep up the good work."
			"Your current spending, and budgeting habits are in a strong position."
		)
	elif financial_health_score >= 75:
		print(
			"Your financial health is good. Keep up the good work."
			"You have a solid financial foundation and should keep building on it."
		)
	elif this_week_spending < last_week_spending and last_week_spending > 0:
		difference = last_week_spending - this_week_spending
		print(
			f"Your spending improved this week. "
			f"You spent ${difference:.2f} less than last week." 
		)
	elif this_month_spending < last_month_spending and last_month_spending > 0:
		difference = last_month_spending - this_month_spending
		print(
			f"Your monthly spending is improving. "
			f"You spent ${difference:.2f} less than last month."
		)
	elif weekly_savings_needed > 0:
		print(
			f"You have a savings plan started, and you know you need "
			f"about ${weekly_savings_needed:.2f} per week."
		)
	elif money_left > 0:
		print(
			f"You currently have ${money_left:.2f} left after expenses, "
			f"which give you flexibility."
		)
	else:
		print(
			"You are tracking your finances, which is the first step "
			"toward improving your."
		)
			
		
	

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
		



