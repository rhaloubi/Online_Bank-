using System;
namespace OnlineBank.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Loan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LoanID { get; set; }
        public decimal Amount { get; set; }
        public float InterestRate { get; set; }
        public int Duration { get; set; }
        public string Status { get; set; } // Requested, Active, Completed
    }
}

