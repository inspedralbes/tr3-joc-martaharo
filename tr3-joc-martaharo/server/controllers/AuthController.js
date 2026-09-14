const crypto = require('crypto');

const sessions = {};

class AuthController {
  constructor(authService) {
    this.authService = authService;
  }

  generateToken() {
    return crypto.randomBytes(32).toString('hex');
  }

  async login(req, res) {
    const { username, password } = req.body;

    if (!username || !password) {
      return res.status(400).json({ error: 'Cal omplir usuari i contrasenya' });
    }

    try {
      console.log('[SERVICE] Validant credencials...');
      const user = await this.authService.login(username, password);
      
      if (!user) {
        console.log('[SERVICE] Credencials incorrectes per usuari: ' + username);
        return res.status(401).json({ error: 'Usuari o contrasenya incorrectes' });
      }

      const token = this.generateToken();
      sessions[token] = { username, createdAt: new Date().toISOString() };
      
      console.log(`[LOGIN] Usuari ${username} ha iniciat sessió correctament.`);
      res.json({ token, username });
      
    } catch (err) {
      console.error('[SERVICE] Error al login:', err);
      res.status(500).json({ error: 'Error al login' });
    }
  }

  async register(req, res) {
    const { username, password } = req.body;
    
    if (!username || !password) {
      return res.status(400).json({ error: 'Cal omplir usuari i contrasenya' });
    }

    try {
      const existingUser = await this.authService.getUserByUsername(username);
      if (existingUser) {
        return res.status(409).json({ error: 'L\'usuari ja existeix' });
      }

      console.log('[SERVICE] Registrant nou usuari...');
      await this.authService.register(username, password);

      const token = this.generateToken();
      sessions[token] = { username, createdAt: new Date().toISOString() };
      
      console.log(`[REGISTRE] Nou usuari creat: ${username}`);
      res.json({ token, username });
      
    } catch (err) {
      console.error('[SERVICE] Error al registrar:', err);
      res.status(500).json({ error: 'Error al registrar' });
    }
  }

  verify(req, res) {
    const token = req.headers.authorization?.replace('Bearer ', '');
    if (!token) {
      return res.status(401).json({ error: 'Token necessari' });
    }

    const session = sessions[token];
    if (!session) {
      return res.status(401).json({ error: 'Token invàlid' });
    }

    res.json({ username: session.username, valid: true });
  }

  logout(req, res) {
    const token = req.headers.authorization?.replace('Bearer ', '');
    if (token) {
      delete sessions[token];
      console.log('[SESSION] Sessió tancada.');
    }
    res.json({ success: true });
  }

  getSessions() {
    return sessions;
  }
}

module.exports = AuthController;
