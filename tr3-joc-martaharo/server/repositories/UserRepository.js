/**
 * UserRepository - Capa d'Accés a Dades
 * 
 * Aquest fitxer implementa el patró Repository per a l'entitat Usuari.
 * Permet canviar la font de dades (MongoDB/InMemory) sense modificar els Services.
 * 
 * INTERFÍCIE PÚBLICA:
 * - findById(id) -> Usuari
 * - findByUsername(username) -> Usuari
 * - create(userData) -> Usuari
 * - update(id, userData) -> Usuari
 * - delete(id) -> Boolean
 */

// =====================
// IMPLEMENTACIÓ MONGODB (PRODUCCIÓ)
// =====================

const Usuari = require('../User');

class UserRepositoryMongoDB {
  async findById(id) {
    console.log('[REPOSITORY] Cercant usuari per ID a MongoDB...');
    return await Usuari.findById(id);
  }

  async findByUsername(username) {
    console.log(`[REPOSITORY] Cercant usuari ${username} a MongoDB...`);
    return await Usuari.findOne({ username });
  }

  async create(userData) {
    console.log(`[REPOSITORY] Creant usuari a MongoDB...`);
    const newUser = new Usuari(userData);
    return await newUser.save();
  }

  async update(id, userData) {
    console.log(`[REPOSITORY] Actualitzant usuari a MongoDB...`);
    return await Usuari.findByIdAndUpdate(id, userData, { new: true });
  }

  async delete(id) {
    console.log(`[REPOSITORY] Eliminant usuari de MongoDB...`);
    const result = await Usuari.findByIdAndDelete(id);
    return result !== null;
  }
}

// =====================
// IMPLEMENTACIÓ IN-MEMORY (TESTING)
// =====================

/*
class UserRepositoryInMemory {
  constructor() {
    this.users = new Map();
    this.idCounter = 1;
  }

  async findById(id) {
    console.log('[REPOSITORY] Cercant usuari per ID (InMemory)...');
    return this.users.get(id) || null;
  }

  async findByUsername(username) {
    console.log(`[REPOSITORY] Cercant usuari ${username} (InMemory)...`);
    for (const user of this.users.values()) {
      if (user.username === username) return user;
    }
    return null;
  }

  async create(userData) {
    console.log(`[REPOSITORY] Creant usuari (InMemory)...`);
    const id = this.idCounter++;
    const user = { _id: id.toString(), ...userData, data_creacio: new Date() };
    this.users.set(id.toString(), user);
    return user;
  }

  async update(id, userData) {
    console.log(`[REPOSITORY] Actualitzant usuari (InMemory)...`);
    const existing = this.users.get(id);
    if (!existing) return null;
    const updated = { ...existing, ...userData };
    this.users.set(id, updated);
    return updated;
  }

  async delete(id) {
    console.log(`[REPOSITORY] Eliminant usuari (InMemory)...`);
    return this.users.delete(id);
  }
}
*/

// =====================
// EXPORT (Canviar segons necessitat)
// =====================

// Per a MongoDB (producció):
module.exports = new UserRepositoryMongoDB();

// Per a InMemory (testing):
// module.exports = new UserRepositoryInMemory();
