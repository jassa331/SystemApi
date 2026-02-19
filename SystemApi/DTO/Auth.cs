namespace SystemApi.DTO
{
    public class loginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }

    }

    public class RegisterRequestDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
    }
    public class SimpleUser
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
