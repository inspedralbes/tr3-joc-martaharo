const express = require('express');
const cors = require('cors');
const http = require('http');
const { Server } = require('socket.io');
const crypto = require('crypto');
const mongoose = require('mongoose'); 
const User = require('./User');

const app = express();
const PORT = 3000;
// Usa tu nueva contraseña aquí
const uri = "mongodb+srv://Marta:Samanthaharo@projectemongo.kdmnf8d.mongodb.net/joc_multijugador?retryWrites=true&w=majority&appName=ProjecteMongo";

mongoose.connect(uri)
  .then(() => console.log('Connectat a MongoDB Atlas ✅'))
  .catch(err => console.error('Error connectant a MongoDB ❌:', err));
// CORS desactivat completament per permetre qualsevol connexió
app.use(cors({
  origin: '*'
}));
app.use(express.json());

// Crear el servidor HTTP i el servidor Socket.io
const server = http.createServer(app);
const io = new Server(server, {
  cors: {
    origin: '*',
    methods: ['GET', 'POST']
  }
});

const USERS_FILE = './users.json';
const SESSIONS_FILE = './sessions.json';

// Variables per emmagatzemar les posicions dels jugadors a cada sala
const roomPositions = {};

function loadUsers() {
  if (!fs.existsSync(USERS_FILE)) return {};
  return JSON.parse(fs.readFileSync(USERS_FILE, 'utf8'));
}

function saveUsers(users) {
  fs.writeFileSync(USERS_FILE, JSON.stringify(users, null, 2));
}

function loadSessions() {
  if (!fs.existsSync(SESSIONS_FILE)) return {};
  return JSON.parse(fs.readFileSync(SESSIONS_FILE, 'utf8'));
}

function saveSessions(sessions) {
  fs.writeFileSync(SESSIONS_FILE, JSON.stringify(sessions, null, 2));
}

function generateToken() {
  return crypto.randomBytes(32).toString('hex');
}

// Gestió de connexió Socket.io
io.on('connection', (socket) => {
  console.log('Nou client connectat:', socket.id);

  // Unir-se a una sala
  socket.on('joinRoom', (data) => {
    const { roomId, playerId, playerName } = data;
    socket.join(roomId);
    
    // Inicialitzar la sala si no existeix
    if (!roomPositions[roomId]) {
      roomPositions[roomId] = {};
    }
    
    // Guardar la posició inicial del jugador
    roomPositions[roomId][playerId] = { x: 0, y: 0, name: playerName };
    
    console.log(`Jugador ${playerName} (${playerId}) s'ha unit a la sala ${roomId}`);
    
    // Enviar les posicions existents al nou jugador
    socket.emit('syncPositions', roomPositions[roomId]);
  });

  // Rebre actualització de posició del client
  socket.on('updatePosition', (data) => {
    const { roomId, playerId, x, y } = data;
    
    // Guardar la posició
    if (!roomPositions[roomId]) {
      roomPositions[roomId] = {};
    }
    roomPositions[roomId][playerId] = { x, y };
    
    // FER BROADCAST: enviar la posició a TOTS els altres jugadors de la sala
    socket.to(roomId).emit('playerMoved', {
      playerId: playerId,
      x: x,
      y: y
    });
    
    console.log(`Posició rebuda de ${playerId}: (${x}, ${y}) a la sala ${roomId}`);
  });

  // Gestionar desconnexió
  socket.on('disconnect', () => {
    console.log('Client disconnectat:', socket.id);
  });
});

app.post('/api/register', async (req, res) => {
  const { username, password } = req.body;
  if (!username || !password) return res.status(400).json({ error: 'Username and password required' });

  try {
    const existingUser = await User.findOne({ username });
    if (existingUser) return res.status(409).json({ error: 'Username already exists' });

    const newUser = new User({ username, password });
    await newUser.save();

    const token = generateToken();
    sessions[token] = { username, createdAt: new Date().toISOString() };
    res.json({ token, username });
  } catch (err) {
    res.status(500).json({ error: 'Error al registrar' });
  }
});

app.post('/api/login', async (req, res) => {
  const { username, password } = req.body;
  
  try {
    const user = await User.findOne({ username, password });
    if (!user) return res.status(401).json({ error: 'Invalid credentials' });

    const token = generateToken();
    sessions[token] = { username, createdAt: new Date().toISOString() };
    res.json({ token, username });
  } catch (err) {
    res.status(500).json({ error: 'Error al login' });
  }
});

// API REST per verificar token
app.get('/api/verify', (req, res) => {
  const token = req.headers.authorization?.replace('Bearer ', '');
  
  if (!token) {
    return res.status(401).json({ error: 'Token required' });
  }

  const sessions = loadSessions();
  const session = sessions[token];

  if (!session) {
    return res.status(401).json({ error: 'Invalid token' });
  }

  res.json({ username: session.username, valid: true });
});

// API REST per logout
app.post('/api/logout', (req, res) => {
  const token = req.headers.authorization?.replace('Bearer ', '');
  
  if (token) {
    const sessions = loadSessions();
    delete sessions[token];
    saveSessions(sessions);
  }

  res.json({ success: true });
});

// Gestió de sales
const rooms = {};

app.post('/api/rooms', (req, res) => {
  const token = req.headers.authorization?.replace('Bearer ', '');
  
  if (!token) {
    return res.status(401).json({ error: 'Token required' });
  }

  const sessions = loadSessions();
  const session = sessions[token];

  if (!session) {
    return res.status(401).json({ error: 'Invalid token' });
  }

  const { roomName, maxPlayers = 2 } = req.body;
  
  if (!roomName) {
    return res.status(400).json({ error: 'Room name required' });
  }

  const roomId = crypto.randomBytes(8).toString('hex');
  rooms[roomId] = {
    id: roomId,
    name: roomName,
    maxPlayers,
    players: [session.username],
    createdBy: session.username,
    createdAt: new Date().toISOString()
  };

  res.json({ roomId, room: rooms[roomId] });
});

app.get('/api/rooms', (req, res) => {
  const roomList = Object.values(rooms).map(r => ({
    id: r.id,
    name: r.name,
    players: r.players.length,
    maxPlayers: r.maxPlayers,
    createdBy: r.createdBy
  }));
  res.json(roomList);
});

app.get('/api/rooms/:roomId', (req, res) => {
  const room = rooms[req.params.roomId];
  if (!room) {
    return res.status(404).json({ error: 'Room not found' });
  }
  res.json(room);
});

app.post('/api/rooms/:roomId/join', (req, res) => {
  const token = req.headers.authorization?.replace('Bearer ', '');
  
  if (!token) {
    return res.status(401).json({ error: 'Token required' });
  }

  const sessions = loadSessions();
  const session = sessions[token];

  if (!session) {
    return res.status(401).json({ error: 'Invalid token' });
  }

  const room = rooms[req.params.roomId];
  if (!room) {
    return res.status(404).json({ error: 'Room not found' });
  }

  if (room.players.length >= room.maxPlayers) {
    return res.status(400).json({ error: 'Room is full' });
  }

  if (room.players.includes(session.username)) {
    return res.status(400).json({ error: 'Already in room' });
  }

  room.players.push(session.username);
  res.json({ room });
});

// Iniciar el servidor
server.listen(PORT, () => {
  console.log(`Servidor executant-se a http://localhost:${PORT}`);
});
