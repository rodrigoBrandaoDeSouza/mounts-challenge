using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

namespace Ambev.DeveloperEvaluation.Common.Security
{
    [ExcludeFromCodeCoverage]
    public static class AuthenticationExtension
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        /// <summary>
        /// Registers JWT bearer authentication. 401/403 responses follow the API error format
        /// (<c>{ "type", "error", "detail" }</c>).
        /// </summary>
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var secretKey = configuration["Jwt:SecretKey"];
            ArgumentException.ThrowIfNullOrWhiteSpace(secretKey);

            var key = Encoding.ASCII.GetBytes(secretKey);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };
                x.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        await WriteErrorAsync(
                            context.Response,
                            StatusCodes.Status401Unauthorized,
                            "AuthenticationError",
                            "Invalid authentication token",
                            "The authentication token is missing, has expired or is invalid");
                    },
                    OnForbidden = context => WriteErrorAsync(
                        context.Response,
                        StatusCodes.Status403Forbidden,
                        "AuthorizationError",
                        "Access denied",
                        "You do not have permission to access this resource")
                };
            });

            services.AddAuthorization();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            return services;
        }

        private static Task WriteErrorAsync(HttpResponse response, int statusCode, string type, string error, string detail)
        {
            response.StatusCode = statusCode;
            response.ContentType = "application/json";
            return response.WriteAsync(JsonSerializer.Serialize(new { type, error, detail }, JsonOptions));
        }
    }
}
