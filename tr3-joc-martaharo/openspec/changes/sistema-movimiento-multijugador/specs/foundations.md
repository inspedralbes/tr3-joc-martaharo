# Fonaments

## Visió General del Projecte

Aquest projecte és un **joc cooperatiu 2D Pixel Art** on dos jugadors han de col·laborar en temps real per escapar d'un enemic IA i arribar a una zona de sortida junts.

## Stack Tecnològic

- **Frontend (Unity)**: C# amb renderització 2D de Unity
- **Backend (Servidor)**: Node.js amb Socket.io
- **Comunicació**: WebSockets per a sincronització en temps real

## Restricció Principal

La restricció principal és que **el moviment ha de ser fluid i sincronitzat via WebSockets** entre ambdós jugadors a la mateixa sala. Això significa:
- Les actualitzacions de posició s'han d'enviar en temps real
- La latència s'ha de minimitzar
- Ambdós jugadors han de veure els moviments de l'altre sense retard significatiu

## Abast

Aquesta especificació cobreix:
1. Gestió d'entrada del moviment del jugador (Tecles de fletxes / WASD)
2. Detecció de col·lisions amb parets
3. Sincronització de posició en temps real via Socket.io
4. Gestió d'esdeveniments del costat del servidor per a actualitzacions de posició
5. Sistema de multijugador basat en la creació de sales privades mitjançant un codi aleatori de 5 caràcters generat pel servidor
