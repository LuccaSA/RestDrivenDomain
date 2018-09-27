using System;

namespace RDD.Domain.Models
{
    public struct Period
    {
        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }

        public Period(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }
    }
}