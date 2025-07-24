using System.IO;
using System.Net;
using System.Threading.Tasks;
using Godot;
using Chickensoft.GoDotTest;
using Shouldly;

public class McpClientTests : TestClass {
    public McpClientTests(Node scene) : base(scene) { }

    [Test]
    public async Task SendCommand_PostsPayload() {
        int port = GetFreePort();
        string url = $"http://localhost:{port}/";
        using var listener = new HttpListener();
        listener.Prefixes.Add(url);
        listener.Start();

        var client = new McpClient { Endpoint = url };
        TestScene.AddChild(client);
        var sendTask = client.SendCommand("Test/Path");

        var context = await listener.GetContextAsync();
        string body;
        using (var reader = new StreamReader(context.Request.InputStream))
            body = await reader.ReadToEndAsync();
        context.Response.StatusCode = 200;
        context.Response.Close();
        listener.Stop();

        await sendTask;
        body.ShouldContain("\"menuPath\":\"Test/Path\"");
    }

    [Test]
    public async Task SendCommand_LogsErrorOnFailure() {
        int port = GetFreePort();
        string url = $"http://localhost:{port}/";
        using var listener = new HttpListener();
        listener.Prefixes.Add(url);
        listener.Start();

        var client = new McpClient { Endpoint = url };
        TestScene.AddChild(client);
        var sendTask = client.SendCommand("Test/Fail");

        var context = await listener.GetContextAsync();
        using (var reader = new StreamReader(context.Request.InputStream))
            await reader.ReadToEndAsync();
        context.Response.StatusCode = 500;
        using var writer = new StreamWriter(context.Response.OutputStream);
        writer.Write("fail");
        writer.Flush();
        context.Response.Close();
        listener.Stop();

        await sendTask;
        client.LastLoggedError.ShouldContain("500");
        client.LastLoggedError.ShouldContain("fail");
    }

    private static int GetFreePort() {
        var l = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        l.Start();
        int p = ((System.Net.IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        return p;
    }
}
