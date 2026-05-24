namespace curs.Models
{
    public class TestResult
    {
        public int Number { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // "Правильно" / "Неправильно"
        public string Background { get; set; } = "Transparent";
    }
}
