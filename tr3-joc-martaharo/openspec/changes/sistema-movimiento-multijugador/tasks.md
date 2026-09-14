## Tasques de Moviment Multijugador

### Fites Assolides (Netcode & Unity 6) ✅

- [x] **Arquitectura Netcode**: Migració de Socket.io manual a Unity Netcode for GameObjects per a sincronització d'objectes.
- [x] **Autoritat del Client**: Implementació de `ClientNetworkTransform` per a moviment sense lag.
- [x] **Sincronización d'Animacions**: Solució per al `NetworkAnimator` mitjançant Reflection en `Awake`.
- [x] **Seguiment de Càmera Fail-Safe**: Implementació de cerca múltiple (per nom/tipus/string) i corrutina de reintents.
- [x] **Diferenciació Visual**: Swap automàtic de sprite (`spriteBlanco`) per als clients.
- [x] **Gestió d'Audio**: Eliminació de conflictes de múltiples AudioListeners.
- [x] **Sistema de Respawn**: Implementació de `Respawn()` amb seguretat RPC per a l'autoritat del client.
- [x] **Neteja de Sessions**: Alliberament de ports en tancar l'app o destruir el jugador local per evitar errors de "bind socket".

### Tasques Pendents 🔲

- [ ] **Proves de Build**: Verificar l'estabilitat del sistema en llançar executables fora de l'editor.
- [ ] **Sincronització de l'Enemic IA**: Connectar el `Respawn()` del jugador amb les col·lisions d'IA al servidor.
- [ ] **Optimització de Latència**: Mesurar el RTT en entorns de xarxa reals (no localhost).
