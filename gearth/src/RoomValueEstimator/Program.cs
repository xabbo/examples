using Xabbo;
using Xabbo.GEarth;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Messages.Outgoing;
using Xabbo.Core.GameData;
using Xabbo.Messages.Flash;

// Create the extension.
var ext = new GEarthExtension(new GEarthOptions {
    Name = "example",
    Description = "room value estimator",
    Author = "b7",
    Version = "1.0"
});

// Use the game data manager to access furni data.
var gameDataManager = new GameDataManager();

// Use the room manager to track furni in the room.
var roomManager = new RoomManager(ext);

// Load game data once connected to a hotel.
ext.Connected += (e) => Task.Run(async () => {
    Console.WriteLine($"Loading game data for hotel: {e.Session.Hotel}...");
    try
    {
        // Load game data for the current hotel.
        await gameDataManager.LoadAsync(e.Session.Hotel);

        // Print the number of furni info loaded.
        Console.WriteLine($"Loaded {gameDataManager.Furni?.Count ?? 0} furni info");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to load game data: {ex.Message}");
    }
});

// Calculate the total estimated room value when the extension is activated.
ext.Activated += () => Task.Run(async () => {
    try
    {
        // Make sure we are in a room and that room state is being tracked.
        if (!roomManager.EnsureInRoom(out var room))
        {
            Console.WriteLine("Room state is not being tracked, please enter a room.");
            return;
        }

        // Get an array of all furni in the room.
        var furni = room.Furni.ToArray();

        // Group furni by their descriptor (type/kind), we only need
        // to fetch the marketplace info of each kind of furni once.
        var groups = room.Furni.GroupBy(x => x.GetDescriptor()).ToArray();

        Console.WriteLine($"Calculating estimated value of {furni.Length} items ({groups.Length} unique furni)");

        // Initialize the room value at 0c.
        int roomValue = 0;

        // Iterate over each furni group.
        for (int i = 0; i < groups.Length; i++)
        {
            // Add a delay between each marketplace info request, so we don't hit the rate limit.
            if (i > 0)
                await Task.Delay(600);

            var group = groups[i];

            // Attempt to get the furni's name using xabbo core extensions.
            if (!group.Key.TryGetName(out string? name))
                // If that failed, just use the type/kind as its name.
                name = $"({group.Key.Type}:{group.Key.Kind})";

            Console.Write($"[{(i+1),4}/{groups.Length,-4}] {name} ... ");

            // Sends a GetMarketPlaceItemStatsMsg request,
            // receives the MarketplaceItemStatsMsg response,
            // and returns the parsed MarketplaceItemStats.
            var stats = await ext.RequestAsync(new GetMarketplaceItemStatsMsg(group.Key.Type, group.Key.Kind));

            // Get the number of furni in this group.
            int groupCount = group.Count();
            // Multiply the average price by the number of furni in this group.
            int averagePriceMultiplied = groupCount * stats.AverageSalePrice;
            // Print the result to the console.
            Console.WriteLine($"{stats.AverageSalePrice}c x {groupCount} = {averagePriceMultiplied}c");
            // Add to the total room value.
            roomValue += averagePriceMultiplied;
        }

        // Show the total estimated room value in-game as a chat bubble.
        ext.Send(In.Whisper, 0, $"Total estimated room value: {roomValue}c", 0, 34, 0, 0);

        Console.WriteLine($"Total estimated room value: {roomValue}c");
    }
    catch (Exception ex) { Console.WriteLine(ex.Message); }
});

// Run the extension.
ext.Run();
