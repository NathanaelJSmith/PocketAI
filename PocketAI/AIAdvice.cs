public class AIAdvice
{
    public int Id { get; set; }
    public string Prompt { get; set; }

    public string AdviceText { get; set; }

    public DateTime DateCreated { get; set; }

    public AIAdvice(int id, string prompt, string adviceText, DateTime dateCreated)
    {
        Id = id;
        Prompt = prompt;
        AdviceText = adviceText;
        DateCreated = dateCreated;
    }
}