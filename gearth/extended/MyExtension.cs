using Xabbo.Core;
using Xabbo.Core.Messages.Incoming;
using Xabbo.Core.Messages.Outgoing;
using Xabbo.GEarth;

namespace Xabbo.Examples.Extended;

[Extension(
    Name = "Example",
    Description = "A xabbo/gearth example",
    Author = "b7",
    Version = "1.0"
)]
partial class MyExtension : GEarthExtension
{
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
    }

    protected override void OnDisconnected()
    {
        base.OnDisconnected();

        Console.WriteLine("Game disconnected.");
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        Console.WriteLine($"Extension activated.");
    }

    // The following code works cross-client (on both Flash and Shockwave)
    // by utilizing messages instead of working with packets directly.

    // Intercept incoming chat (talk, shout, and whisper) messages.
    [Intercepts]
    void HandleEntityChat(EntityChatMsg chat)
    {
        // If any incoming chat message contains "ping":
        if (chat.Message.Contains("ping"))
            // Respond by shouting "pong".
            Send(new ShoutMsg("pong"));
    }

    // Intercept outgoing chat messages. Because we need to block the message,
    // we need to accept an Intercept<T> instance and specify the Message type.
    [Intercepts]
    void HandleChat(Intercept<ChatMsg> e)
    // We could also use an alternative method signature:
    // - void HandleChat(Intercept e, ChatMsg chat)
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
        case "wave":
            // Only Wave is supported on the Shockwave client.
            // On Shockwave, this will throw an exception if the action is not Wave.
            Send(new ActionMsg(Actions.Wave));
            break;
        case "walk":
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
