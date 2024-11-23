using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace OnlineBank.Models
{
    public class Admin 
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AdminID { get; set; }
        public string ? Name { get; set; } 
        public string ? Email { get; set; }
        public string ? Password { get; set; } 
        public string ? Role { get; set; }
        public string ? Permissions { get; set; }
    }
}

