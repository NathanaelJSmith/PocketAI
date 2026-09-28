# PocketAI

PocketAI is a personal finance application built with **C#**, **.NET MAUI**, **SQLite**, and **Python**.

The goal of PocketAI is to go beyond basic expense tracking. The application combines budgeting, savings planning, financial analytics, and a conversational financial assistant to help users understand not only **where their money went**, but also **what they can safely do next**.

PocketAI can answer questions such as:

- How much can I safely spend this month?
- How much can I safely spend today or this week?
- Am I staying within my budgets?
- Am I on track for my savings goals?
- What bills are affecting my financial plan?
- What spending patterns are developing?
- What should I focus on financially right now?

## Project Status

**Active Development**

PocketAI currently includes a working **.NET MAUI application**, local **SQLite persistence**, a centralized **C# financial calculation engine**, and a **Python financial reasoning and conversation engine**.

The MAUI project targets:

- Windows
- Android
- iOS
- Mac Catalyst

The Python bridge is currently being developed **Windows-first**, so the financial application is cross-platform in structure while the Python assistant integration still needs additional work before it is fully cross-platform.

---

## Core Features

### Accounts and Income

- Store checking and savings balances
- Track expected monthly income
- Keep current account balances separate from monthly planning calculations
- Protect savings from being treated as normal discretionary spending

### Transactions

- Add and manage expenses
- Categorize transactions
- Track the account used for spending
- View current and historical transaction activity
- Send recent transaction history to the analysis engine for pattern detection

### Budgeting

- Create category-based budget limits
- Compare current spending against each budget
- Detect over-budget categories
- Include budget performance in financial analysis and financial-health calculations

### Recurring Bills

- Store recurring expenses
- Track bill amount, category, due day, and active status
- Track whether recurring bills have been paid for the current month
- Protect recurring obligations in the user's monthly financial plan

### Savings Goals

- Create multiple savings goals
- Set target amounts and deadlines
- Track current progress
- Rank goals by priority
- Mark goals as essential or optional
- Support user-controlled allocation percentages
- Calculate the amount required this month to stay on pace for each goal
- Recommend additional savings without automatically committing the user's money

### Financial Analytics

PocketAI calculates and tracks:

- Current-month spending
- Average daily spending
- Projected additional spending
- Projected month-end spendable cash
- Budget usage
- Over-budget categories
- Required savings
- Upcoming bills
- Safe to Spend
- Daily Safe to Spend
- Weekly Safe to Spend
- Data confidence
- Financial Health Score

The MAUI analytics interface also uses **LiveCharts2** for financial visualizations.

---

## Safe to Spend

One of PocketAI's main calculations is **Safe to Spend**.

The monthly calculation is based on:

**Expected Monthly Income  
- Current Month Spending  
- Upcoming Bills  
- Required Savings  
- Extra Savings Explicitly Accepted by the User**

PocketAI distinguishes between two different financial questions:

**Account balances answer:**  
> Where is my money right now?

**The monthly plan answers:**  
> How much room do I still have to spend this month?

This distinction prevents expected future income from being treated as money that already exists in the user's checking account.

PocketAI also calculates daily and weekly Safe to Spend values from the remaining monthly amount.

---

## Central Financial Snapshot

A major architectural goal of PocketAI is to prevent different pages from calculating the user's finances differently.

The application therefore builds one trusted **FinancialSnapshot**.

The flow is:

~~~text
SQLite Data
    |
    v
FinancialSnapshotProvider
    |
    v
FinancialCalculationService
    |
    v
FinancialSnapshot
    |
    +--> Home
    +--> Analytics
    +--> Budget
    +--> Savings
    +--> PocketAI Assistant
~~~

The **FinancialSnapshotProvider** retrieves current information from SQLite and sends it through the **FinancialCalculationService**.

The service calculates the user's financial state once and returns a FinancialSnapshot containing values such as:

- Checking balance
- Protected savings balance
- Expected monthly income
- Current-month spending
- Monthly plan remaining
- Upcoming bills
- Required savings
- Safe to Spend
- Daily and weekly Safe to Spend
- Projected month-end money
- Budget status
- Data confidence
- Financial Health Score

Pages consume these values rather than creating their own versions of the financial formulas.

---

## Financial Health and Data Confidence

PocketAI does not assume every financial analysis is equally reliable.

The application calculates a **data-confidence level** based on information such as:

- Whether account balances are available
- Whether income has been entered
- How much transaction history exists
- Whether budgets or recurring bills are available

PocketAI currently uses **Low**, **Medium**, and **High** confidence levels.

The application also avoids showing a Financial Health Score when there is not enough financial data to support one.

When sufficient information is available, the Financial Health Score considers factors such as:

- Obligation shortfalls
- Projected month-end cash
- Monthly-plan health
- Budget performance
- Available spendable cash
- Safe to Spend

---

## Python Financial Assistant

PocketAI includes a Python-based reasoning and conversation system.

An important design decision is that **Python does not calculate the user's trusted balances or Safe to Spend values**.

The **C# financial engine remains the source of truth** for financial calculations.

Python receives the trusted financial snapshot and supporting context, then interprets that information to decide what deserves the user's attention.

The current Python system can work with:

- Financial snapshot values
- Category spending
- Budget limits
- Savings goals
- Recurring bills
- Bill-payment status
- Recent transaction history
- Conversation state

### Conversation Understanding

The assistant can recognize questions related to areas such as:

- Affordability
- Purchase impact on savings
- Budgets
- Spending
- Savings
- Bills
- Financial patterns
- Financial health
- Safe-to-Spend explanations
- General financial focus

Conversation state allows follow-up questions to retain context.

For example:

~~~text
User: Can I afford a $300 purchase?

User: What about $200 instead?
~~~

The second question can use information from the first instead of being treated as a completely unrelated request.

---

## Spending Pattern Detection

PocketAI currently sends up to approximately **90 days of transaction history** into the Python analysis layer.

The PatternAnalyzer can look for behavior such as:

- Significant week-over-week spending acceleration
- Category spending growth
- Unusual transactions

Pattern analysis does not modify account balances or financial calculations. It interprets historical behavior and produces insights for the user.

This keeps the system separated into two responsibilities:

~~~text
C# Financial Engine
Deterministic financial calculations
          |
          v
Python Analysis Engine
Interpretation, patterns, conversation, and insights
~~~

---

## Application Architecture

PocketAI is divided into multiple projects so financial logic, persistence, presentation, and analysis can evolve independently.

~~~text
PocketAI
|
+-- PocketAI.App
|   +-- .NET MAUI user interface
|   +-- Pages
|   +-- Navigation
|   +-- Themes
|   +-- Charts
|   +-- AIService bridge
|
+-- PocketAI.Core
|   +-- Models
|   |   +-- AccountBalance
|   |   +-- Expense
|   |   +-- Income
|   |   +-- BudgetLimit
|   |   +-- SavingsGoal
|   |   +-- RecurringExpenses
|   |   +-- FinancialSnapshot
|   |   +-- FinancialSummary
|   |
|   +-- Services
|       +-- AnalyticsService
|       +-- FinancialCalculationService
|       +-- SavingsAllocationService
|
+-- PocketAI.Data
|   +-- DataBaseManager
|   +-- FinancialSnapshotProvider
|
+-- PocketAI.AI
|   +-- Python models
|   +-- Financial reasoning engine
|   +-- Conversation engine
|   +-- Pattern analyzer
|   +-- CLI / process entry point
|
+-- PocketAI
    +-- Original console application / earlier implementation
~~~

---

## Data Storage

PocketAI currently uses **SQLite** for local persistence.

The database stores information including:

- Expenses
- Income
- Account balances
- Savings goals
- Budget limits
- Recurring expenses
- Recurring bill-payment status
- Accepted extra savings
- AI advice history

The database layer also contains compatibility logic so older PocketAI databases can gain newer columns as the application evolves.

---

## C# to Python Integration

The MAUI application communicates with the Python engine through **JSON**.

The high-level flow is:

~~~text
User Financial Data
        |
        v
SQLite
        |
        v
C# FinancialCalculationService
        |
        v
Trusted FinancialSnapshot
        |
        v
AIService.cs
        |
        | JSON through standard input
        v
Python Engine
        |
        | JSON response
        v
C# MAUI Interface
~~~

AIService also passes conversation state and supporting financial context to Python.

The current bridge starts a local Python process using the Windows Python launcher or Python executable and communicates through redirected standard input and output.

---

## User Interface

The current MAUI application contains interfaces for:

- Onboarding
- Home dashboard
- Accounts
- Transactions
- Budgets
- Savings
- Recurring bills
- Analytics
- PocketAI assistant
- Profile
- Settings

PocketAI also includes application theming and responds to system-theme changes.

---

## Technologies Used

- **C#**
- **.NET 10**
- **.NET MAUI**
- **XAML**
- **Python**
- **SQLite**
- **Microsoft.Data.Sqlite**
- **LiveCharts2**
- **SkiaSharp**
- **LINQ**
- **JSON serialization**
- **Object-Oriented Programming**
- **Git**
- **GitHub**

---

## Design Principles

### Deterministic Financial Math

Financial calculations should produce consistent results from the same data. Trusted balances, obligations, savings requirements, and Safe to Spend calculations remain in C# rather than being generated by the conversational layer.

### One Financial Source of Truth

Pages should display values from the central FinancialSnapshot rather than maintaining separate financial formulas.

### User Control

PocketAI can recommend additional savings, but a recommendation does not reduce Safe to Spend until the user accepts it.

### Explainable Financial Logic

Important calculations are designed so they can be explained in understandable terms instead of appearing as unexplained AI output.

### Conservative Analysis

When financial data is incomplete, PocketAI lowers its confidence and avoids presenting unsupported conclusions as highly reliable.

---

## Current Development Focus

Recent development has focused on:

- Connecting the Python engine to live financial data
- Adding conversation context and follow-up-question understanding
- Sending transaction history to the Python engine
- Detecting spending patterns
- Improving financial calculation consistency
- Improving bill-payment tracking
- Strengthening the central FinancialSnapshot architecture

---

## Future Development

Planned improvements include:

- Refactoring more MAUI presentation logic toward MVVM
- Expanding dependency injection and service separation
- Automated unit tests for financial calculations
- Additional integration tests for the C# to Python pipeline
- Improved transaction-pattern analysis
- More advanced conversational capabilities
- Cross-platform Python/AI integration
- Authentication and multiple-user support
- Cloud synchronization
- Secure production-ready financial data storage
- Automatic alerts and notifications
- Additional financial trends and forecasting
- Continued UI and accessibility improvements

---

## Why I Built PocketAI

PocketAI started as a way to practice software development while solving a real problem I was interested in: understanding personal finances in a more useful way than simply viewing a list of transactions.

As the project grew, it became an opportunity to work with:

- Application architecture
- C# and .NET
- Cross-platform UI development
- Relational databases
- Financial modeling
- Data analysis
- Python interoperability
- Debugging across multiple application layers
- Conversational software
- Git and iterative development

The long-term goal is to build PocketAI into an intelligent personal finance assistant that combines reliable financial calculations with understandable, personalized guidance.
