# PocketAI

PocketAI is a personal finance application built with C#, Python, SQLite, and AI integration. The goal of PocketAI is to help users understand their finances, control spending, build savings, and receive personalized financial insights from an AI-powered financial coach.

Rather than only tracking transactions, PocketAI analyzes a user's financial data to provide useful information such as safe-to-spend amounts, projected spending, savings progress, budget health, and an overall financial health score.

## Current Features

### Expense Tracking

* Add, edit, view, and delete expenses
* Organize expenses by category
* Track expense dates
* View current and previous spending periods
* Calculate spending totals by category

### Income and Account Balances

* Track monthly income
* Store checking, savings, and cash balances
* Calculate total available account balance

### Budgeting

* Create category-based budget limits
* Compare spending against budget limits
* Detect categories that are over budget
* Track money remaining after monthly spending

### Savings Goals

* Create a savings goal with a target amount and deadline
* Track current savings progress
* Calculate remaining savings needed
* Calculate weekly savings needed to reach a goal
* Calculate savings progress percentages

### Financial Analytics

* Analyze current-week and previous-week spending
* Analyze current-month and previous-month spending
* Calculate average daily spending
* Project additional spending for the rest of the month
* Estimate end-of-month money remaining
* Calculate daily and weekly safe-to-spend amounts
* Compare spending between different periods
* Identify the highest spending category
* Calculate an overall Financial Health Score

### Recurring Expenses

* Store recurring bills and expenses
* Track categories, amounts, and due dates
* Calculate when recurring expenses are due
* Include recurring expenses in financial summaries

### AI Financial Coach

* Connect the C# application with a Python AI service
* Send financial information to the AI coach for analysis
* Receive personalized financial feedback
* Save previous AI advice
* View, search, and delete AI advice history

### Data Storage

PocketAI uses SQLite to store:

* Expenses
* Income
* Account balances
* Savings goals
* Budget limits
* Recurring expenses
* AI advice history

## Financial Analytics System

PocketAI builds a financial summary from the user's financial information and uses it to calculate useful metrics such as:

* Monthly income
* Monthly spending
* Money remaining
* Total account balance
* Savings progress
* Weekly savings required
* Largest spending category
* Number of over-budget categories
* Monthly recurring expenses
* Projected end-of-month balance
* Safe-to-spend amount
* Financial Health Score

The Financial Health Score ranges from **0–100** and evaluates several parts of the user's financial situation, including spending, budgets, recurring expenses, savings progress, and projected cash flow.

## Project Structure

```text
PocketAI
│
├── PocketAI
│   ├── Program.cs
│   ├── DataBaseManager.cs
│   ├── AIService.cs
│   └── ai_coach.py
│
├── PocketAI.Core
│   ├── Models
│   │   ├── AccountBalance.cs
│   │   ├── AIAdvice.cs
│   │   ├── BudgetLimit.cs
│   │   ├── Expense.cs
│   │   ├── FinancialSummary.cs
│   │   ├── Income.cs
│   │   ├── RecurringExpenses.cs
│   │   └── SavingsGoal.cs
│   │
│   └── Services
│       └── AnalyticsService.cs
│
└── README.md
```

## Technologies Used

* **C#**
* **.NET**
* **Python**
* **SQLite**
* **Microsoft.Data.Sqlite**
* **Object-Oriented Programming**
* **LINQ**
* **AI API Integration**
* **Git**
* **GitHub**

## Current Architecture

PocketAI separates its financial models and analytics logic from the main application.

**PocketAI.Core** contains reusable financial models and analytics services.

**PocketAI** contains the main application, SQLite database management, and communication with the Python AI financial coach.

This structure is being developed so the same financial logic can later support a graphical desktop or mobile interface.

## Planned UI

PocketAI is currently transitioning from a console application toward a modern personal-finance dashboard.

The planned interface will include:

* Home financial dashboard
* Account balance overview
* Transaction activity
* Budget management
* Savings goal tracking
* Recurring bills
* Financial analytics and charts
* Financial Health Score
* AI financial coach
* AI insight history

## Future Development

* Graphical user interface
* Interactive financial charts
* User authentication
* Multiple user accounts
* Cloud data storage and synchronization
* Improved AI financial recommendations
* Automatic financial alerts
* Expanded financial trend analysis
* Mobile application support

## Project Goal

PocketAI is designed to go beyond traditional expense tracking.

The long-term goal is to create an intelligent personal finance assistant that can answer questions such as:

* **How much can I safely spend today?**
* **Am I on track to reach my savings goal?**
* **Where am I overspending?**
* **How is my spending changing over time?**
* **How much money will I likely have at the end of the month?**
* **What can I change to improve my financial health?**

PocketAI combines traditional budgeting tools with financial analytics and AI-generated insights to help users make better everyday financial decisions.

## Status

**Active Development**

The core financial system, SQLite data storage, analytics service, savings calculations, recurring expense tracking, and initial AI integration are currently implemented.

Development is now focused on expanding analytics and preparing PocketAI for a graphical user interface.
