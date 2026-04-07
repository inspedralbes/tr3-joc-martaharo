## Tasques Pendents

### Funcionalitats Implementades ✅

- [x] 1.1 Configurar connexió MongoDB a la base de dades `joc_multijugador`
- [x] 1.2 Crear schema per a la col·lecció `usuaris`
- [x] 1.3 Implementar ruta POST `/api/login` al servidor
- [x] 1.4 Validar usuari i contrasenya (amb bcrypt)
- [x] 1.5 Retornar token de sessió

- [x] 2.1 Crear ruta POST `/api/rooms` per crear sales (codi generat automàticament)
- [x] 2.2 Generar codi automàtic de 5 caràcters al servidor
- [x] 2.3 Crear ruta GET `/api/rooms` per llistar sales
- [x] 2.4 Crear ruta POST `/api/rooms/:id/join` per unir-se
- [x] 2.5 Emmagatzemar sales a MongoDB amb persistència

- [x] 7.1 Interfície de creació de partides al menú principal
- [x] 7.2 Botó "Crear Sala" sense camp de nom
- [x] 7.3 Mostrar codi de sala rebut del servidor

- [x] 3.1 Implementar esdeveniment `joinRoom` amb `socket.join(roomId)`
- [x] 3.2 Gestionar `updatePosition` amb filtrat per sala
- [x] 3.3 Sincronitzar posicions només entre jugadors de la mateixa sala

- [x] 4.1 Script `PlayerController.cs` a Unity
- [x] 4.2 Implementar detecció d'entrada (Fletxes + WASD)
- [x] 4.3 Configurar Rigidbody2D per al moviment
- [x] 4.4 Afegir col·lisions amb parets

- [x] 5.1 Enviar esdeveniment `updatePosition` quan el jugador es mou
- [x] 5.2 Escoltar esdeveniment al client
- [x] 5.3 Actualitzar posició de l'oponent
- [x] 5.4 Gestionar nou jugador unint-se a la sala

- [x] 6.1 Patró Repository implementat (Punt 4.2)
- [x] 6.2 Mode Single Player implementat

- [x] 8.1 Unificació de col·leccions de base de dades (model 'User' mapejat a 'usuaris')

### Tasques Pendents 🔲

## 1. Sincronització de l'Enemic IA 🔲

- [ ] 1.1 Implementar lògica de moviment de l'enemic al servidor
- [ ] 1.2 Transmetre posició de l'enemic via Socket.io
- [ ] 1.3 Rebre posició a Unity amb Vector3.Lerp
- [ ] 1.4 Gestionar col·lisions amb el jugador

## 2. Poliment de la UI de Rànquing 🔲

- [ ] 2.1 Mostrar rànquing separats per tipus (Single/Multi)
- [ ] 2.2 Afegir animacions visuals
- [ ] 2.3 Millorar feedback d'errors

## 3. Proves de Latència 🔲

- [ ] 3.1 Mesurar Round-Trip Time (RTT)
- [ ] 3.2 Provar amb dos clients a màquines diferents
- [ ] 3.3 Documentar resultats de rendiment

## 4. Interfície de Menú Principal (Completada) ✅

- [x] 4.1 Flux simplificat: crear sala sense nom
- [x] 4.2 Codi de sala mostrat automàticament
- [x] 4.3 Validació de codi de 5 caràcters per unir-se
- [x] 4.4 Corregir error de creació de sala sense nom
- [x] 4.5 Corregir mismatch de rutes API (Unity → /api/rooms)

## 5. Error de Crash del Servidor ✅

- [x] 5.1 Corregit error "require('./AuthController').getSessions is not a function" a GameController.js
- [x] 5.2 La ruta POST /api/rooms coincideix amb la ruta de Unity

## 6. Millores d'Usabilitat del Menú ✅

- [x] 6.1 Implementar redirecció automàtica al joc después de crear sala (2 segons)
- [x] 6.2 Afegir funcionalitat de copiar codi al portapapers

## 7. Sistema de Lobby ✅

- [x] 7.1 LobbyManager.cs amb UI Toolkit (label-codi, llista-jugadors, btn-comencar)
- [x] 7.2 Mostrar codi de sala des de MainMenuManager.roomCode
- [x] 7.3 Escoltar esdeveniment updateLobby per actualitzar llista de jugadors
- [x] 7.4 Botó btn-comencar actiu només amb 2 jugadors
- [x] 7.5 Efecte visual MouseEnter: blau més brillant + escala 1.1x

## 8. Gestió d'Errors i Neteja ✅

- [x] 8.1 room-not-found quan sala no existeix
- [x] 8.2 Eliminar sales amb 0 jugadors de memòria i MongoDB
- [x] 8.3 Signal startGame des del host per carregar escena Joc
- [x] 8.4 Feedback "Aquesta sala no existeix" al MainMenu

## 9. Sincronització de Jugadors ✅

- [x] 9.1 Assignar Jugador 1 (host) i Jugador 2 al joinRoom
- [x] 9.2 Guardar playerNumber a roomPositions per passar a scripts de moviment
- [x] 9.3 Moviment fluid filtrat per roomId
