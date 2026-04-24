require('dotenv').config();

const express = require('express');
const cors = require('cors');
const http = require('http');
const { Server } = require('socket.io');
const mongoose = require('mongoose');
const crypto = require('crypto');
const bcrypt = require('bcrypt');

const app = express();
const PORT = process.env.PORT || 3000;
const MONGO_URL = process.env.MONGO_URL || "mongodb://joc-mongodb:27017/joc_multijugador";

app.use(cors({ origin: '*', methods: ['GET', 'POST'], allowedHeaders: ['Content-Type'] }));
app.use(express.json());

app.use((req, res, next) => {
  console.log(`[LOG] ${req.method} ${req.url}`);
  next();
});

const userSchema = new mongoose.Schema({
  username: { type: String, required: true, unique: true },
  password: { type: String, required: true }
});

const User = mongoose.model('User', userSchema);

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

const rooms = {};
const roomCodes = {}; // Maps roomCode -> roomId
const roomInfo = {}; // Maps roomId -> { roomId, roomCode, timestamp }
const roomPositions = {};
const roomEnemyPositions = {};
const roomGoalStatus = {};
const hostPlayer = {};

app.get('/api/rooms', (req, res) => {
  console.log('[ROUTE] GET /api/rooms');
  const activeRooms = Object.values(roomInfo).map(room => ({
    id: room.roomId,
    nom_sala: `Sala ${room.roomCode}`,
    codi_sala: room.roomCode
  }));
  res.json(activeRooms);
});

app.post('/api/rooms', async (req, res) => {
  console.log('[ROUTE] POST /api/rooms - Creando sala');
  const roomCode = generateRoomCode();
  const roomId = crypto.randomBytes(12).toString('hex');
  
  rooms[roomId] = [];
  roomInfo[roomId] = {
    roomId: roomId,
    roomCode: roomCode,
    timestamp: Date.now()
  };
  roomCodes[roomCode] = roomId;
  
  res.status(200).json({
    success: true,
    roomId: roomId,
    roomCode: roomCode
  });
});

app.post('/api/auth/register', async (req, res) => {
  const { username, password } = req.body;
  console.log(`[AUTH] Solicitud de registro recibida para: ${username}`);
  
  if (!username || !password) {
    return res.status(400).json({ success: false, error: 'Faltan datos' });
  }
  
  try {
    const existingUser = await User.findOne({ username });
    if (existingUser) {
      return res.status(409).json({ success: false, error: 'El usuario ya existe' });
    }
    
    const hashedPassword = await bcrypt.hash(password, 10);
    const user = new User({ username, password: hashedPassword });
    await user.save();
    
    const token = generateToken();
    const userId = user._id.toString();
    
    console.log(`[AUTH] Usuario registrado: ${username}`);
    res.status(201).json({
      success: true,
      token,
      userId,
      username,
      message: 'Usuario creado correctamente'
    });
  } catch (error) {
    console.error('[AUTH] Error en registro:', error.message);
    res.status(500).json({ success: false, error: 'Error en el servidor' });
  }
});

app.post('/api/auth/login', async (req, res) => {
  console.log('[AUTH] Intento de login:', req.body.username);
  const { username, password } = req.body;
  
  if (!username || !password) {
    console.log('[AUTH] Login fallido: faltan datos');
    return res.status(400).json({ success: false, error: 'Faltan datos' });
  }
  
  try {
    const user = await User.findOne({ username });
    
    if (!user) {
      console.log(`[AUTH] Login fallido: usuario "${username}" no encontrado en la base de datos`);
      return res.status(401).json({ success: false, error: 'Usuario no encontrado' });
    }
    
    const isValid = await bcrypt.compare(password, user.password);
    
    if (!isValid) {
      console.log(`[AUTH] Login fallido: contrasena incorrecta para "${username}"`);
      return res.status(401).json({ success: false, error: 'Contrasena incorrecta' });
    }
    
    const token = generateToken();
    const userId = user._id.toString();
    
    console.log(`[AUTH] Login exitoso para: ${username}`);
    res.json({
      success: true,
      token,
      userId,
      username
    });
  } catch (error) {
    console.error('[AUTH] Error en login:', error.message);
    res.status(500).json({ success: false, error: 'Error en el servidor' });
  }
});

app.post('/api/logout', (req, res) => {
  console.log('[ROUTE] POST /api/logout');
  res.json({ success: true });
});

const server = http.createServer(app);
const io = new Server(server, {
  cors: {
    origin: '*',
    methods: ['GET', 'POST'],
    allowedHeaders: ['Content-Type'],
    credentials: false
  },
  pingTimeout: 60000,
  pingInterval: 25000
});

io.on('connection', (socket) => {
  console.log('[SOCKET] Cliente conectado:', socket.id);

  socket.on('joinRoom', (data) => {
    const { roomId, playerId, playerName } = data;
    console.log(`[SOCKET] ${playerName} intenta entrar a sala ${roomId}`);
    
    if (!rooms[roomId]) {
      rooms[roomId] = [];
      roomPositions[roomId] = {};
      roomGoalStatus[roomId] = { blocked: false, winner: null };
    }
    
    if (rooms[roomId].length >= 2) {
      console.log(`[SOCKET] Sala ${roomId} llena. Rechazando a ${playerName}`);
      socket.emit('roomFull');
      return;
    }
    
    socket.join(roomId);
    
    let playerNumber;
    if (rooms[roomId].length === 0) {
      hostPlayer[roomId] = playerId;
      playerNumber = 1;
      console.log(`[SOCKET] ${playerName} entra como Jugador 1 en sala ${roomId}`);
    } else {
      playerNumber = 2;
      console.log(`[SOCKET] ${playerName} entra como Jugador 2 en sala ${roomId}`);
    }
    
    rooms[roomId].push({ id: playerId, name: playerName, playerNumber: playerNumber });
    roomPositions[roomId][playerId] = { x: 0, y: 0, name: playerName, playerNumber: playerNumber };
    
    const playersList = rooms[roomId].map(p => ({ name: p.name, playerNumber: p.playerNumber }));
    io.to(roomId).emit('updateLobby', { players: playersList });
    socket.emit('syncPositions', roomPositions[roomId]);
    socket.emit('goalStatus', roomGoalStatus[roomId]);
    socket.to(roomId).emit('jugadorEntrat', { playerId, playerName, playerNumber });
    
    if (rooms[roomId].length === 2) {
      console.log(`[SOCKET] Sala ${roomId} completa. Emitiendo roomReady`);
      io.to(roomId).emit('roomReady');
    }
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

        if (rooms[roomId]) {
          rooms[roomId] = rooms[roomId].filter(p => p.id !== playerId);
        }

        io.to(roomId).emit('jugadorDesconnectat', { playerId });

        if (rooms[roomId]) {
          const playersList = rooms[roomId].map(p => ({ name: p.name, playerNumber: p.playerNumber }));
          io.to(roomId).emit('updateLobby', { players: playersList });
        }

        console.log(`[SOCKET] El jugador ${playerName} ha salido de la sala ${roomId}.`);

        if (!rooms[roomId] || rooms[roomId].length === 0) {
          roomIdToCleanup = roomId;
          console.log(`[SOCKET] Eliminando sala ${roomId} (0 jugadores)`);
        }
        break;
      }
    }

    if (roomIdToCleanup) {
      const room = roomInfo[roomIdToCleanup];
      if (room) {
        delete roomCodes[room.roomCode];
      }
      delete roomInfo[roomIdToCleanup];
      delete roomPositions[roomIdToCleanup];
      delete roomEnemyPositions[roomIdToCleanup];
      delete roomGoalStatus[roomIdToCleanup];
      delete rooms[roomIdToCleanup];
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
  console.log(`[SERVER] Servidor ejecutandose en puerto ${PORT}`);
  console.log('[SERVER] Rutas disponibles:');
  console.log('[SERVER]   POST /api/auth/register');
  console.log('[SERVER]   POST /api/auth/login');
  console.log('[SERVER]   GET  /api/rooms');
  console.log('[SERVER]   POST /api/rooms');
  console.log('========================================');
});