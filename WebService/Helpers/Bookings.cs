using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebService
{
    public class PeriodTime
        : IComparable
    {
        public static List<DateTime> Periods { get; set; }

        public bool IsPeriod { get; set; }
        public DateTime Slot { get; set; }

        public PeriodTime()
        {
            IsPeriod = false;
            Slot = new DateTime();
        }

        public int CompareTo(object obj)
        {
            if (!(obj is PeriodTime))
                throw new ArgumentException();

            PeriodTime Arg = (PeriodTime)obj;

            if (!Arg.IsPeriod && this.IsPeriod) // If this is an actual period and it's not, order this first
                return 1;
            else if (Arg.IsPeriod && !this.IsPeriod) // If this isn't an actual period and it is, order it first
                return -1;
            else
                return Arg.Slot.CompareTo(this.Slot); // Compare the times
        }
    }
}