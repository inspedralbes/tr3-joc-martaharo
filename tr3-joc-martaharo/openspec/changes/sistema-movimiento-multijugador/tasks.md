## Tasques Pendents

### Funcionalitats Implementades ✅

- [x] 1.1 Configurar connexió MongoDB a la base de dades `joc_multijugador`
- [x] 1.2 Crear schema per a la col·lecció `usuaris`
- [x] 1.3 Implementar ruta POST `/api/login` al servidor
- [x] 1.4 Validar usuari i contrasenya (amb bcrypt)
- [x] 1.5 Retornar token de sessió

- [x] 2.1 Crear ruta POST `/api/rooms` per crear sales
- [x] 2.2 Crear ruta GET `/api/rooms` per llistar sales
- [x] 2.3 Crear ruta POST `/api/rooms/:id/join` per unir-se
- [x] 2.4 Emmagatzemar sales a MongoDB amb persistència

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
