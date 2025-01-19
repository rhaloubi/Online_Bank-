using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OnlineBank_FE.Models
{
    public class Transaction
    {
        [JsonPropertyName("transactionID")]
        public int TransactionID { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("account1ID")]
        public int Account1ID { get; set; }

        [JsonPropertyName("account2ID")]
        public int? Account2ID { get; set; }
    }
}


