class ResultController {
  constructor(resultService) {
    this.resultService = resultService;
  }

  async saveResult(req, res) {
    const { username, puntuacio, tipus, durada } = req.body;

    if (!username || puntuacio === undefined) {
      return res.status(400).json({ error: 'Cal indicar username i puntuacio' });
    }

    try {
      console.log('[SERVICE] Guardant resultat de partida...');
      const result = await this.resultService.saveResult({
        username,
        puntuacio,
        tipus: tipus || 'MULTIPLAYER',
        durada: durada || 0
      });
      
      console.log(`[RANKING] Puntuació guardada per ${username}: ${puntuacio} (${tipus || 'MULTIPLAYER'})`);
      res.json({ success: true, ranking: result });
      
    } catch (err) {
      console.error('[SERVICE] Error guardant ranking:', err);
      res.status(500).json({ error: 'Error al guardar la puntuació' });
    }
  }

  async getRankings(req, res) {
    const { tipus } = req.query;
    
    try {
      console.log(`[SERVICE] Obtenint rànquing (tipus: ${tipus || 'totes'})...`);
      const rankings = await this.resultService.getTopRankings(10, tipus);
      
      res.json(rankings);
    } catch (err) {
      console.error('[SERVICE] Error obtenint rankings:', err);
      res.status(500).json({ error: 'Error al obtenir el rànquing' });
    }
  }
}

module.exports = ResultController;
