using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Central AI director that communicates with the MCP server for narrative and mission updates.
/// </summary>
public class AIDirector : MonoBehaviour
{
    public static AIDirector Instance { get; private set; }

    private ClientWebSocket socket;

    public event Action<string> OnMissionEvent;
    public event Action<string> OnNarrativeUpdate;

    private async void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        await Connect();
    }

    private async Task Connect()
    {
        socket = new ClientWebSocket();
        var uri = new Uri("ws://localhost:8001/McpUnity");
        await socket.ConnectAsync(uri, CancellationToken.None);
        _ = ReceiveLoop();
    }

    private async Task ReceiveLoop()
    {
        var buffer = new byte[4096];
        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Text)
            {
                string msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                HandleMessage(msg);
            }
        }
    }

    private void HandleMessage(string message)
    {
        if (message.Contains("\"mission_event\""))
        {
            OnMissionEvent?.Invoke(message);
        }
        else if (message.Contains("\"narrative_update\""))
        {
            OnNarrativeUpdate?.Invoke(message);
        }
        else
        {
            Debug.Log($"Unhandled message: {message}");
        }
    }

    public async void RequestMission(string crewId)
    {
        if (socket == null || socket.State != WebSocketState.Open) return;
        string request = $"{{\"method\":\"request_mission\",\"params\":{{\"crewId\":\"{crewId}\"}}}}";
        var bytes = Encoding.UTF8.GetBytes(request);
        await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    public async void RequestNarrativeEvent(string context)
    {
        if (socket == null || socket.State != WebSocketState.Open) return;
        string request = $"{{\"method\":\"request_narrative\",\"params\":{{\"context\":\"{context}\"}}}}";
        var bytes = Encoding.UTF8.GetBytes(request);
        await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private async void OnDestroy()
    {
        if (socket != null && socket.State == WebSocketState.Open)
        {
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
        }
    }
}

