# Uso del servidor y cliente Unity

## Servidor Node.js

### Instalación
```bash
npm install
```

### Iniciar servidor
```bash
npm start
```

El servidor escucha en `http://localhost:3000`

### Endpoints API REST

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/register` | Registrar usuario {username, password} |
| POST | `/api/login` | Login usuario {username, password} |
| GET | `/api/verify` | Verificar token (Header: Authorization: Bearer <token>) |
| POST | `/api/logout` | Cerrar sesión |
| POST | `/api/rooms` | Crear sala {roomName, maxPlayers} (Header: Bearer <token>) |
| GET | `/api/rooms` | Listar salas disponibles |
| GET | `/api/rooms/:roomId` | Ver info de una sala |
| POST | `/api/rooms/:roomId/join` | Unirse a una sala (Header: Bearer <token>) |

### Respuestas

**Register/Login exitosos:**
```json
{
  "token": "abc123...",
  "username": "player1"
}
```

**Crear sala:**
```json
{
  "roomId": "abc123",
  "room": { "id": "abc123", "name": "Sala1", "maxPlayers": 2, "players": ["player1"], ... }
}
```

**Error:**
```json
{
  "error": "Mensaje de error"
}
```

## Unity - AuthManager.cs

### Uso en escena

1. Añadir `AuthManager.cs` a un GameObject en la escena
2. Configurar `BASE_URL` si el servidor está en otra máquina
3. Usar los eventos o propiedades:

```csharp
// Login
AuthManager.Instance.Login(username, password);
// Callback
AuthManager.Instance.OnLoginResponse += (success, username) => { ... };

// Registro
AuthManager.Instance.Register(username, password);

// Crear sala
StartCoroutine(CreateRoom(roomName, (success, roomId) => { ... }));

// Unirse a sala
StartCoroutine(JoinRoom(roomId, (success, room) => { ... }));

// Listar salas
StartCoroutine(GetRooms((rooms) => { ... }));

// Logout
AuthManager.Instance.Logout();
```

### Propiedades
- `IsLoggedIn`: Boolean indicando si hay sesión activa
- `AuthToken`: Token actual para usar en WebSocket
- `CurrentUsername`: Nombre de usuario logueado
