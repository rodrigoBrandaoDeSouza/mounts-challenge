namespace Ambev.DeveloperEvaluation.WebApi.Common;

/// <summary>
/// Error response format of the API.
/// </summary>
/// <param name="Type">Machine-readable error type identifier (e.g. ValidationError, ResourceNotFound).</param>
/// <param name="Error">Short, human-readable summary of the problem.</param>
/// <param name="Detail">Human-readable explanation specific to this occurrence of the problem.</param>
public sealed record ErrorResponse(string Type, string Error, string Detail);
