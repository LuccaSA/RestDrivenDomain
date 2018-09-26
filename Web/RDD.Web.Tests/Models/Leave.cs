using System;

namespace RDD.Web.Tests.Models
{
    public class Leave
    {
        public bool IsAM { get; set; }
        public DateTime Date { get; set; }
        public string Reason { get; set; }
        public Owner Owner { get; set; }
        public Account LeaveAccount { get; set; }
    }
}