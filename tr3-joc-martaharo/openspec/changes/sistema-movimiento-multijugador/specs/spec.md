## Requisits del Sistema

### Requisit: Autenticació d'Usuaris (Col·lecció `usuaris`)
El sistema HA DE permetre als jugadors identificar-se abans d'accedir al joc.

#### Escenari: Login correcte
- **QUAN** l'usuari introdueix un usuari i contrasenya vàlids
- **LLAVORS** el servidor valida les credencials contra la col·lecció `usuaris` de la base de dades `joc_multijugador`
- **I** retorna un token de sessió
- **I** l'usuari pot accedir al Lobby

#### Escenari: Login incorrecte
- **QUAN** l'usuari introdueix credencials invàlides
- **LLAVORS** el servidor retorna error 401
- **I** es mostra missatge d'error a la interfície

#### Escenari: Registre de nou usuari
- **QUAN** un nou usuari es registra amb username i password
- **LLAVORS** el servidor desa les dades a la col·lecció `usuaris`
- **I** emmagatzema la data_creacio automàticament
- **I** retorna token de sessió

---

### Requisit: Gestió de Sales (Col·lecció `sales`)
El sistema HA DE permetre crear, llistar i unir-se a sales de joc.

**Endpoint**: `POST /api/rooms`
- **Cos de la petició**: JSON buit `{}` (NO requereix el paràmetre `name`)
- **Resposta**: `{"roomId": "...", "roomCode": "XXXXX"}`

#### Escenari: Crear sala
- **QUAN** l'usuari prem "Crear Partida" sense introduir cap nom
- **LLAVORS** el servidor crea un document a la col·lecció `sales`
- **I** genera automàticament un codi de 5 caràcters
- **I** desa: codi_sala (generat), id_creador, jugadors_actuals (inicialment només el creador), estat (esperant)
- **I** retorna un objecte JSON amb roomId i roomCode
- **I** el client mostra el codi de sala a la interfície

#### Escenari: Llistar sales
- **QUAN** l'usuari carrega la pantalla del Lobby
- **LLAVORS** el servidor retorna la llista de sales des de la col·lecció `sales`
- **I** mostra nom_sala, jugadors_actuals, estat de cada sala

#### Escenari: Unir-se a sala existent
- **QUAN** l'usuari escriu el nom d'una sala existent
- **LLAVORS** el servidor busca la sala a la col·lecció `sales`
- **I** verifica que jugadors_actuals < 2
- **I** afegeix l'usuari a jugadors_actuals
- **I** si està plena, retorna error

#### Escenari: Desconnexió d'un jugador
- **QUAN** un jugador es desconnecta durant la partida
- **LLAVORS** el servidor notifica a l'altre jugador de la sala
- **I** elimina el jugador de la col·lecció `sales`
- **I** la sala queda lliure per a una nova partida

---

### Requisit: Sistema de Rànquings (Col·lecció `rankings`)
El sistema HA DE permetre guardar i mostrar les millors puntuacions.

#### Escenari: Guardar puntuació
- **QUAN** una partida acaba (tots dos jugadors entren a la GoalZone)
- **LLAVORS** el servidor desa la puntuació a la col·lecció `rankings`
- **I** desa: username, puntuacio, data_partida

#### Escenari: Consultar rànquing
- **QUAN** l'usuari demana veure el rànquing
- **LLAVORS** el servidor retorna els jugadors ordenats per puntuacio (descendent)
- **I** mostra username i puntuacio

---

### Requisit: Joc Online via Socket.io
El sistema HA DE funcionar online, no en local.

#### Escenari: Connexió des d'internet
- **QUAN** un usuari es connecta des d'una xarxa diferent a la del servidor
- **LLAVORS** el servidor accepta la connexió via Socket.io
- **I** la comunicació és en temps real

#### Requisit: IP dinàmica
- El servidor ha de ser accessible des d'una IP pública
- Cal utilitzar un servei de DNS dinàmic (com no-ip) perquè la IP no canviï

---

### Requisit: Entrada de Moviment del Jugador
El sistema HA DE permetre als jugadors controlar el seu personatge mitjançant entrada de teclat.

#### Escenari: El jugador es mou
- **QUAN** el jugador prem les tecles de fletxes o WASD
- **LLAVORS** el personatge es mou en la direcció corresponent

---

### Requisit: Sincronització de Posició per Sala
El sistema HA DE sincronitzar les posicions dels jugadors NOMÉS dins de la mateixa sala.

#### Escenari: Posició enviada al servidor
- **QUAN** el jugador es mou
- **LLAVORS** la posició s'envia al servidor amb el `roomId`
- **I** el servidor difon la posició només als jugadors de la mateixa sala

---

### Requisit: Interpolació per suavitzar el moviment
El sistema HA D'utilitzar interpolació (Lerp) perquè el moviment dels jugadors remots sigui suau.

#### Escenari: Moviment suau de l'oponent
- **QUAN** es rep una nova posició de l'altre jugador
- **LLAVORS** el client utilitza `Vector3.Lerp` per moure's suaument
- **I** la latència d'internet no fa que el moviment sigui espasmòdic

---

### Requisit: Enemy IA al Servidor
El sistema HA DE gestionar l'enemic IA des del servidor perquè tots els clients vegin el mateix.

#### Escenari: Enemy IA sincronitzat
- **QUAN** l'enemic IA es mou
- **LLAVORS** el servidor calcula la nova posició
- **I** transmet la posició a tots els clients de la sala
- **I** tots els jugadors veuen el mateix enemy

---

### Requisit: Visualització de l'Oponent
El sistema HA DE mostrar la posició de l'altre jugador de la mateixa sala.

#### Escenari: Veure moviment de l'oponent
- **QUAN** l'altre jugador de la sala es mou
- **LLAVORS** el jugador local veu l'sprite de l'oponent moure's

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

## Models de Dades (Mongoose Schemas)

**Nota**: El model Mongoose 'User' està explícitament mapejat a la col·lecció 'usuaris' mitjançant `mongoose.model('User', userSchema, 'usuaris')` per evitar la creació automàtica de la col·lecció 'users'.

### Usuari (col·lecció: `usuaris`)
| Camp | Tipus | Descripció |
|------|-------|------------|
| username | String | Nom d'usuari (únic) |
| password | String | Contrasenya |
| data_creacio | Date | Data de creació del compte |

### Sala (col·lecció: `sales`)
| Camp | Tipus | Descripció |
|------|-------|------------|
| nom_sala | String | Nom de la sala |
| id_creador | String | Username del creador |
| codi_sala | String | Codi aleatori de 5 caràcters |
| jugadors_actuals | Array[String] | Llista de jugadors (màx 2) |
| tipus | String | "SINGLE" o "MULTIPLAYER" |
| estat | String | "esperant" o "jugant" |
| data_creacio | Date | Data de creació |

### Ranking (col·lecció: `rankings`)
| Camp | Tipus | Descripció |
|------|-------|------------|
| username | String | Nom del jugador |
| puntuacio | Number | Puntuació o temps |
| tipus | String | "SINGLE" o "MULTIPLAYER" |
| durada | Number | Durada de la partida en segons |
| data_partida | Date | Data de la partida |

---

## Base de Dades

**Nom de la base de dades**: `joc_multijugador`

---

## Arquitectura - Patró Repository (Punt 4.2)

### Capa Repository (Accés a Dades)
- **Responsabilitat**: Accedir a MongoDB (o altre emmagatzematge)
- **Exemples**: `UserRepository.js`, `GameRepository.js`, `ResultRepository.js`
- **Interfície**: Mètodes com `findById()`, `create()`, `findAll()`

### Capa Service (Lògica de Negoci)
- **Responsabilitat**: Validar dades, aplicar regles de negoci
- **Exemples**: `AuthService.js` (encriptació bcrypt), `GameService.js` (estats)
- **No coneix** la font de dades (MongoDB o InMemory)

### Capa Controller (Gestió HTTP)
- **Responsabilitat**: Rebre peticions, retornar codis d'estat (200, 400, 401, 500)
- **Exemples**: `AuthController.js`, `GameController.js`
- **Crida** als Services per processar les peticions

### Flux de Dades
```
Unity → Controller → Service → Repository → MongoDB
                ↑           ↓
                ←←←←←←←←←←←
              (Resposta)
```

### Implementació Alternativa (Testing)
Cada Repository inclou una implementació InMemory comentada:
- Canviant l'export, el Service funciona igual
- Permet testing sense base de dades

**Col·leccions**:
- `usuaris` - Autenticació dels jugadors
- `sales` - Partides actives
- `rankings` - Puntuacions històriques