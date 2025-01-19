using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineBank_FE.Models
{
	public class Account
	{
		
        public int AccountID { get; set; }

        public decimal Balance { get; set; }
        public string? AccountType { get; set; }

        // Relationship with Client
        [ForeignKey("Client")]
        public int ClientID { get; set; }
        public Client? Client { get; set; }
    
	}
}

