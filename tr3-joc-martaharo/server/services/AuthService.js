const bcrypt = require('bcrypt');
const userRepository = require('../repositories/UserRepository');

class AuthService {
  async register(username, password) {
    console.log(`[SERVICE] Registrant usuari ${username}...`);
    const hashedPassword = await bcrypt.hash(password, 10);
    return await userRepository.create({
      username,
      password: hashedPassword
    });
  }

  async login(username, password) {
    console.log(`[SERVICE] Autentificant usuari ${username}...`);
    const user = await userRepository.findByUsername(username);
    if (!user) return null;
    
    const isValid = await bcrypt.compare(password, user.password);
    return isValid ? user : null;
  }

  async getUserByUsername(username) {
    return await userRepository.findByUsername(username);
  }

  async getUserById(id) {
    return await userRepository.findById(id);
  }
}

module.exports = new AuthService();
