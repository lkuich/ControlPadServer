# ControlPad Server

ControlPad Server is the Windows half of an old Android-phone-as-controller project. It receives input events from the Android client and turns them into real input on the PC: either a virtual Xbox 360 controller through ViGEm or normal Windows keyboard/mouse events.

The app is a .NET Framework/WPF solution with a small desktop UI, UDP discovery, and gRPC services.

## What It Does

- Runs a Windows desktop app named **ControlPad Server**.
- Lists local network interfaces and lets the user choose which IP address to listen on.
- Broadcasts the selected IP address so the Android app can discover it automatically.
- Hosts a plaintext gRPC server on port `50051`.
- Translates incoming Android touch events into:
  - Xbox 360 button, trigger, and thumbstick reports via ViGEm.
  - Windows keyboard and mouse events via the included `WindowsInput` project.
- Checks an old Firebase config path for update/help URLs.

## How It Works

1. `ControlHubDesktop` starts the WPF window and selects a local IP address.
2. `BroadcastServer` sends that IP address by UDP broadcast on port `58385`.
3. The Android app receives the broadcast and connects back to the selected IP.
4. `ControlHubServer` starts a gRPC server on port `50051`.
5. The Android client streams input events to one of two gRPC services:
   - `XboxButtons`: button down/up, trigger pressure, and thumbstick axes.
   - `StandardInput`: key presses and mouse movement.
6. `XboxImpl` updates an `Xbox360Report` and sends it through ViGEm.
7. `StandardInputImpl` uses Windows input simulation for keyboard and mouse control.

The shared protocol is defined in [`SocketService/protos/services.proto`](SocketService/protos/services.proto).

## Solution Layout

- [`ControlHubDesktop`](ControlHubDesktop): WPF shell. Starts/stops the server, selects the network interface, opens help/site links, and checks for updates.
- [`ControlHubServer`](ControlHubServer): core gRPC server and input translation logic.
- [`BroadcastServer`](BroadcastServer): UDP LAN discovery helper.
- [`SocketService`](SocketService): generated protobuf/gRPC service types.
- [`WindowsInput`](WindowsInput): local Windows keyboard/mouse simulation helpers.
- [`ControlHubClient`](ControlHubClient), [`ControlHubConsole`](ControlHubConsole), [`ControlHubTester`](ControlHubTester): older test/client harnesses.
- [`ViGEmClientTest`](ViGEmClientTest), [`ViGemTest`](ViGemTest): ViGEm experiments.

## Requirements

- Windows.
- Visual Studio with .NET Framework `4.6.1` targeting support.
- NuGet package restore for the packages referenced by the classic `.csproj` files.
- ViGEm components available to the solution.

Important restoration note: some project files reference `ViGEmClient.csproj` through an absolute/local path under a historical `Downloads` folder. To build this on a new machine, update those project references to a local ViGEm client checkout or replace them with the appropriate NuGet/package reference for the ViGEm client library.

## Building

Open [`ControlHub.sln`](ControlHub.sln) in Visual Studio, restore NuGet packages, fix the ViGEm reference if needed, then build `ControlHubDesktop`.

From a Developer Command Prompt, the intended shape is:

```bat
msbuild ControlHub.sln /p:Configuration=Debug
```

The solution predates SDK-style projects, so Visual Studio is usually the easiest path for package restore and reference repair.

## Running

1. Build and launch `ControlHubDesktop`.
2. Choose the LAN IP address that is reachable from the Android device.
3. Allow the app through Windows Firewall if prompted.
4. Start the Android client on the same Wi-Fi network.
5. Connect from the Android app once the server IP appears.

For Xbox mode, Windows must have the ViGEm driver/runtime pieces installed so games see the virtual Xbox 360 controller.

## Ports And Protocols

- UDP `58385`: desktop IP broadcast/discovery.
- TCP `50051`: gRPC input streams.
- gRPC uses insecure/plaintext channels because this was designed for local-network experimentation.

## Input Mapping

Xbox mode:

- Button streams set and clear `Xbox360Buttons` flags.
- Trigger streams set left/right trigger axes.
- Thumbstick streams scale Android touch deltas into signed Xbox axis values.
- The Android client sends a special `0x9001` init button to connect the virtual controller before normal reports.

Standard input mode:

- Keyboard messages are interpreted as Windows virtual key codes.
- A cancel key releases currently held keys and mouse buttons.
- Mouse movement is sent as relative deltas and applied through the local mouse simulator.

## Project Status

This is a historical prototype. It is useful for understanding the architecture and reviving the experiment, but it relies on old package versions, legacy `.csproj` files, plaintext networking, and environment-specific ViGEm references. Treat it as a starting point rather than production-ready software.

## Related Project

The Android companion app is expected to live beside this project as `../ControlPadClient`.
