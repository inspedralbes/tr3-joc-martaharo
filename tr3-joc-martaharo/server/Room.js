const mongoose = require('mongoose');

/**
 * Schema per a la col·lecció 'sales' de MongoDB.
 * Emmagatzema les dades de les sales de joc creades pels jugadors.
 */
const salaSchema = new mongoose.Schema({
  // Nom de la sala triat pel creador
  nom_sala: {
    type: String,
    required: true,
    trim: true
  },
  // Codi aleatori de 5 caràcters per identificar la sala
  codi_sala: {
    type: String,
    unique: true,
    uppercase: true
  },
  // Username del jugador que ha creat la sala
  id_creador: {
    type: String,
    required: true
  },
  // Llista de jugadors actualment a la sala (màxim 2)
  jugadors_actuals: {
    type: [String],
    default: [],
    validate: {
      validator: function(v) {
        return v.length <= 2;
      },
      message: 'Una sala pot tenir un màxim de 2 jugadors'
    }
  },
  // Tipus de partida: 'SINGLE' (un jugador) o 'MULTIPLAYER' (dos jugadors)
  tipus: {
    type: String,
    enum: ['SINGLE', 'MULTIPLAYER'],
    default: 'MULTIPLAYER'
  },
  // Estat de la sala: 'esperant' (esperant jugadors) o 'jugant' (partida en curs)
  estat: {
    type: String,
    enum: ['esperant', 'jugant'],
    default: 'esperant'
  },
  // Data de creació de la sala (s'estableix automàticament)
  data_creacio: {
    type: Date,
    default: Date.now
  }
}, { collection: 'sales' });

module.exports = mongoose.model('Sala', salaSchema);