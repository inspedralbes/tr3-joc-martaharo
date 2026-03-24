# Foundations

## Project Overview

This project is a **cooperative 2D Pixel Art game** where two players must collaborate in real-time to escape an AI enemy and reach an exit zone together.

## Technology Stack

- **Frontend (Unity)**: C# with Unity 2D rendering
- **Backend (Server)**: Node.js with Socket.io
- **Communication**: WebSockets for real-time synchronization

## Core Constraint

The primary constraint is that **movement must be fluid and synchronized via WebSockets** between both players in the same room. This means:
- Position updates must be sent in real-time
- Latency should be minimized
- Both players must see each other's movements without significant delay

## Scope

This specification covers:
1. Player movement input handling (Arrow keys / WASD)
2. Collision detection with walls
3. Real-time position synchronization via Socket.io
4. Server-side event handling for position updates
