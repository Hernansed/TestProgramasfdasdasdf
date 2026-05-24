namespace curs.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = "user";
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
