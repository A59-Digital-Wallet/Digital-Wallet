using System;
using Wallet.Data.Models.Enums;

namespace Wallet.Services.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime AddInterval(this DateTime dateTime, RecurrenceInterval? interval)
        {
            return interval switch
            {
                RecurrenceInterval.Daily => dateTime.AddSeconds(30),
                RecurrenceInterval.Weekly => dateTime.AddDays(7),
                RecurrenceInterval.Monthly => dateTime.AddMonths(1),
                RecurrenceInterval.Yearly => dateTime.AddYears(1),
                _ => throw new ArgumentOutOfRangeException(nameof(interval), interval, null),
            };
        }
    }
}
