# Pla d'Implementació

## Estratègia de Desenvolupament

### Fase 1: Backend i Autenticació ✅
- **MongoDB**: Configuració de col·leccions `usuaris`, `sales` i `rankings`.
- **Login**: Sistema de validació JWT amb bcrypt.
- **Rooms**: API per crear i llistar sales amb codis de 5 caràcters.

### Fase 2: Infraestructura de Xarxa (Netcode) ✅
- **Setup**: Integració de Unity Netcode for GameObjects.
- **Autoritat**: Implementació de `ClientNetworkTransform` per a moviment fluid.
- **Animacions**: Sincronització de Blend Trees mitjançant NetworkAnimator.

### Fase 3: Estabilitat de la Partida ✅
- **Càmera**: Corrutina de triple verificació per garantir el seguiment del jugador tras el canvi d'escena.
- **Audio**: Solució per al conflicte de múltiples AudioListeners.
- **Respawn**: Sistema RPC per a reaparicions sincronitzades decidides pel servidor (IA).

### Fase 4: Neteja i Debugging ✅
- **Ports**: Shutdown segur del `NetworkManager` per alliberar el port 7777.
- **UI Diagnostics**: Try-catch en el parseig de JSON del menú principal i logs de resposta del servidor.

---

## Estatus del Projecte
1. **Autenticació**: Operativa.
2. **Sales/Lobby**: Operativa amb Socket.io.
3. **Moviment Multijugador**: Operatiu amb Netcode (Autoritat Client).
4. **Resolució de Problemes**: Sistema de reintents de càmera i alliberament de ports implementats.