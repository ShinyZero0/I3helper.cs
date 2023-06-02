using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace I3Helper;

public class I3Socket
{
    static readonly byte[] _MAGICBYTES = Encoding.UTF8.GetBytes("i3-ipc");
    static readonly int _MAGICBYTECOUNT = _MAGICBYTES.Length;
    static readonly string _I3SOCK = Environment.GetEnvironmentVariable("I3SOCK");
    static readonly UnixDomainSocketEndPoint _endPoint = new(_I3SOCK);
    Socket _socket;

    public bool IsReadable =>
        !(_socket.Available == 0 && _socket.Poll(1000, SelectMode.SelectRead));
    private bool _subscribed = false;
    public bool Subscribed
    {
        get { return _subscribed; }
    }

    public I3Socket()
    {
        _socket = new(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
    }

    public async Task<bool> SubscribeAsync(string type)
    {
        _subscribed = true;
        byte[] message = _makeMessage(MessageType.Subscribe, $"""["{type}"]""");
        await _socket.ConnectAsync(_endPoint);
        await _socket.SendAsync(message);
        Task<string> answerAsync = ReadOutputAsync();
        JsonDocument answerJson = JsonDocument.Parse(await answerAsync);
        return answerJson.RootElement.GetProperty("success").GetBoolean();
    }

    public async Task<string> ReadOutputAsync()
    {
        byte[] buffer = new byte[8_192];
        Task<int> received = _socket.ReceiveAsync(buffer);
        // length of magic string + length of command int + length of length
        int toSkip = _MAGICBYTECOUNT + 4 + 4;
        string response = Encoding.UTF8.GetString(buffer, toSkip, await received - toSkip);
        // await Logger.Log("response got");
        return response;
    }

    private byte[] _makeMessage(MessageType type, string message)
    {
        return new List<byte>()
            .Concat(_MAGICBYTES)
            .Concat(BitConverter.GetBytes(Encoding.UTF8.GetByteCount(message)))
            .Concat(BitConverter.GetBytes((int)type))
            .Concat(Encoding.UTF8.GetBytes(message))
            .ToArray();
    }

    public enum MessageType
    {
        RunCommand = 0,
        GetWorkspaces = 1,
        Subscribe = 2
    }
}
