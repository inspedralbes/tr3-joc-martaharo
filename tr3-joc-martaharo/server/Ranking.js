const mongoose = require('mongoose');

/**
 * Schema per a la col·lecció 'rankings' de MongoDB.
 * Emmagatzema les puntuacions dels jugadors per mostrar el rànquing.
 */
const rankingSchema = new mongoose.Schema({
  // Nom del jugador
  username: {
    type: String,
    required: true,
    trim: true
  },
  // Puntuació obtinguda a la partida
  puntuacio: {
    type: Number,
    required: true
  },
  // Tipus de partida: 'SINGLE' (un jugador) o 'MULTIPLAYER' (dos jugadors)
  tipus: {
    type: String,
    enum: ['SINGLE', 'MULTIPLAYER'],
    default: 'MULTIPLAYER'
  },
  // Durada de la partida en segons
  durada: {
    type: Number,
    default: 0
  },
  // Data en què es va jugar la partida (s'estableix automàticament)
  data_partida: {
    type: Date,
    default: Date.now
  }
}, { collection: 'rankings' });

// Índex per ordenar les puntuacions de major a menor
rankingSchema.index({ puntuacio: -1 });

module.exports = mongoose.model('Ranking', rankingSchema);