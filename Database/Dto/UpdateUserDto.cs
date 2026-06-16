namespace Database.Dto
{
    public class UpdateUserDto
    {
        public string? Login { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public int? TypeId { get; set; }
    }
}
