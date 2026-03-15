# Drone Swarm Intercept System

A Unity simulation of an autonomous drone swarm defense system. A network of radar stations (RLS) tracks an incoming aerial target, and a coordinated swarm of 15 counter-drones intercepts it.

## What it does

- **Navigation system** (`Distance.cs`) — uses 4 active and 1 passive radar stations to triangulate a target's 3D position with configurable measurement accuracy
- **Swarm controller** (`SwarmControlSystem.cs`) — arranges 15 drones in a circular formation on the ground, lifts them on target detection, then organizes them into a three-front attack formation (1 lead + 5 inner ring + 9 outer ring)
- **Target drone** (`MoveScript.cs`) — simulates the incoming drone flying toward the origin
- **Collision detection** (`CollisionCheck.cs`) — logs intercept events

## Stack

- Unity (tested with Unity 2020+)
- C# / .NET

## Project structure

```
Assets/Scripts/
├── Navigation/
│   └── Distance.cs         # Radar network + navigation system
├── Swarm/
│   └── SwarmControlSystem.cs  # Swarm coordination and flight control
└── Drone/
    ├── MoveScript.cs       # Target drone movement
    └── CollisionCheck.cs   # Collision handler
Tests/
└── ...                     # NUnit tests for navigation math
```

## Setup

1. Open the project in Unity
2. Create a scene with:
   - 5 radar station GameObjects (RLS1–RLS5) with the `Distance` script
   - 15 drone GameObjects with `SwarmControlSystem` script and individual drone references assigned
   - A target drone GameObject with `MoveScript`
3. Assign references in the Inspector
4. Hit Play

## Running tests

The navigation math is tested independently via NUnit (no Unity license required):

```bash
cd Tests
dotnet test
```

## Notes

This was a university practice project exploring multi-agent coordination and radar-based navigation. The swarm uses spherical coordinate geometry to compute target position from bearing/distance readings, averaging over multiple samples to reduce simulated sensor noise.
