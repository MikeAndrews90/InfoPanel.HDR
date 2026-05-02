# InfoPanel HDR Plugin

A plugin for [InfoPanel](https://github.com/habibrehmansg/infopanel) that reports whether HDR (High Dynamic Range) is currently active on any connected Windows display.

## Sensors

The plugin exposes two sensors under the **HDR** container:

| ID | Name | Type | Values |
|----|------|------|--------|
| `hdr-status` | HDR Status | Text | `On` / `Off` |
| `hdr-enabled` | HDR Enabled | Sensor | `1.0` (on) / `0.0` (off) |

Values are refreshed every **2 seconds**.

## How It Works

The plugin queries the Windows Display Configuration API to detect whether any active display has HDR enabled:

- **Windows 11 22H2+ (build 22621+):** Uses `DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO_2` (type 14), which exposes an `activeColorMode` field. Only reports HDR when the mode is `HDR`, correctly excluding Auto HDR (`AdvancedSDR`) and Wide Color Gamut (`WCG`).
- **Windows 10 / older Windows 11:** Falls back to `DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO` (type 9). Reports HDR when `advancedColorEnabled` is true and `wideColorEnforced` is false (to exclude WCG-only displays).

## Requirements

- Windows 10 or later
- .NET 8 (net8.0-windows)
- InfoPanel with plugin support

## Building

```bash
dotnet build InfoPanel.HDR.slnx
```

The output assembly (`InfoPanel.HDR.dll`) and `PluginInfo.ini` are placed in `InfoPanel.HDR/bin/<Configuration>/net8.0-windows/`.

## Installation

Copy the contents of the build output folder into your InfoPanel plugins directory. InfoPanel provides `InfoPanel.Plugins.dll` at runtime — do not include it in the plugin folder.

## Author

Andre — v1.0.0
