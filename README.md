# How to run

To run this project, start by initializing and updating git submodules by

```bash
git submodule init
git submodule update
```

To run the project, it is important to first build the .NET projects:

```bash
dotnet build
```

After the projects have been build, change directory into Simulation and run godot from there:

```bash
cd Simulation
godot-mono
```

> [!IMPORTANT] Godot binary:
> Note that your godot binary may be called something else entirely, 
> please be aware of where you have installed godot. Also, you must install the Godot version that has C# support.


# Simulation

This is a 3-part executable Godot project consisting of

- Simulation.Lib, 
- Protos and 
- Simulation

## Simulation.Lib

Consists mainly of interfaces and high-level abstractions. The Simulation.Lib library concerns itself with how to execute simulations and defining the interface of a simulation, however it does NOT concern itself with the implementation of a simulation, this is the responsibility of the Simulation executable.

## Protos

A git submodule used for defining types across languages. It is used for ensuring typesafe remote procedure calls, and is the backbone of the communication between our PyADRL library and the simulation environment.

## Simulation

This is the last part of this three-part project, the executable C# project. This is a godot project and is used for the visualization of simulations done when learning.
