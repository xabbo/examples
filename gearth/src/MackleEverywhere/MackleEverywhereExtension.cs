using Xabbo;
using Xabbo.Core;
using Xabbo.Core.Messages.Incoming;
using Xabbo.Core.Messages.Outgoing;
using Xabbo.GEarth;
using Xabbo.Messages;

namespace MackleEverywhere;

// Based on the example G-Earth extension by sirjonasxx
// https://github.com/sirjonasxx/G-Earth-template-extensions

// Works on both Flash and Shockwave clients.

[Extension(
    Name = "MackleEverywhere",
    Description = "Mackle Bee Everywhere",
    Version = "1.0",
    Author = "b7"
)]
partial class MackleEverywhereExtension : GEarthExtension
{
    private string ownName = "";

    protected override void OnConnected(GameConnectedArgs e)
    {
        base.OnConnected(e);
        if (e.PreEstablished)
            Send(new GetUserDataMsg());
    }

    [Intercept]
    private void HandleUserData(UserDataMsg msg) => ownName = msg.UserData.Name;

    [Intercept]
    private IMessage? MakeMacklebees(AvatarsAddedMsg avatars)
    {
        foreach (var user in avatars.OfType<User>())
        {
            if (user.Name != ownName)
                user.Name = "Macklebee";
            user.Figure = Session.IsShockwave
                ? "8311518001295012801125525"
                : "hr-828-58.hd-180-1.ch-210-73.lg-280-82.sh-295-1408";
            user.Gender = Gender.Male;
        }
        return avatars;
    }
}
