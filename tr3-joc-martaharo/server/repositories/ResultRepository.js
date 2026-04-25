/**
 * ResultRepository - Capa d'Accés a Dades
 * 
 * Aquest fitxer implementa el patró Repository per a l'entitat Result (Ranking).
 * Permet canviar la font de dades (MongoDB/InMemory) sense modificar els Services.
 * 
 * INTERFÍCIE PÚBLICA:
 * - create(resultData) -> Result
 * - findAll(tipus?) -> Result[]
 * - findTop(limit, tipus?) -> Result[]
 * - findByUsername(username, tipus?) -> Result[]
 */

// =====================
// IMPLEMENTACIÓ MONGODB (PRODUCCIÓ)
// =====================

const Ranking = require('../Ranking');

class ResultRepositoryMongoDB {
  async create(resultData) {
    console.log(`[REPOSITORY] Escrivint resultat a MongoDB (tipus: ${resultData.tipus || 'MULTIPLAYER'})...`);
    const newResult = new Ranking(resultData);
    return await newResult.save();
  }

  async findAll(tipus = null) {
    console.log(`[REPOSITORY] Obtenint tots els resultats de MongoDB...`);
    const filter = tipus ? { tipus } : {};
    return await Ranking.find(filter).sort({ puntuacio: -1 });
  }

  async findTop(limit = 10, tipus = null) {
    console.log(`[REPOSITORY] Obtenint top ${limit} resultats de MongoDB...`);
    const filter = tipus ? { tipus } : {};
    return await Ranking.find(filter)
      .sort({ puntuacio: -1 })
      .limit(limit);
  }

  async findByUsername(username, tipus = null) {
    console.log(`[REPOSITORY] Cercant resultats de l'usuari ${username}...`);
    const filter = { username };
    if (tipus) filter.tipus = tipus;
    return await Ranking.find(filter).sort({ data_partida: -1 });
  }
}

// =====================
// IMPLEMENTACIÓ IN-MEMORY (TESTING)
// =====================

/*
class ResultRepositoryInMemory {
  constructor() {
    this.results = [];
    this.idCounter = 1;
  }

  async create(resultData) {
    console.log(`[REPOSITORY] Escrivint resultat (InMemory)...`);
    const result = {
      _id: this.idCounter++,
      ...resultData,
      data_partida: new Date()
    };
    this.results.push(result);
    return result;
  }

  async findAll(tipus = null) {
    console.log(`[REPOSITORY] Obtenint tots els resultats (InMemory)...`);
    let results = [...this.results];
    if (tipus) {
      results = results.filter(r => r.tipus === tipus);
    }
    return results.sort((a, b) => b.puntuacio - a.puntuacio);
  }

  async findTop(limit = 10, tipus = null) {
    console.log(`[REPOSITORY] Obtenint top ${limit} (InMemory)...`);
    let results = [...this.results];
    if (tipus) {
      results = results.filter(r => r.tipus === tipus);
    }
    return results
      .sort((a, b) => b.puntuacio - a.puntuacio)
      .slice(0, limit);
  }

  async findByUsername(username, tipus = null) {
    console.log(`[REPOSITORY] Cercant resultats de ${username} (InMemory)...`);
    let results = this.results.filter(r => r.username === username);
    if (tipus) {
      results = results.filter(r => r.tipus === tipus);
    }
    return results.sort((a, b) => b.data_partida - a.data_partida);
  }
}
*/

// =====================
// EXPORT (Canviar segons necessitat)
// =====================

module.exports = new ResultRepositoryMongoDB();

// Per a InMemory (testing):
// module.exports = new ResultRepositoryInMemory();
