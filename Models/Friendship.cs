using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineBank.Models
{
    public class Friendship
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ? FriendshipID { get; set; }

        // ID of the client who sent the request
        [ForeignKey("Client1")]
        public int ? Friend1ID { get; set; }
        public Client ? Client1 { get; set; }

        // ID of the client who received the request
        [ForeignKey("Client2")]
        public int ? Friend2ID { get; set; }
        public Client ? Client2 { get; set; }

        public string ? Status { get; set; } // Pending, Accepted
    }
}
