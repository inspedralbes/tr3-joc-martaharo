# Pla d'Implementació

## Estratègia: Enfocament de 10 Passos

### Pas 1: Configuració de la Base de Dades ✅ COMPLET

**Objectiu**: Configurar la connexió a MongoDB Atlas i crear els esquemes de les col·leccions.

**Accions**:
- Connectar a MongoDB Atlas (base de dades `joc_multijugador`)
- Utilitzar variables d'entorn (.env) per protegir la MONGO_URI
- Crear schema `Usuari` (usuaris): username, password, data_creacio
- Crear schema `Sala` (sales): nom_sala, id_creador, jugadors_actuals, estat
- Crear schema `Ranking` (rankings): username, puntuacio, data_partida

**Entregable**: Connexió a MongoDB funcional amb 3 col·leccions

---


### Pas 2: Sistema d'Autenticació (Login) ✅ COMPLET

**Objectiu**: Implementar el sistema de login per validar usuaris contra la col·lecció `usuaris`.

**Accions**:
- Crear la ruta `/api/login` al servidor Node.js
- Validar usuari i contrasenya contra la col·lecció `usuaris`
- Emmagatzemar data_creacio si és nou usuari
- Retornar token de sessió
- **Contrasenyes encriptades amb bcrypt** (Punt 4.2)

**Entregable**: Sistema de login funcional

---

### Pas 3: Gestió de Sales (Rooms) - Col·lecció `sales` ✅ COMPLET

**Objectiu**: Permetre crear i unir-se a sales de joc, emmagatzemant a la col·lecció `sales`.

**Accions**:
- Crear ruta POST `/api/rooms` per crear sales (codi generat automàticament)
- El servidor genera automàticament un codi de 5 caràcters
- Emmagatzemar: codi_sala, id_creador, jugadors_actuals, estat
- Crear ruta GET `/api/rooms` per llistar sales
- Crear ruta POST `/api/rooms/:id/join` per unir-se
- Actualitzar `jugadors_actuals` (màxim 2) i `estat` (esperant/jugant)
- **Codi aleatori de 5 caràcters** per identificar sales

**Entregable**: Jugadors poden crear i unir-se a sales sense necessitat d'escriure un nom

---

### Pas 4: Sistema de Rànquings - Col·lecció `rankings` ✅ COMPLET

**Objectiu**: Guardar les puntuacions dels jugadors per mostrar els millors.

**Accions**:
- Crear ruta POST `/api/rankings` per guardar puntuació
- Emmagatzemar: username, puntuacio, data_partida
- Crear ruta GET `/api/rankings` per obtenir el rànquing
- Ordenar per puntuacio (descendent)

**Entregable**: Sistema de rànquings funcional

---

### Pas 5: Servidor Online amb Socket.io ✅ COMPLET

**Objectiu**: Configurar el servidor perquè funcioni online (no local).

**Accions**:
- Implementar Socket.io per a comunicació en temps real
- Configurar el servidor per acceptar connexions des de qualsevol IP
- Documentar la necessitat d'una IP pública dinàmica (DNS dinàmic)
- Implementar la gestió de desconnexions: si un jugador marxa, notificar a l'altre i alliberar la sala
- **Logs en català** per a traçabilitat

**Entregable**: Servidor accessible des d'internet

---

### Pas 6: Integració Socket.io per Sales ✅ COMPLET

**Objectiu**: Unir els jugadors a sales específiques mitjançant Socket.io.

**Accions**:
- Implementar esdeveniment `joinRoom` que faci `socket.join(roomId)`
- Gestionar `updatePosition` per sala
- Sincronitzar posicions només entre jugadors de la mateixa sala

**Entregable**: Sistema de sales funcional amb Socket.io

---

### Pas 7: Script de Moviment a Unity ✅ COMPLET

**Objectiu**: Implementar el script de moviment del jugador.

**Accions**:
- Crear el script `PlayerController.cs` a Unity
- Afegir detecció d'entrada per a tecles de fletxes i WASD
- Implementar moviment amb `Rigidbody2D`
- Afegir col·lisions amb parets

**Entregable**: Moviment local funcional

---

### Pas 8: Sincronització de Posició amb Interpolació ✅ COMPLET

**Objectiu**: Els jugadors es veuen entre ells en temps real, suau davant la latència.

**Accions**:
- Enviar `updatePosition` quan el jugador es mou
- Rebre posicions dels altres jugadors de la sala
- Utilitzar `Vector3.Lerp` a Unity per suavitzar el moviment de l'oponent
- Compensar la latència d'internet

**Entregable**: Moviment suau dels jugadors remots

---

### Pas 9: Enemy IA al Servidor ✅ COMPLET

**Objectiu**: La lògica de l'enemic IA corre al servidor i es transmet als clients.

**Accions**:
- Implementar l'enemic IA al servidor Node.js
- Transmetre la posició de l'enemic via Socket.io a tots els clients
- Sincronitzar l'estat de l'enemic a totes les sales

**Entregable**: Enemy IA funcional compartit entre jugadors

---

### Pas 10: Proves de Latència

**Objectiu**: Verificar que la sincronització funciona correctament.

**Accions**:
- Mesurar temps d'anada i tornada (RTT)
- Provar amb dos clients des de diferents xarxes
- Documentar resultats

**Entregable**: Informe de rendiment

---

## Estatus del Projecte

La creació de sales ja no requereix un "Nom de sala", s'ha simplificat el flux per a millorar l'experiència d'usuari. Ara el servidor genera automàticament un codi de 5 caràcters quan es crea una sala.

**Correcció de rutes API**: S'ha unificat les rutes entre client (Unity) i servidor:
- Client Unity: `/api/rooms` (POST per crear sala)
- Servidor: `/api/rooms` (POST per crear sala)
- El model 'User' segueix apuntant a la col·lecció 'usuaris'

---

## Funcionalitats Extra Implementades

### Mode Single Player ✅
- Partida individual contra la IA
- Sistema de reserves separades per tipus (SINGLE/MULTIPLAYER)
- Validació de camps al client (Unity)

### Patró Repository (Punt 4.2) ✅
- **Repositories**: UserRepository.js, GameRepository.js, ResultRepository.js
- **Services**: AuthService.js, GameService.js
- Separació clara de responsabilitats
- Interfície per canvi de base de dades

### Documentació ✅
- README.md amb instruccions d'instal·lació
- Plan.md actualitzat amb estat del projecte

---

## Estructura de la Base de Dades (MongoDB Atlas)

**Nom de la base de dades**: `joc_multijugador`

### Col·lecció: `usuaris`
```
{
  username: String (únic),
  password: String (hash bcrypt),
  data_creacio: Date
}
```

### Col·lecció: `sales`
```
{
  nom_sala: String,
  id_creador: String,
  codi_sala: String (codi aleatori de 5 caràcters),
  jugadors_actuals: [String] (màx 2),
  tipus: String (SINGLE/MULTIPLAYER),
  estat: String (esperant/jugant),
  data_creacio: Date
}
```

### Col·lecció: `rankings`
```
{
  username: String,
  puntuacio: Number,
  tipus: String (SINGLE/MULTIPLAYER),
  durada: Number,
  data_partida: Date
}
```

---

## Arquitectura - Patró Repository (Punt 4.2)

### Estructura de Capes

```
server/
├── controllers/         # Gestió de peticions HTTP
│   ├── AuthController.js
│   ├── GameController.js
│   └── ResultController.js
├── services/           # Lògica de negoci
│   ├── AuthService.js
│   ├── GameService.js
│   └── ResultService.js
├── repositories/       # Accés a dades
│   ├── UserRepository.js
│   ├── GameRepository.js
│   └── ResultRepository.js
└── server.js           # Punt d'entrada
```

### Flux de Dades

1. **Controller**: Rep la petició HTTP (Unity)
2. **Service**: Valida i aplica lògica de negoci
3. **Repository**: Accedeix a MongoDB (o InMemory per testing)

### Implementació InMemory (Testing)

Els Repositories inclouen una implementació InMemory comentada per a testing:
- Canviar l'export per utilitzar la versió alternativa
- El Service NO sap quin backend s'utilitza

### Seguretat

- **Contrasenyes**: Hash bcrypt amb salt de 10
- **.env**: Protegim la MONGO_URI
- **Sessions**: Token aleatori per sessió

---

## Flux de Joc (Game Loop)

1. **Login**: L'usuari s'autentica contra la col·lecció `usuaris`
2. **Selecció de Mode**: L'usuari tria entre:
   - **Single Player**: Partida individual contra l'IA
   - **Multiplayer**: Crear o unir-se a una sala (col·lecció `sales`)
3. **Partida**: 
   - Single: El jugador juga sol
   - Multi: Els jugadors es sincronitzen via Socket.io
4. **Victòria/Derrota**: Es guarda la puntuació (col·lecció `rankings`) amb el tipus de partida
5. **Rànquing**: Es poden veure les puntuacions separades per tipus (SINGLE/MULTIPLAYER)
6. **Desconnexió (Multiplayer)**: Si un jugador marxa, s'avisa a l'altre i la sala es tanca

---

## Seguretat

- Les credencials de MongoDB es guarden al fitxer `.env` (dins de `server/`)
- El `.env` està al `.gitignore` per no pujar-lo a GitHub
- El servidor valida tokens de sessió per a les rutes protegides

---

## Dependències

- El Pas 1 ha de completar-se abans del Pas 2
- El Pas 2 ha de completar-se abans del Pas 3
- El Pas 3 ha de completar-se abans del Pas 6
- El Pas 4 és independent
- El Pas 5 ha de completar-se abans del Pas 6
- El Pas 6 ha de completar-se abans del Pas 8
- El Pas 7 ha de completar-se abans del Pas 8