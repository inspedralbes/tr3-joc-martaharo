const express = require('express');
const cors = require('cors');
const http = require('http');
const { Server } = require('socket.io');
const crypto = require('crypto');
const mongoose = require('mongoose');
const User = require('./User');

const app = express();
const PORT = 3000;

// MongoDB Atlas Connection
const uri = "mongodb+srv://Marta:Samanthaharo@projectemongo.kdmnf8d.mongodb.net/joc_multijugador?retryWrites=true&w=majority&appName=ProjecteMongo";

mongoose.connect(uri)
  .then(() => console.log('Connectat a MongoDB Atlas ✅'))
  .catch(err => console.error('Error connectant a MongoDB ❌:', err));

app.use(cors({ origin: '*' }));
app.use(express.json());

const server = http.createServer(app);
const io = new Server(server, {
  cors: {
    origin: '*',
    methods: ['GET', 'POST']
  }
});

// Memory storage for sessions and room positions
const sessions = {};
const roomPositions = {};
const rooms = {};

function generateToken() {
  return crypto.randomBytes(32).toString('hex');
}

// Socket.io connection management
io.on('connection', (socket) => {
  console.log('Nou client connectat:', socket.id);

  socket.on('joinRoom', (data) => {
    const { roomId, playerId, playerName } = data;
    socket.join(roomId);

    if (!roomPositions[roomId]) {
      roomPositions[roomId] = {};
    }

    roomPositions[roomId][playerId] = { x: 0, y: 0, name: playerName };
    console.log(`Jugador ${playerName} (${playerId}) s'ha unit a la sala ${roomId}`);

    socket.emit('syncPositions', roomPositions[roomId]);
  });

  socket.on('updatePosition', (data) => {
    const { roomId, playerId, x, y } = data;

    if (!roomPositions[roomId]) {
      roomPositions[roomId] = {};
    }
    roomPositions[roomId][playerId] = { x, y };

    // Broadcast player movement to everyone in the room except the sender
    socket.to(roomId).emit('updatePosition', {
      playerId: playerId,
      x: x,
      y: y
    });

    console.log(`Posició rebuda de ${playerId}: (${x}, ${y}) a la sala ${roomId}`);
  });

  socket.on('disconnect', () => {
    console.log('Client disconnectat:', socket.id);
  });
});

// Authentication Routes
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
    console.error('Error al registrar:', err);
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
    console.error('Error al login:', err);
    res.status(500).json({ error: 'Error al login' });
  }
});

app.get('/api/verify', (req, res) => {
  const token = req.headers.authorization?.replace('Bearer ', '');
  if (!token) return res.status(401).json({ error: 'Token required' });

  const session = sessions[token];
  if (!session) return res.status(401).json({ error: 'Invalid token' });

  res.json({ username: session.username, valid: true });
});

app.post('/api/logout', (req, res) => {
  const token = req.headers.authorization?.replace('Bearer ', '');
  if (token) delete sessions[token];
  res.json({ success: true });
});

// Room Management
app.post('/api/rooms', (req, res) => {
  const token = req.headers.authorization?.replace('Bearer ', '');
  if (!token || !sessions[token]) return res.status(401).json({ error: 'Valid token required' });

  const session = sessions[token];
  const { roomName, maxPlayers = 2 } = req.body;
  if (!roomName) return res.status(400).json({ error: 'Room name required' });

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

app.post('/api/rooms/:roomId/join', (req, res) => {
  const token = req.headers.authorization?.replace('Bearer ', '');
  if (!token || !sessions[token]) return res.status(401).json({ error: 'Valid token required' });

  const session = sessions[token];
  const room = rooms[req.params.roomId];
  if (!room) return res.status(404).json({ error: 'Room not found' });

  if (room.players.length >= room.maxPlayers) return res.status(400).json({ error: 'Room is full' });
  if (room.players.includes(session.username)) return res.status(400).json({ error: 'Already in room' });

  room.players.push(session.username);
  res.json({ room });
});

server.listen(PORT, () => {
  console.log(`Servidor executant-se a http://localhost:${PORT}`);
});
