Space Engineers REST API Plugin
===============================

A small plugin that spins up a REST API server inside of Space Engineers, to ease the integration with external tools.

Current functionality
---------------------

- Player access `/api/r0/player`
  - Get health, oxygen, hydrogen(?)
  - Read and set jetpack status `/jetpack`
- GPS access `/api/r0/gps`
  - Read list of player GPSes
  - Create new GPSes
  - CRUD access to GPS objects by name `/.*`
- Chat access `/api/r0/chat`
  - Post messages to chat (and run commands)
    - Including with custom author `?author=.*`
  - Get chat history `/history`
- Grid access `/api/r0/grid`
  - Local grid for the current player `/local`
  - By grid ID `/id/.*`
  - By grid name `/name/.*`

- Block access on grids `/api/r0/grid/.*/block`
  - By ID `/id/.*`
  - By name `/name/.*`
- Limited multiple nlock access on grids `/api/r0/grid/.*/blocks`
  - By name `/name/.*`
  - By group `/group/.*`

- Several interfaces for communicating with blocks, some examples;

```
$ curl http://localhost:9000/api/r0/grid/local/block/name/Corner%20Light
{"functional":true,"id":994836253988502996,"interfaces":["data","functional","light","name","upgradable"],"mass":25,"name":"Corner Light","type":"Corner Light","working":true}
$ curl http://localhost:9000/api/r0/grid/local/block/name/Corner%20Light/functional
True
$ curl http://localhost:9000/api/r0/grid/local/block/name/Corner%20Light/functional -d 'false'
$ curl http://localhost:9000/api/r0/grid/local/block/name/Corner%20Light/functional
False
$ curl http://localhost:9000/api/r0/grid/local/block/name/Corner%20Light/light
{"blink_interval_seconds":0,"blink_length":0,"blink_offset":0,"color":{"a":255,"b":255,"g":255,"r":255},"falloff":0,"intensity":4","radius":2}
$ curl http://localhost:9000/api/r0/grid/local/block/name/Corner%20Light/light -d '{"falloff": 0.1, "radius": 10}'
$ curl http://localhost:9000/api/r0/grid/local/block/name/Corner%20Light/light
{"blink_interval_seconds":0,"blink_length":0,"blink_offset":0,"color":{"a":255,"b":255,"g":255,"r":255},"falloff":0,"intensity":4","radius":2}
$ curl http://localhost:9000/api/r0/grid/local/block/name/Corner%20Light/name -d 'Corner Light - new name'
$ curl http://localhost:9000/api/r0/grid/local/block/name/Corner%20Light%20-%20new%20name/data -d 'Custom Data'
$ curl http://localhost:9000/api/r0/grid/local/block/name/Corner%20Light%20-%20new%20name
{"functional":true,"id":994836253988502996,"interfaces":["data","functional","light","name","upgradable"],"mass":25,"name":"Corner Light - new name","type":"Corner Light","working":false}
$ curl http://localhost:9000/api/r0/grid/local/block/name/LCD%20Screen/text -d 'New screen text'
$ curl http://localhost:9000/api/r0/grid/local/block/name/Ion%20Thruster/thrust -d '{"override_perc": 0.5}'
$ curl http://localhost:9000/api/r0/grid/local/block/name/Gyroscope/gyro -d '{"override": true, "yaw": 1}'
$ curl http://localhost:9000/api/r0/grid/local/block/name/Programmable%20block/script -d @script.cs
$ curl http://localhost:9000/api/r0/grid/local/block/name/Programmable%20block
{"compile_errors":false,"running":false}
$ curl http://localhost:9000/api/r0/grid/local/block/name/Programmable%20block/run -X POST
$ curl http://localhost:9000/api/r0/grid/local/block/name/Programmable%20block
{"compile_errors":false,"running":true}
```

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
