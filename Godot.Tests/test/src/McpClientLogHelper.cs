public partial class McpClient {
    public string? LastLoggedError { get; private set; }
    partial void LogError(string message) => LastLoggedError = message;
}
