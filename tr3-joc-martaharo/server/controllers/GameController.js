class GameController {
  constructor(gameService, authController) {
    this.gameService = gameService;
    this.authController = authController;
  }

  generateRoomCode() {
    const chars = 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789';
    let code = '';
    for (let i = 0; i < 5; i++) {
      code += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    return code;
  }

  async createSinglePlayer(req, res) {
    const token = req.headers.authorization?.replace('Bearer ', '');
    const sessions = this.authController.getSessions();
    
    if (!token || !sessions[token]) {
      return res.status(401).json({ error: 'Cal iniciar sessió' });
    }

    const session = sessions[token];

    try {
      console.log('[SERVICE] Creant partida single-player...');
      const room = await this.gameService.startSinglePlayerGame(session.username);
      
      console.log(`[GAME] Iniciada partida en mode SOLITARI per a l'usuari ${session.username}.`);
      res.json({ roomId: room._id, roomCode: room.codi_sala, room: room, tipus: 'SINGLE' });
    } catch (err) {
      console.error('[SERVICE] Error creant partida single-player:', err);
      res.status(500).json({ error: 'Error al crear la partida single-player' });
    }
  }

  async createRoom(req, res) {
    const token = req.headers.authorization?.replace('Bearer ', '');
    
    if (!token) {
      return res.status(401).json({ error: 'Cal iniciar sessió' });
    }

    const sessions = this.authController.getSessions();
    const session = sessions[token];
    
    if (!session) {
      return res.status(401).json({ error: 'Cal iniciar sessió' });
    }
    
    try {
      console.log('[SERVICE] Creant sala multiplayer...');
      const roomCode = this.generateRoomCode();
      const nomSala = `Sala ${roomCode}`; // Nom automàtic basat en el codi
      
      const room = await this.gameService.createRoom({
        nom_sala: nomSala,
        id_creador: session.username,
        codi_sala: roomCode,
        jugadors_actuals: [session.username],
        tipus: 'MULTIPLAYER',
        estat: 'esperant'
      });
      
      console.log(`[SALA] Sala ${roomCode} creada amb èxit.`);
      res.json({ roomId: room._id, roomCode: roomCode, room: room, tipus: 'MULTIPLAYER' });
      
    } catch (err) {
      console.error('[SERVICE] Error creant sala:', err);
      res.status(500).json({ error: 'Error al crear la sala' });
    }
  }

  async getRooms(req, res) {
    const { tipus } = req.query;
    
    try {
      console.log(`[SERVICE] Obtenint sales (tipus: ${tipus || 'totes'})...`);
      const rooms = await this.gameService.getAllRooms();
      
      const filteredRooms = tipus ? rooms.filter(r => r.tipus === tipus) : rooms;
      
      const roomList = filteredRooms.map(s => ({
        id: s._id,
        nom_sala: s.nom_sala,
        codi_sala: s.codi_sala,
        jugadors_actuals: s.jugadors_actuals.length,
        estat: s.estat,
        tipus: s.tipus,
        id_creador: s.id_creador
      }));
      
      res.json(roomList);
    } catch (err) {
      console.error('[SERVICE] Error obtenint sales:', err);
      res.status(500).json({ error: 'Error al obtenir sales' });
    }
  }

  async joinRoom(req, res) {
    const token = req.headers.authorization?.replace('Bearer ', '');
    const sessions = this.authController.getSessions();
    
    if (!token || !sessions[token]) {
      return res.status(401).json({ error: 'Cal iniciar sessió' });
    }

    const session = sessions[token];
    const { roomId } = req.params;
    
    try {
      console.log(`[SERVICE] Usuari ${session.username} unint-se a la sala...`);
      const room = await this.gameService.joinRoom(roomId, session.username);
      
      const roomCode = room.codi_sala || room._id.toString().slice(-5).toUpperCase();
      console.log(`[CONNEXIÓ] Jugador ${session.username} s'ha unit a la Sala ${roomCode}.`);
      
      if (room.jugadors_actuals.length >= 2) {
        console.log(`[SALA] La Sala ${roomCode} ja està plena (2/2). Començant partida...`);
      }
      
      res.json({ room: room, roomCode: roomCode });
      
    } catch (err) {
      console.error('[SERVICE] Error unint a sala:', err.message);
      res.status(400).json({ error: err.message || 'Error al unir-se a la sala' });
    }
  }
}

module.exports = GameController;
