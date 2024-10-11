# Room Value Estimator

This example fetches the marketplace information of all furni in a room to calculate its estimated
total value in credits. It uses the `RoomManager` to track the state of all furni in the room, the
`GameDataManager` to fetch furni data and obtain furni names, and the `GetMarketplaceInfoMsg`
request message to asynchronously request the marketplace information of each group of furni in the
room.

## How to run

Connect to a hotel via G-Earth, change into this directory and run `dotnet run` to build and run the
extension, wait for the game data to load, then enter a room and activate the extension by pressing
the green play button in G-Earth. You should see the average price of each furni fetched from the
marketplace, multiplied by the number of that furni in the room:

```sh
❯ dotnet run
Loading game data for hotel: US...
Loaded 15215 furni
Calculating worth of 563 items (177 unique furni)
[   1/177 ] Iron Helmet ... 150c x 2 = 300c
[   2/177 ] Habbo-lympix Cauldron ... 235c x 3 = 705c
[   3/177 ] Rose Gold Ice Cream Maker ... 231c x 1 = 231c
[   4/177 ] Black Marble Floor ... 4c x 189 = 756c
[   5/177 ] Leap Day Pillar ... 144c x 1 = 144c
[   6/177 ] Throne ... 1143c x 1 = 1143c
# ...
[ 172/177 ] Hologirl ... 207c x 1 = 207c
[ 173/177 ] Cosmos Horns ... 475c x 1 = 475c
[ 174/177 ] Rainbow Dragon Lamp LTD ... 1107c x 1 = 1107c
[ 175/177 ] Teal Elephant Statue ... 181c x 1 = 181c
[ 176/177 ] (Wall:4001) ... 3c x 42 = 126c
[ 177/177 ] Mood Light ... 25c x 1 = 25c
Total estimated room value: 206845c
```
