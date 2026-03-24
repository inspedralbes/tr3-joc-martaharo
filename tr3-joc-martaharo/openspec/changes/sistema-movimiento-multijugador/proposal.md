## Why

El proyecto requiere un sistema de movimiento multijugador sincronizado en tiempo real. Actualmente no existe comunicación en tiempo real entre clientes para la sincronización de posiciones de los personajes (Bird_Blue y Bird_White), lo cual es esencial para el gameplay cooperativo de escapar de un enemigo IA.

## What Changes

- Implementación de script de movimiento en Unity para personajes controlables por el jugador
- Integración de Socket.io para comunicación WebSocket en tiempo real
- Sistema de sincronización bidireccional de posiciones (x, y) entre clientes
- Detección de colisiones con paredes en el cliente
- Envío de posición al servidor cada vez que cambie

## Capabilities

### New Capabilities
- `multiplayer-movement`: Sistema de movimiento sincronizado para dos jugadores en tiempo real

### Modified Capabilities
- (Ninguno - funcionalidad nueva)

## Impact

- **Cliente Unity**: Nuevo script de movimiento y integración con Socket.io
- **Servidor Node.js**: Nuevo endpoint/evento Socket.io para recibir y reenviar posiciones
- **Dependencias**: Socket.io-client en Unity, socket.io en servidor
