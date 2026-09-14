## Context

El projecte és un joc cooperatiu 2D Pixel Art on dos jugadors han d'escapar d'un enemic IA en temps real. L'arquitectura ha evolucionat cap a Unity Netcode for GameObjects per a la sincronització de joc, mantenint Node.js per a l'autenticació i gestió de sales.

**Estat Actual**:
- Servidor Node.js per a autenticació (JWT) i gestió de sales (MongoDB).
- Unity Netcode per a la sincronització de transformacions i estats de joc.
- Autoritat del Client per a moviment fluid.

## decisions

### 1. Autoritat de Moviment (Unity Netcode)
**Decisió**: Utilitzar `ClientNetworkTransform` per habilitar autoritat del client.
**Justificació**: Resposta instantània a l'input local sense lag ni rubber-banding. Els clients són amos de la seva posició i el servidor la replica.

### 2. Sincronització d'Animacions
**Decisió**: Assignació dinàmica via Reflection de l'Animator al `NetworkAnimator`.
**Justificació**: Evita la limitació de l'Inspector de Unity que sovint perd la referència del component en Prefabs niats.

### 3. Sistema de Seguiment de Càmera
**Decisió**: Cerca dinàmica de la `Main Camera` amb corrutina de reintents.
**Justificació**: En xarxa, l'ordre de càrrega d'objectes és impredictible. Els reintents (3 vegades amb 0.1s de retard) garanteixen que el jugador local trobi la seva càmera.

### 4. Gestió d'Audio
**Decisió**: Desactivació global d'AudioListeners i activació selectiva per al propietari.
**Justificació**: Soluciona el conflicte de "2 audio listeners" quan apareixen diversos ocells a l'escena.

### 5. Sistema de Reaparició (Respawn)
**Decisió**: Sincronització via `ClientRpc`.
**Justificació**: Com que el client té l'autoritat del moviment, el servidor ha d'utilitzar un RPC per indicar al client que mogui el seu propi objecte a `(0,0,0)`.

### 6. Neteja de Xarxa i Shutdown
**Decisió**: Alliberament explícit del port 7777 en `OnDestroy` i `OnApplicationQuit`.
**Justificació**: Prevé errors de "Address already in use" en reiniciar partides ràpidament.

## Riscos / Trade-offs
- **[Risc] Hackers de Moviment**: En donar autoritat al client, un usuari malintencionat podria teles-transportar-se. *Mitigació*: No és crític per a aquest context cooperatiu educatiu.
- **[Risc] Race Conditions**: La càmera pot trigar a aparèixer. *Mitigació*: Corrutines de verificació triple.
