using System;
namespace OnlineBank_FE.Models
{
	public class Client
	{
        public int ClientID { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
    }
}

