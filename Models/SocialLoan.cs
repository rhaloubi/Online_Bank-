using System;
namespace OnlineBank.Models
{
    public class SocialLoan : Loan
    {
        public int LenderID { get; set; }
        public int BorrowerID { get; set; }
        public string RepaymentSchedule { get; set; }
        public string Reason { get; set; }
    }
}

