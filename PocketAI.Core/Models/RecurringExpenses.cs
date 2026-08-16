public class RecurringExpenses
{
   public int Id { get; set; }

    public string Name { get; set; }

    public string Category { get; set; }

    public double Amount { get; set; }

    public int DueDay { get; set; }

    public bool IsActive { get; set; }

    public RecurringExpenses(int id, string name, string category, double amount, int dueDay, bool isActive)
    {
        Id = id;
        Name = name;
        Category = category;
        Amount = amount;
        DueDay = dueDay;
        IsActive = isActive;
    }
}
