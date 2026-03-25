const resultRepository = require('../repositories/ResultRepository');

class ResultService {
  async saveResult(resultData) {
    console.log(`[SERVICE] Processant resultat de partida per a ${resultData.username}...`);
    return await resultRepository.create(resultData);
  }

  async getAllResults(tipus = null) {
    return await resultRepository.findAll(tipus);
  }

  async getTopResults(limit = 10, tipus = null) {
    return await resultRepository.findTop(limit, tipus);
  }

  async getResultsByUsername(username, tipus = null) {
    return await resultRepository.findByUsername(username, tipus);
  }
}

module.exports = new ResultService();
