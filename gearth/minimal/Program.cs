using Xabbo;
using Xabbo.GEarth;

using Xabbo.Core;
using Xabbo.Core.Messages.Incoming;
using Xabbo.Core.Messages.Outgoing;

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
ext.Intercept<EntityChatMsg>(chat => {
    // If any incoming chat message contains "ping":
    if (chat.Message.Contains("ping"))
        // Respond by shouting "pong".
        ext.Send(new ShoutMsg("pong"));
});

// Intercept outgoing chat messages. Because we need to block the message,
// we need to bring in the Intercept instance by accepting 2 arguments: (e, chat).
ext.Intercept<ChatMsg>((e, chat) => {
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
});

// Run the extension.
await ext.RunAsync();

void HandleCommand(string cmd, string[] args)
{
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
