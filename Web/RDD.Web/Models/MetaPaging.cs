namespace RDD.Web.Models
{
    public class MetaPaging
    { 
        public int Count { get; set; } 
        public int Offset { get; set; } 
        public string previous { get; set; } 
        public string next { get; set; }
    }
}