## Per què

El projecte requereix un sistema de moviment multijugador sincronitzat en temps real. Actualment no existeix comunicació en temps real entre clients per a la sincronització de posicions dels personatges (Bird_Blue i Bird_White), la qual cosa és essencial per al gameplay cooperatiu d'escapar d'un enemic IA.

## Què Canvia

- Implementació de script de moviment a Unity per a personatges controlables pel jugador
- Integració de Socket.io per a comunicació WebSocket en temps real
- Sistema de sincronització bidireccional de posicions (x, y) entre clients
- Detecció de col·lisions amb parets al client
- Enviament de posició al servidor cada cop que canviï

## Capacitats

### Noves Capacitats
- `multiplayer-movement`: Sistema de moviment sincronitzat per a dos jugadors en temps real

### Capacitats Modificades
- (Cap - funcionalitat nova)

## Impacte

- **Client Unity**: Nou script de moviment i integració amb Socket.io
- **Servidor Node.js**: Nou endpoint/esdeveniment Socket.io per a rebre i reenviar posicions
- **Dependències**: Socket.io-client a Unity, socket.io al servidor
