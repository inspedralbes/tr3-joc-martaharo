# Pla d'Implementació

## Estratègia: Enfocament de 4 Passos

### Pas 1: Configurar Script de Moviment a Unity

**Objectiu**: Implementar el script de moviment del jugador que gestiona l'entrada i la detecció de col·lisions.

**Accions**:
- Crear el script `PlayerMovement.cs` a Unity
- Afegir detecció d'entrada per a les tecles de fletxes i WASD
- Implementar la lògica de moviment amb `Rigidbody2D` o `Transform`
- Afegir `BoxCollider2D` o `TilemapCollider2D` per a col·lisions amb parets
- Testar el moviment local abans de la xarxa

**Entregable**: Moviment local funcional per als personatges Bird_Blue i Bird_White

---

### Pas 2: Crear l'Esdeveniment 'updatePosition' a Socket.io

**Objective**: Implementar la gestió d'esdeveniments Socket.io del costat del servidor per a actualitzacions de posició.

**Accions**:
- Afegir el gestor de l'esdeveniment `updatePosition` a `server-socket.js`
- Rebre dades de posició: `{ playerId, x, y }`
- Emmagatzemar posició a la memòria del servidor (per sala)
- Registar les posicions rebudes per a depuració

**Entregable**: Servidor capaç de rebre actualitzacions de posició dels clients

---

### Pas 3: Sincronitzar Dades de Retorn - El Jugador A veu el Jugador B

**Objectiu**: Habilitar la sincronització bidireccional de posició perquè els jugadors es puguin veure.

**Accions**:
- A Unity: Enviar l'esdeveniment `updatePosition` quan el jugador local es mou
- Al Servidor: Difondre la posició rebuda a altres jugadors de la sala via l'esdeveniment `playerMoved`
- A Unity: Escoltar l'esdeveniment `playerMoved` i actualitzar la posició de l'sprite de l'oponent
- Gestionar l'entrada d'un nou jugador a meitat de partida (sincronitzar posicions existents)

**Entregable**: Ambdós jugadors veuen els moviments de l'altre en temps real

---

### Pas 4: Proves de Latència

**Objectiu**: Verificar que la sincronització funciona dins dels paràmetres de latència acceptables.

**Accions**:
- Mesurar el temps d'anada i tornada (RTT) per a les actualitzacions de posició
- Provar amb dos clients a màquines diferents
- Registar timestamps per a identificar problemes de lag
- Optimitzar si la latència supera el llindar de 100ms

**Entregable**: Informe de rendiment amb mesures de latència

---

## Dependències

- El Pas 1 ha de completar-se abans del Pas 3
- El Pas 2 ha de completar-se abans del Pas 3
- El Pas 3 ha de completar-se abans del Pas 4
