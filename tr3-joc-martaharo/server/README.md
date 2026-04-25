# TR3 - Joc Multiplayer Marta Haro

Servidor Node.js per al joc cooperatiu 2D multijugador.

## Requisits

- Node.js (v16+)
- MongoDB Atlas (compte actiu)

## Instal·lació

1. **Instal·lar dependències:**
   ```bash
   cd server
   npm install
   ```

2. **Configurar variables d'entorn:**

   Crear un fitxer `.env` a la carpeta `server/`:
   ```env
   PORT=3000
   MONGO_URI=mongodb+srv://<usuari>:<contrasenya>@cluster.mongodb.net/joc_multijugador?retryWrites=true&w=majority
   ```

   **Nota:** La contrasenya ha d'estar codificada URL si conté caràcters especials.

## Executar el servidor

```bash
npm start
```

El servidor s'iniciarà a `http://localhost:3000`

## Estructura del Projecte

```
server/
├── .env                 # Variables d'entorn (NO pujar a GitHub)
├── package.json
├── server.js            # Punt d'entrada
├── User.js              # Schema Mongoose (usuaris)
├── Room.js              # Schema Mongoose (sales)
├── Ranking.js           # Schema Mongoose (rankings)
├── repositories/        # Capa d'accés a dades
│   ├── UserRepository.js
│   ├── GameRepository.js
│   └── ResultRepository.js
└── services/            # Capa de lògica de negoci
    ├── AuthService.js
    └── GameService.js
```

## API Endpoints

### Autenticació
- `POST /api/login` - Iniciar sessió
- `POST /api/register` - Registrar usuari
- `GET /api/verify` - Verificar token

### Sales
- `POST /api/rooms/single` - Crear partida single-player
- `POST /api/rooms` - Crear sala multiplayer
- `GET /api/rooms` - Llistar sales (parametre opcional: `?tipus=SINGLE|MULTIPLAYER`)
- `POST /api/rooms/:id/join` - Unir-se a una sala

### Rànquings
- `POST /api/rankings` - Guardar puntuació
- `GET /api/rankings` - Obtenir rànquing (parametre opcional: `?tipus=SINGLE|MULTIPLAYER`)

## Característiques

- **Patró Repository**: Separació de responsabilitats (Repositories → Services → Controllers)
- **Contrasenyes encriptades**: bcrypt amb salt de 10
- **Mode Single Player**: Joca contra la IA
- **Mode Multiplayer**: Crea sales amb codi aleatori de 5 caràcters
- **Sincronització**: Socket.io per a comunicació en temps real
- **Logs en català**: Traçabilitat completa

## Base de Dades

- **Nom**: `joc_multijugador`
- **Col·leccions**:
  - `usuaris` - Usuaris registrats
  - `sales` - Partides actives
  - `rankings` - Puntuacions històriques

---

Projecte TR3 - DAM - Marta Haro
