using Xabbo;
using Xabbo.Core;
using Xabbo.Core.Messages.Incoming;
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

ext.Intercept<AvatarsAddedMsg>(avatars => {
    foreach (var user in avatars.OfType<User>())
    {
        user.Name = "Macklebee";
        user.Figure = ext.Session.IsShockwave
            ? "8281718001280082950921022"
            : "hr-828-58.hd-180-1.ch-210-73.lg-280-82.sh-295-1408";
        user.Gender = Gender.Male;
    }
    return avatars;
});

ext.Run();
