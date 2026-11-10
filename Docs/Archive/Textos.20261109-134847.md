# Airumã: todos os outros textos do jogo

**Como editar:** mude apenas a coluna **Texto** (a coluna Contexto não é lida; pode escrever observações nela). Não mude a coluna **Chave**, não apague linhas e não use o caractere `|` dentro de um texto. Depois é só me pedir para ler os documentos: eu aplico tudo de uma vez no jogo (`python3 Docs/tools/textos.py import`). Para gerar os documentos de novo a partir do jogo: `python3 Docs/tools/textos.py export`.

- Os dois documentos são só em português. O inglês do jogo não é tocado; quando você terminar, me diga se quer que eu traduza o que mudou.
- Cada fala cabe em até 5 linhas curtas na caixa de diálogo (uns 28 letras por linha), então textos muito longos são cortados.
- A fonte do jogo não tem alguns sinais, como aspas e ponto e vírgula; prefira vírgula, ponto, dois pontos, traço, parênteses, `!`, `?`, `/`, `+`, `%` e `...`.
- Os diálogos entre os personagens estão em `Docs/Dialogos.md`.
- Os nomes dos ataques dos chefes aparecem como o título de uma carta de feitiço ("Tema: Nome") quando o ataque começa.

---

## Interface

### Menu principal


| Chave | Texto (PT-BR) | Contexto |
| --- | --- | --- |
| `Title` | Airumã | Título do jogo, no alto do menu principal. |
| `MenuStart` | Iniciar | Menu principal: começa uma partida (leva à escolha da dificuldade). |
| `MenuBossPractice` | Treino | Menu principal: abre o treino (de fases ou de chefes). |
| `MenuScores` | Recordes | Menu principal: mostra os recordes. |
| `MenuMusicRoom` | Sala de música | Menu principal: sala de músicas (ainda não existe; mostra o aviso "Em breve"). |
| `MenuConfig` | Opções | Menu principal: abre as opções. |
| `MenuQuit` | Sair | Menu principal: fecha o jogo. |
| `MenuHint` | Z: Escolher   X: Voltar | Dica das teclas, no canto de baixo do menu. |
| `ComingSoon` | Em breve | Aviso que pisca ao escolher uma opção que ainda não existe. |
| `Back` | Voltar | Voltar, nos submenus. |
| `Menu` | Menu | Botão "ir para o menu", na tela final da partida. |
| `DemoLabel` | Demonstração | Aviso que pisca durante a demonstração (o jogo joga sozinho quando o menu fica 10 segundos parado). |

### Dificuldade, treino e recordes


| Chave | Texto (PT-BR) | Contexto |
| --- | --- | --- |
| `DifficultyTitle` | Dificuldade | Título da escolha de dificuldade. |
| `DifficultyEasy` | Fácil | Dificuldade fácil. |
| `DifficultyNormal` | Normal | Dificuldade normal. |
| `DifficultyHard` | Difícil | Dificuldade difícil. |
| `PracticeTitle` | Treino de Chefe | Título da lista de treino de chefes. |
| `PracticeStages` | Fases | Treino: opção "treinar uma fase inteira". |
| `PracticeBosses` | Chefes | Treino: opção "treinar só o chefe". |
| `PracticeStageTitle` | Treino de Fase | Título da lista de treino de fases. |
| `LockedBoss` | ????? | Nome de um chefe ainda não existente na lista de treino (aparece com um cadeado). |
| `ScoresTitle` | Recordes | Título da tela de recordes. |
| `NoScore` | ------- | Linha vazia da tabela de recordes. |
| `Cleared` | Completo | Marca ao lado do recorde de uma partida que chegou ao fim. |

### Opções


| Chave | Texto (PT-BR) | Contexto |
| --- | --- | --- |
| `CfgTitle` | Configurações | Título da tela de opções. |
| `CfgLives` | Vidas | Opções: vidas no início da partida (1 a 5). |
| `CfgBombs` | Bombas | Opções: bombas no início da partida (0 a 3). |
| `CfgMusic` | Volume da Música | Opções: volume da música. |
| `CfgSfx` | Efeitos Sonoros | Opções: volume dos efeitos sonoros. |
| `CfgMode` | Modo | Opções: modo da tela. |
| `CfgFullscreen` | Tela Cheia | Valor do modo de tela: tela cheia. |
| `CfgWindowed` | Janela | Valor do modo de tela: janela. |
| `CfgControls` | Controles | Opções: abre a tela de controles (também é o título dela). |
| `CfgDefaults` | Restaurar Padrões | Opções e Controles: botão que restaura os padrões. |
| `CfgExit` | Sair | Opções e Controles: botão que volta. |
| `CfgKeyUp` | Cima | Controles: tecla de mover para cima. |
| `CfgKeyDown` | Baixo | Controles: tecla de mover para baixo. |
| `CfgKeyLeft` | Esquerda | Controles: tecla de mover para a esquerda. |
| `CfgKeyRight` | Direita | Controles: tecla de mover para a direita. |
| `CfgKeyShoot` | Atirar | Controles: tecla de atirar. |
| `CfgKeyBomb` | Bomba | Controles: tecla da bomba. |
| `CfgKeyFocus` | Foco | Controles: tecla de foco (movimento lento e mostra a hitbox). |
| `CfgPressKey` | Pressione uma tecla | Aviso ao escolher uma tecla para trocar. |
| `CfgHint` | Esq/Dir: Alterar   Z: Escolher   X: Voltar | Dica das teclas, no rodapé das opções. |

### HUD (painel da direita)


| Chave | Texto (PT-BR) | Contexto |
| --- | --- | --- |
| `HiScore` | Recorde | HUD: r��tulo do maior recorde. |
| `Score` | Pontos | HUD: rótulo da pontuação atual. |
| `Lives` | Vidas | HUD: rótulo das vidas (corações). |
| `Lantern` | Bombas | HUD: rótulo das bombas (lamparinas). |
| `Power` | Poder | HUD: rótulo do poder da lamparina (chamas). |
| `Wind` | Vento | HUD: rótulo do vento (bússola). |
| `WindCalm` | Calmo | HUD: vento calmo. |
| `WindNone` | Sem vento | HUD: sem vento. |
| `WindWeak` | Fraco | HUD: vento fraco. |
| `WindMedium` | Médio | HUD: vento médio. |
| `WindStrong` | Forte | HUD: vento forte. |
| `North` | Norte | Direção do vento: norte. |
| `South` | Sul | Direção do vento: sul. |
| `East` | Leste | Direção do vento: leste. |
| `West` | Oeste | Direção do vento: oeste. |
| `FairyName` | Naru | Nome do espírito da lamparina. NÃO aparece mais no painel; ainda é o nome mostrado nas falas dela (ela ainda vai ganhar outro nome). |
| `FairyRole` | Espírito da lamparina | Título mostrado ao lado do retrato do espírito da lamparina, no painel (quebra em duas linhas). |
| `BoyName` | Atli | Nome do menino (o jogador), nas falas. |
| `DialogueHint` | Z: Avançar   X: Pular | Dica das teclas, no canto da caixa de diálogo. |

### Pausa e fim da partida


| Chave | Texto (PT-BR) | Contexto |
| --- | --- | --- |
| `Paused` | Pausado | Título do menu de pausa. |
| `Continue` | Continuar | Pausa: continuar. |
| `GiveUp` | Desistir | Pausa: desistir (volta ao menu). |
| `PauseHint` | Esc: Continuar | Dica das teclas, no menu de pausa. |
| `Victory` | Vitória! | Tela final: título quando o jogador vence. |
| `Defeat` | Derrota... | Tela final: título quando o jogador perde. |
| `Restart` | Reiniciar | Tela final: botão de jogar de novo. |
| `NewRecord` | Novo recorde! | Tela final: aviso de novo recorde. |

### Escolha de cartas


| Chave | Texto (PT-BR) | Contexto |
| --- | --- | --- |
| `ChooseCard` | Escolha uma carta | Título da escolha de carta (genérico). |
| `ChooseCardStart` | Uma nova fase começa | Título da escolha de carta no começo de uma fase. |
| `ChooseCardBoss` | Um grande poder! | Título da escolha de carta depois de vencer um chefe. |
| `CardsHint` | Esq Dir: Escolher   Z: Pegar | Dica das teclas, na escolha de carta. |
| `YourCards` | Suas cartas | Rótulo da lista das cartas que o jogador já tem. |

### Falas curtas do espírito da lamparina (caixa de fala da HUD)


| Chave | Texto (PT-BR) | Contexto |
| --- | --- | --- |
| `SayStart` | Vamos lá! | Fala no começo da partida. |
| `SayHit` | Ai! Aguenta firme! | Fala quando o barco é atingido. |
| `SayBomb` | Eu te protejo! | Fala quando o jogador usa uma bomba. |
| `SayRage` | Ficou furioso! | Fala quando o chefe entra na segunda forma (fúria). |
| `SayEnraged` | Lá vem outro! | Fala a cada novo ataque do chefe enfurecido. |
| `SayVictory` | Vencemos!!! | Fala na tela de vitória. |
| `SayDefeat` | Não desista! | Fala na tela de derrota. |
| `SayPowerUp` | Me sinto mais forte! | Fala quando o poder da lamparina sobe. |
| `SayFullPower` | Poder total! | Fala quando o poder chega ao máximo. |
| `SayBombRefill` | Uma bomba reserva! | Fala quando uma bomba reserva é pega. |
| `SayLifeRefill` | Uma vida reserva! | Fala quando uma vida reserva é pega. |
| `SayCard` | Boa escolha! | Fala quando o jogador escolhe uma carta. |
| `SayWard` | Estamos salvos! | Fala quando o escudo de uma carta segura um golpe. |

### História contada fora das falas (abertura e fim)


| Chave | Texto (PT-BR) | Contexto |
| --- | --- | --- |
| `IntroHint` | Pressione qualquer tecla | Dica no canto de baixo à direita, na abertura do jogo e na cena do fim da história (cada uma fecha sozinha em 10 segundos, ou com qualquer tecla). |
| `IntroPage1` | A sacerdotisa das terras sagradas sumiu. | Abertura do jogo: parágrafo 1 da história, escrito na escuridão, embaixo à esquerda, sobre a cena da noite sem fim (os quatro aparecem juntos). |
| `IntroPage2` | Sem ela para realizar o rito de solstício anual, o mundo cai na noite eterna. | Abertura do jogo: parágrafo 2 da história, escrito na escuridão, embaixo à esquerda, sobre a cena da noite sem fim (os quatro aparecem juntos). |
| `IntroPage3` | O jovem velejador, acompanhado de sua lamparina, parte em uma jornada para encontrá-la... | Abertura do jogo: parágrafo 3 da história, escrito na escuridão, embaixo à esquerda, sobre a cena da noite sem fim (os quatro aparecem juntos). |
| `IntroPage4` | ...para que o sol possa nascer novamente. | Abertura do jogo: parágrafo 4 da história, escrito na escuridão, embaixo à esquerda, sobre a cena da noite sem fim (os quatro aparecem juntos). |
| `S6Epi01` | Após voltarem do reino espiritual, Atli leva a sacerdotisa Ariah de volta para casa, para que ela possa completar o rito de solstício. | Fim do jogo: parágrafo 1 da história final, escrito embaixo da cena do nascer do sol (os quatro aparecem juntos). |
| `S6Epi02` | A sacerdotisa subiu ao alto da colina e completou o ritual antes que a noite terminasse de ceder. | Fim do jogo: parágrafo 2 da história final, escrito embaixo da cena do nascer do sol (os quatro aparecem juntos). |
| `S6Epi03` | E o sol nasceu de novo, quente demais, como sempre. Atli suou, reclamou... e achou graça. | Fim do jogo: parágrafo 3 da história final, escrito embaixo da cena do nascer do sol (os quatro aparecem juntos). |
| `S6Epi04` | Ambos então sentam à beira do rio e admiram o nascer do sol que resplandece sob as águas, uma vez mais. | Fim do jogo: parágrafo 4 da história final, escrito embaixo da cena do nascer do sol (os quatro aparecem juntos). |
| `TheEnd` | FIM | Fim do jogo: escrito bem grande embaixo, no centro, na cena do nascer do sol que fecha a história. |

### Música


| Chave | Texto (PT-BR) | Contexto |
| --- | --- | --- |
| `StageMusic` | ♪ Os espíritos tomam a noite | Nome da música da fase 1, mostrado no título da fase. |
| `Stage2Music` | ♪ Um festival animado para a noite | Nome da música da fase 2, mostrado no título da fase: um quadro animado, com trompetes, saxofones e bateria. |
| `Stage3Music` | ♪ Criaturas brincalhonas sob a tempestade | Nome da música da fase 3, mostrado no título da fase: sombria, com trompetes e pianos. |
| `Boss3Music` | ♪ A servente arisca que espirra raios | Nome da música do chefe da fase 3 (Thaqai), em toda a luta: mais forte, com piano grave e trompetes. |
| `Stage4Music` | ♪ Hibernação em meio a morcegos e musgo | Nome da música da fase 4, mostrado no título da fase: calma, com tambores lentos e trompetes. |
| `Boss4Music` | ♪ A grande fera Deidane | Nome da música do chefe da fase 4 (Ukhumo), em toda a luta: violenta, animalesca, com tambores e trompetes. |
| `Stage5Music` | ♪ Cachoeira invertida e mil maravilhas | Nome da música da fase 5, mostrado no título da fase: épica, com flautas, trompetes e saxofones (era a música do chefe). |
| `Boss5Music` | ♪ A serpe guardiã dos dois reinos | Nome da música do chefe da fase 5 (Kuychu), em toda a luta: um hino divino, com trompetes e coros, que lembra a música católica. |
| `Stage6Music` | ♪ O rio que percorre o universo | Nome da música da fase 6, mostrado no título da fase: divina e cósmica, com órgão, piano, coros, canto gregoriano e trompetes. |
| `Boss2Music` | ♪ O dançarino das estrelas | Nome da música do chefe da fase 2 (Baylo), em toda a luta: intensa, com trompetes, saxofones e bateria. |
| `BossMusic` | ♪ Um veneno que queima como fogo | Nome da música do chefe da fase 1 (Iryssh), em toda a luta. |


---

## Fase 1: Iryssh, a mulher-serpente de fogo

### Título e nomes


| Chave | Texto (PT-BR) | Contexto |
| --- | --- | --- |
| `StageTitle` | Fase 1 | Título da fase 1, mostrado quando ela começa. |
| `IrysshName` | Iryssh | Nome de personagem da fase (mostrado no título do chefe e nas falas). |

### Ataques do chefe


| Chave | Texto (PT-BR) | Contexto |
| --- | --- | --- |
| `AttackRingOfEmbers` | Fogo: Anel de Brasas | Nome do ataque do chefe da fase 1 (a primeira forma, calma). |
| `AttackPiercingTongue` | Chama: Língua Perfurante | Nome do ataque do chefe da fase 1 (a primeira forma, calma). |
| `AttackTwinWhirlwinds` | Labareda: Redemoinhos Gêmeos | Nome do ataque do chefe da fase 1 (a primeira forma, calma). |
| `AttackBlazingBloom` | Lótus: Flor Flamejante | Nome do ataque do chefe da fase 1 (a segunda forma, enfurecida). |
| `AttackFuryOfTheRiver` | Fúria do Rio Incandescente | Nome do ataque do chefe da fase 1 (a segunda forma, enfurecida). |
| `AttackTideOfEmbers` | Investida da Grande Serpente | Nome do ataque do chefe da fase 1 (a segunda forma, enfurecida). |

---

## Fase 2: Baylo, o boto da festa

### Título e nomes


| Chave | Texto (PT-BR) | Contexto |
| --- | --- | --- |
| `Stage2Title` | Fase 2 | Título da fase 2, mostrado quando ela começa. |
| `BayloName` | Baylo | Nome de personagem da fase (mostrado no título do chefe e nas falas). |

### Ataques do chefe


| Chave | Texto (PT-BR) | Contexto |
| --- | --- | --- |
| `AttackDoubleStep` | Dança: Passo Duplo | Nome do ataque do chefe da fase 2 (a primeira forma, calma). |
| `AttackConfettiRain` | Música: Chuva de Notas | Nome do ataque do chefe da fase 2 (a primeira forma, calma). |
| `AttackSambaWheel` | Música: Roda de Pétalas | Nome do ataque do chefe da fase 2 (a primeira forma, calma). |
| `AttackDolphinLeap` | Grande Mergulho do Rei | Nome do ataque do chefe da fase 2 (a primeira forma, calma). |
| `AttackPartyWave` | Jato do Rio Profundo | Nome do ataque do chefe da fase 2 (a primeira forma, calma). |

---

## Fase 3: Thaqai, a gata das estrelas

### Título e nomes


| Chave | Texto (PT-BR) | Contexto |
| --- | --- | --- |
| `Stage3Title` | Fase 3 | Título da fase 3, mostrado quando ela começa. |
| `ThaqaiName` | Thaqai | Nome de personagem da fase (mostrado no título do chefe e nas falas). |

### Ataques do chefe


| Chave | Texto (PT-BR) | Contexto |
| --- | --- | --- |
| `AttackHailCurtain` | Granizo: Cortina Branca | Nome do ataque do chefe da fase 3 (a primeira forma, calma). |
| `AttackStarPentagram` | Estrela: Pentagrama | Nome do ataque do chefe da fase 3 (a primeira forma, calma). |
| `AttackZigzagLightning` | Raio: Zigue-zague | Nome do ataque do chefe da fase 3 (a primeira forma, calma). |
| `AttackFineWeave` | Gelo: Tecido Fino | Nome do ataque do chefe da fase 3 (a primeira forma, calma). |
| `AttackSkyFalls` | Tempestade: O Céu Cai | Nome do ataque do chefe da fase 3 (a segunda forma, enfurecida). |
| `AttackEclipse` | Eclipse: A Lua Coberta | Nome do ataque do chefe da fase 3 (a segunda forma, enfurecida). |

---

## Fase 4: Ukhumo, a ursa da caverna

### Título e nomes


| Chave | Texto (PT-BR) | Contexto |
| --- | --- | --- |
| `Stage4Title` | Fase 4 | Título da fase 4, mostrado quando ela começa. |
| `UkhumoName` | Ukhumo | Nome de personagem da fase (mostrado no título do chefe e nas falas). |

### Ataques do chefe


| Chave | Texto (PT-BR) | Contexto |
| --- | --- | --- |
| `AttackClawRake` | Garras: Três Cortes | Nome do ataque do chefe da fase 4 (a primeira forma, calma). |
| `AttackRoar` | Rugido: Sopro Brutal | Nome do ataque do chefe da fase 4 (a primeira forma, calma). |
| `AttackStomp` | Pisada: Chuva de Estalactites | Nome do ataque do chefe da fase 4 (a primeira forma, calma). |
| `AttackBruteForce` | Força Bruta: Pedra Arremessada | Nome do ataque do chefe da fase 4 (a primeira forma, calma). |
| `AttackCrossingClaws` | Fúria: Garras Cruzadas | Nome do ataque do chefe da fase 4 (a segunda forma, enfurecida). |
| `AttackCaveIn` | Fúria: Desabamento | Nome do ataque do chefe da fase 4 (a segunda forma, enfurecida). |
| `AttackFeast` | Fome: Banquete de Garras | Nome do ataque do chefe da fase 4 (a segunda forma, enfurecida). |

---

## Fase 5: Kuychu, o guardião de duas cabeças

### Título e nomes


| Chave | Texto (PT-BR) | Contexto |
| --- | --- | --- |
| `Stage5Title` | Fase 5 | Título da fase 5, mostrado quando ela começa. |
| `KuychuName` | Kuychu | Nome de personagem da fase (mostrado no título do chefe e nas falas). |
| `SerpentLeftName` | Serpente esquerda | Nome de personagem da fase (mostrado no título do chefe e nas falas). |
| `SerpentRightName` | Serpente direita | Nome de personagem da fase (mostrado no título do chefe e nas falas). |

### Ataques do chefe


| Chave | Texto (PT-BR) | Contexto |
| --- | --- | --- |
| `AttackTwoHeads` | Duas Cabeças: Por Turnos | Nome do ataque do chefe da fase 5 (a primeira forma, calma). |
| `AttackSevenArcs` | Arco-Íris: Sete Arcos | Nome do ataque do chefe da fase 5 (a primeira forma, calma). |
| `AttackBorderQuake` | Terremoto: A Fronteira Treme | Nome do ataque do chefe da fase 5 (a primeira forma, calma). |
| `AttackTide` | Maré: Muralhas d'Água | Nome do ataque do chefe da fase 5 (a primeira forma, calma). |
| `AttackTrial` | Prova de Valor: Cabeça a Cabeça | Nome do ataque do chefe da fase 5 (a primeira forma, calma). |
| `AttackBrokenRainbow` | Dragão: Arco-Íris Partido | Nome do ataque do chefe da fase 5 (a segunda forma, enfurecida). |
| `AttackTwoWorlds` | Dois Mundos: O Abalo | Nome do ataque do chefe da fase 5 (a segunda forma, enfurecida). |
| `AttackGuardian` | Guardião dos Dois Reinos | Nome do ataque do chefe da fase 5 (a segunda forma, enfurecida). |

---

## Fase 6: Mama Killa, a Lua

### Título e nomes


| Chave | Texto (PT-BR) | Contexto |
| --- | --- | --- |
| `Stage6Title` | Fase 6 | Título da fase 6, mostrado quando ela começa. |
| `KillaName` | Killa | Nome de personagem da fase (mostrado no título do chefe e nas falas). |
| `IllariName` | Illari | Nome de personagem da fase (mostrado no título do chefe e nas falas). |

### Ataques do chefe


A Killa ainda usa os ataques da fase 3 (Thaqai); os nomes são os mesmos daquela fase.
