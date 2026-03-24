const express = require('express');
const cors = require('cors');
const crypto = require('crypto');
const fs = require('fs');

const app = express();
const PORT = 3000;

app.use(cors());
app.use(express.json());

const USERS_FILE = './users.json';
const SESSIONS_FILE = './sessions.json';

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

app.post('/api/register', (req, res) => {
  const { username, password } = req.body;
  
  if (!username || !password) {
    return res.status(400).json({ error: 'Username and password required' });
  }

  const users = loadUsers();
  
  if (users[username]) {
    return res.status(409).json({ error: 'Username already exists' });
  }

  users[username] = { password, createdAt: new Date().toISOString() };
  saveUsers(users);

  const token = generateToken();
  const sessions = loadSessions();
  sessions[token] = { username, createdAt: new Date().toISOString() };
  saveSessions(sessions);

  res.json({ token, username });
});

app.post('/api/login', (req, res) => {
  const { username, password } = req.body;
  
  if (!username || !password) {
    return res.status(400).json({ error: 'Username and password required' });
  }

  const users = loadUsers();
  const user = users[username];

  if (!user || user.password !== password) {
    return res.status(401).json({ error: 'Invalid credentials' });
  }

  const token = generateToken();
  const sessions = loadSessions();
  sessions[token] = { username, createdAt: new Date().toISOString() };
  saveSessions(sessions);

  res.json({ token, username });
});

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

app.post('/api/logout', (req, res) => {
  const token = req.headers.authorization?.replace('Bearer ', '');
  
  if (token) {
    const sessions = loadSessions();
    delete sessions[token];
    saveSessions(sessions);
  }

  res.json({ success: true });
});

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

app.listen(PORT, () => {
  console.log(`Server running on http://localhost:${PORT}`);
});
