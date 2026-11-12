# Airumã: todos os outros textos do jogo

**Como editar:** mude apenas a coluna **Texto** (a coluna Contexto não é lida; pode escrever observações nela). Não mude a coluna **Chave**, não apague linhas e não use o caractere `|` dentro de um texto. Depois é só me pedir para ler os documentos: eu aplico tudo de uma vez no jogo (`python3 Docs/tools/textos.py import`). Para gerar os documentos de novo a partir do jogo: `python3 Docs/tools/textos.py export`.

- Estes documentos são a tradução para o idioma ES: edite só a coluna Texto (ES); a coluna Referência (PT) é só para consulta. Cada fala cabe em 28 letras por 5 linhas.
- Cada fala cabe em até 5 linhas curtas na caixa de diálogo (uns 28 letras por linha), então textos muito longos são cortados.
- A fonte do jogo não tem alguns sinais, como aspas e ponto e vírgula; prefira vírgula, ponto, dois pontos, traço, parênteses, `!`, `?`, `/`, `+`, `%` e `...`.
- Os diálogos entre os personagens estão em `Docs/Dialogos.md`.
- Os nomes dos ataques dos chefes aparecem como o título de uma carta de feitiço ("Tema: Nome") quando o ataque começa.

---

## Interface


### Menu principal


| Chave | Texto (ES) | Referência (PT) | Contexto |
| --- | --- | --- | --- |
| `Title` | Airumã | Airumã | Título do jogo, no alto do menu principal. |
| `MenuStart` | Iniciar | Iniciar | Menu principal: começa uma partida (leva à escolha da dificuldade). |
| `MenuBossPractice` | Práctica | Treino | Menu principal: abre o treino (de fases ou de chefes). |
| `MenuScores` | Récords | Recordes | Menu principal: mostra os recordes. |
| `MenuMusicRoom` | Sala de música | Sala de música | Menu principal: sala de músicas (ainda não existe; mostra o aviso "Em breve"). |
| `MenuConfig` | Opciones | Opções | Menu principal: abre as opções. |
| `MenuQuit` | Salir | Sair | Menu principal: fecha o jogo. |
| `MenuHint` | Z: Elegir   X: Volver | Z: Escolher   X: Voltar | Dica das teclas, no canto de baixo do menu. |
| `ComingSoon` | Próximamente | Em breve | Aviso que pisca ao escolher uma opção que ainda não existe. |
| `Back` | Volver | Voltar | Voltar, nos submenus. |
| `Menu` | Menú | Menu | Botão "ir para o menu", na tela final da partida. |
| `DemoLabel` | Demostración | Demonstração | Aviso que pisca durante a demonstração (o jogo joga sozinho quando o menu fica 10 segundos parado). |

### Dificuldade, treino e recordes


| Chave | Texto (ES) | Referência (PT) | Contexto |
| --- | --- | --- | --- |
| `DifficultyTitle` | Dificultad | Dificuldade | Título da escolha de dificuldade. |
| `DifficultyEasy` | Fácil | Fácil | Dificuldade fácil. |
| `DifficultyNormal` | Normal | Normal | Dificuldade normal. |
| `DifficultyHard` | Difícil | Difícil | Dificuldade difícil. |
| `PracticeTitle` | Práctica de Jefe | Treino de Chefe | Título da lista de treino de chefes. |
| `PracticeStages` | Fases | Fases | Treino: opção "treinar uma fase inteira". |
| `PracticeBosses` | Jefes | Chefes | Treino: opção "treinar só o chefe". |
| `PracticeStageTitle` | Práctica de Fase | Treino de Fase | Título da lista de treino de fases. |
| `LockedBoss` | ????? | ????? | Nome de um chefe ainda não existente na lista de treino (aparece com um cadeado). |
| `ScoresTitle` | Récords | Recordes | Título da tela de recordes. |
| `NoScore` | ------- | ------- | Linha vazia da tabela de recordes. |
| `Cleared` | Completo | Completo | Marca ao lado do recorde de uma partida que chegou ao fim. |

### Opções


| Chave | Texto (ES) | Referência (PT) | Contexto |
| --- | --- | --- | --- |
| `CfgTitle` | Opciones | Configurações | Título da tela de opções. |
| `CfgLives` | Vidas | Vidas | Opções: vidas no início da partida (1 a 5). |
| `CfgBombs` | Bombas | Bombas | Opções: bombas no início da partida (0 a 3). |
| `CfgMusic` | Volumen de Música | Volume da Música | Opções: volume da música. |
| `CfgSfx` | Efectos de Sonido | Efeitos Sonoros | Opções: volume dos efeitos sonoros. |
| `CfgMode` | Modo | Modo | Opções: modo da tela. |
| `CfgFullscreen` | Pantalla | Tela Cheia | Valor do modo de tela: tela cheia. |
| `CfgWindowed` | Ventana | Janela | Valor do modo de tela: janela. |
| `CfgControls` | Controles | Controles | Opções: abre a tela de controles (também é o título dela). |
| `CfgDefaults` | Restaurar Valores | Restaurar Padrões | Opções e Controles: botão que restaura os padrões. |
| `CfgExit` | Salir | Sair | Opções e Controles: botão que volta. |
| `CfgKeyUp` | Arriba | Cima | Controles: tecla de mover para cima. |
| `CfgKeyDown` | Abajo | Baixo | Controles: tecla de mover para baixo. |
| `CfgKeyLeft` | Izquierda | Esquerda | Controles: tecla de mover para a esquerda. |
| `CfgKeyRight` | Derecha | Direita | Controles: tecla de mover para a direita. |
| `CfgKeyShoot` | Disparar | Atirar | Controles: tecla de atirar. |
| `CfgKeyBomb` | Bomba | Bomba | Controles: tecla da bomba. |
| `CfgKeyFocus` | Foco | Foco | Controles: tecla de foco (movimento lento e mostra a hitbox). |
| `CfgPressKey` | Pulsa una tecla | Pressione uma tecla | Aviso ao escolher uma tecla para trocar. |
| `CfgHint` | Izq/Der: Cambiar   Z: Elegir   X: Volver | Esq/Dir: Alterar   Z: Escolher   X: Voltar | Dica das teclas, no rodapé das opções. |

### HUD (painel da direita)


| Chave | Texto (ES) | Referência (PT) | Contexto |
| --- | --- | --- | --- |
| `HiScore` | Récord | Recorde | HUD: r��tulo do maior recorde. |
| `Score` | Puntos | Pontos | HUD: rótulo da pontuação atual. |
| `Lives` | Vidas | Vidas | HUD: rótulo das vidas (corações). |
| `Lantern` | Bombas | Bombas | HUD: rótulo das bombas (lamparinas). |
| `Power` | Poder | Poder | HUD: rótulo do poder da lamparina (chamas). |
| `Wind` | Viento | Vento | HUD: rótulo do vento (bússola). |
| `WindCalm` | Calmo | Calmo | HUD: vento calmo. |
| `WindNone` | Sin viento | Sem vento | HUD: sem vento. |
| `WindWeak` | Débil | Fraco | HUD: vento fraco. |
| `WindMedium` | Medio | Médio | HUD: vento médio. |
| `WindStrong` | Fuerte | Forte | HUD: vento forte. |
| `North` | Norte | Norte | Direção do vento: norte. |
| `South` | Sur | Sul | Direção do vento: sul. |
| `East` | Este | Leste | Direção do vento: leste. |
| `West` | Oeste | Oeste | Direção do vento: oeste. |
| `FairyName` | Naru | Naru | Nome do espírito da lamparina. NÃO aparece mais no painel; ainda é o nome mostrado nas falas dela (ela ainda vai ganhar outro nome). |
| `FairyRole` | Espíritu de la lámpara | Espírito da lamparina | Título mostrado ao lado do retrato do espírito da lamparina, no painel (quebra em duas linhas). |
| `BoyName` | Atli | Atli | Nome do menino (o jogador), nas falas. |
| `DialogueHint` | Z: Seguir   X: Saltar | Z: Avançar   X: Pular | Dica das teclas, no canto da caixa de diálogo. |

### Pausa e fim da partida


| Chave | Texto (ES) | Referência (PT) | Contexto |
| --- | --- | --- | --- |
| `Paused` | Pausa | Pausado | Título do menu de pausa. |
| `Continue` | Continuar | Continuar | Pausa: continuar. |
| `GiveUp` | Rendirse | Desistir | Pausa: desistir (volta ao menu). |
| `PauseHint` | Esc: Continuar | Esc: Continuar | Dica das teclas, no menu de pausa. |
| `Victory` | ¡Victoria! | Vitória! | Tela final: título quando o jogador vence. |
| `Defeat` | Derrota... | Derrota... | Tela final: título quando o jogador perde. |
| `Restart` | Reiniciar | Reiniciar | Tela final: botão de jogar de novo. |
| `NewRecord` | ¡Nuevo récord! | Novo recorde! | Tela final: aviso de novo recorde. |

### Escolha de cartas


| Chave | Texto (ES) | Referência (PT) | Contexto |
| --- | --- | --- | --- |
| `ChooseCard` | Elige una carta | Escolha uma carta | Título da escolha de carta (genérico). |
| `ChooseCardStart` | Empieza una nueva fase | Uma nova fase começa | Título da escolha de carta no começo de uma fase. |
| `ChooseCardBoss` | ¡Un gran poder! | Um grande poder! | Título da escolha de carta depois de vencer um chefe. |
| `CardsHint` | Izq Der: Elegir   Z: Tomar | Esq Dir: Escolher   Z: Pegar | Dica das teclas, na escolha de carta. |
| `YourCards` | Tus cartas | Suas cartas | Rótulo da lista das cartas que o jogador já tem. |

### Falas curtas do espírito da lamparina (caixa de fala da HUD)


| Chave | Texto (ES) | Referência (PT) | Contexto |
| --- | --- | --- | --- |
| `SayStart` | ¡Vamos! | Vamos lá! | Fala no começo da partida. |
| `SayHit` | ¡Ay! ¡Aguanta! | Ai! Aguenta firme! | Fala quando o barco é atingido. |
| `SayBomb` | ¡Yo te protejo! | Eu te protejo! | Fala quando o jogador usa uma bomba. |
| `SayRage` | ¡Se enfureció! | Ficou furioso! | Fala quando o chefe entra na segunda forma (fúria). |
| `SayEnraged` | ¡Ahí viene otro! | Lá vem outro! | Fala a cada novo ataque do chefe enfurecido. |
| `SayVictory` | ¡¡¡Ganamos!!! | Vencemos!!! | Fala na tela de vitória. |
| `SayDefeat` | ¡No te rindas! | Não desista! | Fala na tela de derrota. |
| `SayPowerUp` | ¡Me siento fuerte! | Me sinto mais forte! | Fala quando o poder da lamparina sobe. |
| `SayFullPower` | ¡Poder total! | Poder total! | Fala quando o poder chega ao máximo. |
| `SayBombRefill` | ¡Bomba extra! | Uma bomba reserva! | Fala quando uma bomba reserva é pega. |
| `SayLifeRefill` | ¡Vida extra! | Uma vida reserva! | Fala quando uma vida reserva é pega. |
| `SayCard` | ¡Excelente! | Boa escolha! | Fala quando o jogador escolhe uma carta. |
| `SayWard` | ¡A salvo! | Estamos salvos! | Fala quando o escudo de uma carta segura um golpe. |

### História contada fora das falas (abertura e fim)


| Chave | Texto (ES) | Referência (PT) | Contexto |
| --- | --- | --- | --- |
| `IntroHint` | Pulsa cualquier tecla | Pressione qualquer tecla | Dica no canto de baixo à direita, na abertura do jogo e na cena do fim da história (cada uma fecha sozinha em 10 segundos, ou com qualquer tecla). |
| `IntroPage1` | El día fue devorado por la noche sin fin, la luna flota en el cielo. | O dia foi consumido pela noite sem fim, a lua paira sob o céu. | Abertura do jogo: parágrafo 1 da história, escrito na escuridão, embaixo à esquerda, sobre a cena da noite sem fim (todos os parágrafos aparecem juntos). |
| `IntroPage2` | Un joven navegante, acompañado de su lámpara, parte en un viaje para descubrir lo que ocurrió. | Um jovem velejador, acompanhado de sua lamparina, parte em uma jornada para descobrir o que houve. | Abertura do jogo: parágrafo 2 da história, escrito na escuridão, embaixo à esquerda, sobre a cena da noite sem fim (todos os parágrafos aparecem juntos). |
| `S6Epi01` | Tras regresar del reino espiritual, Atli y Naru vuelven a navegar por las aguas. | Após voltarem do reino espiritual, Atli e Naru voltam a velejar pelas águas. | Fim do jogo: parágrafo 1 da história final, escrito embaixo da cena do nascer do sol (todos os parágrafos aparecem juntos). |
| `S6Epi02` | Sentados a la orilla del río para pescar, mientras admiran el amanecer que resplandece sobre las aguas, | Sentando-se à beira do rio para pescar, enquanto admiram o nascer do sol que resplandece sob as águas, | Fim do jogo: parágrafo 2 da história final, escrito embaixo da cena do nascer do sol (todos os parágrafos aparecem juntos). |
| `S6Epi03` | Una vez más... | Uma vez mais... | Fim do jogo: parágrafo 3 da história final, escrito embaixo da cena do nascer do sol (todos os parágrafos aparecem juntos). |
| `TheEnd` | FIN | FIM | Fim do jogo: escrito bem grande embaixo, no centro, na cena do nascer do sol que fecha a história. |

### Música


| Chave | Texto (ES) | Referência (PT) | Contexto |
| --- | --- | --- | --- |
| `StageMusic` | ♪ Los espíritus toman la noche | ♪ Os espíritos tomam a noite | Nome da música da fase 1, mostrado no título da fase. |
| `Stage2Music` | ♪ Luces de fiesta en medio de la oscuridad | ♪ Luzes de festa em meio à escuridão | Nome da música da fase 2, mostrado no título da fase: um quadro animado, com trompetes, saxofones e bateria. |
| `Stage3Music` | ♪ Seres tramposos bajo la tormenta | ♪ Seres trapaceiros sob a tempestade | Nome da música da fase 3, mostrado no título da fase: sombria, com trompetes e pianos. |
| `Boss3Music` | ♪ La sirvienta esquiva que estornuda rayos | ♪ A serva arisca que espirra raios | Nome da música do chefe da fase 3 (Thaqai), em toda a luta: mais forte, com piano grave e trompetes. |
| `Stage4Music` | ♪ Olor a murciélagos y musgo | ♪ Cheiro de morcegos e musgo | Nome da música da fase 4, mostrado no título da fase: calma, com tambores lentos e trompetes. |
| `Boss4Music` | ♪ La gran bestia Deidane | ♪ A grande fera Deidane | Nome da música do chefe da fase 4 (Ukhumo), em toda a luta: violenta, animalesca, com tambores e trompetes. |
| `Stage5Music` | ♪ La cascada invertida y mil maravillas | ♪ Cachoeira invertida e mil maravilhas | Nome da música da fase 5, mostrado no título da fase: épica, com flautas, trompetes e saxofones (era a música do chefe). |
| `Boss5Music` | ♪ La serpiente guardiana de los reinos | ♪ A serpe guardiã dos reinos | Nome da música do chefe da fase 5 (Kuychu), em toda a luta: um hino divino, com trompetes e coros, que lembra a música católica. |
| `Stage6Music` | ♪ El río que recorre el universo | ♪ O rio que percorre o universo | Nome da música da fase 6, mostrado no título da fase: divina e cósmica, com órgão, piano, coros, canto gregoriano e trompetes. |
| `Boss2Music` | ♪ El bailarín de las estrellas | ♪ O dançarino das estrelas | Nome da música do chefe da fase 2 (Baylo), em toda a luta: intensa, com trompetes, saxofones e bateria. |
| `BossMusic` | ♪ Un veneno que quema como fuego | ♪ Um veneno que queima como fogo | Nome da música do chefe da fase 1 (Iryssh), em toda a luta. |


---

## Fase 1: Iryssh, a mulher-serpente de fogo


### Título e nomes


| Chave | Texto (ES) | Referência (PT) | Contexto |
| --- | --- | --- | --- |
| `StageTitle` | Fase 1 | Fase 1 | Título da fase 1, mostrado quando ela começa. |
| `IrysshName` | Iryssh | Iryssh | Nome de personagem da fase (mostrado no título do chefe e nas falas). |

### Ataques do chefe


| Chave | Texto (ES) | Referência (PT) | Contexto |
| --- | --- | --- | --- |
| `AttackRingOfEmbers` | Fuego: Anillo de Brasas | Fogo: Anel de Brasas | Nome do ataque do chefe da fase 1 (a primeira forma, calma). |
| `AttackPiercingTongue` | Llama: Lengua Perforante | Chama: Língua Perfurante | Nome do ataque do chefe da fase 1 (a primeira forma, calma). |
| `AttackTwinWhirlwinds` | Llamarada: Torbellinos Gemelos | Labareda: Redemoinhos Gêmeos | Nome do ataque do chefe da fase 1 (a primeira forma, calma). |
| `AttackBlazingBloom` | Loto: Flor Llameante | Lótus: Flor Flamejante | Nome do ataque do chefe da fase 1 (a segunda forma, enfurecida). |
| `AttackFuryOfTheRiver` | Furia del Río Incandescente | Fúria do Rio Incandescente | Nome do ataque do chefe da fase 1 (a segunda forma, enfurecida). |
| `AttackTideOfEmbers` | Embestida de la Gran Serpiente | Investida da Grande Serpente | Nome do ataque do chefe da fase 1 (a segunda forma, enfurecida). |

---

## Fase 2: Baylo, o boto da festa


### Título e nomes


| Chave | Texto (ES) | Referência (PT) | Contexto |
| --- | --- | --- | --- |
| `Stage2Title` | Fase 2 | Fase 2 | Título da fase 2, mostrado quando ela começa. |
| `BayloName` | Baylo | Baylo | Nome de personagem da fase (mostrado no título do chefe e nas falas). |

### Ataques do chefe


| Chave | Texto (ES) | Referência (PT) | Contexto |
| --- | --- | --- | --- |
| `AttackDoubleStep` | Baile: Paso Doble | Dança: Passo Duplo | Nome do ataque do chefe da fase 2 (a primeira forma, calma). |
| `AttackConfettiRain` | Música: Lluvia de Notas | Música: Chuva de Notas | Nome do ataque do chefe da fase 2 (a primeira forma, calma). |
| `AttackSambaWheel` | Música: Rueda de Pétalos | Música: Roda de Pétalas | Nome do ataque do chefe da fase 2 (a primeira forma, calma). |
| `AttackDolphinLeap` | El Gran Salto del Rey de la Fiesta | Grande Mergulho do Rei da Festa | Nome do ataque do chefe da fase 2 (a primeira forma, calma). |
| `AttackPartyWave` | Disparo del Río Profundo | Disparo do Rio Profundo | Nome do ataque do chefe da fase 2 (a primeira forma, calma). |

---

## Fase 3: Thaqai, a gata das estrelas


### Título e nomes


| Chave | Texto (ES) | Referência (PT) | Contexto |
| --- | --- | --- | --- |
| `Stage3Title` | Fase 3 | Fase 3 | Título da fase 3, mostrado quando ela começa. |
| `ThaqaiName` | Thaqai | Thaqai | Nome de personagem da fase (mostrado no título do chefe e nas falas). |

### Ataques do chefe


| Chave | Texto (ES) | Referência (PT) | Contexto |
| --- | --- | --- | --- |
| `AttackHailCurtain` | Jaula: Cortina Blanca | Jaula: Cortina Branca | Nome do ataque do chefe da fase 3 (a primeira forma, calma). |
| `AttackStarPentagram` | Estrella: Pentagrama | Estrela: Pentagrama | Nome do ataque do chefe da fase 3 (a primeira forma, calma). |
| `AttackZigzagLightning` | Rayo: Zigzag | Raio: Zigue-zague | Nome do ataque do chefe da fase 3 (a primeira forma, calma). |
| `AttackFineWeave` | Jaula: Tejido Fino | Jaula: Tecido Fino | Nome do ataque do chefe da fase 3 (a primeira forma, calma). |
| `AttackSkyFalls` | El Cielo Cae | O Céu Cai | Nome do ataque do chefe da fase 3 (a segunda forma, enfurecida). |
| `AttackEclipse` | La Luna Cubierta | A Lua Coberta | Nome do ataque do chefe da fase 3 (a segunda forma, enfurecida). |

---

## Fase 4: Ukhumo, a ursa da caverna


### Título e nomes


| Chave | Texto (ES) | Referência (PT) | Contexto |
| --- | --- | --- | --- |
| `Stage4Title` | Fase 4 | Fase 4 | Título da fase 4, mostrado quando ela começa. |
| `UkhumoName` | Ukhumo | Ukhumo | Nome de personagem da fase (mostrado no título do chefe e nas falas). |

### Ataques do chefe


| Chave | Texto (ES) | Referência (PT) | Contexto |
| --- | --- | --- | --- |
| `AttackClawRake` | Garras: Tres Cortes | Garras: Três Cortes | Nome do ataque do chefe da fase 4 (a primeira forma, calma). |
| `AttackRoar` | Rugido: Aliento Brutal | Rugido: Sopro Brutal | Nome do ataque do chefe da fase 4 (a primeira forma, calma). |
| `AttackStomp` | Pisotón: Lluvia de Estalactitas | Pisada: Chuva de Estalactites | Nome do ataque do chefe da fase 4 (a primeira forma, calma). |
| `AttackBruteForce` | Fuerza Bruta: Roca Lanzada | Força Bruta: Pedra Arremessada | Nome do ataque do chefe da fase 4 (a primeira forma, calma). |
| `AttackCrossingClaws` | Garras Cruzadas | Garras Cruzadas | Nome do ataque do chefe da fase 4 (a segunda forma, enfurecida). |
| `AttackCaveIn` | Derrumbe Feroz | Desabamento Feroz | Nome do ataque do chefe da fase 4 (a segunda forma, enfurecida). |
| `AttackFeast` | Gran Banquete de Garras | Grande Banquete de Garras | Nome do ataque do chefe da fase 4 (a segunda forma, enfurecida). |

---

## Fase 5: Kuychu, o guardião de duas cabeças


### Título e nomes


| Chave | Texto (ES) | Referência (PT) | Contexto |
| --- | --- | --- | --- |
| `Stage5Title` | Fase 5 | Fase 5 | Título da fase 5, mostrado quando ela começa. |
| `KuychuName` | Kuychu | Kuychu | Nome de personagem da fase (mostrado no título do chefe e nas falas). |
| `SerpentLeftName` | Serpiente izquierda | Serpente esquerda | Nome de personagem da fase (mostrado no título do chefe e nas falas). |
| `SerpentRightName` | Serpiente derecha | Serpente direita | Nome de personagem da fase (mostrado no título do chefe e nas falas). |

### Ataques do chefe


| Chave | Texto (ES) | Referência (PT) | Contexto |
| --- | --- | --- | --- |
| `AttackTwoHeads` | Dos Cabezas: Por Turnos | Duas Cabeças: Por Turnos | Nome do ataque do chefe da fase 5 (a primeira forma, calma). |
| `AttackSevenArcs` | Arcoíris: Siete Arcos | Arco-Íris: Sete Arcos | Nome do ataque do chefe da fase 5 (a primeira forma, calma). |
| `AttackBorderQuake` | Terremoto: La Frontera Tiembla | Terremoto: A Fronteira Treme | Nome do ataque do chefe da fase 5 (a primeira forma, calma). |
| `AttackTide` | Marea: Murallas de Agua | Maré: Muralhas d'Água | Nome do ataque do chefe da fase 5 (a primeira forma, calma). |
| `AttackTrial` | Prueba de Valor: Cara a Cara | Prova de Valor: Cabeça a Cabeça | Nome do ataque do chefe da fase 5 (a primeira forma, calma). |
| `AttackBrokenRainbow` | El Arcoíris Roto | Arco-Íris Partido | Nome do ataque do chefe da fase 5 (a segunda forma, enfurecida). |
| `AttackTwoWorlds` | Dos Mundos: El Temblor | Dois Mundos: O Abalo | Nome do ataque do chefe da fase 5 (a segunda forma, enfurecida). |
| `AttackGuardian` | Guardián de los Dos Reinos | Guardião dos Dois Reinos | Nome do ataque do chefe da fase 5 (a segunda forma, enfurecida). |

---

## Fase 6: Mama Killa, a Lua


### Título e nomes


| Chave | Texto (ES) | Referência (PT) | Contexto |
| --- | --- | --- | --- |
| `Stage6Title` | Fase 6 | Fase 6 | Título da fase 6, mostrado quando ela começa. |
| `KillaName` | Killa | Killa | Nome de personagem da fase (mostrado no título do chefe e nas falas). |

### Ataques do chefe


A Killa ainda usa os ataques da fase 3 (Thaqai); os nomes são os mesmos daquela fase.

---

## Cartas


Cada carta tem um nome (**Título**) e uma descrição curta do que ela faz (**Detalhe**), mostrados na escolha de carta e na HUD.

| Chave | Texto (ES) | Referência (PT) | Contexto |
| --- | --- | --- | --- |
| `CardHomingWisp.Title` | Llama Guía | Chama Guia | Nome da carta (HomingWisp). |
| `CardHomingWisp.Detail` | Una llama te sigue y caza al enemigo más cercano. | Uma chama segue você e caça o inimigo mais próximo. | Descrição da carta (HomingWisp). |
| `CardTwinFan.Title` | Abanico Doble | Leque Duplo | Nome da carta (TwinFan). |
| `CardTwinFan.Detail` | Dos rayos más débiles se abren con cada disparo. | Dois raios mais fracos se abrem a cada tiro. | Descrição da carta (TwinFan). |
| `CardPiercingLight.Title` | Luz Perforante | Luz Perfurante | Nome da carta (PiercingLight). |
| `CardPiercingLight.Detail` | Tus rayos atraviesan a un enemigo más. | Seus raios atravessam mais um inimigo. | Descrição da carta (PiercingLight). |
| `CardFervor.Title` | Fervor | Fervor | Nome da carta (Fervor). |
| `CardFervor.Detail` | +25% de daño en cada disparo. | +25% de dano em cada tiro. | Descrição da carta (Fervor). |
| `CardQuickWick.Title` | Mecha Rápida | Pavio Rápido | Nome da carta (QuickWick). |
| `CardQuickWick.Detail` | +20% de cadencia de disparo. | +20% de cadência de tiro. | Descrição da carta (QuickWick). |
| `CardBurstingBolts.Title` | Rayos Explosivos | Raios Explosivos | Nome da carta (BurstingBolts). |
| `CardBurstingBolts.Detail` | Explotan al impactar y hieren a los cercanos. | Explodem ao acertar e ferem quem está perto. | Descrição da carta (BurstingBolts). |
| `CardSpiritWard.Title` | Protección del Espíritu | Proteção do Espírito | Nome da carta (SpiritWard). |
| `CardSpiritWard.Detail` | Un escudo de luz bloquea el próximo golpe. | Um escudo de luz bloqueia o próximo golpe. | Descrição da carta (SpiritWard). |
| `CardMendingWard.Title` | Escudo Vivo | Escudo Vivo | Nome da carta (MendingWard). |
| `CardMendingWard.Detail` | Un escudo bloquea un golpe y se rehace cada 45 segundos. | Um escudo bloqueia um golpe e se refaz a cada 45 segundos. | Descrição da carta (MendingWard). |
| `CardSlimWake.Title` | Casco Fino | Casco Fino | Nome da carta (SlimWake). |
| `CardSlimWake.Detail` | Tu barco es más difícil de golpear: hitbox menor. | Seu barco é mais difícil de acertar: hitbox menor. | Descrição da carta (SlimWake). |
| `CardCalmSails.Title` | Velas en Calma | Velas Calmas | Nome da carta (CalmSails). |
| `CardCalmSails.Detail` | El viento te empuja un 40% menos. | O vento empurra você 40% menos. | Descrição da carta (CalmSails). |
| `CardBrighterLight.Title` | Luz Más Fuerte | Luz Mais Forte | Nome da carta (BrighterLight). |
| `CardBrighterLight.Detail` | +20% de alcance de la bomba. | +20% de alcance da bomba. | Descrição da carta (BrighterLight). |
| `CardLongBurn.Title` | Llama Larga | Chama Longa | Nome da carta (LongBurn). |
| `CardLongBurn.Detail` | Las bombas duran 1 segundo más. | As bombas duram 1 segundo a mais. | Descrição da carta (LongBurn). |
| `CardSearingLight.Title` | Luz Ardiente | Luz Ardente | Nome da carta (SearingLight). |
| `CardSearingLight.Detail` | +50% de daño de la bomba. | +50% de dano da bomba. | Descrição da carta (SearingLight). |
| `CardSpareLantern.Title` | Bomba de Reserva | Bomba Reserva | Nome da carta (SpareLantern). |
| `CardSpareLantern.Detail` | Recarga una bomba. | Recarrega uma bomba. | Descrição da carta (SpareLantern). |
| `CardHungryLantern.Title` | Bomba Hambrienta | Bomba Faminta | Nome da carta (HungryLantern). |
| `CardHungryLantern.Detail` | Las luces dan +50% de energía. | As luzes dão +50% de energia. | Descrição da carta (HungryLantern). |
| `CardWideNet.Title` | Red Ancha | Rede Larga | Nome da carta (WideNet). |
| `CardWideNet.Detail` | +40% de alcance para atraer las luces. | +40% de alcance para atrair as luzes. | Descrição da carta (WideNet). |
| `CardGoldenSpirits.Title` | Espíritus Dorados | Espíritos Dourados | Nome da carta (GoldenSpirits). |
| `CardGoldenSpirits.Detail` | +25% de puntos. | +25% de pontos. | Descrição da carta (GoldenSpirits). |
| `CardSerpentFlame.Title` | Llama de la Serpiente | Chama da Serpente | Nome da carta (SerpentFlame). Carta especial, deixada pelo chefe da fase 1. |
| `CardSerpentFlame.Detail` | +35% de daño, los disparos explotan en brasas. | +35% de dano, tiros explodem em brasas. | Descrição da carta (SerpentFlame). Carta especial, deixada pelo chefe da fase 1. |
| `CardPartyFever.Title` | Fiebre de Fiesta | Febre da Festa | Nome da carta (PartyFever). Carta especial, deixada pelo chefe da fase 2. |
| `CardPartyFever.Detail` | +40% de puntos, las luces dan +30% de energía. | +40% de pontos, luzes dão +30% de energia. | Descrição da carta (PartyFever). Carta especial, deixada pelo chefe da fase 2. |
| `CardStormBolt.Title` | Rayo de la Tormenta | Raio da Tempestade | Nome da carta (StormBolt). Carta especial, deixada pelo chefe da fase 3. |
| `CardStormBolt.Detail` | Los disparos atraviesan 2 más y vuelan un 25% más rápido. | Tiros atravessam +2 e voam 25% mais rápido. | Descrição da carta (StormBolt). Carta especial, deixada pelo chefe da fase 3. |
| `CardBearCloak.Title` | Manto de Oso | Manto de Urso | Nome da carta (BearCloak). Carta especial, deixada pelo chefe da fase 4. |
| `CardBearCloak.Detail` | Un escudo bloquea el próximo golpe, y el viento te empuja un 30% menos. | Um escudo bloqueia o próximo golpe, e o vento empurra 30% menos. | Descrição da carta (BearCloak). Carta especial, deixada pelo chefe da fase 4. |


---

## Outros


| Chave | Texto (ES) | Referência (PT) | Contexto |
| --- | --- | --- | --- |
| `AchUnlocked` | ¡Logro desbloqueado! | Conquista desbloqueada! | Texto sem contexto catalogado ainda. |
| `AchBoss` | Jefe derrotado: {0} ({1}) | Chefe derrotado: {0} ({1}) | Texto sem contexto catalogado ainda. |
| `AchJoke` | ¿Modo fácil? ¿En serio? | Modo fácil? Sério? | Texto sem contexto catalogado ainda. |
| `AchEndingBad` | Final: Noche sin fin | Final: Noite sem fim | Texto sem contexto catalogado ainda. |
| `AchEndingTrue` | Final: Aurora | Final: Aurora | Texto sem contexto catalogado ainda. |
| `WeaponTitle` | Elige tu ataque | Escolha o ataque | Texto sem contexto catalogado ainda. |
| `WeaponStreamName` | Lluvia frontal ondulante | Chuva frontal ondulada | Texto sem contexto catalogado ainda. |
| `WeaponStreamDesc` | Mucho daño, pero difícil de controlar. | Muito dano, porém difícil de controlar. | Texto sem contexto catalogado ainda. |
| `WeaponHomingName` | Luz quebrada perseguidora | Luz quebrada perseguidora | Texto sem contexto catalogado ainda. |
| `WeaponHomingDesc` | Acierto garantizado, daño reducido. | Acerto garantido, dano reduzido. | Texto sem contexto catalogado ainda. |
| `WeaponHint` | Izq Der: Elegir   Z: Confirmar   X: Volver | Esq Dir: Escolher   Z: Confirmar   X: Voltar | Texto sem contexto catalogado ainda. |
| `CfgLanguage` | Idioma | Idioma | Texto sem contexto catalogado ainda. |
| `Boss6Music` | ♪ La luna plateada se refleja en el lago | ♪ A lua prateada reflete sob o lago | Texto sem contexto catalogado ainda. |
| `MenuMusic` | ♪ Aguas quietas - entrada y menú | ♪ Águas quietas - entrada e menu | Texto sem contexto catalogado ainda. |
| `SadEndingMusic` | ♪ Noche sin fin - final malo | ♪ Noite sem fim - final ruim | Texto sem contexto catalogado ainda. |
| `HappyEndingMusic` | ♪ Aurora - final | ♪ Aurora - final | Texto sem contexto catalogado ainda. |
| `MusicRoomTitle` | Sala de música | Sala de música | Texto sem contexto catalogado ainda. |
| `RestartsLeft` | Reinicios restantes: | Reinícios restantes: | Texto sem contexto catalogado ainda. |
| `GameOver` | Game Over | Game Over | Texto sem contexto catalogado ainda. |
| `GameOverMusic` | ♪ Luz que se apaga - game over | ♪ Luz que se apaga - game over | Texto sem contexto catalogado ainda. |
| `AttackBonus` | ¡Bonus de tiempo! | Bônus de tempo! | Texto sem contexto catalogado ainda. |
| `TimeUp` | ¡Tiempo agotado! | Tempo esgotado! | Texto sem contexto catalogado ainda. |
| `DifficultyStages` | ({0} Fases) | ({0} Fases) | Texto sem contexto catalogado ainda. |
| `AttackCatPlay` | Juego de la Gata: Ovillos que Rebotan | Brincadeira da Gata: Novelos que Quicam | Texto sem contexto catalogado ainda. |
| `AttackKillaCrescent` | Luna Creciente: Los Cuernos de Plata | Lua Crescente: Os Chifres de Prata | Texto sem contexto catalogado ainda. |
| `AttackKillaTide` | Marea Lunar: El Río del Cielo | Maré Lunar: O Rio do Céu | Texto sem contexto catalogado ainda. |
| `AttackKillaEclipse` | Eclipse Total: El Sol Cubierto | Eclipse Total: O Sol Coberto | Texto sem contexto catalogado ainda. |
| `AttackKillaNewMoon` | Luna Nueva: El Cielo Sin Luna | Lua Nova: O Céu Sem Lua | Texto sem contexto catalogado ainda. |
| `AttackKillaFullMoon` | Luna Llena: El Ojo de Plata | Lua Cheia: O Olho de Prata | Texto sem contexto catalogado ainda. |
| `AttackKillaConstellations` | Constelaciones: El Mapa de las Estrellas | Constelações: O Mapa das Estrelas | Texto sem contexto catalogado ainda. |
| `AttackKillaSilverRain` | Lluvia de Plata: Los Rayos de la Luna | Chuva de Prata: Os Raios da Lua | Texto sem contexto catalogado ainda. |
| `AttackKillaWaning` | Luna Menguante: La Sombra Crece | Lua Minguante: A Sombra Cresce | Texto sem contexto catalogado ainda. |
| `AttackKillaPentagram` | Pentagrama Lunar: La Estrella de Cinco Puntas | Pentagrama Lunar: A Estrela de Cinco Pontas | Texto sem contexto catalogado ainda. |
| `AttackKillaEternalNight` | Noche Eterna | Noite Eterna | Texto sem contexto catalogado ainda. |
| `BadEnd1` | Rechazados por el guardián, Atli y Naru vuelven a navegar por la noche sin fin. | Não aceitos pelo guardião, Atli e Naru voltam a velejar pela noite sem fim. | Texto sem contexto catalogado ainda. |
| `BadEnd2` | El cielo sigue tomado por la luna, y a Atli solo le queda intentar pescar en la oscuridad. | O céu continua tomado pela lua, e resta apenas a Atli tentar pescar no escuro. | Texto sem contexto catalogado ainda. |
| `BadEnd3` | *Suspiros* ¿Podrán los peces ver mi carnada? | *Suspiros* Será que os peixes conseguem ver minha isca? | Texto sem contexto catalogado ainda. |
| `BadEnd4` | FIN | FIM | Texto sem contexto catalogado ainda. |
