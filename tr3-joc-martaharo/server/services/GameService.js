const gameRepository = require('../repositories/GameRepository');
const resultRepository = require('../repositories/ResultRepository');

class GameService {
  async createRoom(roomData) {
    console.log(`[SERVICE] Creant sala de tipus ${roomData.tipus}...`);
    return await gameRepository.createRoom(roomData);
  }

  async createSinglePlayerRoom(username) {
    console.log(`[SERVICE] Creant sala single-player per ${username}...`);
    const roomCode = this.generateRoomCode();
    
    return await gameRepository.createRoom({
      nom_sala: `Partida de ${username}`,
      id_creador: username,
      codi_sala: roomCode,
      jugadors_actuals: [username],
      tipus: 'SINGLE',
      estat: 'jugant'
    });
  }

  async getAllRooms() {
    return await gameRepository.findAllRooms();
  }

  async getRoomById(roomId) {
    return await gameRepository.findRoomById(roomId);
  }

  async joinRoom(roomId, username) {
    const room = await gameRepository.findRoomById(roomId);
    
    if (!room) {
      throw new Error('La sala no existeix');
    }

    if (room.tipus === 'SINGLE') {
      throw new Error('No pots unir-te a una partida single-player');
    }

    if (room.jugadors_actuals.length >= 2) {
      throw new Error('La sala està plena');
    }

    const updatedRoom = await gameRepository.joinRoom(roomId, username);
    
    if (updatedRoom.jugadors_actuals.length >= 2) {
      await gameRepository.updateRoomStatus(roomId, 'jugant');
      console.log(`[SALA] La Sala ${updatedRoom.codi_sala} ja està plena (2/2). Començant partida...`);
    }

    return updatedRoom;
  }

  async leaveRoom(roomId, username) {
    return await gameRepository.leaveRoom(roomId, username);
  }

  async startSinglePlayerGame(username) {
    console.log(`[GAME] Iniciada partida en mode SOLITARI per a l'usuari ${username}.`);
    return await this.createSinglePlayerRoom(username);
  }

  async saveResult(resultData) {
    console.log(`[SERVICE] Guardant resultat de partida...`);
    return await resultRepository.create(resultData);
  }

  async getRankings(tipus = null) {
    return await resultRepository.findAll(tipus);
  }

  async getTopRankings(limit = 10, tipus = null) {
    return await resultRepository.findTop(limit, tipus);
  }

  generateRoomCode() {
    const chars = 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789';
    let code = '';
    for (let i = 0; i < 5; i++) {
      code += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    return code;
  }
}

module.exports = new GameService();
