const mongoose = require('mongoose');

/**
 * Schema per a la col·lecció 'usuaris' de MongoDB.
 * Emmagatzema les dades dels usuaris registrats al joc.
 */
const usuariSchema = new mongoose.Schema({
  // Nom d'usuari (únic)
  username: {
    type: String,
    required: true,
    unique: true,
    trim: true
  },
  // Contrasenya de l'usuari
  password: {
    type: String,
    required: true
  },
  // Data de creació del compte (s'estableix automàticament)
  data_creacio: {
    type: Date,
    default: Date.now
  }
}, { collection: 'usuaris' });

module.exports = mongoose.model('Usuari', usuariSchema);