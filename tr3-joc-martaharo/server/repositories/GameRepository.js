/**
 * GameRepository - Capa d'Accés a Dades
 * 
 * Aquest fitxer implementa el patró Repository per a l'entitat Sala (Game).
 * Permet canviar la font de dades (MongoDB/InMemory) sense modificar els Services.
 * 
 * INTERFÍCIE PÚBLICA:
 * - createRoom(roomData) -> Sala
 * - findAllRooms(tipus?) -> Sala[]
 * - findRoomById(id) -> Sala
 * - findRoomsByType(tipus) -> Sala[]
 * - joinRoom(id, username) -> Sala
 * - leaveRoom(id, username) -> Sala
 * - updateRoomStatus(id, estat) -> Sala
 * - deleteRoom(id) -> Boolean
 */

// =====================
// IMPLEMENTACIÓ MONGODB (PRODUCCIÓ)
// =====================

const Sala = require('../Room');

class GameRepositoryMongoDB {
  async createRoom(roomData) {
    console.log(`[REPOSITORY] Creant sala (tipus: ${roomData.tipus || 'MULTIPLAYER'}) a MongoDB...`);
    const newRoom = new Sala(roomData);
    return await newRoom.save();
  }

  async findAllRooms(tipus = null) {
    console.log(`[REPOSITORY] Obtenint totes les sales de MongoDB...`);
    const filter = tipus ? { tipus } : {};
    return await Sala.find(filter).sort({ data_creacio: -1 });
  }

  async findRoomById(roomId) {
    console.log(`[REPOSITORY] Cercant sala per ID a MongoDB...`);
    return await Sala.findById(roomId);
  }

  async findRoomsByType(tipus) {
    console.log(`[REPOSITORY] Cercant sales de tipus ${tipus}...`);
    return await Sala.find({ tipus }).sort({ data_creacio: -1 });
  }

  async joinRoom(roomId, username) {
    console.log(`[REPOSITORY] Afegint jugador ${username} a la sala...`);
    return await Sala.findByIdAndUpdate(
      roomId,
      { $push: { jugadors_actuals: username } },
      { new: true }
    );
  }

  async leaveRoom(roomId, username) {
    console.log(`[REPOSITORY] Treient jugador ${username} de la sala...`);
    return await Sala.findByIdAndUpdate(
      roomId,
      { $pull: { jugadors_actuals: username } },
      { new: true }
    );
  }

  async updateRoomStatus(roomId, estat) {
    console.log(`[REPOSITORY] Actualitzant estat de la sala a MongoDB...`);
    return await Sala.findByIdAndUpdate(
      roomId,
      { estat },
      { new: true }
    );
  }

  async deleteRoom(roomId) {
    console.log(`[REPOSITORY] Eliminant sala de MongoDB...`);
    const result = await Sala.findByIdAndDelete(roomId);
    return result !== null;
  }
}

// =====================
// IMPLEMENTACIÓ IN-MEMORY (TESTING)
// =====================

/*
class GameRepositoryInMemory {
  constructor() {
    this.rooms = new Map();
    this.idCounter = 1;
  }

  async createRoom(roomData) {
    console.log(`[REPOSITORY] Creant sala (InMemory)...`);
    const id = this.idCounter++;
    const room = {
      _id: id.toString(),
      ...roomData,
      data_creacio: new Date()
    };
    this.rooms.set(id.toString(), room);
    return room;
  }

  async findAllRooms(tipus = null) {
    console.log(`[REPOSITORY] Obtenint totes les sales (InMemory)...`);
    let rooms = Array.from(this.rooms.values());
    if (tipus) {
      rooms = rooms.filter(r => r.tipus === tipus);
    }
    return rooms.sort((a, b) => b.data_creacio - a.data_creacio);
  }

  async findRoomById(roomId) {
    console.log(`[REPOSITORY] Cercant sala per ID (InMemory)...`);
    return this.rooms.get(roomId) || null;
  }

  async findRoomsByType(tipus) {
    console.log(`[REPOSITORY] Cercant sales de tipus ${tipus} (InMemory)...`);
    return Array.from(this.rooms.values()).filter(r => r.tipus === tipus);
  }

  async joinRoom(roomId, username) {
    console.log(`[REPOSITORY] Afegint jugador (InMemory)...`);
    const room = this.rooms.get(roomId);
    if (!room) return null;
    room.jugadors_actuals.push(username);
    return room;
  }

  async leaveRoom(roomId, username) {
    console.log(`[REPOSITORY] Treient jugador (InMemory)...`);
    const room = this.rooms.get(roomId);
    if (!room) return null;
    room.jugadors_actuals = room.jugadors_actuals.filter(j => j !== username);
    return room;
  }

  async updateRoomStatus(roomId, estat) {
    console.log(`[REPOSITORY] Actualitzant estat (InMemory)...`);
    const room = this.rooms.get(roomId);
    if (!room) return null;
    room.estat = estat;
    return room;
  }

  async deleteRoom(roomId) {
    console.log(`[REPOSITORY] Eliminant sala (InMemory)...`);
    return this.rooms.delete(roomId);
  }
}
*/

// =====================
// EXPORT (Canviar segons necessitat)
// =====================

module.exports = new GameRepositoryMongoDB();

// Per a InMemory (testing):
// module.exports = new GameRepositoryInMemory();
