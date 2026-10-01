namespace Ambev.DeveloperEvaluation.Application.Models
{
    public class OperationResult<T>
    {
        /// <summary>
        /// Indicates whether the operation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Provides a message describing the result.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Contains the data payload for successful operations.
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Creates a successful result with no data.
        /// </summary>
        public static OperationResult<T> Ok(string message = "Operation successful")
            => new() { Success = true, Message = message };

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        public static OperationResult<T> Ok(T data, string message = "Operation successful")
            => new() { Success = true, Message = message, Data = data };

        /// <summary>
        /// Creates a failed result.
        /// </summary>
        public static OperationResult<T> Fail(string message)
            => new() { Success = false, Message = message };
    }
}
