# Docker - Joc 2D DAM

## Estructura

```
projecte-arrel/
├── docker-compose.yml      # Orquestració de serveis
├── nginx/
│   └── nginx.conf         # Proxy invers
├── server/
│   ├── Dockerfile         # Imatge Docker del backend
│   └── ...
└── Assets/                # Projecte Unity
```

## Serveis

| Servei | Port | Descripció |
|--------|-----|------------|
| **nginx** | 8080 | Proxy invers (HTTP + WebSockets) |
| **joc-backend** | 3000 | Node.js (amagat darrere Nginx) |

> **Nota**: MongoDB ara està a MongoDB Atlas (Cloud), no cal executar-lo com a container.

## Iniciar

```bash
# Des de l'arrel del projecte
docker-compose up -d

# Veure logs
docker-compose logs -f

# Aturar
docker-compose down
```

## Accés

- **API**: http://localhost:8080/api/*
- **WebSockets**: http://localhost:8080/socket.io/
- **Health**: http://localhost:8080/health

## Variables d'entorn

| Variable | Valor per defecte |
|----------|------------------|
| NODE_ENV | production |
| PORT | 3000 |
| MONGO_URI | mongodb+srv://... (Atlas Cloud - BD: joc_multijugador) |

## Arquitectura Backend DAM

```
Controller (HTTP) → Service (Lògica) → Repository (Dades)
                                        ↓
                              MongoDB / InMemory
```
