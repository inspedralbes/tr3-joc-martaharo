## 1. Configurar Script de Movimiento en Unity

- [ ] 1.1 Crear script PlayerMovement.cs en Assets/
- [ ] 1.2 Implementar detección de input (Arrow keys + WASD)
- [ ] 1.3 Configurar Rigidbody2D en el prefab del jugador
- [ ] 1.4 Implementar movimiento con velocity
- [ ] 1.5 Añadir BoxCollider2D para colisiones
- [ ] 1.6 Probar movimiento local sin red

## 2. Crear Evento 'updatePosition' en Socket.io

- [ ] 2.1 Añadir handler 'updatePosition' en server-socket.js
- [ ] 2.2 Recibir datos de posición: { playerId, x, y }
- [ ] 2.3 Almacenar posición en memoria del servidor
- [ ] 2.4 Añadir logging para debug

## 3. Sincronizar Posición - Jugador A ve a Jugador B

- [ ] 3.1 Emitir evento 'updatePosition' desde Unity al moverse
- [ ] 3.2 Configurar servidor para hacer broadcast a otros jugadores
- [ ] 3.3 Crear script NetworkManager.cs en Unity
- [ ] 3.4 Escuchar evento 'playerMoved' en cliente
- [ ] 3.5 Actualizar posición del oponente al recibir evento
- [ ] 3.6 Manejar nuevo jugador uniendose (sincronizar posiciones existentes)

## 4. Pruebas de Latencia

- [ ] 4.1 Medir Round-Trip Time (RTT) para actualizaciones de posición
- [ ] 4.2 Probar con dos clientes en máquinas diferentes
- [ ] 4.3 Registrar timestamps para identificar problemas de lag
- [ ] 4.4 Optimizar si la latencia excede 100ms
- [ ] 4.5 Documentar resultados de rendimiento
