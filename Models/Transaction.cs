using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace OnlineBank.Models
{
   
    public class Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionID { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }

        // sender
        [ForeignKey("Account1")]
        public int? Account1ID { get; set; }
        public Account? Account1 { get; set; }

        // receiver
        [ForeignKey("Account2")]
        public int? Account2ID { get; set; }
        public Account? Account2 { get; set; }
    }
}

