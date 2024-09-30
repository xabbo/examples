using Xabbo;
using Xabbo.Core;
using Xabbo.Core.Messages.Incoming;
using Xabbo.Core.Messages.Outgoing;
using Xabbo.GEarth;

// Based on the example G-Earth extension by sirjonasxx
// https://github.com/sirjonasxx/G-Earth-template-extensions

// Works on both Flash and Shockwave clients.

var ext = new GEarthExtension(new GEarthOptions {
    Name = "MackleEverywhere",
    Description = "Mackle Bee Everywhere",
    Version = "1.0",
    Author = "b7"
});

string ownName = "";

ext.Intercept<AvatarsAddedMsg>(avatars => {
    foreach (var user in avatars.OfType<User>())
    {
        if (user.Name != ownName)
            user.Name = "Macklebee";
        user.Figure = ext.Session.Is(ClientType.Shockwave)
            ? "8311518001295012801125525"
            : "hr-828-58.hd-180-1.ch-210-73.lg-280-82.sh-295-1408";
        user.Gender = Gender.Male;
    }
    return avatars;
});

// Required for Shockwave because it appears to use your name to control your avatar.
ext.Connected += (e) => {
    if (e.PreEstablished)
        ext.Send(new GetUserDataMsg());
};
ext.Intercept<UserDataMsg>(msg => ownName = msg.UserData.Name);

ext.Run();
