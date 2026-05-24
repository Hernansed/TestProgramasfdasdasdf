namespace curs.Models
{
    public class Question
    {
        public string Text { get; set; } = string.Empty;
        public string[] Answers { get; set; } = System.Array.Empty<string>();
    }
}
