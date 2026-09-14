const express = require('express');
const http = require('http');
const { Server } = require('socket.io');
const cors = require('cors');

const app = express();
app.use(cors());
app.use(express.json());

const server = http.createServer(app);
const io = new Server(server, {
  cors: {
    origin: '*',
    methods: ['GET', 'POST']
  }
});

const rooms = new Map();
const players = new Map();

io.on('connection', (socket) => {
  console.log(`Player connected: ${socket.id}`);

  socket.on('joinRoom', ({ roomId, username }) => {
    const room = rooms.get(roomId);
    
    if (!room) {
      socket.emit('error', { message: 'Room not found' });
      return;
    }

    if (room.players.length >= room.maxPlayers) {
      socket.emit('error', { message: 'Room is full' });
      return;
    }

    socket.join(roomId);
    players.set(socket.id, { username, roomId, position: { x: 0, y: 0 } });
    room.players.push({ id: socket.id, username });

    socket.emit('joined', { roomId, players: room.players });
    socket.to(roomId).emit('playerJoined', { id: socket.id, username });
    
    console.log(`${username} joined room ${roomId}`);
  });

  socket.on('createRoom', ({ roomName, username, maxPlayers = 2 }) => {
    const roomId = Math.random().toString(36).substring(2, 10);
    
    rooms.set(roomId, {
      id: roomId,
      name: roomName,
      maxPlayers,
      players: [{ id: socket.id, username }],
      gameState: null
    });

    socket.join(roomId);
    players.set(socket.id, { username, roomId, position: { x: 0, y: 0 } });

    socket.emit('roomCreated', { roomId, room: rooms.get(roomId) });
    console.log(`Room ${roomName} created with id ${roomId}`);
  });

  socket.on('playerMove', ({ position, rotation }) => {
    const player = players.get(socket.id);
    if (!player) return;

    player.position = position;
    player.rotation = rotation;

    socket.to(player.roomId).emit('playerMoved', {
      id: socket.id,
      position,
      rotation
    });
  });

  socket.on('syncState', ({ gameState }) => {
    const player = players.get(socket.id);
    if (!player) return;

    const room = rooms.get(player.roomId);
    if (room) {
      room.gameState = gameState;
      socket.to(player.roomId).emit('stateSync', { gameState });
    }
  });

  socket.on('disconnect', () => {
    const player = players.get(socket.id);
    if (player) {
      const room = rooms.get(player.roomId);
      if (room) {
        room.players = room.players.filter(p => p.id !== socket.id);
        socket.to(player.roomId).emit('playerLeft', { id: socket.id });
        
        if (room.players.length === 0) {
          rooms.delete(player.roomId);
        }
      }
      players.delete(socket.id);
    }
    console.log(`Player disconnected: ${socket.id}`);
  });
});

const PORT = process.env.PORT || 3001;
server.listen(PORT, () => {
  console.log(`Socket.io server running on port ${PORT}`);
});
