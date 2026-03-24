## Context

The project is a cooperative 2D Pixel Art game where two players must escape an AI enemy in real-time. The current architecture has a Node.js server with Socket.io and a Unity client. The missing piece is the synchronized movement system that allows both players to see each other's positions in real-time.

**Current State**:
- Server has basic room management and Socket.io setup
- Unity client has authentication and room management via HTTP
- No real-time position synchronization exists

**Constraints**:
- Movement must be fluid (high update frequency)
- Synchronization via WebSockets only
- Must work with existing room-based architecture

## Goals / Non-Goals

**Goals:**
- Implement player movement in Unity with keyboard input
- Add collision detection with walls
- Create bidirectional position synchronization via Socket.io
- Test and verify real-time performance

**Non-Goals:**
- Enemy AI movement (out of scope for this change)
- Victory/defeat conditions (out of scope for this change)
- Persistent game state storage (out of scope for this change)

## Decisions

### 1. Movement Implementation (Unity)

**Decision**: Use `Rigidbody2D` for movement with velocity-based control.

**Rationale**: `Rigidbody2D` provides built-in physics integration and smooth movement. Using velocity is more reliable for network synchronization than directly modifying transform position.

**Alternative Considered**: Direct transform modification - Rejected because it doesn't integrate well with physics collisions.

### 2. Position Update Frequency

**Decision**: Send position updates on every physics frame update (FixedUpdate) rather than throttled intervals.

**Rationale**: For smooth movement, we need frequent updates. With only 2 players, network bandwidth is not a concern. Throttling would introduce visual lag.

**Alternative Considered**: Throttle to 10 updates/second - Rejected for visual smoothness.

### 3. Socket.io Event Naming

**Decision**: Use `updatePosition` (client→server) and `playerMoved` (server→clients).

**Rationale**: Clear naming that indicates direction. Matches existing socket events in the codebase.

### 4. Position Data Structure

**Decision**: Send `{ x: number, y: number }` as a simple JSON object.

**Rationale**: Minimal overhead. Can extend to include rotation later if needed.

## Risks / Trade-offs

**[Risk] Network Latency** → Mitigation: For local testing, latency should be negligible. Will measure in Step 4.

**[Risk] Both players choosing same character** → Mitigation: Server assigns character IDs (Bird_Blue, Bird_White) based on join order.

**[Risk] Player disconnects mid-game** → Mitigation: Server handles disconnect and notifies other player. Reconnection logic is out of scope but can be added later.

## Open Questions

1. Should we interpolate positions on the receiving client for smoother visuals?
2. How do we handle the initial spawn positions?
3. What happens if a third player tries to join a full room?
