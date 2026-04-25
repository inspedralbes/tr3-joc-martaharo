require('dotenv').config();

const express = require('express');
const cors = require('cors');
const http = require('http');
const { Server } = require('socket.io');
const mongoose = require('mongoose');
const crypto = require('crypto');
const bcrypt = require('bcrypt');

const app = express();
const PORT = 3000;

// DIRECCIÓN DE ATLAS FORZADA PARA MARÍA (Garantiza que se guarde en la nube)
const MONGO_URL = "mongodb+srv://Marta:Samanthaharo@projectemongo.kdmnf8d.mongodb.net/joc_multijugador?retryWrites=true&w=majority&appName=ProjecteMongo";

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

const User = mongoose.model('User', userSchema, 'usuaris');

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

// --- RUTAS API ---

app.get('/api/rooms', (req, res) => {
  const activeRooms = Object.values(roomInfo).map(room => ({
    id: room.roomId,
    nom_sala: `Sala ${room.roomCode}`,
    codi_sala: room.roomCode
  }));
  res.json(activeRooms);
});

// Crear Sala
app.post('/api/rooms', async (req, res) => {
  const roomCode = generateRoomCode();
  const roomId = crypto.randomBytes(12).toString('hex');
  
  rooms[roomId] = [];
  roomInfo[roomId] = { roomId, roomCode, timestamp: Date.now() };
  roomCodes[roomCode] = roomId;
  
  console.log(`[ROOM] Creada sala: ${roomCode} (${roomId})`);
  res.status(200).json({ success: true, roomId, roomCode });
});

// Unirse a Sala (LA RUTA QUE FALTABA)
app.post('/api/rooms/join', (req, res) => {
  const { roomCode } = req.body;
  console.log(`[ROOM] Intento de unión a código: ${roomCode}`);
  
  const roomId = roomCodes[roomCode];
  if (roomId && roomInfo[roomId]) {
    res.json({ success: true, roomId, roomCode });
  } else {
    res.status(404).json({ success: false, error: 'Sala no trobada' });
  }
});

// Registro
app.post('/api/auth/register', async (req, res) => {
  const { username, password } = req.body;
  if (!username || !password) return res.status(400).json({ success: false, error: 'Falten dades' });
  
  try {
    const existingUser = await User.findOne({ username });
    if (existingUser) return res.status(409).json({ success: false, error: 'Usuari ja existeix' });
    
    const hashedPassword = await bcrypt.hash(password, 10);
    const user = new User({ username, password: hashedPassword });
    await user.save();
    
    console.log(`[DB] Guardat en Atlas: ${username}`);
    res.status(201).json({ success: true, token: generateToken(), username });
  } catch (error) {
    res.status(500).json({ success: false, error: error.message });
  }
});

// Login
app.post('/api/auth/login', async (req, res) => {
  const { username, password } = req.body;
  try {
    const user = await User.findOne({ username });
    if (!user || !(await bcrypt.compare(password, user.password))) {
      return res.status(401).json({ success: false, error: 'Usuari/Pass malament' });
    }
    res.json({ success: true, token: generateToken(), username: user.username });
  } catch (error) {
    res.status(500).json({ success: false, error: error.message });
  }
});

const server = http.createServer(app);
const io = new Server(server, { cors: { origin: '*' } });

// --- SOCKETS ---

io.on('connection', (socket) => {
  console.log('[SOCKET] Cliente conectado:', socket.id);

  socket.on('joinRoom', (data) => {
    const { roomId, playerId, playerName } = data;
    if (!rooms[roomId]) {
      rooms[roomId] = [];
      roomPositions[roomId] = {};
      roomGoalStatus[roomId] = { blocked: false, winner: null };
    }
    
    if (rooms[roomId].length >= 2) return socket.emit('roomFull');

    socket.join(roomId);
    let playerNumber = rooms[roomId].length === 0 ? 1 : 2;
    if (playerNumber === 1) hostPlayer[roomId] = playerId;

    rooms[roomId].push({ id: playerId, name: playerName, playerNumber });
    roomPositions[roomId][playerId] = { x: 0, y: 0, name: playerName, playerNumber };

    io.to(roomId).emit('updateLobby', { players: rooms[roomId] });
    if (rooms[roomId].length === 2) io.to(roomId).emit('roomReady');
    console.log(`[SOCKET] ${playerName} entró en sala ${roomId}`);
  });

  socket.on('updatePosition', (data) => {
    const { roomId, playerId, x, y } = data;
    socket.to(roomId).emit('playerMoved', { playerId, x, y });
  });

  socket.on('disconnect', () => {
    console.log('[SOCKET] Desconectado:', socket.id);
  });
});

mongoose.connect(MONGO_URL)
  .then(() => console.log('[MONGO] ¡CONECTADO CON ÉXITO A ATLAS!'))
  .catch(err => console.error('[MONGO] Error Atlas:', err.message));

server.listen(PORT, '0.0.0.0', () => {
  console.log('========================================');
  console.log(`[SERVER] Servidor 100% Atlas en puerto ${PORT}`);
  console.log('========================================');
});