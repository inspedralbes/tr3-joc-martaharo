# Fonaments

## Visió General del Projecte

Aquest projecte és un **joc cooperatiu 2D Pixel Art** on dos jugadors han de col·laborar en temps real per escapar d'un enemic IA i arribar a una zona de sortida junts.

## Stack Tecnològic

- **Frontend (Unity)**: C# amb Unity 6.
- **Backend (Servidor)**: Node.js amb Socket.io (Autenticació i Sales).
- **Sincronització de Joc**: **Unity Netcode for GameObjects** per a moviment i animacions.
- **Comunicació**: WebSockets per a dades de sessió i UDP (Netcode) per a moviment.

## Restricció Principal

La restricció principal és que **el moviment ha de ser fluid i sincronitzat** entre ambdós jugadors a la mateixa sala. Això s'assoleix mitjançant:
- **Autoritat del Client**: Resposta instantània sense lag visual per al jugador local.
- **ClientNetworkTransform**: Sincronització física autoritativa.
- **Sincronització d'Animacions**: NetworkAnimator per replicar els estats del personatge.

## Abast

Aquesta especificació cobreix:
1. Gestió d'entrada del moviment amb autoritat del client.
2. Detecció de col·lisions física mitjançant Rigidbody2D.
3. Codi aleatori de 5 caràcters per a la creació de sales privades.
4. Sistema de Seguiment de Càmera Robusta tras canvi d'escena.
5. Sistema de reaparició (Respawn) sincronitzat via ClientRpc.
6. Neteja de ports (7777) i tancament segur de sessions.
