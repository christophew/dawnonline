# Introduction #

Just get the thing running, without all the hassle of compiling, etc...

# HostFile #

Add the location of the dawnserver to your hostfile.

Hostfile example location: C:\Windows\System32\drivers\etc\hosts
For a local server: 127.0.0.1 dawnserver


# Release folder #

I included a \Release folder with all files necessary to get up and running.


## DawnServer ##

The first thing to start.

Contains the Photon server app.

Startup using the PhotonControl:
  1. Start photon control: \Release\DawnServer\bin\_win32\PhotonControl.exe
  1. Use the system tray menu to start the DawnServer: \Default\Start as application

## DawnClientConsole ##

Simple console app to monitor the dawnserver.
Start the exe to test whether your server is up and running.

After the necessary connection messages, you should start getting these kind lines:
> --> Total: 592, Walls: 540, Boxes: 0, Creatures: 52, SpawnPoints: 0

If not... start cursing and ask for help.
Possible causes:
  * could not find "dawnserver" (make sure it is in your host file)
  * port 5055 or port 9090 are not accessible or used by other apps
  * firewall settings, virus checkers, UAC policies, ...

## UnityClient ##

Prebuild 3D client.
Start this after your server. Will give you an overview of first person view of the eco-system.

Sometimes this client has problems connecting to the server. When you get a connection error, just try again.

Keys:
  * use F1/F2 to switch view
  * use numeric keyboard to control camera in overhead view
    * move: 4, 8, 6, 2
    * zoom: 7, 9
  * use cursor to move (best in first person view)

## DawnAgentMatrix ##

Now it's time to populate the eco-system.

Start as many "AgentMatrix.exe" consoles as your machine can handle.
Each will have 5 spawnpoints to control, and all it's spawned creatures.

The population for the genetic algorithms is always autosaved/loaded.
= every time you start an AgentMatrix, the creatures will get better





.... enjoy