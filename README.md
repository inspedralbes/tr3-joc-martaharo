# <h1 align="center">🎮 TR3 - Joc Multiplayer Marta Haro</h1>

<p align="center">
  <img src="BIRDSPRITESHEET_Blue.gif" alt="Blue Bird Animation" width="180">
</p>

<p align="center">
  <img src="https://img.shields.io/badge/STATUS-EN%20DESENVOLUPAMENT-green?style=for-the-badge" alt="Status Badge">
  <img src="https://img.shields.io/badge/PLATAFORMA-PC%20/%20WEB-blue?style=for-the-badge" alt="Platform Badge">
  <img src="https://img.shields.io/badge/ENGINE-UNITY%206-black?style=for-the-badge&logo=unity" alt="Unity Badge">
</p>

<p align="center">
  <strong>Projecte DAM (Desenvolupament d'Aplicacions Multiplataforma)</strong><br>
  Un joc cooperatiu 2D en línia on la col·laboració és la clau per a la supervivència.
</p>

---

## 👥 Integrants
* **Marta Haro**

## 🎮 Descripció del Projecte
Aquest és un joc cooperatiu 2D amb estètica Pixel Art dissenyat per a dos jugadors. L'objectiu principal és col·laborar en temps real per esquivar els enemics controlats per la IA i arribar conjuntament a la zona de meta. El projecte integra un client desenvolupat en **Unity** i un servidor robust en **Node.js**.

---

## 🚀 Estat del Projecte
Actualment, el projecte es troba en una fase avançada de desenvolupament:
* ✅ **Autenticació:** Sistema de login i registre totalment operatiu.
* ✅ **Sales/Lobby:** Gestió de partides mitjançant Socket.io.
* ✅ **Sincronització:** Moviment multijugador implementat amb Netcode (Client Authority).
* ✅ **Estabilitat:** Sistemes de reintents de càmera i gestió de ports optimitzats.

---

## 📊 Diagrames del Sistema

### 👤 Casos d'Ús
Defineix les interaccions dels jugadors amb el sistema (Login, Creació de Sala, Joc).
![Diagrama de Casos d'Ús](docs/casos_us.png)

### 🔄 Seqüència: Reserva i Compra (Socket.IO)
Flux de dades en temps real per a la gestió de sessions i reserves mitjançant WebSockets.
![Diagrama de Seqüència](docs/sequencia_socket.png)

### 🗄️ Entitat-Relació
Estructura de la base de dades (usuaris, partides, estadístiques).
![Diagrama Entitat-Relació](docs/entitat_relacio.png)

### 🏗️ Arquitectura de Microserveis
Organització modular del backend i la seva comunicació amb el client Unity.
![Diagrama de Microserveis](docs/microserveis.png)

---

## 🔗 Enllaços d'interès
| Recurs | Enllaç |
| :--- | :--- |
| **Gestor de tasques (Jira)** | [🌐 Tauler del Projecte](https://jocmultijugador.atlassian.net/jira/software/projects/JOC/boards/2/backlog?selectedIssue=JOC-7) |
| **Prototip gràfic** | [🎨 Enllaç a Figma/Penpot](#) |
| **URL de producció** | 🚀 *(Pendent de desplegament)* |

---

## 📁 Estructura del Repositori
* **`/Assets`**: Projecte complet d'Unity (Scripts de C#, prefabs i assets gràfics).
* **`/server`**: Backend desenvolupat en Node.js (API i WebSockets).
* **`/openspec`**: Documentació tècnica detallada i diagrames.

---

## 🛠️ Tecnologies Utilitzades
<div align="center">
  <img src="https://github-readme-tech-stack.vercel.app/api/cards?title=Tech%20Stack&lineCount=2&theme=dark&line1=unity,unity,auto;csharp,csharp,auto;&line2=nodejs,nodejs,auto;mongodb,mongodb,auto;socketio,socketio,auto;" alt="Marta Haro Tech Stack" />
</div>

---

## 👤 Autoria
* **Marta Haro** - *Desenvolupament Full-stack*
* [GitHub Profile](https://github.com/inspedralbes/tr3-joc-martaharo)
