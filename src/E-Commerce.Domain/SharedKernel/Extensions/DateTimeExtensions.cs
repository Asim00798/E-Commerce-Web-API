
namespace E_Commerce.Domain.SharedKernel.Extensions
{
    public static class DateTimeExtensions
    {
        public static bool IsBusinessDay(this DateTime date) => date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
    }
}
