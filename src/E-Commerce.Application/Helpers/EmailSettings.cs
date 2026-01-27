namespace E_Commerce.Domain.Helpers
{
    public class EmailSettings
    {
        public int Port { get; set; }
        public string? Host { get; set; }
        public string? FromEmail { get; set; }
        public string? Password { get; set; }
        public string? UserName { get; set; }
        public bool UseSSL { get; set; }
    }
}
