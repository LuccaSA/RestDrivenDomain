using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra
{
	public static class DateTimeExtensions
	{
		// eg: 2010-10-21T18:38:35 => no bad surprise when parsing
		public static String ToISO(this DateTime d)
		{
			return d.ToString("s");
		}
		public static String ToISOz(this DateTime d)
		{
			return d.ToString("o");
		}
		public static DateTime ToMidnightTimeIfEmpty(this DateTime date)
		{
			if (date.TimeOfDay.Ticks == 0)
			{
				return date.AddDays(1).AddMilliseconds(-1);
			}
			else
			{
				return date.AddDays(0);
			}
		}
		public static DateTime LastMonday(this DateTime d)
		{
			DateTime d_ = d.Date;
			while (d_.DayOfWeek != DayOfWeek.Monday)
			{
				d_ = d_.AddDays(-1);
			}
			return d_;
		}
		public static DateTime NextMonday(this DateTime d)
		{
			DateTime d_ = d.Date;
			while (d_.DayOfWeek != DayOfWeek.Monday)
			{
				d_ = d_.AddDays(1);
			}
			return d_;
		}
		public static DateTime NextSunday(this DateTime d)
		{
			DateTime d_ = d.Date;
			while (d_.DayOfWeek != DayOfWeek.Sunday)
			{
				d_ = d_.AddDays(+1);
			}
			return d_;
		}
	}
}
