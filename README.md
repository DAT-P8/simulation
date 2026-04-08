# How to Run

## Nix

If you have [Nix](https://nixos.org/download/) installed, the development environment is fully reproducible with no manual dependency setup.

### 1. Enter the Development Shell

```bash
nix develop
```

Alternatively automate this by installing direnv and allow it

```bash
direnv allow
```

This will automatically:
- Make `dotnet`, `godot-mono`, `protobuf`, and `grpc` available
- Initialize and update all Git submodules
- Print next steps as a reminder

### 2. Build the .NET Projects

```bash
dotnet build
```

### 3. Run the Simulation


```bash
run-simulation
```

The `run-simulation` command is provided by the Nix shell. It navigates to the `Simulation` directory and launches Godot automatically.

---

## Manual setup

### 1. Initialize Git Submodules

Before building the project, make sure all Git submodules are initialized and updated:

```bash
git submodule init
git submodule update
```

### 2. Build the .NET Projects

The .NET projects must be built before running the simulation:

```bash
dotnet build
```

### 3. Run the Simulation

After a successful build, navigate to the `Simulation` directory:

```bash
cd Simulation
```

The first time runnning the simulation we have to import the assets:

```bash
godot-mono --headless --import
```

Then run the simulation:

```bash
godot-mono
```

> [!IMPORTANT]
> **Godot Binary**
>
> - Your Godot executable may have a different name depending on your installation.
> - Ensure that Godot is installed and available in your system PATH.
> - You must use the version of Godot that includes **C# (Mono/.NET) support**.

---

# Simulation

This is a three-part executable Godot project consisting of:

- `Simulation.Lib`
- `Protos`
- `Simulation`

## Simulation.Lib

`Simulation.Lib` contains interfaces and high-level abstractions related to running simulations.

It defines:
- How simulations are executed
- The core simulation interfaces

It does **not** implement simulation logic. Concrete implementations are handled by the `Simulation` executable project.

## Protos

`Protos` is a Git submodule used for defining shared types across multiple languages.

It is responsible for:
- Enabling type-safe remote procedure calls (RPC)
- Providing a shared contract between components
- Serving as the communication backbone between the PyADRL library and the simulation environment

## Simulation

`Simulation` is the executable C# Godot project.

It is responsible for:
- Implementing the simulation logic
- Visualizing simulations during training and learning
- Acting as the runtime entry point of the system

