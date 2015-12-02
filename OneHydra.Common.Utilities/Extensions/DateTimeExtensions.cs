using System;
using NCrontab;

namespace OneHydra.Common.Utilities.Extensions
{
    public static class DateTimeExtensions
    {
        public static bool MatchesCronExpression(this DateTime theDateTime, string cronExpression)
        {
            CrontabSchedule schedule;
            ExceptionProvider ex;
            var valueOrException = CrontabSchedule.TryParse(cronExpression,
                input =>
                {
                    schedule = input;
                    return true;
                },
                exception =>
                {
                    ex = exception;
                    ex.Invoke();
                    return false;
                });
            

            var cronSchedule = CrontabSchedule.Parse(cronExpression);
            var nextOccurrence = cronSchedule.GetNextOccurrence(theDateTime.AddMinutes(-1).AddSeconds(-1 * theDateTime.Second));
            var matches = Math.Abs((theDateTime.AddSeconds(-1*theDateTime.Second) - nextOccurrence).TotalMinutes) < 1 ;
            return matches;
        }

        public static string ForSql(this DateTime? theDateTime)
        {
            return !theDateTime.HasValue ? "NULL" : ((DateTime)theDateTime).ForSql();
        }

        public static string ForSql(this DateTime theDateTime)
        {
            if (theDateTime == DateTime.MinValue)
            {
                return "NULL";
            }
            return "'" + theDateTime + "'";
        }
    }
}