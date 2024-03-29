# MLA Terminal

A recreation of the terminal interface from the video game [The Talos Principle](https://en.wikipedia.org/wiki/The_Talos_Principle).

## Installation

Download the project folder as a ZIP archive from this GitHub page and unzip the contents into any folder on your Windows PC.

## Usage

### Running The Program

Open the project folder and double click the shortcut named `MLA Terminal`.

### Commands

#### `help`

Returns a list of basic commands.

#### `list`

The word `list` followed by the directory you want to view.
If no directory is specified it will list the root `resources` directory.

#### `open`

The word `open` followed by the text file you want to view.
If no file is specified it will return `Unknown directory.`

### More Commands and "Secret" Commands

#### `screensaver`

Play the "Eye" screensaver until interrupted by a keypress.

#### `crash`

Show the "Blue Screen of Death" easter egg until interrupted by a keypress.

#### `/transcend`

Start the "Tower" ending sequence from the game.

#### `/eternalize`

Start the "Gates of Eternity" ending sequence from the game.

#### `/messenger`

Start the "Messenger" ending sequence from the game.

### Full Command List

```test
help
list <path>
open <file>
exit [force]
run <program>
    mla

screensaver
crash
admin
device_manager
access_comm_portal
debug <on | off>

/eternalize
/transcend
/messenger
/banish
```
