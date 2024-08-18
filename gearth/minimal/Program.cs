// A minimal G-Earth extension example.

using Xabbo;
using Xabbo.GEarth;
using Xabbo.Messages.Flash;

var ext = new GEarthExtension(new GEarthOptions {
    Title = "Minimal",
    Description = "A minimal xabbo/gearth example",
    Author = "b7",
    Version = "1.0"
});
ext.Intercept(In.Chat, e => {
    e.Packet.Read<int>();
    if (e.Packet.Read<string>().Contains("ping"))
        ext.Send(Out.Shout, "pong");
});
await ext.RunAsync();

