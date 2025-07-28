using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public partial class McpClient {
    public async Task<JsonElement> SendWebSocketCommand(string item) {
        var uri = new Uri(Endpoint);
        if (!uri.Scheme.StartsWith("ws"))
            uri = new Uri($"ws://{uri.Host}:{uri.Port}/");
        using var ws = new ClientWebSocket();
        await ws.ConnectAsync(uri, CancellationToken.None);
        var id = Guid.NewGuid().ToString();
        var payload = JsonSerializer.Serialize(new {
            type = "execute_menu_item",
            id,
            parameters = new { item }
        });
        var bytes = Encoding.UTF8.GetBytes(payload);
        await ws.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
        var buffer = new byte[1024];
        var result = await ws.ReceiveAsync(buffer, CancellationToken.None);
        var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
        return JsonDocument.Parse(json).RootElement.Clone();
    }
}
