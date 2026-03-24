## 1. Configurar Script de Moviment a Unity

- [ ] 1.1 Crear script PlayerMovement.cs a Assets/
- [ ] 1.2 Implementar detecció d'entrada (Tecles de fletxes + WASD)
- [ ] 1.3 Configurar Rigidbody2D al prefab del jugador
- [ ] 1.4 Implementar moviment amb velocity
- [ ] 1.5 Afegir BoxCollider2D per a col·lisions
- [ ] 1.6 Provar moviment local sense xarxa

## 2. Crear Esdeveniment 'updatePosition' a Socket.io

- [ ] 2.1 Afegir gestor 'updatePosition' a server-socket.js
- [ ] 2.2 Rebre dades de posició: { playerId, x, y }
- [ ] 2.3 Emmagatzemar posició a la memòria del servidor
- [ ] 2.4 Afegir logging per a depuració

## 3. Sincronitzar Posició - El Jugador A veu el Jugador B

- [ ] 3.1 Enviar esdeveniment 'updatePosition' des d'Unity en moure's
- [ ] 3.2 Configurar servidor per a fer broadcast a altres jugadors
- [ ] 3.3 Crear script NetworkManager.cs a Unity
- [ ] 3.4 Escoltar esdeveniment 'playerMoved' al client
- [ ] 3.5 Actualitzar posició de l'oponent en rebre l'esdeveniment
- [ ] 3.6 Gestionar nou jugador unint-se (sincronitzar posicions existents)

## 4. Proves de Latència

- [ ] 4.1 Mesurar Round-Trip Time (RTT) per a actualitzacions de posició
- [ ] 4.2 Provar amb dos clients a màquines diferents
- [ ] 4.3 Registar timestamps per a identificar problemes de lag
- [ ] 4.4 Optimitzar si la latència excedeix 100ms
- [ ] 4.5 Documentar resultats de rendiment
