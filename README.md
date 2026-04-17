# <h1 align="center">🎮 TR3 - Joc Multiplayer Marta Haro</h1>

<p align="center">
  <img src="https://img.shields.io/badge/STATUS-EN%20DESENVOLUPAMENT-green?style=for-the-badge" alt="Status Badge">
  <img src="https://img.shields.io/badge/PLATAFORMA-PC%20/%20WEB-blue?style=for-the-badge" alt="Platform Badge">
  <img src="https://img.shields.io/badge/ENGINE-UNITY%206-black?style=for-the-badge&logo=unity" alt="Unity Badge">
</p>

<p align="center">
  <strong>Projecte DAM (Desenvolupament d'Aplicacions Multiplataforma)</strong><br>
  Un joc cooperatiu 2D Pixel Art on dos jugadors han de col·laborar en temps real per escapar d'un enemic IA i arribar a una zona de sortida junts.
</p>

---

## 📌 Índex
* [Descripció del Projecte](#-descripció-del-projecte)
* [Estat del Projecte](#-estat-del-projecte)
* [Enllaços d'Interès](#-enllaços-dinterès)
* [Estructura del Repositori](#-estructura-del-repositori)
* [Tecnologies Utilitzades](#-tecnologies-utilitzades)
* [Autors](#-autors)

---

## 👥 Integrants
* **Marta Haro**

## 🎮 Descripció del Projecte
El joc se centra en la cooperació sincrònica i la supervivència. Utilitza una arquitectura híbrida on **Unity 6** gestiona el gameplay i la renderització, mentre que un servidor **Node.js** amb **Socket.io** i **MongoDB** s'encarrega de l'autenticació, la gestió de sales privades i el ranking.

**Característiques clau:**
* **Moviment Fluid:** Implementació de `ClientNetworkTransform` per eliminar el lag visual.
* **Seguretat:** Autenticació amb JWT i xifrat de dades.
* **Cooperació:** Sistema de meta on ambdós jugadors han de ser presents per guanyar.

---

## 🚀 Estat del Projecte
Actualment, el projecte es troba en una fase avançada de desenvolupament:

* ✅ **Autenticació:** Operativa amb validació JWT i persistència en MongoDB.
* ✅ **Sales/Lobby:** Operativa amb Socket.io i codis aleatoris de 5 caràcters.
* ✅ **Moviment Multijugador:** Operatiu amb Unity Netcode (Autoritat del Client).
* ✅ **Resolució de Problemes:** Sistema de reintents de càmera i alliberament de ports implementats.
* 🚧 **IA Enemiga:** En procés de millora de la sincronització en xarxa.

---

## 🔗 Enllaços d'Interès
| Recurs | Enllaç |
| :--- | :--- |
| **Gestor de Tasques (Jira)** | [🌐 Tauler del Projecte](https://jocmultijugador.atlassian.net/jira/software/projects/JOC/boards/2/backlog?selectedIssue=JOC-7) |
| **Prototip Gràfic (Figma)** | [🎨 Veure Disseny](#) |
| **URL de Producció** | 🚀 *Pendent de desplegament* |

---

## 📁 Estructura del Repositori
*Obligatori seguir aquesta estructura:*

* **`/Assets`**: Projecte Unity. Inclou scripts de moviment, `NetworkAnimator` i gestió de col·lisions.
* **`/server`**: Backend Node.js. Lògica de rutes API per a rànquings i servidor de WebSockets.
* **`/openspec`**: Documentació tècnica detallada i especificacions del sistema.

---

## 🛠️ Tecnologies Utilitzades
<div align="center">
  <img src="https://github-readme-tech-stack.vercel.app/api/cards?title=Tech%20Stack&lineCount=2&theme=dark&line1=unity,unity,auto;csharp,csharp,auto;&line2=nodejs,nodejs,auto;mongodb,mongodb,auto;socketio,socketio,auto;" alt="Marta Haro Tech Stack" />
</div>

---

## 👥 Autors
| [<img src="https://github.com/identicons/m.png" width=115><br><sub>**Marta Haro**</sub>](https://github.com/teu-usuari-github) |
| :---: |
| Desenvolupadora Full-stack |

---
<p align="center">
  <em>Aquest README compleix amb l'esquema mínim obligatori de carpetes per als projectes transversals.</em>
</p>
