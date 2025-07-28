public partial class McpClient {
    public string? LastLoggedError { get; private set; }
    partial void LogErrorExtra(string message) => LastLoggedError = message;
}
