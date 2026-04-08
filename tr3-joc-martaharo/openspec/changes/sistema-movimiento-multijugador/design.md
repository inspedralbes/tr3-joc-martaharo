## Context

El projecte és un joc cooperatiu 2D Pixel Art on dos jugadors han d'escapar d'un enemic IA en temps real. L'arquitectura actual té un servidor Node.js amb Socket.io i un client Unity. El sistema inclou autenticació, gestió de sales i sincronització de moviment.

**Estat Actual**:
- El servidor té gestió d'usuaris a MongoDB (base de dades `joc`, col·lecció `usuaris`)
- El model 'User' està mapejat a la col·lecció 'usuaris' (evitant la creació automàtica de 'users')
- El client Unity té autenticació (Login) i gestió de sales (Lobby)
- Socket.io gestiona l'entrada a sales i sincronització de posicions

**Restriccions**:
- El moviment ha de ser fluid (alta frequiència d'actualització)
- Sincronització via WebSockets
- Sistema de sales existent (socket.join(roomId))

## Objectius / No-Objectius

**Objectius**:
- Implementar sistema de login que validi usuaris contra MongoDB (col·lecció `usuaris`)
- Implementar gestió de sales (crear, llistar, unir-se)
- Implementar el moviment del jugador a Unity amb entrada de teclat
- Afegir detecció de col·lisions amb parets
- Crear sincronització bidireccional de posició via Socket.io per sala
- Testar i verificar el rendiment en temps real

**No-Objectius**:
- Moviment de l'enemic IA (fora de l'abast d'aquest canvi)
- Condicions de victòria/derrota (fora de l'abast d'aquest canvi)
- Emmagatzematge persistent de l'estat del joc (fora de l'abast d'aquest canvi)

## Decisions

### 1. Implementació del Moviment (Unity)

**Decisió**: Utilitzar `Rigidbody2D` per al moviment amb control basat en velocitat.

**Justificació**: `Rigidbody2D` proporciona integració de física integrada i moviment suau. Utilitzar velocitat és més fiable per a la sincronització de xarxa que modificar directament la posició del transform.

**Alternativa Considerada**: Modificació directa del transform - Rebutjada perquè no s'integra bé amb les col·lisions de física.

### 2. Freqüència d'Actualització de Posició

**Decisió**: Enviar actualitzacions de posició a cada actualització de frame de física (FixedUpdate) en lloc d'intervals limitats.

**Justificació**: Per a un moviment suau, necessitem actualitzacions freqÜents. Amb només 2 jugadors, l'amplada de banda de xarxa no és un problema. La limitació introduiria lag visual.

**Alternativa Considerada**: Limitació a 10 actualitzacions/segon - Rebutjada per a la suavitat visual.

### 3. Nomenclatura d'Esdeveniments Socket.io

**Decisió**: Utilitzar `updatePosition` (client→servidor) i `playerMoved` (servidor→clients).

**Justificació**: Nomenclatura clara que indica direcció. Coincideix amb els esdeveniments de socket existents al codi base.

### 4. Estructura de Dades de Posició

**Decisió**: Enviar `{ x: number, y: number }` com a objecte JSON senzill.

**Justificació**: Overhead mínim. Es pot estendre per incloure rotació més tard si cal.

### 5. Seguretat Multijugador amb Unity Netcode

**Decisió**: Utilitzar `IsOwner` per garantir que només el jugador propietari controla el seu personatge.

**Justificació**: Unity Netcode for GameObjects requereix controls d'autoritat per evitar que jugadors remots moguin objectes que no els pertanyen.

**Implementació**: 
- `if (!IsOwner) return;` a `Update()` abans de qualsevol lògica de moviment
- `if (!IsOwner) return;` a `FixedUpdate()` per a la física

### 6. Càmera Individual per Jugador

**Decisió**: Fer la Main Camera filla del transform del propietari.

**Justificació**: Cada jugador necessita la seva pròpia càmera que el segueixi.

**Implementació**: 
- A `OnNetworkSpawn()`, si `IsOwner` és cert:
  - Buscar `Camera.main`
  - `transform.SetParent(transform)`
  - Posició local `(0, 0, -10)`

### 7. Animació de Moviment

**Decisió**: Utilitzar paràmetre booleà `isMoving` a l'Animator.

**Justificació**: Control senzill de l'estat d'animació segons el moviment del jugador.

**Implementació**:
- Crear variable `Animator animator`
- Utilitzar `Input.GetAxisRaw` per a moviment horitzontal i vertical
- `animator.SetBool("isMoving", isMoving)` on `isMoving = inputX != 0 || inputY != 0`

### 8. Spawn Inicial

**Decisió**: Configurar la posició inicial a `Vector3.zero`.

**Justificació**: Tots els jugadors comencen al centre de l'escena.

**Implementació**: A `Start()`, configurar `transform.position = Vector3.zero`

## Riscos / Trade-offs

**[Risc] Latència de Xarxa** → Mitigació: Per a proves locals, la latència hauria de ser insignificant. Es mesurarà al Pas 4.

**[Risc] Ambdós jugadors escollint el mateix personatge** → Mitigació: El servidor assigna IDs de personatge (Bird_Blue, Bird_White) basant-se en l'ordre d'entrada.

**[Risc] El jugador es desconnecta a meitat de partida** → Mitigació: El servidor gestiona la desconnexió i notifica a l'altre jugador. La lògica de reconnexió és fora de l'abast però es pot afegir més tard.

## Preguntes Obertes

1. Hauríem d'interpolar les posicions al client receptor per a visuals més suaus?
2. Com gestionem les posicions inicials d'aparició?
3. Què passa si un tercer jugador intenta entrar a una sala plena?
