namespace Ambev.DeveloperEvaluation.Common.Security
{
    /// <summary>
    /// Contract of an authenticated user, used to generate the JWT.
    /// </summary>
    public interface IUser
    {
        /// <summary>Identifier of the user.</summary>
        string Id { get; }

        /// <summary>Username.</summary>
        string Username { get; }

        /// <summary>Role of the user.</summary>
        string Role { get; }
    }
}
