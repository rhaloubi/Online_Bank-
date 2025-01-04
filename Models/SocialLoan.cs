using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineBank.Models
{
    public class SocialLoan 
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LoanID { get; set; }
        public int Duration { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; } // Requested, Active, Completed, rejected
        public DateTime? Date { get; set; }
        public decimal? Amount { get; set; }

        // receiver
        [ForeignKey("Account1")]
        public int? Account1ID { get; set; }
        public Account? Account1 { get; set; }

        // sender
        [ForeignKey("Account2")]
        public int? Account2ID { get; set; }
        public Account? Account2 { get; set; }

    }
}

