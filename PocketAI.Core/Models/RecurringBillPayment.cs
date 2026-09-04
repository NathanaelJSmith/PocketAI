public class RecurringBillPayment
{
    public int Id {get; set;}
    public int RecurringExpensesId {get; set;}

    public string Monthkey {get; set;}
    public bool IsPaid { get; set; }

    public DateTime? DatePaid { get; set;}


    public RecurringBillPayment(int id, int recurringExpenseId, string monthkey, bool isPaid, DateTime? datePaid)
    {
        Id = id;
        RecurringExpensesId = recurringExpenseId;
        Monthkey = monthkey;
        IsPaid = isPaid;
        DatePaid = datePaid;
    }
}