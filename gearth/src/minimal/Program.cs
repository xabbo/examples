using Xabbo;
using Xabbo.GEarth;

using Xabbo.Core;
using Xabbo.Core.Messages.Incoming;
using Xabbo.Core.Messages.Outgoing;

using Xabbo.Messages.Flash;
using Xabbo.Core.Game;
using Xabbo.Core.GameData;

// Create the extension, providing the extension information.
var ext = new GEarthExtension(new GEarthOptions {
    Name = "Example",
    Description = "A xabbo/gearth example",
    Author = "b7",
    Version = "1.0"
});

// Register event handlers.
ext.Initialized += (e) => Console.WriteLine($"Extension initialized. (connected:{e.IsGameConnected})");
ext.Connected += (e) => Console.WriteLine($"Game connected. ({e.Host}:{e.Port})");
ext.Disconnected += () => Console.WriteLine("Game disconnected!");

// The following code works cross-client (on both Flash and Shockwave)
// by utilizing Message classes instead of working with packets directly.

// Intercept incoming chat messages.
ext.Intercept<AvatarChatMsg>(chat => {
    // If any incoming chat message contains "ping":
    if (chat.Message.Contains("ping"))
        // Respond by shouting "pong".
        ext.Send(new ShoutMsg("pong"));
});

// Block avatar chat messages if they contain the word "block".
ext.Intercept<AvatarChatMsg>((e, chat) => {
    if (chat.Message.Contains("block"))
        e.Block();
});

// Replace "apple" with "orange" in incoming chat messages.
ext.Intercept<AvatarChatMsg>(chat => chat with {
    Message = chat.Message.Replace("apple", "orange")
});

var roomManager = new RoomManager(ext);
roomManager.AvatarsAdded += (e) => {
    if (e.Avatars.Length == 1)
        Console.WriteLine($"{e.Avatars[0].Name} entered the room.");
};
roomManager.AvatarRemoved += (e) => Console.WriteLine($"{e.Avatar.Name} left the room.");
roomManager.AvatarChat += (e) => Console.WriteLine($"{e.Avatar.Name}: {e.Message}");

var gameDataManager = new GameDataManager();
gameDataManager.Loaded += () => Console.WriteLine($"Loaded {gameDataManager.Furni?.Count} furni");
ext.Connected += (e) => {
    Task.Run(async () => {
        try
        {
            Console.WriteLine($"Loading game data for hotel: {e.Session.Hotel}.");
            await gameDataManager.LoadAsync(e.Session.Hotel);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load game data: {ex}");
        }
    });
};

// Intercept outgoing chat messages. Because we need to block the message,
// we need to bring in the Intercept instance by accepting 2 arguments: (e, chat).
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
            case "wave":
                // Only Wave is supported on the Shockwave client.
                // On Shockwave, this will throw an exception if the action is not Wave.
                ext.Send(new ActionMsg(Actions.Wave));
                break;
            case "walk":
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
