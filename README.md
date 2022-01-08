Space Engineers REST API Plugin
===============================

A small plugin that spins up a REST API server inside of Space Engineers, to ease the integration with external tools.

Building
--------

To keep things reasonable, the project expects there to be a `Bin64` folder next to the solution, containing the SE DLLs.
This can be done with a symlink or similar, or the paths can be edited in the csproj for local builds.

On Windows, it should hopefully just build with Visual Studio.

On Linux, the `dotnet` CLI tool should be enough (from either the Core or 5+ SDKs).

TODO
----

- Configuration for the server - port, hostname, etc
- Handle dedicated servers (some endpoints are nonsensical on there, local player access etc)
- Expose more data (in reasonable amounts)
- Some kind of simple Web UI?
- SSE streams? (chat? some other kind of events?)
