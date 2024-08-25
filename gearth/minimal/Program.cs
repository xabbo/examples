using Xabbo;
using Xabbo.GEarth;
using Xabbo.Messages.Flash;

// Create the extension, providing the extension information.
var ext = new GEarthExtension(new GEarthOptions {
    Name = "Example",
    Description = "A xabbo/gearth example",
    Author = "b7",
    Version = "1.0"
});

// Register event handlers.
ext.Initialized += (e) => Console.WriteLine($"Extension initialized. (connected:{e.IsGameConnected})");
ext.Activated += () => Console.WriteLine("Extension activated.");
ext.Connected += (e) => Console.WriteLine($"Game connected. ({e.Host}:{e.Port})");
ext.Disconnected += () => Console.WriteLine("Game disconnected!");

// We will be implementing a simple command system.
// Any outgoing chat message beginning with `/` will be blocked and handled as a command.
// The first word will be the command name, followed by an array of arguments.
// For example, `/move <x> <y>` to move to the specified coordinates.

// This works on both the Flash and Shockwave clients.
// Although the underlying encoding is different, the packet structure is the same.
// Message identifiers are mapped between clients using the `messages.ini` map file.
// For example, the `In.Shout` identifier is mapped to Shockwave's `In.CHAT_3`.
// If you prefer to work with the Shockwave message names, you can use `Xabbo.Messages.Shockwave`.

// Intercept outgoing Chat, Shout and Whisper messages.
ext.Intercept([Out.Chat, Out.Shout, Out.Whisper], e => {
    // Read the outgoing chat message contents.
    var msg = e.Packet.Read<string>();

    // Whisper packets start with the recipient's name, so we remove it.
    // Since we are intercepting multiple messages, we must first check if this is a whisper message.
    if (e.Is(Out.Whisper))
    {
        // Remove everything up until, and including the first space.
        int index = msg.IndexOf(' ');
        if (index >= 0)
            msg = msg[(index+1)..];
    }

    // Check if the message starts with "/".
    if (msg.StartsWith('/'))
    {
        // If so, we block this packet.
        e.Block();
        string[] split = msg[1..].Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        // Then parse the command and args and pass it to the handler.
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
        // On Shockwave, send the WAVE packet.
        // On Flash, send the AvatarExpression packet with 1 to specify wave.
        if (ext.Session.Is(ClientType.Shockwave))
            ext.Send(Xabbo.Messages.Shockwave.Out.WAVE);
        else
            ext.Send(Out.AvatarExpression, 1);
        break;
    case "move":
        // Check if we have two arguments for the x, y coordinates.
        if (args.Length < 2) return;
        // Parse the coordinates.
        if (!int.TryParse(args[0], out int x)) return;
        if (!int.TryParse(args[1], out int y)) return;
        Console.WriteLine($"Moving to {x}, {y}");
        // Shockwave requires the move coordinates to be short integers.
        if (ext.Session.Is(ClientType.Shockwave))
            ext.Send(Out.MoveAvatar, (short)x, (short)y);
        else
            ext.Send(Out.MoveAvatar, x, y);
        break;
    }
}
