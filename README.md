# iv4xr-se-plugin
Integration of **[Space Engineers](https://www.spaceengineersgame.com/)** to the **iv4XR framework**. You can find the project page at [iv4xr-project.eu](https://iv4xr-project.eu/).

Status: Prototype / early development

## Introduction

Space Engineers is a sandbox game by Keen Software House. This project is a plugin for the game which enables its integration with the iv4XR testing framework. The plugin runs a TCP/IP server with JSON-based API. It allows to access surrounding of the player's character in a structured form (a World Object Model, WOM) and to control the character. Those are the two defining features, more will be added during the development.

## How to run the game with this plugin

It's not necessary to build anything to try out this plugin. This section describes how to do it.

1. Obtain the binary release of Space Engineers (buy it on Steam or get a key). Install the game.
2. Obtain a binary release of the plugin. Look for [releases](https://github.com/iv4xr-project/iv4xr-se-plugin/releases) in this repository and for Assets of the chosen release. Download the two DLL libraries.
3. IMPORTANT: Make sure Windows is OK to run the libraries. Windows 10 blocks "randomly" downloaded libraries. To unblock them, right-click each of them and open file properties. Look for Security section on the bottom part of the General tab. You might see a message: "*This file came from another computer and might be blocked...*". If so, check the `Unblock` checkbox.
   (If you skip this step, the game will probably crash with a message: `System.NotSupportedException`: *An attempt was made to load an assembly from a network location...*)
4. Put the plugin libraries into the folder with the game binaries. A common location is `C:\Program Files (x86)\Steam\steamapps\common\SpaceEngineers\Bin64`.
   Tip: It's you can put the libraries into a subfolder (such as `ivxr-debug`). Or, it can be a symbolic link to the build folder of the plugin project. In that case, you must prefix the name of each library with `ivxr-debug\` in the following step. 
5. Right-click on the game title in the Steam library list and open its properties window. Click on the **Set launch options...** button. Add the option `-plugin` and list the libraries. The result should be something like this: `-plugin Ivxr.PlugIndependentLib.dll Ivxr.SePlugin.dll`.
6. Run the game. (If the game crashes, make sure you've seen step 3.)
7. Start a scenario. (It's necessary to do it manually for now. Should be done automatically by the testing framework in the future.)
8. If the plugin works correctly, a TCP/IP server is listening for JSON-based commands on a fixed port number. (The current development version uses the port number 9678.) 
   Another sign of life is a log file present in user's app data folder such as: `C:\Users\<username>\AppData\Roaming\SpaceEngineers\ivxr-plugin.log`

## API

The network protocol is just some proof of concept for now, so it's possible it will change. It is based on [the Lab Recruits demo](https://github.com/iv4xr-project/iv4xrDemo). The protocol is based on JSON commands split by newlines.

Currently implemented commands:

- *Session* command: **Load** – Loads a scenario.
- **Observe** – experimental; returns list of entities and their location in the agent's surrounding. It has several different modes. One of them is to return only new blocks not reported in past observations.
- **MoveAndRotate** – allows to move and and rotate the agent in all directions.
- **Interact** – implements some rudimentary commands for building new blocks in the game. Current interaction types:
  - Equip – Selects a block or tool from the toolbar as the current tool.
  - Place – Places a new block into the game if the conditions are right.
- Disconnect

There's a Java project derived from the Lab Recruits demo that contains a demo client in the form of unit tests. The [repository is here](https://github.com/iv4xr-project/iv4xrDemo-space-engineers).

## How to build

The plug-in requires Space Engineers codebase (which is not open) to compile. The resulting plug-in (a couple of .NET libraries), however, works with the official Steam version of Space Engineers without any modification of the game.

### How to build if you have SE sources

There's a VS solution file in this repository (in the `SpaceEngineSolution` folder) that contains the plugin projects as well as Space Engineers projects, some of which are dependencies of the plug-in. For this solution file to work, you need to checkout Space Engineers sources to a specific location relative to this Git repository – the relevant branch (such as "Major") has to be checked-out into a directory called "`se`" located next to the checkout of this Git repository. See the nested list below, which corresponds to the required directory structure:

* `se-plugin` – just a top level directory, can have any name
  * `iv4xr-se-plugin` – a checkout of this Git repository
  * `se` – a checkout of a Space Engineers branch (presumably from it's Subversion repository)

Before starting the build of the solution, make sure a correct build configuration is selected. Either **Debug** or **Release** configuration and the **x64** platform.

## Architecture Overview

Overview of the solution projects:

* **`Ivxr.SePlugin`** – The **main plugin project**. Contains most of the important logic. It is one of the plugin libraries, the main one.
  * See the project details below.
* **`Ivxr.SePlugin.Tests`** – Unit tests for the main project.
* **`Ivxr.PlugIndependentLib`** – Contains service code that is *entirely independent of the Space Engineers codebase* for better dependency management and easier testing.
  * Currently contains mostly the TCP/IP server and some basic interfaces such as the logging interface.
  * It is a secondary plugin library.
  * Notable classes: `PluginServer` – a TCP/IP server.
* **`SeServerMock`** – A testing project which runs a TCP/IP server based on the infrastructure from the two main libraries and using some simple mock implementations of the classes which would normally depend on a running game.

#### Project details: `Ivxr.SePlugin`

List of notable classes – top level:

* `IvxrPlugin` – Entry point of the plugin, implements the game's `IPlugin` interface.
* `IvxrSessionComponent` –  Inherits from game's `MySessionComponentBase` which allows the component to hook the plugin into game events such as `UpdateBeforeSimulation` called each timestep of the game.
* `IvxrPluginContext` – Root of the dependency tree of the plugin, constructs all the important objects.

Notable sub-namespaces (and the solution sub-folders):

* `Control` – Interfacing with the game: Obtaining observation and control of the character. Notable classes:
  * `Dispatcher` – The command hub.
  * `CharacterController` – Self-explanatory.
  * `Observer` – Extracts observations from the game.
* `Session` – Session control such as loading a saved game. Has a separate command dispatcher because it needs to run (in the "lobby") even when no actual game is running.
* `WorldModel` – Classes supporting the communication (JSON over TCP/IP).



