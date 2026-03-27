require('dotenv').config();

const express = require('express');
const cors = require('cors');
const http = require('http');
const { Server } = require('socket.io');
const mongoose = require('mongoose');

const app = express();
const PORT = process.env.PORT || 3000;
const MONGO_URI = process.env.MONGO_URI;

// Funció per connectar amb reintents
async function connectToMongoDB(retries = 5, delay = 3000) {
  for (let i = 0; i < retries; i++) {
    try {
      console.log(`[MONGO] Intentant connexió a MongoDB (intent ${i + 1}/${retries})...`);
      await mongoose.connect(MONGO_URI);
      console.log('Connectat a la base de dades joc_multijugador ✅');
      await initializeDatabase();
      return;
    } catch (err) {
      console.error(`[MONGO] Error a l'intent ${i + 1}:`, err.message);
      if (i < retries - 1) {
        console.log(`[MONGO] Reintentant en ${delay / 1000} segons...`);
        await new Promise(resolve => setTimeout(resolve, delay));
      }
    }
  }
  console.error('[MONGO] Error permanent: No s\'ha pogut connectar a MongoDB');
}

// Iniciar connexió
connectToMongoDB();

const authService = require('./services/AuthService');
const gameService = require('./services/GameService');
const resultService = require('./services/ResultService');

const AuthController = require('./controllers/AuthController');
const GameController = require('./controllers/GameController');
const ResultController = require('./controllers/ResultController');

const authController = new AuthController(authService);
const gameController = new GameController(gameService, authController);
const resultController = new ResultController(resultService);

async function initializeDatabase() {
  console.log('[INIT] Comprovant usuari de proves a la base de dades...');
  try {
    const existingUser = await authService.getUserByUsername('Marta_Test');
    if (!existingUser) {
      await authService.register('Marta_Test', 'test1234');
      console.log('[INIT] Usuari Marta_Test creat correctament! ✅');
    } else {
      console.log('[INIT] Usuari Marta_Test ja existeix.');
    }
  } catch (err) {
    console.error('[INIT] Error inicialitzant la base de dades:', err);
  }
}

app.use(cors({ origin: '*' }));
app.use(express.json());

const server = http.createServer(app);
const io = new Server(server, {
  cors: {
    origin: '*',
    methods: ['GET', 'POST']
  }
});

const sessions = authController.getSessions();
const roomPositions = {};
const roomEnemyPositions = {};
const roomGoalStatus = {};

// =====================
// GESTIÓ DE SOCKET.IO
// =====================

io.on('connection', (socket) => {
  console.log('Nou client connectat:', socket.id);

  // Quan un jugador s'uneix a una sala
  socket.on('joinRoom', (data) => {
    const { roomId, playerId, playerName } = data;
    
    // Unir el socket a la sala de Socket.io
    socket.join(roomId);

    if (!roomPositions[roomId]) {
      roomPositions[roomId] = {};
    }

    // Inicialitzar estat de la meta si no existeix
    if (!roomGoalStatus[roomId]) {
      roomGoalStatus[roomId] = { blocked: false, winner: null };
    }

    roomPositions[roomId][playerId] = { x: 0, y: 0, name: playerName };
    console.log(`[CONNEXIÓ] Jugador ${playerName} s'ha unit a la Sala ${roomId}.`);

    // Enviar posicions existents al nou jugador
    socket.emit('syncPositions', roomPositions[roomId]);

    // Enviar estat de la meta
    socket.emit('goalStatus', roomGoalStatus[roomId]);

    // Notificar a l'altre jugador que hem entrat (per al Lobby)
    socket.to(roomId).emit('jugadorEntrat', { playerId: playerId, playerName: playerName });
  });

  // Quan l'enemic es registra a la sala
  socket.on('registerEnemy', (data) => {
    const { roomId } = data;
    
    if (!roomEnemyPositions[roomId]) {
      roomEnemyPositions[roomId] = { x: 0, y: 0 };
    }

    console.log(`[ENEMIC] Enemic registrat a la sala ${roomId}`);
  });

  // Quan l'host actualitza la posició de l'enemic
  socket.on('enemyMoved', (data) => {
    const { roomId, x, y } = data;

    if (!roomEnemyPositions[roomId]) {
      roomEnemyPositions[roomId] = { x: 0, y: 0 };
    }
    roomEnemyPositions[roomId] = { x, y };

    // Sincronitzar posició de l'enemic a tots els jugadors de la sala
    socket.to(roomId).emit('enemyMovedFromServer', {
      x: x,
      y: y
    });

    //console.log(`[ENEMIC] Posició sincronitzada: (${x}, ${y}) a la sala ${roomId}`);
  });

  // Quan un jugador actualitza la seva posició
  socket.on('updatePosition', (data) => {
    const { roomId, playerId, x, y } = data;

    if (!roomPositions[roomId]) {
      roomPositions[roomId] = {};
    }
    roomPositions[roomId][playerId] = { x, y };

    // Enviar posició als altres jugadors de la mateixa sala
    socket.to(roomId).emit('playerMoved', {
      playerId: playerId,
      x: x,
      y: y
    });

    //console.log(`Posició rebuda de ${playerId}: (${x}, ${y}) a la sala ${roomId}`);
  });

    // Quan un jugador guanya la partida
    socket.on('gameFinished', (data) => {
        const { roomId, winnerId, winnerName } = data;
        
        // Bloquejar la meta perquè l'altre jugador no pugui guanyar
        if (roomGoalStatus[roomId]) {
          roomGoalStatus[roomId].blocked = true;
          roomGoalStatus[roomId].winner = winnerName;
        }
        
        io.to(roomId).emit('goalStatus', roomGoalStatus[roomId]);
        
        io.to(roomId).emit('gameFinished', {
            winnerId: winnerId,
            winnerName: winnerName
        });

        io.to(roomId).emit('playerWon', {
            winnerName: winnerName
        });

        console.log(`[VICTÒRIA] El jugador ${winnerName} ha guanyat a la sala ${roomId}`);
    });

    // Quan un jugador és atrapat per l'enemic (multijugador)
    socket.on('playerCaught', (data) => {
        const { roomId, playerId } = data;
        
        // Notificar a tots els jugadors que facin respawn
        io.to(roomId).emit('playerCaught', {
            playerId: playerId
        });

        // Notificar per fer respawn
        io.to(roomId).emit('doRespawn', {
            playerId: playerId
        });

        console.log(`[ENEMIC] El jugador ${playerId} ha estat atrapat a la sala ${roomId}`);
    });

    // Quan un jugador es desconnecta
    socket.on('jugadorDesconnectat', async (data) => {
    const { playerId } = data;

    // Notificar a l'altre jugador de la mateixa sala
    for (const [roomId, positions] of Object.entries(roomPositions)) {
      if (positions[playerId]) {
        const playerName = positions[playerId].name;
        
        // Eliminar el jugador de les posicions
        delete roomPositions[roomId][playerId];
        
        // Notificar a la sala
        io.to(roomId).emit('jugadorDesconnectat', { playerId: playerId });

        console.log(`[DESCONNEXIÓ] El jugador ${playerName} ha marxat. Sala ${roomId} tancada.`);

        // Actualitzar la sala a MongoDB: treure el jugador
        try {
          await Sala.findOneAndUpdate(
            { _id: roomId, jugadors_actuals: { $in: [Object.keys(positions).find(k => k !== playerId)] } },
            { $pull: { jugadors_actuals: Object.values(positions).find(p => p.name)?.name } },
            { new: true }
          );
        } catch (err) {
          console.error('Error actualitzant la sala:', err);
        }
        break;
      }
    }
  });

  // Quan es perd la connexió del socket
  socket.on('disconnect', () => {
    console.log('Client desconnectat:', socket.id);
  });
});

// =====================
// RUTES D'AUTENTIFICACIÓ (CONTROLLER)
// =====================

app.post('/api/login', (req, res) => authController.login(req, res));
app.post('/api/auth/login', (req, res) => authController.login(req, res));
app.post('/api/auth/register', (req, res) => authController.register(req, res));
app.post('/api/register', (req, res) => authController.register(req, res));
app.get('/api/verify', (req, res) => authController.verify(req, res));
app.get('/api/auth/verify', (req, res) => authController.verify(req, res));

// POST /api/logout - Tancar sessió
app.post('/api/logout', (req, res) => authController.logout(req, res));

// =====================
// RUTES DE GESTIÓ DE SALES (CONTROLLER)
// =====================

app.post('/api/rooms/single', (req, res) => gameController.createSinglePlayer(req, res));
app.post('/api/rooms', (req, res) => gameController.createRoom(req, res));
app.get('/api/rooms', (req, res) => gameController.getRooms(req, res));
app.post('/api/rooms/:roomId/join', (req, res) => gameController.joinRoom(req, res));

// =====================
// RUTES DE RÀNQUINGS (CONTROLLER)
// =====================

app.post('/api/rankings', (req, res) => resultController.saveResult(req, res));
app.get('/api/rankings', (req, res) => resultController.getRankings(req, res));

// Iniciar el servidor
server.listen(PORT, () => {
  console.log(`Servidor executant-se a http://localhost:${PORT}`);
});