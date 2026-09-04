public class RecurringBillPayment
{
    public int Id { get; set; }

    public int RecurringExpenseId { get; set; }

    public string MonthKey { get; set; }

    public bool IsPaid { get; set; }

    public DateTime? DatePaid { get; set; }


    public RecurringBillPayment(
        int id,
        int recurringExpenseId,
        string monthKey,
        bool isPaid,
        DateTime? datePaid)
    {
        Id =
            id;

        RecurringExpenseId =
            recurringExpenseId;

        MonthKey =
            monthKey;

        IsPaid =
            isPaid;

        DatePaid =
            datePaid;
    }
}