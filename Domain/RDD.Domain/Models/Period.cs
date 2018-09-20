using System;

namespace RDD.Domain.Models
{
    public struct Period
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public Period(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }
    }
}