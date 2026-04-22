require('dotenv').config();

const express = require('express');
const cors = require('cors');
const http = require('http');
const { Server } = require('socket.io');
const mongoose = require('mongoose');
const crypto = require('crypto');

const app = express();
const PORT = process.env.PORT || 3000;
const MONGO_URL = process.env.MONGO_URI || process.env.MONGO_URL || "mongodb://joc-mongodb:27017/joc_multijugador";

app.use(cors({ origin: '*' }));
app.use(express.json());

const sessions = {};

function generateRoomCode() {
  const chars = 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789';
  let code = '';
  for (let i = 0; i < 5; i++) {
    code += chars.charAt(Math.floor(Math.random() * chars.length));
  }
  return code;
}

function generateToken() {
  return crypto.randomBytes(32).toString('hex');
}

app.use((req, res, next) => {
  console.log(`[LOG] ${req.method} ${req.url}`);
  next();
});

app.get('/api/rooms', (req, res) => {
  console.log('[ROUTE] GET /api/rooms - Devolviendo array vacío');
  res.json([]);
});

app.post('/api/rooms', (req, res) => {
  console.log('[ROUTE] POST /api/rooms - Creando sala');
  const roomCode = generateRoomCode();
  const roomId = crypto.randomBytes(12).toString('hex');
  
  // Guardem la sala en memòria perquè el socket la trobi
  roomPlayers[roomId] = []; 
  
  res.status(200).json({ 
    success: true,
    roomId: roomId, 
    roomCode: roomCode 
  });
});

// MODIFICA EL REGISTRE
app.post('/api/auth/register', (req, res) => {
  console.log('[ROUTE] POST /api/auth/register');
  const { username, password } = req.body;
  
  if (!username || !password) {
    return res.status(400).json({ success: false, error: 'Faltan datos' });
  }

  const token = generateToken();
  const userId = crypto.randomBytes(4).toString('hex'); // Generem un ID temporal
  sessions[token] = { username, userId };

  console.log(`[AUTH] Usuario registrado: ${username}`);
  // Enviem success: true perquè Unity ho sàpiga segur
  res.status(201).json({ 
    success: true,
    token, 
    userId,
    username, 
    message: 'Usuario creado correctamente' 
  });
});

// MODIFICA EL LOGIN
app.post('/api/auth/login', (req, res) => {
  console.log('[ROUTE] POST /api/auth/login');
  const { username, password } = req.body;

  if (!username || !password) {
    return res.status(400).json({ success: false, error: 'Faltan datos' });
  }

  // Per ara fem "bypass": si posen qualsevol dada, els loguegem
  const token = generateToken();
  const userId = crypto.randomBytes(4).toString('hex');
  sessions[token] = { username, userId };

  console.log(`[AUTH] Usuario logueado: ${username}`);
  res.json({ 
    success: true,
    token, 
    userId,
    username 
  });
});

app.post('/api/logout', (req, res) => {
  console.log('[ROUTE] POST /api/logout');
  res.json({ success: true });
});

app.get('/api/verify', (req, res) => {
  console.log('[ROUTE] GET /api/verify');
  const token = req.headers.authorization?.replace('Bearer ', '');
  if (token && sessions[token]) {
    res.json({ username: sessions[token].username, valid: true });
  } else {
    res.status(401).json({ error: 'Token inválido' });
  }
});

const server = http.createServer(app);
const io = new Server(server, {
  cors: {
    origin: '*',
    methods: ['GET', 'POST']
  }
});

const roomPositions = {};
const roomEnemyPositions = {};
const roomGoalStatus = {};
const roomPlayers = {};
const hostPlayer = {};

io.on('connection', (socket) => {
  console.log('[SOCKET] Cliente conectado:', socket.id);

  socket.on('joinRoom', (data) => {
    const { roomId, playerId, playerName } = data;
    socket.join(roomId);

    if (!roomPositions[roomId]) roomPositions[roomId] = {};
    if (!roomPlayers[roomId]) roomPlayers[roomId] = [];

    let playerNumber = 'Jugador 1';
    if (roomPlayers[roomId].length === 0) {
      hostPlayer[roomId] = playerId;
      playerNumber = 'Jugador 1';
    } else if (roomPlayers[roomId].length === 1) {
      playerNumber = 'Jugador 2';
    }

    roomPlayers[roomId].push({ id: playerId, name: playerName, playerNumber: playerNumber });

    if (!roomGoalStatus[roomId]) {
      roomGoalStatus[roomId] = { blocked: false, winner: null };
    }

    roomPositions[roomId][playerId] = { x: 0, y: 0, name: playerName, playerNumber: playerNumber };
    console.log(`[SOCKET] ${playerName} se unió a la Sala ${roomId} como ${playerNumber}`);

    const playersList = roomPlayers[roomId].map(p => ({ name: p.name, playerNumber: p.playerNumber }));
    io.to(roomId).emit('updateLobby', { players: playersList });

    socket.emit('syncPositions', roomPositions[roomId]);
    socket.emit('goalStatus', roomGoalStatus[roomId]);

    socket.to(roomId).emit('jugadorEntrat', { playerId: playerId, playerName: playerName, playerNumber: playerNumber });
  });

  socket.on('registerEnemy', (data) => {
    const { roomId } = data;
    if (!roomEnemyPositions[roomId]) {
      roomEnemyPositions[roomId] = { x: 0, y: 0 };
    }
    console.log(`[SOCKET] Enemigo registrado en sala ${roomId}`);
  });

  socket.on('enemyMoved', (data) => {
    const { roomId, x, y } = data;
    if (!roomEnemyPositions[roomId]) {
      roomEnemyPositions[roomId] = { x: 0, y: 0 };
    }
    roomEnemyPositions[roomId] = { x, y };
    socket.to(roomId).emit('enemyMovedFromServer', { x, y });
  });

  socket.on('updatePosition', (data) => {
    const { roomId, playerId, x, y } = data;
    if (!roomPositions[roomId]) {
      roomPositions[roomId] = {};
    }
    roomPositions[roomId][playerId] = { x, y };
    socket.to(roomId).emit('playerMoved', { playerId, x, y });
  });

  socket.on('gameFinished', (data) => {
    const { roomId, winnerId, winnerName } = data;
    if (roomGoalStatus[roomId]) {
      roomGoalStatus[roomId].blocked = true;
      roomGoalStatus[roomId].winner = winnerName;
    }
    io.to(roomId).emit('goalStatus', roomGoalStatus[roomId]);
    io.to(roomId).emit('gameFinished', { winnerId, winnerName });
    io.to(roomId).emit('playerWon', { winnerName });
    console.log(`[SOCKET] El jugador ${winnerName} ha ganado en la sala ${roomId}`);
  });

  socket.on('playerCaught', (data) => {
    const { roomId, playerId } = data;
    io.to(roomId).emit('playerCaught', { playerId });
    io.to(roomId).emit('doRespawn', { playerId });
    console.log(`[SOCKET] El jugador ${playerId} fue atrapado en la sala ${roomId}`);
  });

  socket.on('startGame', (data) => {
    const { roomId, playerId } = data;
    if (hostPlayer[roomId] === playerId) {
      console.log(`[SOCKET] El host inicia la partida en la sala ${roomId}`);
      io.to(roomId).emit('startGame', { roomId: roomId });
    }
  });

  socket.on('jugadorDesconnectat', async (data) => {
    const { playerId } = data;
    let roomIdToCleanup = null;

    for (const [roomId, positions] of Object.entries(roomPositions)) {
      if (positions[playerId]) {
        const playerName = positions[playerId].name;
        delete roomPositions[roomId][playerId];

        if (roomPlayers[roomId]) {
          roomPlayers[roomId] = roomPlayers[roomId].filter(p => p.id !== playerId);
        }

        io.to(roomId).emit('jugadorDesconnectat', { playerId });

        if (roomPlayers[roomId]) {
          const playersList = roomPlayers[roomId].map(p => ({ name: p.name, playerNumber: p.playerNumber }));
          io.to(roomId).emit('updateLobby', { players: playersList });
        }

        console.log(`[SOCKET] El jugador ${playerName} ha salido de la sala ${roomId}.`);

        if (!roomPlayers[roomId] || roomPlayers[roomId].length === 0) {
          roomIdToCleanup = roomId;
          console.log(`[SOCKET] Eliminando sala ${roomId} (0 jugadores)`);
        }
        break;
      }
    }

    if (roomIdToCleanup) {
      delete roomPositions[roomIdToCleanup];
      delete roomEnemyPositions[roomIdToCleanup];
      delete roomGoalStatus[roomIdToCleanup];
      delete roomPlayers[roomIdToCleanup];
      delete hostPlayer[roomIdToCleanup];
    }
  });

  socket.on('disconnect', () => {
    console.log('[SOCKET] Cliente desconectado:', socket.id);
  });
});

mongoose.connect(MONGO_URL).then(() => {
  console.log('[MONGO] Conectado a MongoDB');
}).catch(err => {
  console.error('[MONGO] Error conectando:', err.message);
});

server.listen(PORT, '0.0.0.0', () => {
  console.log('========================================');
  console.log(`[SERVER] Servidor ejecutándose en puerto ${PORT}`);
  console.log('[SERVER] Rutas disponibles:');
  console.log('[SERVER]   POST /api/auth/register');
  console.log('[SERVER]   POST /api/auth/login');
  console.log('[SERVER]   GET  /api/rooms');
  console.log('[SERVER]   POST /api/rooms');
  console.log('========================================');
});
