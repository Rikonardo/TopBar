# TopBar

Simple and modern topbar for Windows

[**To download the installer, click here**](https://github.com/Rikonardo/TopBar/releases/latest)

![GitHub release (latest by date)](https://img.shields.io/github/v/release/Rikonardo/TopBar) ![GitHub all releases](https://img.shields.io/github/downloads/Rikonardo/TopBar/total) ![.NET Framework version](https://img.shields.io/badge/.NET%20Framework-4.6%2B-green)

Keep all the data you need in sight with convenient widgets on right side of TopBar
![](https://i.imgur.com/rGjVMu9.png) 

The title of the selected window is always displayed on the left side of the TopBar
![](https://i.imgur.com/h8RZA6b.png) 

Everything is configured using a single json configuration file (`%AppData%/TopBar/config.json`)

## Extentions

You can create your own TopBar widgets by developing an extension

- The widget class must be public public and implement the "TopBar.iWidgetBase" interface.
- In addition, the widget class must contain a constructor that takes a settings object of type "dynamic" as an argument.
- The extension must be a dll .NET class library.
- Extensions are installed by being located in the `%AppData%/TopBar/extensions` folder

## Default widgets

By default, TopBar has the following pre-installed widgets:

- [TopBar.Widgets.Clock](#TopBar.Widgets.Clock)
- [TopBar.Widgets.CPULoad](#TopBar.Widgets.CPULoad)
- [TopBar.Widgets.RAMUsageBar](#TopBar.Widgets.RAMUsageBar)
- [TopBar.Widgets.RAMUsageText](#TopBar.Widgets.RAMUsageText)
- [TopBar.Widgets.CustomText](#TopBar.Widgets.CustomText)
- [TopBar.Widgets.TextFromFile](#TopBar.Widgets.TextFromFile)
- [TopBar.Widgets.Shortcut](#TopBar.Widgets.Shortcut)
- [TopBar.Widgets.LineSpacer](#TopBar.Widgets.LineSpacer)

### TopBar.Widgets.Clock

Displays the current time

**Settings:**

- `date` (bool) - Whether the date should be displayed| Default: `false`
- `seconds` (bool) - Whether the seconds counter should be displayed | Default: `false`
- `text-color` (string) - HEX color code | Default: `"#BBBBBB"`

### TopBar.Widgets.CPULoad

Displays the load on each of the processor cores

**WARNING: Due to the specifics of the implementation, using this widget slows down the loading of TopBar by 1-5 seconds**

**Settings:**

- `color` (string) - HEX color code or one of next values: `"rainbow"`, `"from-percentage"` | Default: `"#BBBBBB"`
- `line-width` (int) - Width of rectangles| Default: `5`
- `radius` (int) - Radius of rectangles | Default: `2`

### TopBar.Widgets.RAMUsageBar

Displays the current RAM usage

**Settings:**

- `color` (string) - HEX color code or `"from-percentage"` | Default: `"#BBBBBB"`**
- `background-color` (string) - HEX color code | Default: `"#33BBBBBB"`
- `width` (int) - Width of the progress bar| Default: `60`
- `height` (int) - Height of the progress bar| Default: `10`
- `radius` (int) - Radius of the progress bar rectangle | Default: `2`

### TopBar.Widgets.RAMUsageText

Displays the current RAM usage in percent

**Settings:**

- `color` (string) - HEX color code| Default: `"#BBBBBB"`

### TopBar.Widgets.CustomText

Displays custom text

**Settings:**

- `color` (string) - HEX color code| Default: `"#BBBBBB"`
- `text` (string) - Text to display| Default: `""`

### TopBar.Widgets.TextFromFile

Displays contents of text file (refresh every 500ms)

**Settings:**

- `color` (string) - HEX color code| Default: `"#BBBBBB"`
- `file` (string) - Path to text file| Default: `""`

### TopBar.Widgets.Shortcut

Shortcut to lunch app or run command in one click

**Settings:**

- `command` (string) - Path to executable or cmd command| Default: `"explorer.exe"`
- `icon` (string) - Path to icon file| Optional,  by default icon extracting from target executable
- `title` (string) - Tooltip text| Optional,  by default equals `command` value

### TopBar.Widgets.LineSpacer

Spacer used to visually separate widget groups

**Settings:**

- `color` (string) - HEX color code | Default: `"#888888"`
- `width` (int) - Width of the  spacer| Default: `1`
- `height` (int) - Height of the spacer| Default: `10`

