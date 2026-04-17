# Documentació del Projecte - Joc 2D DAM (Unity 6000.0.69f1)

## 1. Requisits Tècnics

- **Unity**: 6000.0.69f1 (Unity 6)
- **Render Pipeline**: Universal Render Pipeline (URP)
- **ML-Agents**: Versió 4.0.0+ (compatible amb Unity 6)
- **Backend**: Node.js + Express + Socket.io + MongoDB
- **Idioma**: Català

---

## 2. Estructura del Projecte

```
openspec/
├── Assets/_Project/Scripts/
│   ├── Network/
│   │   ├── AuthManager.cs          # Login/Registre
│   │   └── NetworkManager.cs        # Socket.io
│   ├── Player/
│   │   ├── PlayerController.cs      # Moviment
│   │   └── PlayerCameraFollow.cs    # Càmera
│   ├── Game/
│   │   ├── EnemyAI.cs               # IA tradicional
│   │   ├── EnemyAgent.cs            # ML-Agents
│   │   ├── FogOfWar.cs             # Visió limitada
│   │   ├── GoalZone.cs             # Meta
│   │   └── GameManager.cs           # Lògica
│   └── UI/
│       ├── MainMenuManager.cs       # Menú
│       ├── LobbyManager.cs          # Sales
│       └── GameUIManager.cs         # UI joc
│
└── server/
    ├── server.js                    # Entry point
    ├── controllers/                 # HTTP
    ├── services/                   # Lògica negoci
    ├── repositories/               # Dades
    └── models/                     # MongoDB
```

---

## 3. Scripts Creats/Actualitzats

### 3.1 FogOfWar.cs (Unity 6 Compatible)
**Ubicació**: `Assets/_Project/Scripts/Game/FogOfWar.cs`

**Descripció**: Visió limitada amb Light2D (URP). No usa RenderPipelineManager.

**Configuració**:
- Crear un GameObject "FogOfWar" a l'escena
- Assignar script
- Arrossegar Player a playerTarget
- visionRadius: 5 (mida de la visió)

### 3.2 EnemyAgent.cs (ML-Agents)
**Ubicació**: `Assets/_Project/Scripts/Game/EnemyAgent.cs`

**Descripció**: Agent ML per a l'enemic. Hereta de `Agent`. Si ML-Agents no funciona, fa servir IA tradicional.

**Dependències**:
- `com.unity.ml-agents` (versió 4.0.0+)
- `com.unity.ai.inference`

**Configuració**:
```
GameObject "Enemy"
├── Tag: Enemy
├── Rigidbody2D (Dynamic, Freeze Rotation Z)
├── CircleCollider2D (Is Trigger: true)
└── EnemyAgent.cs
    ├── moveSpeed: 3
    ├── chaseRadius: 10
    ├── puntInici: (arrossegar PuntInici)
    └── useMLAgents: true/false
```

**Nota**: Si `useMLAgents = false`, fa servir IA tradicional.

---

## 4. ArQUITECTURA BACKEND DAM

### 4.1 Estructura (Patró Repository)

```
server/
├── controllers/        → Rep peticions HTTP/Socket
│   ├── AuthController.js
│   ├── GameController.js
│   └── ResultController.js
│
├── services/          → Lògica de negoci
│   ├── AuthService.js    (hash bcrypt, validació)
│   ├── GameService.js
│   └── ResultService.js
│
├── repositories/      → Accés a dades
│   ├── UserRepository.js   (Interfície + MongoDB + InMemory)
│   ├── GameRepository.js
│   └── ResultRepository.js
│
└── models/            → Models MongoDB
    ├── User.js
    ├── Room.js
    └── Ranking.js
```

### 4.2 Interfície Repository (Exemple: UserRepository.js)

```javascript
/**
 * INTERFÍCIE PÚBLICA:
 * - findById(id) → Usuari
 * - findByUsername(username) → Usuari
 * - create(userData) → Usuari
 * - update(id, userData) → Usuari
 * - delete(id) → Boolean
 */

// IMPLEMENTACIÓ MONGODB (Producció)
// Implementació InMemory (Testing - comentada)
```

### 4.3 API REST

| Mètode | Ruta | Fitxer |
|--------|------|--------|
| POST | `/api/auth/login` | `controllers/AuthController.js` |
| POST | `/api/auth/register` | `controllers/AuthController.js` |
| POST | `/api/rooms` | `controllers/GameController.js` |
| GET | `/api/rooms` | `controllers/GameController.js` |
| POST | `/api/rooms/:id/join` | `controllers/GameController.js` |
| POST | `/api/rankings` | `controllers/ResultController.js` |
| GET | `/api/rankings` | `controllers/ResultController.js` |

### 4.4 WebSockets (Socket.io)

| Event | Direcció | Fitxer |
|-------|----------|--------|
| `joinRoom` | Client→Server | `server.js` |
| `updatePosition` | Client→Server | `server.js` |
| `playerMoved` | Server→Client | `server.js` |
| `enemyMoved` | Client→Server | `server.js` |
| `enemyMovedFromServer` | Server→Client | `server.js` |
| `gameFinished` | Client→Server | `server.js` |
| `playerCaught` | Client→Server | `server.js` |
| `doRespawn` | Server→Client | `server.js` |

---

## 5. CONFIGURACIÓ UNITY

### 5.1 Tags
**Inspector > Tag > Add Tag**:
- `Player` - Jugador
- `Enemy` - Enemic
- `Meta` - Objectiu

### 5.2 Global Light 2D
1. **Window > Rendering > Lighting**
2. Buscar **Global Light 2D**
3. **Intensity: 0** (foscor total)
4. La llum del FogOfWar serà l'única font de llum

### 5.3 Configuració per Objecte

**Player**:
```
Tag: Player
Rigidbody2D (Dynamic, Freeze Rotation Z)
BoxCollider2D
PlayerController.cs
```

**Enemy** (ML-Agents):
```
Tag: Enemy
Rigidbody2D (Dynamic, Freeze Rotation Z)
CircleCollider2D (Is Trigger: true)
EnemyAgent.cs
```

**Càmera**:
```
Camera (Orthographic)
PlayerCameraFollow.cs
FogOfWar.cs
```

**Meta**:
```
Tag: Meta
BoxCollider2D (Is Trigger: true)
GoalZone.cs
```

**PuntInici**:
```
GameObject buit: "PuntInici"
Transform (position: on comença el jugador)
```

---

## 6. LÒGICA DEL JOC (CATALÀ)

### 6.1 Mode Individual
- Enemy persegueix jugador
- Si toca → **"Game Over"**

### 6.2 Mode Multijugador
- 2 jugadors per sala
- Primer a Meta → **"Has guanyat!"**
- Altre jugador → **"Has perdut!"**
- Si Enemy toca → **Respawn** a puntInici

### 6.3 Interfície (Català)
| Text |
|------|
| "Escriu un nom d'usuari!" |
| "Escriu la contrasenya!" |
| "Has guanyat!" |
| "Game Over" |
| "Torna a jugar" |
| "Menú principal" |
| "Esperant un altre jugador..." |
| "Error de connexió. Servidor actiu?" |

---

## 7. ML-AGENTS (Unity 6)

### 7.1 Instal·lació
1. **Window > Package Manager**
2. **+ > Install package by name**
3. Escriure: `com.unity.ml-agents@4.0.0`

### 7.2 Training (Opcional)
```bash
pip install mlagents
mlagents-learn config.yaml --run-id=enemy_train
```

### 7.3 Configuració EnemyAgent
- `useMLAgents = true`: Usa ML-Agents
- `useMLAgents = false`: IA tradicional (fallback)

---

## 8. Fitxers del Backend

| Fitxer | Carpeta | Responsabilitat |
|--------|---------|-------------------|
| `server.js` | arrel | Entry point, Socket.io |
| `AuthController.js` | controllers | Login/Registre HTTP |
| `GameController.js` | controllers | Sales HTTP |
| `ResultController.js` | controllers | Rànquings HTTP |
| `AuthService.js` | services | Lògica auth (bcrypt) |
| `GameService.js` | services | Lògica sales |
| `ResultService.js` | services | Lògica resultats |
| `UserRepository.js` | repositories | CRUD usuari |
| `GameRepository.js` | repositories | CRUD sales |
| `ResultRepository.js` | repositories | CRUD resultats |
| `User.js` | models | Model MongoDB |
| `Room.js` | models | Model MongoDB |
| `Ranking.js` | models | Model MongoDB |

---

## 9. Errors Comuns i Solucions

### Error: CS0246 RenderingData
**Solució**: No usar RenderPipelineManager. FogOfWar.cs us només Light2D.

### Error: ML-Agents no funciona amb Unity 6
**Solució**: Usar EnemyAgent.cs amb `useMLAgents = false` (IA tradicional).

### Error: Global Light 2D no funciona
**Solució**: Posar Intensity a 0 a Window > Rendering > Lighting.

---

## 10. Ordre d'Escenes

1. `Login` → AuthManager
2. `Menu` → MainMenuManager  
3. `Lobby` → LobbyManager (multijugador)
4. `Joc` → GameManager, PlayerController, EnemyAgent, FogOfWar
