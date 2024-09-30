using Xabbo;
using Xabbo.GEarth;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.GameData;
using Xabbo.Core.Messages.Incoming;
using Xabbo.Core.Messages.Outgoing;
using Xabbo.Messages.Flash;

// Create the extension, providing the extension information.
var ext = new GEarthExtension(new GEarthOptions {
    Name = "Example",
    Description = "A xabbo/gearth example",
    Author = "b7",
    Version = "1.0"
});
var roomManager = new RoomManager(ext);
var gameDataManager = new GameDataManager();

// Register event handlers.
ext.Initialized += (e) => Console.WriteLine($"Extension initialized. (connected:{e.IsGameConnected})");
ext.Connected += (e) => Console.WriteLine($"Game connected. ({e.Host}:{e.Port})");
ext.Disconnected += () => Console.WriteLine("Game disconnected!");

// ----- Intercepting packets -----
// Intercepts the incoming "Ping" packet.
ext.Intercept(In.Ping, e => {
    Console.WriteLine("Received ping.");
});

// The following code works cross-client (on both Flash and Shockwave)
// by utilizing message classes instead of working with packets directly.

// ----- Intercepting messages -----
// Intercept incoming chat messages by specifying the message type as a generic type argument.
ext.Intercept<AvatarChatMsg>(chat => {
    // If any incoming chat message contains "ping":
    if (chat.Message.Contains("ping"))
        // Respond by shouting "pong".
        ext.Send(new ShoutMsg("pong"));
});

// ----- Blocking messages -----
// Block avatar chat messages if they contain the word "block"
// by accepting 2 arguments: Intercept and AvatarChatMsg.
ext.Intercept<AvatarChatMsg>((e, chat) => {
    if (chat.Message.Contains("block"))
        e.Block();
});

// ----- Modifying messages ------
// Replace "apple" with "orange" in incoming chat messages
// by returning a modified instance of AvatarChatMsg.
ext.Intercept<AvatarChatMsg>(chat => chat with {
    Message = chat.Message.Replace("apple", "orange")
});

// ----- Request messages -----
// Request messages allow you to send a request and asynchronously receive its response.
// These are messages such as "GetUserDataMsg" and "GetRoomDataMsg".

// Activated is called when the user activates the extension by
// clicking the green play button in G-Earth.
ext.Activated += () => {
    // Run a task so we don't block the extension processing loop.
    Task.Run(async () => {
        try
        {
            Console.WriteLine("Requesting user data...");
            var userData = await ext.RequestAsync(new GetUserDataMsg());
            Console.WriteLine("Received user data.");
            Console.WriteLine($"Your ID is: {userData.Id}. Your name is: '{userData.Name}'.");
        }
        catch (TimeoutException)
        {
            Console.WriteLine("The request timed out.");
        }
    });
};

// ----- Game state manager events -----
// Game state managers such as the room manager do all the work
// of intercepting, parsing and interpreting packets.
// They allow you to subscribe to high level events without
// needing to process packets or messages yourself.

// AvatarsAdded is called when avatars (Users, Bots, Pets) are added to the room.
roomManager.AvatarsAdded += (e) => {
    if (e.Avatars.Length == 1)
        Console.WriteLine($"{e.Avatars[0].Name} entered the room.");
};
// AvatarRemoved is called when an avatar is removed from the room.
roomManager.AvatarRemoved += (e) => Console.WriteLine($"{e.Avatar.Name} left the room.");
// AvatarChat is called when an a chat message is received.
roomManager.AvatarChat += (e) => Console.WriteLine($"{e.Avatar.Name}: {e.Message}");

// ----- Game data management -----
// The game data manager lets you easily load resources
// such as furni data for the current hotel.
gameDataManager.Loaded += () => Console.WriteLine($"Loaded {gameDataManager.Furni?.Count} furni");
ext.Connected += (e) => {
    // Run an asynchronous task so we don't block the extension processing loop.
    Task.Run(async () => {
        try
        {
            Console.WriteLine($"Loading game data for hotel: {e.Session.Hotel}.");
            await gameDataManager.LoadAsync(e.Session.Hotel);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load game data: {ex.Message}");
        }
    });
};

// ----- Simple command manager -----
// Intercept outgoing chat messages, handling commands starting with a '/'.
ext.Intercept<ChatMsg>((e, chat) => {
    // If the message begins with '/':
    if (chat.Message.StartsWith('/'))
    {
        // Block the packet.
        e.Block();
        // Parse the command and args and pass it to the command handler.
        string[] fields = chat.Message[1..].Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (fields.Length == 0) return;
        var (cmd, args) = (fields[0], fields[1..]);

        switch (cmd.ToLowerInvariant())
        {
            case "wave": // Usage: /wave
                // Only Wave is supported on the Shockwave client.
                // On Shockwave, this will throw an exception if the action is not Wave.
                ext.Send(new ActionMsg(AvatarAction.Wave));
                break;
            case "walk": // Usage: /walk x y
                // Check if we have two arguments for the x, y coordinates.
                if (args.Length < 2) return;
                // Parse the coordinates.
                if (!int.TryParse(args[0], out int x)) return;
                if (!int.TryParse(args[1], out int y)) return;
                Console.WriteLine($"Walking to {x}, {y}");
                // Send a walk message to (x, y).
                ext.Send(new WalkMsg(x, y));
                break;
        }
    }
});

// Run the extension.
await ext.RunAsync();
