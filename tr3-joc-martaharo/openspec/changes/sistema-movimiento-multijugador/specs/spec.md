## Especificacions Tècniques

### 1. Sistema d'Autenticació i Sales (Backend)
- **Tecnologia**: Socket.io + Node.js.
- **Requisit**: Creació de sales privades amb codi de 5 caràcters.
- **Acció**: El servidor gestiona l'estat del Lobby i notifica quan la sala està plena (2 jugadors) per permetre començar la partida.

### 2. Sincronització de Moviment (Gameplay)
- **Tecnologia**: Unity Netcode for GameObjects.
- **Component**: `ClientNetworkTransform`.
- **Autoritat**: El propietari (Client) té l'autoritat per garantir zero latència en l'input local.
- **Animacions**: Sincronització d'estats de l'Animator automàtica mitjançant `NetworkAnimator`.

### 3. Sistema de Seguiment i Visuals
- **Càmera**: El script `SeguimentOcell` busca dinàmicament el target tras canvis d'escena amb corrutines de reintents.
- **Diferenciació**: Swap automàtic a `spriteBlanco` per als clients (`OwnerClientId != 0`).

### 4. Sistema de Victòria i Ranking
- **Inici de Partida**: El Servidor inicialitza `NetworkVariable<float> startTime` en `OnNetworkSpawn()` amb `Time.time`.
- **Detecció de Meta**: `OnTriggerEnter2D` comprova el tag "Finish". Si és vera i el servidor:
  - Calcula `durada = Time.time - startTime.Value`
  - Envia POST a `http://localhost:3000/api/rankings` amb format JSON:
    ```json
    { "username": "...", "puntuacio": durada, "tipus": "MULTIPLAYER", "durada": durada }
    ```
  - Executa `FinalitzarPartidaRpc(username, durada)` per mostrar resultats a tots els clients.

### 5. Interfície de Resultats
- **UIDocument**: Utilitza `Resultats.uxml` amb panel `display: none` inicialment.
- **RPC FinalitzarPartidaRpc**: `[Rpc(SendTo.Everyone)]` que:
  - Activa el panel amb `display: Flex`
  - Formata temps com a `MM:SS`
  - Assigna botons: `btn-tornar` → Lobby, `btn-sortir` → MainMenu

### 6. Integració MongoDB (Rankings)
- **Col·lecció**: MongoDB `rankings` emmagatzema:
  - `username`: Nom del jugador (string)
  - `puntuacio`: Temps en segons (float)
  - `tipus`: "MULTIPLAYER" o "SOLO" (string)
  - `durada`: Alias de puntuacio per compatibilitat
- **Endpoint API**: `POST /api/rankings` rep JSON i desa a la col·lecció

### 7. Reaparició (Respawn)
- **Lògicase via `ClientNetworkTransform.Teleport()` amb autoritat del propietari.
- **Forçat d'Eix Z**: `transform.position.z = 0f` en Update per evitar que els jugadors passin sota l'enemic.

### 9. Robustesa del Sistema
- **Alliberament de Ports**: Shutdown explícit en `OnDestroy` i `OnApplicationQuit` per evitar el bloqueig del port 7777.
- **Diagnòstics**: Logging detallat de les respostes JSON del servidor per a debugging ràpid.
- **Tancament de Partida**: Flag `partidaAcabada` impedeix múltiples execucions simultànies.