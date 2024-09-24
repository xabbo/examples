using Xabbo.GEarth;
using Xabbo.Messages;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.GameData;
using Xabbo.Core.Messages.Incoming;
using Xabbo.Core.Messages.Outgoing;
using Xabbo.Core.Events;

namespace Xabbo.Examples.Extended;

[Extension(
    Name = "Example",
    Description = "A xabbo/gearth example",
    Author = "b7",
    Version = "1.0"
)]
partial class MyExtension : GEarthExtension
{
    private readonly RoomManager _roomManager;
    private readonly GameDataManager _gameData;

    public MyExtension()
    {
        _roomManager = new RoomManager(this);
        _gameData = new GameDataManager();

        // ----- Game state manager events -----
        // Game state managers such as the room manager do all the work
        // of intercepting, parsing and interpreting packets.
        // They allow you to subscribe to high level events without
        // needing to process packets or messages yourself.

        // AvatarsAdded is called when avatars (Users, Bots, Pets) are added to the room.
        _roomManager.AvatarsAdded += OnAvatarsAdded;
        // AvatarRemoved is called when an avatar is removed from the room.
        _roomManager.AvatarRemoved += OnAvatarRemoved;
        // AvatarChat is called when an a chat message is received.
        _roomManager.AvatarChat += OnAvatarChat;
    }

    protected override void OnInitialized(InitializedArgs e)
    {
        base.OnInitialized(e);

        Console.WriteLine($"Extension intialized. (connected:{e.IsGameConnected})");
    }

    protected override void OnConnected(GameConnectedArgs e)
    {
        base.OnConnected(e);

        Console.WriteLine($"Game connected.");
        Console.WriteLine("");
        Console.WriteLine($"    Host: {e.Host}:{e.Port}");
        Console.WriteLine($"   Hotel: {e.Session.Hotel.Name}");
        Console.WriteLine($"  Client: {e.Session.Client.Identifier} {e.Session.Client.Version}");
        Console.WriteLine("");

        Task.Run(LoadGameDataAsync);
    }

    protected override void OnDisconnected()
    {
        base.OnDisconnected();

        Console.WriteLine("Game disconnected.");
    }

    // Activated is called when the user activates the extension in G-Earth
    // by clicking the green play button next to the extension.
    protected override void OnActivated()
    {
        base.OnActivated();

        Console.WriteLine($"Extension activated.");

        // ----- Request messages -----
        // Request messages allow you to send a request and asynchronously receive its response.
        // These are messages such as "GetUserDataMsg" and "GetRoomDataMsg".

        // Run a task so we don't block the extension processing loop.
        Task.Run(PrintUserDataAsync);
    }

    async Task PrintUserDataAsync()
    {
        try
        {
            Console.WriteLine("Requesting user data...");
            var userData = await RequestAsync(new GetUserDataMsg());
            Console.WriteLine("Received user data.");
            Console.WriteLine($"Your ID is: {userData.Id}. Your name is: '{userData.Name}'.");
        }
        catch (TimeoutException)
        {
            Console.WriteLine("The request timed out.");
        }
    }

    // ----- Intercepting packets -----
    // Intercepts the incoming "Ping" packet.
    [InterceptIn("Ping")]
    void HandlePing(Intercept e)
    {
        Console.WriteLine("Received ping.");
    }

    // The following code works cross-client (on both Flash and Shockwave)
    // by utilizing messages instead of working with packets directly.

    // ----- Intercepting messages -----
    // Intercept incoming chat messages by specifying the message type as the method argument.
    [Intercept]
    void HandlePingPong(AvatarChatMsg chat)
    {
        // If any incoming chat message contains "ping":
        if (chat.Message.Contains("ping"))
            // Respond by shouting "pong".
            Send(new ShoutMsg("pong"));
    }

    // ----- Blocking messages -----
    // Block avatar chat messages if they contain the word "block"
    // by accepting 2 arguments: Intercept and AvatarChatMsg.
    [Intercept]
    void BlockChatMessages(Intercept e, AvatarChatMsg chat)
    {
        // Block avatar chat messages if they contain the word "block".
        if (chat.Message.Contains("block"))
            e.Block();
    }

    // ----- Modifying messages ------
    // Replace "apple" with "orange" in incoming chat messages
    // by returning a modified instance of AvatarChatMsg.
    [Intercept]
    IMessage? ModifyChatMessages(AvatarChatMsg chat)
    {
        return chat with {
            Message = chat.Message.Replace("apple", "orange")
        };
    }

    // ----- Room manager -----
    private void OnAvatarsAdded(AvatarsEventArgs e)
    {
        if (e.Avatars.Length == 1)
            Console.WriteLine($"{e.Avatars[0].Name} entered the room.");
    }

    private void OnAvatarRemoved(AvatarEventArgs e)
    {
        Console.WriteLine($"{e.Avatar.Name} left the room.");
    }

    private void OnAvatarChat(AvatarChatEventArgs e)
    {
        Console.WriteLine($"{e.Avatar.Name}: {e.Message}");
    }

    // ----- Game data management -----
    // The game data manager lets you easily load resources
    // such as furni data for the current hotel.
    private async Task LoadGameDataAsync()
    {
        try
        {
            Console.WriteLine($"Loading game data for hotel: {Session.Hotel}.");
            await _gameData.LoadAsync(Session.Hotel);
            Console.WriteLine($"Loaded {_gameData.Furni?.Count} furni");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load game data: {ex.Message}");
        }
    }


    // ----- Simple command manager -----
    // Intercept outgoing chat messages, handling commands starting with a '/'.
    [Intercept]
    void HandleChat(Intercept<ChatMsg> e)
    {
        // The ChatMsg can be accessed via e.Msg.
        var chat = e.Msg;
        // If the message begins with '/':
        if (chat.Message.StartsWith('/'))
        {
            // Block the packet.
            e.Block();
            // Parse the command and args and pass it to the command handler.
            string[] split = chat.Message[1..].Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (split.Length == 0) return;
            HandleCommand(split[0], split[1..]);
        }
    }

    void HandleCommand(string cmd, string[] args)
    {
        switch (cmd.ToLowerInvariant())
        {
        case "wave": // Usage: /wave
            // Only Wave is supported on the Shockwave client.
            // On Shockwave, this will throw an exception if the action is not Wave.
            Send(new ActionMsg(Actions.Wave));
            break;
        case "walk": // Usage: /walk x y
            // Check if we have two arguments for the x, y coordinates.
            if (args.Length < 2) return;
            // Parse the coordinates.
            if (!int.TryParse(args[0], out int x)) return;
            if (!int.TryParse(args[1], out int y)) return;
            Console.WriteLine($"Walking to {x}, {y}");
            // Send a walk message to (x, y).
            Send(new WalkMsg(x, y));
            break;
        }
    }
}
