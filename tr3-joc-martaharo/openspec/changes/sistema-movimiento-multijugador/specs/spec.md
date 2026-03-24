## Requisits Afegits

### Requisit: Entrada de Moviment del Jugador
El sistema HA DE permetre als jugadors controlar el seu personatge mitjançant entrada de teclat (Tecles de fletxes o WASD).

#### Escenari: El jugador es mou cap a la dreta
- **QUAN** el jugador prem la tecla Fletxa dreta o la tecla D
- **LLAVORS** el personatge es mou en direcció positiva X

#### Escenari: El jugador es mou cap a l'esquerra
- **QUAN** el jugador prem la tecla Fletxa esquerra o la tecla A
- **LLAVORS** el personatge es mou en direcció negativa X

#### Escenari: El jugador es mou cap amunt
- **QUAN** el jugador prem la tecla Fletxa amunt o la tecla W
- **LLAVORS** el personatge es mou en direcció positiva Y

#### Escenari: El jugador es mou cap avall
- **QUAN** el jugador prem la tecla Fletxa avall o la tecla S
- **LLAVORS** el personatge es mou en direcció negativa Y

### Requisit: Detecció de Col·lisions amb Parets
El sistema HA DE preventir que els jugadors es moguin a través de les parets mitjançant la detecció de col·lisions amb objectes de paret.

#### Escenari: El jugador col·lideix amb una paret
- **QUAN** el jugador intenta moure's cap a una paret
- **LLAVORS** el moviment es bloqtiga i el jugador roman a la posició actual

### Requisit: Sincronització de Posició en Temps Real
El sistema HA D'enviar la posició del jugador (x, y) al servidor sempre que la posició canviï.

#### Escenari: Posició enviada al servidor en moure's
- **QUAN** el jugador es mou a una nova posició
- **LLAVORS** la nova posició (x, y) s'envia al servidor via l'esdeveniment Socket.io 'updatePosition'

### Requisit: Visualització de la Posició de l'Oponent
El sistema HA DE mostrar la posició de l'altre jugador en temps real perquè els jugadors es puguin veure.

#### Escenari: El jugador veu el moviment de l'oponent
- **QUAN** el jugador oponent es mou a una nova posició
- **LLAVORS** el jugador local veu l'sprite de l'oponent moure's a aquesta posició
