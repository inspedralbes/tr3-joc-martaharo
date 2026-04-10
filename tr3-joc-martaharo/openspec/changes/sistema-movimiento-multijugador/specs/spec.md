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

### 4. Reaparició (Respawn)
- **Lògica**: Sincronitzat via `ClientRpc`. El servidor envia l'ordre i el client propietari executa el moviment a `(0,0,0)`.

### 5. Robustesa del Sistema
- **Alliberament de Ports**: Shutdown explícit en `OnDestroy` i `OnApplicationQuit` per evitar el bloqueig del port 7777.
- **Diagnòstics**: Logging detallat de les respostes JSON del servidor per a debugging ràpid.