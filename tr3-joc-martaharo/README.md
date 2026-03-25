# TR3 - Joc Multiplayer Marta Haro

Projecte DAM per a un joc cooperatiu 2D multijugador online.

## Estructura del Projecte

```
tr3-joc-martaharo/
├── Assets/                   # Projecte Unity
│   └── Scripts/
│       ├── AuthManager.cs
│       ├── MainMenuManager.cs
│       ├── LobbyManager.cs
│       ├── PlayerController.cs
│       ├── EnemyNetworkSync.cs
│       └── GoalZone.cs
├── server/                   # Backend Node.js
│   ├── .env                 # Configuració (NO pujar a GitHub)
│   ├── server.js            # Punt d'entrada
│   ├── controllers/         # Capa HTTP
│   ├── services/           # Lògica de negoci
│   └── repositories/       # Accés a dades
└── openspec/               # Documentació tècnica
```

## Requisits

- **Unity**: 2022.3+ (URP)
- **Node.js**: v16+
- **MongoDB Atlas**: Compte actiu amb base de dades `joc_multijugador`

## Instal·lació i Execució

### 1. Configurar el servidor

```bash
cd server
npm install
```

### 2. Configurar variables d'entorn

Crear fitxer `server/.env`:
```env
PORT=3000
MONGO_URI=mongodb+srv://<usuari>:<contrasenya>@cluster.mongodb.net/joc_multijugador?retryWrites=true&w=majority
```

### 3. Iniciar el servidor

```bash
npm start
```

El servidor s'iniciarà a `http://localhost:3000`

## Arquitectura - Patró Repository (Punt 4.2)

El projecte segueix una arquitectura de 3 capes:

### Capa Repository (Accés a Dades)
- `UserRepository.js` - Gestió d'usuaris
- `GameRepository.js` - Gestió de sales/partides
- `ResultRepository.js` - Gestió de rànquings

### Capa Service (Lògica de Negoci)
- `AuthService.js` - Autenticació amb bcrypt
- `GameService.js` - Lògica de joc (Single/Multiplayer)
- `ResultService.js` - Gestió de resultats

### Capa Controller (HTTP)
- `AuthController.js` - Rutes d'autenticació
- `GameController.js` - Rutes de sales
- `ResultController.js` - Rutes de rànquings

## API Endpoints

### Autenticació
| Mètode | Endpoint | Descripció |
|--------|----------|-------------|
| POST | `/api/register` | Registrar usuari |
| POST | `/api/login` | Iniciar sessió |
| GET | `/api/verify` | Verificar token |
| POST | `/api/logout` | Tancar sessió |

### Sales
| Mètode | Endpoint | Descripció |
|--------|----------|-------------|
| POST | `/api/rooms/single` | Crear partida single-player |
| POST | `/api/rooms` | Crear sala multiplayer |
| GET | `/api/rooms` | Llistar sales (parametre: `?tipus=SINGLE\|MULTIPLAYER`) |
| POST | `/api/rooms/:id/join` | Unir-se a una sala |

### Rànquings
| Mètode | Endpoint | Descripció |
|--------|----------|-------------|
| POST | `/api/rankings` | Guardar puntuació |
| GET | `/api/rankings` | Obtenir rànquing (parametre: `?tipus=SINGLE\|MULTIPLAYER`) |

## Modes de Joc

### Single Player
- El jugador juga sol contra la IA
- La partida comença immediatament
- La puntuació es guarda amb tipus `SINGLE`

### Multiplayer
- Crear sala → Rebre codi de 5 caràcters
- Compartir codi amb el company
- Unir-se amb el codi
- Quan 2 jugadors, la partida comença

## Seguretat

- Contrasenyes encriptades amb bcrypt (salt: 10)
- Variables d'entorn protegides al .env
- Tokens de sessió aleatoris

## Logs en Català

El servidor mostra logs de traçabilitat:
```
[REPOSITORY] Cercant usuari a MongoDB...
[SERVICE] Validant credencials...
[LOGIN] Usuari Maria ha iniciat sessió correctament.
[SALA] Sala ABC12 creada amb èxit.
[GAME] Iniciada partida en mode SOLITARI...
[RANKING] Puntuació guardada per Maria: 100 (SINGLE)
```

---

Projecte TR3DAM 2025-26 - Marta Haro
