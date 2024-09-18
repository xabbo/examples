# [xabbo/gearth](https://github.com/xabbo/gearth)

A C# library for creating G-Earth extensions.

## examples

To run these examples, first pull in the submodules with:
```sh
git submodule update --init
git submodule foreach 'git checkout -b xabbo/examples'
```

Then run an example by name e.g. `dotnet run --project minimal`.

### [minimal](src/minimal)

A minimal example using C# top-level statements.

### [extended](src/extended)

An extended example using inheritance of the GEarthExtension class.

### [MackleEverywhere](src/MackleEverywhere) ([minimal](src/MackleEverywhere.Minimal))

An inherited / minimal example extension based on the [example extension](https://github.com/sirjonasxx/G-Earth-template-extensions/tree/master/MackleEverywhere) by sirjonasxx.
