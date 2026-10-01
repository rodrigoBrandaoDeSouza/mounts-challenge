namespace Ambev.DeveloperEvaluation.Application.Common
{
    /// <summary>
    /// Helpers to normalize dates before persisting them as "timestamp with time zone" (PostgreSQL).
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Converts the value to UTC. Values with <see cref="DateTimeKind.Unspecified"/> are assumed to already be UTC.
        /// </summary>
        public static DateTime ToUtc(this DateTime value) =>
            value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
            };
    }
}
