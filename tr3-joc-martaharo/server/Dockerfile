FROM node:20-alpine

WORKDIR /app

# Copiar package files
COPY package*.json ./

# Instal·lar dependències
RUN npm install

# Copiar codi font (excloent node_modules)
COPY . .

# Crear .env si no existeix (per defecte Docker)
RUN if [ ! -f .env ]; then \
    echo "PORT=3000" > .env && \
    echo "NODE_ENV=production" >> .env && \
    echo "MONGO_URI=mongodb://mongo:27017/joc_multijugador" >> .env; \
    fi

# Exponer port
EXPOSE 3000

# Iniciar servidor
CMD ["node", "server.js"]
