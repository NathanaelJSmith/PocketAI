using System;

// Represents one expense the user adds
class Expense
{
    public int Id { get; set; }
    // Stores the expense name
    public string Name { get; set; }

    // Stores the expense amount
    public double Amount { get; set; }

    // Stores the expense category
    public string Category { get; set; }

    // Stores the expense date
    public DateTime Date { get; set; }

    // Builds a new expense object
    public Expense(int id, string name, double amount, string category, DateTime date)
    {
        Id = id;
        Name = name;
        Amount = amount;
        Category = category;
        Date = date;
    }
}
