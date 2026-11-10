# Airumã: todos os textos do jogo

Este documento reúne **todos os textos que aparecem no jogo**, com o contexto de cada um, para você poder editá-los em um lugar só.

**Como editar:** mude apenas as colunas **Português** e **Inglês** (e, se quiser, acrescente observações na coluna Contexto: ela não é lida). Não mude a coluna **Chave**, não apague linhas e não use o caractere `|` dentro de um texto. Depois é só me pedir para ler o documento: eu aplico tudo de uma vez nas tabelas do jogo (`python3 Docs/tools/textos.py import`). Para gerar o documento de novo a partir do jogo: `python3 Docs/tools/textos.py export`.

**Detalhes do jogo:**

- Cada fala de diálogo cabe em até 3 linhas curtas da caixa (uns 36 letras por linha), então textos muito longos são cortados.
- Nas falas, o personagem "o chefe da fase" mostra o nome do chefe da fase (veja a tabela de nomes de cada fase).
- A fonte do jogo não tem alguns sinais, como parênteses, aspas e ponto e vírgula; prefira vírgula, ponto, dois pontos, traço, `!`, `?`, `/`, `+`, `%` e `...`.
- Os nomes dos ataques dos chefes aparecem como o título de uma carta de feitiço ("Tema: Nome") quando o ataque começa.
- O espírito da lamparina ainda se chama **Naru** nas falas (chave `FairyName`); no painel só aparece o título dela (`FairyRole`).

---

## Interface

### Menu principal

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `Title` | Airumã | Airumã | Título do jogo, no alto do menu principal. |
| `MenuStart` | Iniciar | Start | Menu principal: começa uma partida (leva à escolha da dificuldade). |
| `MenuBossPractice` | Treino | Boss Practice | Menu principal: abre o treino (de fases ou de chefes). |
| `MenuScores` | Recordes | Scores | Menu principal: mostra os recordes. |
| `MenuMusicRoom` | Sala de música | Music Room | Menu principal: sala de músicas (ainda não existe; mostra o aviso "Em breve"). |
| `MenuConfig` | Opções | Config | Menu principal: abre as opções. |
| `MenuQuit` | Sair | Quit | Menu principal: fecha o jogo. |
| `MenuHint` | Z: Escolher   X: Voltar | Z: Select   X: Back | Dica das teclas, no canto de baixo do menu. |
| `ComingSoon` | Em breve | Coming soon | Aviso que pisca ao escolher uma opção que ainda não existe. |
| `Back` | Voltar | Back | Voltar, nos submenus. |
| `Menu` | Menu | Menu | Botão "ir para o menu", na tela final da partida. |
| `DemoLabel` | Demonstração | Demonstration | Aviso que pisca durante a demonstração (o jogo joga sozinho quando o menu fica 10 segundos parado). |

### Dificuldade, treino e recordes

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `DifficultyTitle` | Dificuldade | Difficulty | Título da escolha de dificuldade. |
| `DifficultyEasy` | Fácil | Easy | Dificuldade fácil. |
| `DifficultyNormal` | Normal | Normal | Dificuldade normal. |
| `DifficultyHard` | Difícil | Hard | Dificuldade difícil. |
| `PracticeTitle` | Treino de Chefe | Boss Practice | Título da lista de treino de chefes. |
| `PracticeStages` | Fases | Stages | Treino: opção "treinar uma fase inteira". |
| `PracticeBosses` | Chefes | Bosses | Treino: opção "treinar só o chefe". |
| `PracticeStageTitle` | Treino de Fase | Stage Practice | Título da lista de treino de fases. |
| `LockedBoss` | ????? | ????? | Nome de um chefe ainda não existente na lista de treino (aparece com um cadeado). |
| `ScoresTitle` | Recordes | Scores | Título da tela de recordes. |
| `NoScore` | ------- | ------- | Linha vazia da tabela de recordes. |
| `Cleared` | Completo | Clear | Marca ao lado do recorde de uma partida que chegou ao fim. |

### Opções

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `CfgTitle` | Configurações | Config | Título da tela de opções. |
| `CfgLives` | Vidas | Lives | Opções: vidas no início da partida (1 a 5). |
| `CfgBombs` | Bombas | Bombs | Opções: bombas no início da partida (0 a 3). |
| `CfgMusic` | Volume da Música | Music Volume | Opções: volume da música. |
| `CfgSfx` | Efeitos Sonoros | Sound Effects | Opções: volume dos efeitos sonoros. |
| `CfgMode` | Modo | Mode | Opções: modo da tela. |
| `CfgFullscreen` | Tela Cheia | Fullscreen | Valor do modo de tela: tela cheia. |
| `CfgWindowed` | Janela | Windowed | Valor do modo de tela: janela. |
| `CfgControls` | Controles | Controls | Opções: abre a tela de controles (também é o título dela). |
| `CfgDefaults` | Restaurar Padrões | Restore Defaults | Opções e Controles: botão que restaura os padrões. |
| `CfgExit` | Sair | Exit | Opções e Controles: botão que volta. |
| `CfgKeyUp` | Cima | Up | Controles: tecla de mover para cima. |
| `CfgKeyDown` | Baixo | Down | Controles: tecla de mover para baixo. |
| `CfgKeyLeft` | Esquerda | Left | Controles: tecla de mover para a esquerda. |
| `CfgKeyRight` | Direita | Right | Controles: tecla de mover para a direita. |
| `CfgKeyShoot` | Atirar | Shoot | Controles: tecla de atirar. |
| `CfgKeyBomb` | Bomba | Bomb | Controles: tecla da bomba. |
| `CfgKeyFocus` | Foco | Focus | Controles: tecla de foco (movimento lento e mostra a hitbox). |
| `CfgPressKey` | Pressione uma tecla | Press a key | Aviso ao escolher uma tecla para trocar. |
| `CfgHint` | Esq/Dir: Alterar   Z: Escolher   X: Voltar | Left/Right: Change   Z: Select   X: Back | Dica das teclas, no rodapé das opções. |

### HUD (painel da direita)

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `HiScore` | Recorde | Best | HUD: r��tulo do maior recorde. |
| `Score` | Pontos | Score | HUD: rótulo da pontuação atual. |
| `Lives` | Vidas | Lives | HUD: rótulo das vidas (corações). |
| `Lantern` | Bombas | Bombs | HUD: rótulo das bombas (lamparinas). |
| `Power` | Poder | Power | HUD: rótulo do poder da lamparina (chamas). |
| `Wind` | Vento | Wind | HUD: rótulo do vento (bússola). |
| `WindCalm` | Calmo | Calm | HUD: vento calmo. |
| `WindNone` | Sem vento | No wind | HUD: sem vento. |
| `WindWeak` | Fraco | Weak | HUD: vento fraco. |
| `WindMedium` | Médio | Medium | HUD: vento médio. |
| `WindStrong` | Forte | Strong | HUD: vento forte. |
| `North` | Norte | North | Direção do vento: norte. |
| `South` | Sul | South | Direção do vento: sul. |
| `East` | Leste | East | Direção do vento: leste. |
| `West` | Oeste | West | Direção do vento: oeste. |
| `FairyName` | Naru | Naru | Nome do espírito da lamparina. NÃO aparece mais no painel; ainda é o nome mostrado nas falas dela (ela ainda vai ganhar outro nome). |
| `FairyRole` | Espírito da lamparina | Lantern Spirit | Título mostrado ao lado do retrato do espírito da lamparina, no painel (quebra em duas linhas). |
| `BoyName` | Atli | Atli | Nome do menino (o jogador), nas falas. |
| `DialogueHint` | Z: Avançar   X: Pular | Z: Next   X: Skip | Dica das teclas, no canto da caixa de diálogo. |

### Pausa e fim da partida

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `Paused` | Pausado | Paused | Título do menu de pausa. |
| `Continue` | Continuar | Continue | Pausa: continuar. |
| `GiveUp` | Desistir | Give Up | Pausa: desistir (volta ao menu). |
| `PauseHint` | Esc: Continuar | Esc: Continue | Dica das teclas, no menu de pausa. |
| `Victory` | Vitória! | Victory! | Tela final: título quando o jogador vence. |
| `Defeat` | Derrota... | Defeat... | Tela final: título quando o jogador perde. |
| `Restart` | Reiniciar | Restart | Tela final: botão de jogar de novo. |
| `NewRecord` | Novo recorde! | New record! | Tela final: aviso de novo recorde. |

### Escolha de cartas

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `ChooseCard` | Escolha uma carta | Choose a card | Título da escolha de carta (genérico). |
| `ChooseCardStart` | Uma nova fase começa | A new stage begins | Título da escolha de carta no começo de uma fase. |
| `ChooseCardBoss` | Um grande poder! | A great power! | Título da escolha de carta depois de vencer um chefe. |
| `CardsHint` | Esq Dir: Escolher   Z: Pegar | Left Right: Pick   Z: Take | Dica das teclas, na escolha de carta. |
| `YourCards` | Suas cartas | Your cards | Rótulo da lista das cartas que o jogador já tem. |

### Falas curtas do espírito da lamparina (caixa de fala da HUD)

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `SayStart` | Vamos lá! | Let's go! | Fala no começo da partida. |
| `SayHit` | Ai! Aguenta firme! | Ouch! Hang in there! | Fala quando o barco é atingido. |
| `SayBomb` | Eu te protejo! | I'll protect you! | Fala quando o jogador usa uma bomba. |
| `SayRage` | Ficou furioso! | Now it's furious! | Fala quando o chefe entra na segunda forma (fúria). |
| `SayEnraged` | Lá vem outro! | Here comes another! | Fala a cada novo ataque do chefe enfurecido. |
| `SayVictory` | Vencemos!!! | We won!!! | Fala na tela de vitória. |
| `SayDefeat` | Não desista! | Don't give up! | Fala na tela de derrota. |
| `SayPowerUp` | Me sinto mais forte! | I feel stronger! | Fala quando o poder da lamparina sobe. |
| `SayFullPower` | Poder total! | Full power! | Fala quando o poder chega ao máximo. |
| `SayBombRefill` | Uma bomba reserva! | A spare bomb! | Fala quando uma bomba reserva é pega. |
| `SayLifeRefill` | Uma vida reserva! | A spare life! | Fala quando uma vida reserva é pega. |
| `SayCard` | Boa escolha! | Nice pick! | Fala quando o jogador escolhe uma carta. |
| `SayWard` | Estamos salvos! | We're safe! | Fala quando o escudo de uma carta segura um golpe. |

### História contada fora das falas (abertura e fim)

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `IntroHint` | Pressione qualquer tecla | Press any key | Dica no canto de baixo à direita, na abertura do jogo e na cena do fim da história (cada uma fecha sozinha em 10 segundos, ou com qualquer tecla). |
| `IntroPage1` | A sacerdotisa das terras sagradas sumiu. | The priestess of the sacred lands has vanished. | Abertura do jogo: parágrafo 1 da história, escrito na escuridão, embaixo à esquerda, sobre a cena da noite sem fim (os quatro aparecem juntos). |
| `IntroPage2` | Sem ela para realizar o rito de solstício anual, o mundo cai na noite eterna. | Without her to perform the yearly solstice ritual, the world falls into eternal night. | Abertura do jogo: parágrafo 2 da história, escrito na escuridão, embaixo à esquerda, sobre a cena da noite sem fim (os quatro aparecem juntos). |
| `IntroPage3` | O jovem velejador, acompanhado de sua lamparina, parte em uma jornada para encontrá-la... | The young sailor, accompanied by his lantern, sets out on a journey to find her... | Abertura do jogo: parágrafo 3 da história, escrito na escuridão, embaixo à esquerda, sobre a cena da noite sem fim (os quatro aparecem juntos). |
| `IntroPage4` | ...para que o sol possa nascer novamente. | ...so that the sun may rise again. | Abertura do jogo: parágrafo 4 da história, escrito na escuridão, embaixo à esquerda, sobre a cena da noite sem fim (os quatro aparecem juntos). |
| `S6Epi01` | Após voltarem do reino espiritual, Atli leva a sacerdotisa Ariah de volta para casa, para que ela possa completar o rito de solstício. | After returning from the spirit realm, Atli takes the priestess Ariah back home, so that she can complete the solstice rite. | Fim do jogo: parágrafo 1 da história final, escrito embaixo da cena do nascer do sol (os quatro aparecem juntos). |
| `S6Epi02` | A sacerdotisa subiu ao alto da colina e completou o ritual antes que a noite terminasse de ceder. | The priestess climbed to the top of the hill and finished the ritual before the night had stopped giving way. | Fim do jogo: parágrafo 2 da história final, escrito embaixo da cena do nascer do sol (os quatro aparecem juntos). |
| `S6Epi03` | E o sol nasceu de novo, quente demais, como sempre. Atli suou, reclamou... e achou graça. | And the sun rose again, too hot, as always. Atli sweated, complained... and found it funny. | Fim do jogo: parágrafo 3 da história final, escrito embaixo da cena do nascer do sol (os quatro aparecem juntos). |
| `S6Epi04` | Ambos então sentam à beira do rio e admiram o nascer do sol que resplandece sob as águas, uma vez mais. | The two then sit by the river and admire the sunrise that shines upon the waters once more. | Fim do jogo: parágrafo 4 da história final, escrito embaixo da cena do nascer do sol (os quatro aparecem juntos). |
| `TheEnd` | FIM | THE END | Fim do jogo: escrito bem grande embaixo, no centro, na cena do nascer do sol que fecha a história. |

### Música

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `StageMusic` | ♪ Os espíritos tomam a noite | ♪ The Spirits Take the Night | Nome da música da fase 1, mostrado no título da fase. |
| `Stage2Music` | ♪ Um festival animado para a noite | ♪ A Lively Festival for the Night | Nome da música da fase 2, mostrado no título da fase: um quadro animado, com trompetes, saxofones e bateria. |
| `Stage3Music` | ♪ Criaturas brincalhonas sob a tempestade | ♪ Playful Creatures Under the Storm | Nome da música da fase 3, mostrado no título da fase: sombria, com trompetes e pianos. |
| `Boss3Music` | ♪ A servente arisca que espirra raios | ♪ The Skittish Servant Who Spits Lightning | Nome da música do chefe da fase 3 (Thaqai), em toda a luta: mais forte, com piano grave e trompetes. |
| `Stage4Music` | ♪ Hibernação em meio a morcegos e musgo | ♪ Hibernation Among Bats and Moss | Nome da música da fase 4, mostrado no título da fase: calma, com tambores lentos e trompetes. |
| `Boss4Music` | ♪ A grande fera Deidane | ♪ The Great Beast Deidane | Nome da música do chefe da fase 4 (Ukhumo), em toda a luta: violenta, animalesca, com tambores e trompetes. |
| `Stage5Music` | ♪ Cachoeira invertida e mil maravilhas | ♪ The Inverted Waterfall and a Thousand Wonders | Nome da música da fase 5, mostrado no título da fase: épica, com flautas, trompetes e saxofones (era a música do chefe). |
| `Boss5Music` | ♪ A serpe guardiã dos dois reinos | ♪ The Serpent Guardian of the Two Realms | Nome da música do chefe da fase 5 (Kuychu), em toda a luta: um hino divino, com trompetes e coros, que lembra a música católica. |
| `Stage6Music` | ♪ O rio que percorre o universo | ♪ The River That Runs Through the Universe | Nome da música da fase 6, mostrado no título da fase: divina e cósmica, com órgão, piano, coros, canto gregoriano e trompetes. |
| `Boss2Music` | ♪ O dançarino das estrelas | ♪ The Dancer of the Stars | Nome da música do chefe da fase 2 (Baylo), em toda a luta: intensa, com trompetes, saxofones e bateria. |
| `BossMusic` | ♪ Um veneno que queima como fogo | ♪ A Venom That Burns Like Fire | Nome da música do chefe da fase 1 (Iryssh), em toda a luta. |

---

## Fase 1: Iryssh, a mulher-serpente de fogo

### Título e nomes

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `StageTitle` | Fase 1 | Stage 1 | Título da fase 1, mostrado quando ela começa. |
| `IrysshName` | Iryssh | Iryssh | Nome de personagem da fase (mostrado no título do chefe e nas falas). |

### Ataques do chefe

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `AttackRingOfEmbers` | Fogo: Anel de Brasas | Fire: Ring of Embers | Nome do ataque do chefe da fase 1 (a primeira forma, calma). |
| `AttackPiercingTongue` | Chama: Língua Perfurante | Flame: Piercing Tongue | Nome do ataque do chefe da fase 1 (a primeira forma, calma). |
| `AttackTwinWhirlwinds` | Labareda: Redemoinhos Gêmeos | Blaze: Twin Whirlwinds | Nome do ataque do chefe da fase 1 (a primeira forma, calma). |
| `AttackBlazingBloom` | Lótus: Flor Flamejante | Lotus: Blazing Bloom | Nome do ataque do chefe da fase 1 (a segunda forma, enfurecida). |
| `AttackFuryOfTheRiver` | Fúria do Rio Incandescente | Fury of the Blazing River | Nome do ataque do chefe da fase 1 (a segunda forma, enfurecida). |
| `AttackTideOfEmbers` | Investida da Grande Serpente | Charge of the Great Serpent | Nome do ataque do chefe da fase 1 (a segunda forma, enfurecida). |

### Falas: Abertura da fase (antes dos inimigos)

*Resumo, do código (em inglês): Stage 1, at the start: the boy and his lantern are lost.*

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `Intro01` | Hmm... tenho certeza que já vi aquela árvore. | Hmm... I'm sure I've seen that tree before. | 1ª fala; quem fala: Naru (espírito da lamparina); expressão irritada. |
| `Intro02` | É porque já vimos aquela árvore. | That's because we've already seen that tree. | 2ª fala; quem fala: Atli; expressão neutra. |
| `Intro03` | ESTAMOS PERDIDOS?! | ARE WE LOST?! | 3ª fala; quem fala: Naru (espírito da lamparina); expressão irritada; GRITADA (a caixa treme). |
| `Intro04` | Não estamos perdidos... só está difícil enxergar nesse escuro. | We're not lost... it's just hard to see in this darkness. | 4ª fala; quem fala: Atli; expressão sem graça. |
| `Intro05` | Peraí, você tá me culpando?! A culpa não é minha, estou quase sem óleo! | Wait, are you blaming me?! It's not my fault, I'm almost out of oil! | 5ª fala; quem fala: Naru (espírito da lamparina); expressão preocupada. |
| `Intro06` | Foi mal, foi mal... vou colocar óleo mais tarde. | My bad, my bad... I'll add some oil later. | 6ª fala; quem fala: Atli; expressão neutra. |
| `Intro07` | A gente está PERDIDO, Atli! | We're LOST, Atli! | 7ª fala; quem fala: Naru (espírito da lamparina); expressão irritada; GRITADA (a caixa treme). |
| `Intro08` | Rota panorâmica. Rápida. Fiuuu! | Scenic route. Fast. Whoosh! | 8ª fala; quem fala: Atli; expressão sem graça. |
| `Intro09` | Então vai pela ESQUERDA! A outra esquerda! Argh! | Then scenic-route LEFT! Your other left! Argh! | 9ª fala; quem fala: Naru (espírito da lamparina); expressão feliz; GRITADA (a caixa treme). |
| `Intro10` | Espera... fru-fru... tem algo se mexendo. | Wait... rustle, rustle... something's moving. | 10ª fala; quem fala: Atli; expressão surpresa. |
| `Intro11` | Moradores! Peça informação. Educadamente! | Locals! Ask for directions. Politely! | 11ª fala; quem fala: Naru (espírito da lamparina); expressão preocupada. |
| `Intro12` | Estão brilhando. E de mau humor. SEGURA! | They're glowing. And cranky. HOLD ON! | 12ª fala; quem fala: Atli; expressão neutra; GRITADA (a caixa treme). |

### Falas: Chegada ao chefe (antes de ele aparecer)

*Resumo, do código (em inglês): Before the boss, part one: they reach a lake, and the water goes still.*

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `BossA1` | Atli, um lago! Conseguimos sair! | Atli, a lake! We made it out! | 1ª fala; quem fala: Naru (espírito da lamparina); expressão feliz. |
| `BossA2` | Tem muitos espíritos nessa região, isso não é bom sinal... | There are a lot of spirits in this region, that's not a good sign... | 2ª fala; quem fala: Atli; expressão surpresa. |
| `BossA3` | Não importa! Agora eu preciso de óleo! | Never mind that! Now I need oil! | 3ª fala; quem fala: Naru (espírito da lamparina); expressão irritada. |
| `BossA4` | Entendi, entendi... Acho que devo ter guard... | Got it, got it... I think I must have sav... | 4ª fala; quem fala: Atli; expressão neutra. |
| `BossA5` | *Tremores* | *Tremors* | 5ª fala; quem fala: Naru (espírito da lamparina); expressão preocupada. |

### Falas: Encontro com o chefe (antes da luta)

*Resumo, do código (em inglês): Before the boss, part two: she rises, and is not pleased.*

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `BossB1` | Quem está fazendo tanto barulho no meu território?! | Who is making so much noise in my territory?! | 1ª fala; quem fala: o chefe da fase; expressão irritada; GRITADA (a caixa treme). |
| `BossB2` | Estão atrapalhando meu sono da beleza! | You're disturbing my beauty sleep! | 2ª fala; quem fala: o chefe da fase; expressão neutra. |
| `BossB3` | Sono da beleza? Embaixo da água? | Beauty sleep? Underwater? | 3ª fala; quem fala: Atli; expressão sem graça. |
| `BossB4` | A água faz bem para o cabelo e para a pele, pirralho! Um garoto que nem você não entende nada de beleza! | Water is good for the hair and the skin, brat! A boy like you knows nothing about beauty! | 4ª fala; quem fala: o chefe da fase; expressão irritada; GRITADA (a caixa treme). |
| `BossB5` | E não parece estar dando muito certo... | And it doesn't seem to be working very well... | 5ª fala; quem fala: Naru (espírito da lamparina); expressão irritada. |
| `BossB6` | COMO É?! VOCÊS VÃO PAGAR POR ISSO, INVASORES!!! | WHAT?! YOU'LL PAY FOR THIS, INTRUDERS!!! | 6ª fala; quem fala: o chefe da fase; expressão irritada; GRITADA (a caixa treme). |
| `BossB7` | Pode esquecer o óleo que eu prometi... lamparina desbocada. | Forget the oil I promised... you foul-mouthed lantern. | 7ª fala; quem fala: Atli; expressão neutra. |

### Falas: Final da fase (depois da luta)

*Resumo, do código (em inglês): After the boss: it is better that she sleeps forever.*

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `EndE1` | Ela... dormiu de novo? | Is she... asleep again? | 1ª fala; quem fala: Naru (espírito da lamparina); expressão preocupada. |
| `EndE2` | E durma para sempre agora! | And sleep forever now! | 2ª fala; quem fala: Atli; expressão neutra. |
| `EndE3` | O sono da beleza dela quase nos TORROU! | Her beauty sleep nearly ROASTED us! | 3ª fala; quem fala: Naru (espírito da lamparina); expressão irritada; GRITADA (a caixa treme). |
| `EndE4` | Talvez ela devesse dormir para sempre. | Maybe she should sleep forever. | 4ª fala; quem fala: Atli; expressão sem graça. |
| `EndE5` | Concordo. Na ponta dos pés. Corre depois! | Agreed. Tiptoe now. Run later! | 5ª fala; quem fala: Naru (espírito da lamparina); expressão feliz. |
| `EndE6` | Então... qual é a saída? | So... which way is out? | 6ª fala; quem fala: Atli; expressão neutra. |
| `EndE7` | ...Não olha pra mim. Hmpf. | ...Don't look at me. Hmph. | 7ª fala; quem fala: Naru (espírito da lamparina); expressão irritada. |

---

## Fase 2: Baylo, o boto da festa

### Título e nomes

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `Stage2Title` | Fase 2 | Stage 2 | Título da fase 2, mostrado quando ela começa. |
| `BayloName` | Baylo | Baylo | Nome de personagem da fase (mostrado no título do chefe e nas falas). |

### Ataques do chefe

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `AttackDoubleStep` | Dança: Passo Duplo | Dance: Double Step | Nome do ataque do chefe da fase 2 (a primeira forma, calma). |
| `AttackConfettiRain` | Música: Chuva de Notas | Music: Rain of Notes | Nome do ataque do chefe da fase 2 (a primeira forma, calma). |
| `AttackSambaWheel` | Música: Roda de Pétalas | Music: Wheel of Petals | Nome do ataque do chefe da fase 2 (a primeira forma, calma). |
| `AttackDolphinLeap` | Grande Mergulho do Rei | The King's Great Dive | Nome do ataque do chefe da fase 2 (a primeira forma, calma). |
| `AttackPartyWave` | Jato do Rio Profundo | Jet of the Deep River | Nome do ataque do chefe da fase 2 (a primeira forma, calma). |

### Falas: Abertura da fase (antes dos inimigos)

*Resumo, do código (em inglês): Stage 2, at the start: the trail comes back, and with it a party.*

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `S2Intro01` | Acho que encontramos um caminho, talvez para o norte? | I think we found a path, maybe to the north? | 1ª fala; quem fala: Atli; expressão feliz. |
| `S2Intro02` | Isso são luzes coloridas? Uma festa?! | Are those colored lights? A party?! | 2ª fala; quem fala: Naru (espírito da lamparina); expressão preocupada. |
| `S2Intro03` | É o que parece... mas uma festa no meio do nada? | That's what it looks like... but a party in the middle of nowhere? | 3ª fala; quem fala: Atli; expressão surpresa. |
| `S2Intro04` | Talvez encontremos alguém que saiba de algo! | Maybe we'll find someone who knows something! | 4ª fala; quem fala: Naru (espírito da lamparina); expressão irritada. |
| `S2Intro05` | Quem sabe... | Who knows... | 5ª fala; quem fala: Atli; expressão sem graça. |
| `S2Intro06` | NÃO DÁ! | WE CAN'T! | 6ª fala; quem fala: Naru (espírito da lamparina); expressão irritada; GRITADA (a caixa treme). |

### Falas: Encontro com o chefe (antes da luta)

*Resumo, do código (em inglês): Before Baylo: he comes dancing and will not take no for an answer.*

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `S2Meet01` | Ora, ora! Temos convidados! Que alegria! | Well, well! We have guests! What a joy! | 1ª fala; quem fala: o chefe da fase; expressão feliz. |
| `S2Meet02` | O que acham de se divertir comigo? | How about having some fun with me? | 2ª fala; quem fala: o chefe da fase; expressão feliz. |
| `S2Meet03` | Valeu, mas a gente meio que está com pressa... | Thanks, but we're kind of in a hurry... | 3ª fala; quem fala: Atli; expressão sem graça. |
| `S2Meet04` | Quem tem pressa não tem ritmo, meu amigo! | Whoever's in a hurry has no rhythm, my friend! | 4ª fala; quem fala: o chefe da fase; expressão feliz. |
| `S2Meet05` | O quê? Mas o que uma coisa tem a ve... | What? But what does that have to do wi... | 5ª fala; quem fala: Naru (espírito da lamparina); expressão preocupada. |
| `S2Meet06` | VAMOS DANÇAR, MEU POVO! | LET'S DANCE, EVERYONE! | 6ª fala; quem fala: o chefe da fase; expressão feliz; GRITADA (a caixa treme). |

### Falas: Final da fase (depois da luta)

*Resumo, do código (em inglês): After Baylo: he is out of breath, and, for the fun of it, tells them where the boat went.*

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `S2End01` | Ha ha! Faz anos que alguém não me acompanhava assim! | Ha ha! It's been years since anyone kept up with me like that! | 1ª fala; quem fala: o chefe da fase; expressão feliz. |
| `S2End02` | Nós podemos ir agora? Estamos no meio de algo importante... | Can we go now? We're in the middle of something important... | 2ª fala; quem fala: Atli; expressão neutra. |
| `S2End03` | Mas vocês sabem para onde estão indo, meus jovens? | But do you know where you're going, my young ones? | 3ª fala; quem fala: o chefe da fase; expressão irritada. |
| `S2End04` | Ehhh... | Ehhh... | 4ª fala; quem fala: o chefe da fase; expressão neutra. |
| `S2End05` | Hahaha! Eu realmente curti vocês, hmm... acho que não tem problema contar. | Hahaha! I really enjoyed you two, hmm... I guess there's no harm in telling. | 5ª fala; quem fala: Naru (espírito da lamparina); expressão surpresa. |
| `S2End06` | Sigam pelo veio direito do rio, lá talvez encontrem uma pista do que procuram. | Follow the right branch of the river, there you might find a clue to what you're looking for. | 6ª fala; quem fala: o chefe da fase; expressão feliz. |
| `S2End07` | Hã? Mas você nem sab... | Huh? But you don't even kno... | 7ª fala; quem fala: Atli; expressão irritada. |
| `S2End08` | Confie em mim... uma recompensa por terem me entretido. | Trust me... a reward for entertaining me. | 8ª fala; quem fala: o chefe da fase; expressão feliz. |
| `S2End09` | Não é como se vocês tivessem muita escolha, haha! | It's not like you have much choice, haha! | 9ª fala; quem fala: o chefe da fase; expressão feliz. |
| `S2End10` | *Suspiros* Acho que não tem jeito, vamos lá então... | *Sighs* I guess there's no way around it, let's go then... | 10ª fala; quem fala: Atli; expressão sem graça. |

---

## Fase 3: Thaqai, a gata das estrelas

### Título e nomes

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `Stage3Title` | Fase 3 | Stage 3 | Título da fase 3, mostrado quando ela começa. |
| `ThaqaiName` | Thaqai | Thaqai | Nome de personagem da fase (mostrado no título do chefe e nas falas). |

### Ataques do chefe

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `AttackHailCurtain` | Granizo: Cortina Branca | Hail: White Curtain | Nome do ataque do chefe da fase 3 (a primeira forma, calma). |
| `AttackStarPentagram` | Estrela: Pentagrama | Star: Pentagram | Nome do ataque do chefe da fase 3 (a primeira forma, calma). |
| `AttackZigzagLightning` | Raio: Zigue-zague | Lightning: Zigzag | Nome do ataque do chefe da fase 3 (a primeira forma, calma). |
| `AttackFineWeave` | Gelo: Tecido Fino | Ice: Fine Weave | Nome do ataque do chefe da fase 3 (a primeira forma, calma). |
| `AttackSkyFalls` | Tempestade: O Céu Cai | Storm: The Sky Falls | Nome do ataque do chefe da fase 3 (a segunda forma, enfurecida). |
| `AttackEclipse` | Eclipse: A Lua Coberta | Eclipse: The Covered Moon | Nome do ataque do chefe da fase 3 (a segunda forma, enfurecida). |

### Falas: Abertura da fase (antes dos inimigos)

*Resumo, do código (em inglês): Stage 3, at the start: the hail begins, and Atli has no hat.*

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `S3Intro01` | Estou com frio. | I'm cold. | 1ª fala; quem fala: Atli; expressão preocupada. |
| `S3Intro02` | Está caindo... gelo? | Is that... ice falling? | 2ª fala; quem fala: Naru (espírito da lamparina); expressão surpresa. |
| `S3Intro03` | Ai! Granizo! Na minha cabeça! | Ow! Hail! On my head! | 3ª fala; quem fala: Atli; expressão surpresa; GRITADA (a caixa treme). |
| `S3Intro04` | Você perdeu o chapéu. | You lost your hat. | 4ª fala; quem fala: Naru (espírito da lamparina); expressão irritada. |
| `S3Intro05` | Sem chapéu, o granizo é pessoal. | Without a hat, the hail is personal. | 5ª fala; quem fala: Atli; expressão sem graça. |

### Falas: Encontro com o chefe (antes da luta)

*Resumo, do código (em inglês): Before Thaqai: she bars the way, on orders she will not tell.*

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `S3Meet01` | Parados. Ninguém passa por este lago. | Halt. No one passes this lake. | 1ª fala; quem fala: o chefe da fase; expressão irritada. |
| `S3Meet02` | Por ordem de quem? | On whose orders? | 2ª fala; quem fala: Atli; expressão neutra. |
| `S3Meet03` | Isso não é da sua conta. Estou aqui para que ninguém atrapalhe. | That is none of your business. I am here so that no one gets in the way. | 3ª fala; quem fala: o chefe da fase; expressão irritada. |
| `S3Meet04` | Atrapalhe o quê? | Gets in the way of what? | 4ª fala; quem fala: Naru (espírito da lamparina); expressão preocupada. |
| `S3Meet05` | Eu disse que não é da sua conta! Vocês são uma AFRONTA! | I said it is none of your business! You are an AFFRONT! | 5ª fala; quem fala: o chefe da fase; expressão irritada; GRITADA (a caixa treme). |

### Falas: Final da fase (depois da luta)

*Resumo, do código (em inglês): After Thaqai: she lets "she" slip, and Airu works out that it must be a god, and that the great serpent may know.*

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `S3End01` | Não... não pode ser. Eu perdi para um menino? | No... it can't be. I lost to a boy? | 1ª fala; quem fala: o chefe da fase; expressão preocupada. |
| `S3End02` | Ela vai me dar uma bronca daquelas! | She is going to give me such a scolding! | 2ª fala; quem fala: o chefe da fase; expressão preocupada; GRITADA (a caixa treme). |
| `S3End03` | Ela? | She? | 3ª fala; quem fala: Atli; expressão surpresa. |
| `S3End04` | ...Eu não disse nada. Eu NÃO disse NADA! | ...I didn't say anything. I did NOT say ANYTHING! | 4ª fala; quem fala: o chefe da fase; expressão surpresa; GRITADA (a caixa treme). |
| `S3End05` | Ela quem? | She who? | 5ª fala; quem fala: Atli; expressão neutra. |
| `S3End06` | Uma gata alada, com estrelas no pelo, que obedece a alguém... Isso não é bicho de qualquer um. | A winged cat with stars in her fur that obeys someone... That is no one's ordinary pet. | 6ª fala; quem fala: Naru (espírito da lamparina); expressão preocupada. |
| `S3End07` | Deve ser de alguém grande. Um deus, talvez. | It must belong to someone great. A god, maybe. | 7ª fala; quem fala: Naru (espírito da lamparina); expressão preocupada. |
| `S3End08` | Um deus levou a Illari? | A god took Illari? | 8ª fala; quem fala: Atli; expressão surpresa. |
| `S3End09` | Eu não disse isso. Disse que a gata é de alguém grande. | I didn't say that. I said the cat belongs to someone great. | 9ª fala; quem fala: Naru (espírito da lamparina); expressão irritada. |
| `S3End10` | Mas tem alguém que sabe das coisas entre os mundos: a grande serpente, que guarda a fronteira. | But someone knows about the things between the worlds: the great serpent, who guards the border. | 10ª fala; quem fala: Naru (espírito da lamparina); expressão neutra. |
| `S3End11` | Onde ela mora? | Where does she live? | 11ª fala; quem fala: Atli; expressão neutra. |
| `S3End12` | Onde a água cai. O rastro entra na montanha. Vamos pela caverna. | Where the water falls. The trail goes into the mountain. We take the cave. | 12ª fala; quem fala: Naru (espírito da lamparina); expressão neutra. |

---

## Fase 4: Ukhumo, a ursa da caverna

### Título e nomes

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `Stage4Title` | Fase 4 | Stage 4 | Título da fase 4, mostrado quando ela começa. |
| `UkhumoName` | Ukhumo | Ukhumo | Nome de personagem da fase (mostrado no título do chefe e nas falas). |

### Ataques do chefe

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `AttackClawRake` | Garras: Três Cortes | Claws: Three Cuts | Nome do ataque do chefe da fase 4 (a primeira forma, calma). |
| `AttackRoar` | Rugido: Sopro Brutal | Roar: Brutal Breath | Nome do ataque do chefe da fase 4 (a primeira forma, calma). |
| `AttackStomp` | Pisada: Chuva de Estalactites | Stomp: Rain of Stalactites | Nome do ataque do chefe da fase 4 (a primeira forma, calma). |
| `AttackBruteForce` | Força Bruta: Pedra Arremessada | Brute Force: Thrown Boulder | Nome do ataque do chefe da fase 4 (a primeira forma, calma). |
| `AttackCrossingClaws` | Fúria: Garras Cruzadas | Fury: Crossing Claws | Nome do ataque do chefe da fase 4 (a segunda forma, enfurecida). |
| `AttackCaveIn` | Fúria: Desabamento | Fury: Cave-In | Nome do ataque do chefe da fase 4 (a segunda forma, enfurecida). |
| `AttackFeast` | Fome: Banquete de Garras | Hunger: Feast of Claws | Nome do ataque do chefe da fase 4 (a segunda forma, enfurecida). |

### Falas: Abertura da fase (antes dos inimigos)

*Resumo, do código (em inglês): Stage 4, at the start: the dark of the cave, and Atli's stomach.*

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `S4Intro01` | Escuro. Muito escuro. | Dark. Very dark. | 1ª fala; quem fala: Atli; expressão preocupada. |
| `S4Intro02` | Fica perto de mim. | Stay close to me. | 2ª fala; quem fala: Naru (espírito da lamparina); expressão preocupada. |
| `S4Intro03` | Grrr... Isso foi você? | Grrr... Was that you? | 3ª fala; quem fala: Atli; expressão sem graça. |
| `S4Intro04` | Eu não tenho barriga. | I have no stomach. | 4ª fala; quem fala: Naru (espírito da lamparina); expressão irritada. |
| `S4Intro05` | Então foi o eco. Que eco bonzinho. | Then it was the echo. What a friendly echo. | 5ª fala; quem fala: Atli; expressão feliz. |

### Falas: Encontro com o chefe (antes da luta)

*Resumo, do código (em inglês): Before Ukhumo: she smells the toasted corn, and she is hungry.*

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `S4Meet01` | Fungando... Milho tostado. | Sniff, sniff... Toasted corn. | 1ª fala; quem fala: o chefe da fase; expressão neutra. |
| `S4Meet02` | É a minha merenda! | That's my snack! | 2ª fala; quem fala: Atli; expressão surpresa. |
| `S4Meet03` | Grrr... Passa para cá. | Grrr... Hand it over. | 3ª fala; quem fala: o chefe da fase; expressão irritada. |
| `S4Meet04` | Ela não está de brincadeira. | She's not joking. | 4ª fala; quem fala: Naru (espírito da lamparina); expressão preocupada. |
| `S4Meet05` | FAZ DIAS QUE EU NÃO COMO NADA! | I HAVEN'T EATEN FOR DAYS! | 5ª fala; quem fala: o chefe da fase; expressão irritada; GRITADA (a caixa treme). |

### Falas: Final da fase (depois da luta)

*Resumo, do código (em inglês): After Ukhumo: Atli gives her his snack, and she will guide them through the tunnels to the great serpent.*

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `S4End01` | ...Perdi. E ainda estou com fome. | ...I lost. And I'm still hungry. | 1ª fala; quem fala: o chefe da fase; expressão preocupada. |
| `S4End02` | ...Toma. A merenda. | ...Here. The snack. | 2ª fala; quem fala: Atli; expressão sem graça. |
| `S4End03` | ...Você me dá comida? Depois de eu tentar te comer? | ...You're giving me food? After I tried to eat you? | 3ª fala; quem fala: o chefe da fase; expressão surpresa. |
| `S4End04` | Foi só a merenda que você tentou pegar. Eu acho. | You only tried to take the snack. I think. | 4ª fala; quem fala: Atli; expressão neutra. |
| `S4End05` | ...Ninguém nunca me deu comida. Só tomavam. | ...Nobody ever gave me food. They only took it. | 5ª fala; quem fala: o chefe da fase; expressão feliz. |
| `S4End06` | Eu vou ajudar vocês. O que procuram? | I'll help you. What are you looking for? | 6ª fala; quem fala: o chefe da fase; expressão feliz. |
| `S4End07` | Uma sacerdotisa. Levaram ela num barco prateado. | A priestess. They took her away in a silver boat. | 7ª fala; quem fala: Atli; expressão neutra. |
| `S4End08` | E o dia que sumiu. Procuramos a grande serpente. | And the day that vanished. We're looking for the great serpent. | 8ª fala; quem fala: Naru (espírito da lamparina); expressão neutra. |
| `S4End09` | O dragão da Grande Queda? Eu sei onde ele mora. | The dragon of the Great Fall? I know where he lives. | 9ª fala; quem fala: o chefe da fase; expressão surpresa. |
| `S4End10` | Os túneis confundem quem não é urso. Venham. | The tunnels confuse anyone who isn't a bear. Come. | 10ª fala; quem fala: o chefe da fase; expressão neutra. |
| `S4End11` | Ele guarda a porta entre os mundos. Deve saber da sua amiga. | He guards the door between the worlds. He must know about your friend. | 11ª fala; quem fala: o chefe da fase; expressão neutra. |
| `S4End12` | E do que está acontecendo com o dia e a noite. | And about what is happening to the day and the night. | 12ª fala; quem fala: o chefe da fase; expressão neutra. |

---

## Fase 5: Kuychu, o guardião de duas cabeças

### Título e nomes

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `Stage5Title` | Fase 5 | Stage 5 | Título da fase 5, mostrado quando ela começa. |
| `KuychuName` | Kuychu | Kuychu | Nome de personagem da fase (mostrado no título do chefe e nas falas). |
| `SerpentLeftName` | Serpente esquerda | Left Serpent | Nome de personagem da fase (mostrado no título do chefe e nas falas). |
| `SerpentRightName` | Serpente direita | Right Serpent | Nome de personagem da fase (mostrado no título do chefe e nas falas). |

### Ataques do chefe

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `AttackTwoHeads` | Duas Cabeças: Por Turnos | Two Heads: Taking Turns | Nome do ataque do chefe da fase 5 (a primeira forma, calma). |
| `AttackSevenArcs` | Arco-Íris: Sete Arcos | Rainbow: Seven Arcs | Nome do ataque do chefe da fase 5 (a primeira forma, calma). |
| `AttackBorderQuake` | Terremoto: A Fronteira Treme | Earthquake: The Border Shakes | Nome do ataque do chefe da fase 5 (a primeira forma, calma). |
| `AttackTide` | Maré: Muralhas d'Água | Tide: Walls of Water | Nome do ataque do chefe da fase 5 (a primeira forma, calma). |
| `AttackTrial` | Prova de Valor: Cabeça a Cabeça | Trial of Worth: Head to Head | Nome do ataque do chefe da fase 5 (a primeira forma, calma). |
| `AttackBrokenRainbow` | Dragão: Arco-Íris Partido | Dragon: The Broken Rainbow | Nome do ataque do chefe da fase 5 (a segunda forma, enfurecida). |
| `AttackTwoWorlds` | Dois Mundos: O Abalo | Two Worlds: The Tremor | Nome do ataque do chefe da fase 5 (a segunda forma, enfurecida). |
| `AttackGuardian` | Guardião dos Dois Reinos | Guardian of the Two Realms | Nome do ataque do chefe da fase 5 (a segunda forma, enfurecida). |

### Falas: Abertura da fase (antes dos inimigos)

*Resumo, do código (em inglês): Stage 5, at the start: Ukhumo leaves them at the foot of the fall, and warns them about the two heads.*

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `S5Intro01` | Daqui eu volto. A queda é logo ali. | I turn back here. The fall is just ahead. | 1ª fala; quem fala: Ukhumo; expressão neutra. |
| `S5Intro02` | Obrigado pela ajuda. | Thanks for the help. | 2ª fala; quem fala: Atli; expressão feliz. |
| `S5Intro03` | E obrigada pela merenda. | And thanks for the snack. | 3ª fala; quem fala: Ukhumo; expressão feliz. |
| `S5Intro04` | Cuidado com as duas cabeças. Elas brigam o tempo todo. | Beware of the two heads. They fight all the time. | 4ª fala; quem fala: Ukhumo; expressão preocupada. |
| `S5Intro05` | Todo mundo por aqui tem um defeito... | Everyone here has a flaw... | 5ª fala; quem fala: Naru (espírito da lamparina); expressão irritada. |

### Falas: Encontro com o chefe (antes da luta)

*Resumo, do código (em inglês): Before Kuychu: the two serpents that he wears speak first, and he asks for proof of their worth, which is a fight.*

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `S5Meet01` | Estrangeiros. | Foreigners. | 1ª fala; quem fala: Serpente esquerda; expressão neutra. |
| `S5Meet02` | Estranhos. | Strangers. | 2ª fala; quem fala: Serpente direita; expressão irritada. |
| `S5Meet03` | Parem, vocês dois. | Stop, you two. | 3ª fala; quem fala: o chefe da fase; expressão irritada. |
| `S5Meet04` | A gente quer atravessar a fronteira. | We want to cross the border. | 4ª fala; quem fala: Atli; expressão neutra. |
| `S5Meet05` | Ninguém cruza sem provar o seu valor. É meu dever proteger os dois reinos. | No one crosses without proving their worth. It is my duty to protect the two realms. | 5ª fala; quem fala: o chefe da fase; expressão neutra. |
| `S5Meet06` | Prova... como? | Proving it... how? | 6ª fala; quem fala: Naru (espírito da lamparina); expressão preocupada. |
| `S5Meet07` | Lutando, claro! | By fighting, of course! | 7ª fala; quem fala: o chefe da fase; expressão irritada; GRITADA (a caixa treme). |

### Falas: Final da fase (depois da luta)

*Resumo, do código (em inglês): After Kuychu: he knows who took Illari and will not say, but he will carry them to her, up the fall, by a bridge of water.*

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `S5End01` | Hmpf... vocês provaram o seu valor. | Hmpf... you have proven your worth. | 1ª fala; quem fala: o chefe da fase; expressão irritada. |
| `S5End02` | Foram ótimos. | They were great. | 2ª fala; quem fala: Serpente esquerda; expressão feliz. |
| `S5End03` | Fracos. | Weak. | 3ª fala; quem fala: Serpente direita; expressão irritada. |
| `S5End04` | Fracos, é? Quem caiu primeiro? | Weak, is it? Who fell first? | 4ª fala; quem fala: Serpente esquerda; expressão irritada. |
| `S5End05` | Quietos. O que querem do outro lado? | Quiet. What do you want on the other side? | 5ª fala; quem fala: o chefe da fase; expressão irritada. |
| `S5End06` | A nossa amiga. Uma sacerdotisa. Levaram ela num barco prateado. | Our friend. A priestess. They took her away in a silver boat. | 6ª fala; quem fala: Atli; expressão neutra. |
| `S5End07` | Eu vi o barco. Passou sem pedir licença. E levou a moça junto. | I saw the boat. It passed without asking leave. And took the girl along. | 7ª fala; quem fala: o chefe da fase; expressão neutra. |
| `S5End08` | Aquela birrenta... Quer uma coisa e o mundo inteiro paga por isso. | That brat... She wants one thing and the whole world pays for it. | 8ª fala; quem fala: o chefe da fase; expressão irritada. |
| `S5End09` | Quem? | Who? | 9ª fala; quem fala: Atli; expressão surpresa. |
| `S5End10` | Vão encontrar ela. Não me cabe dizer. | You will find her. It is not for me to say. | 10ª fala; quem fala: o chefe da fase; expressão neutra. |
| `S5End11` | O senhor sabe, mas não diz. | You know, but you won't tell. | 11ª fala; quem fala: Naru (espírito da lamparina); expressão preocupada. |
| `S5End12` | Guardiões sabem de tudo e falam de nada. | Guardians know everything and say nothing. | 12ª fala; quem fala: o chefe da fase; expressão neutra. |
| `S5End13` | Vou ajudar vocês a chegarem até ela. | I will help you get to her. | 13ª fala; quem fala: o chefe da fase; expressão neutra. |
| `S5End14` | Eu guardo os dois reinos: não gosto de quem desequilibra a ordem. | I guard the two realms: I do not like those who unbalance the order. | 14ª fala; quem fala: o chefe da fase; expressão neutra. |
| `S5End15` | A gente vai subir... por água? | We're going up... by water? | 15ª fala; quem fala: Atli; expressão preocupada. |
| `S5End16` | Água é água, em cima ou embaixo! | Water is water, above or below! | 16ª fala; quem fala: Atli; expressão feliz. |
| `S5End17` | Esperto, o menino. Segurem firme. | Clever boy. Hold on tight. | 17ª fala; quem fala: o chefe da fase; expressão feliz. |

### Falas: Final da fase (depois da luta)

*Resumo, do código (em inglês): Easy mode ends at stage 5: after Kuychu falls too easily he calls them unworthy and turns them back (the bad ending follows).*

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `S5Easy01` | Hmpf... vocês me derrotaram. | Hmpf... you have defeated me. | 1ª fala; quem fala: o chefe da fase; expressão irritada. |
| `S5Easy02` | Com facilidade. | With ease. | 2ª fala; quem fala: Serpente esquerda; expressão neutra. |
| `S5Easy03` | Facilidade demais. | Too much ease. | 3ª fala; quem fala: Serpente direita; expressão irritada. |
| `S5Easy04` | Vitória sem suor não prova valor algum. Quem passa assim, sem ter sido posto à prova de verdade, não é digno de cruzar a fronteira. | A victory without sweat proves no worth at all. Whoever passes like this, never truly tested, is not worthy of crossing the border. | 4ª fala; quem fala: o chefe da fase; expressão neutra. |
| `S5Easy05` | Mas... a gente ganhou! | But... we won! | 5ª fala; quem fala: Atli; expressão surpresa. |
| `S5Easy06` | Indignos! Voltem para o rio, e só me procurem quando souberem o que é lutar! | Unworthy! Go back to the river, and look for me only when you know what a fight is! | 6ª fala; quem fala: o chefe da fase; expressão irritada; GRITADA (a caixa treme). |
| `S5Easy07` | Atli... acho que a gente não vai passar. | Atli... I don't think we're getting through. | 7ª fala; quem fala: Naru (espírito da lamparina); expressão preocupada. |

---

## Fase 6: Mama Killa, a Lua

### Título e nomes

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `Stage6Title` | Fase 6 | Stage 6 | Título da fase 6, mostrado quando ela começa. |
| `KillaName` | Killa | Killa | Nome de personagem da fase (mostrado no título do chefe e nas falas). |
| `IllariName` | Illari | Illari | Nome de personagem da fase (mostrado no título do chefe e nas falas). |

### Ataques do chefe

A Killa ainda usa os ataques da fase 3 (Thaqai); os nomes são os mesmos daquela fase.

### Falas: Abertura da fase (antes dos inimigos)

*Resumo, do código (em inglês): Stage 6, at the start: the water of the sky, shining, and the trail goes on.*

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `S6Intro01` | É... água. Só que brilhando. | It's... water. Only it's shining. | 1ª fala; quem fala: Atli; expressão surpresa. |
| `S6Intro02` | Fica perto de mim. Tudo aqui é estranho. | Stay close to me. Everything here is strange. | 2ª fala; quem fala: Naru (espírito da lamparina); expressão preocupada. |
| `S6Intro03` | O rastro do barco continua! | The boat's trail goes on! | 3ª fala; quem fala: Atli; expressão feliz. |
| `S6Intro04` | Então estamos perto. | Then we are close. | 4ª fala; quem fala: Naru (espírito da lamparina); expressão neutra. |

### Falas: Chegada ao chefe (antes de ele aparecer)

*Resumo, do código (em inglês): Before the goddess: they find Illari asleep on the ground, in the middle of the river, and guess whose cat it was.*

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `S6Arr01` | Illari! Ela está ali, deitada no chão! | Illari! She is over there, lying on the ground! | 1ª fala; quem fala: Atli; expressão surpresa. |
| `S6Arr02` | Calma. Ela está só dormindo. Isso aqui tem cheiro de armadilha. | Easy. She is only sleeping. This smells like a trap. | 2ª fala; quem fala: Naru (espírito da lamparina); expressão preocupada. |
| `S6Arr03` | Aquela gata! Então a dona dela é... | That cat! Then her owner is... | 3ª fala; quem fala: Atli; expressão preocupada. |

### Falas: Encontro com o chefe (antes da luta)

*Resumo, do código (em inglês): The goddess comes down and sends the sleeping Illari away (at the first line), and the longer talk that ends with the fight.*

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `S6Meet01` | Killa, se me permite. | Killa, if you please. | 1ª fala; quem fala: o chefe da fase; expressão neutra. |
| `S6Meet02` | E vocês são o barquinho que incomodou a minha gata. | And you are the little boat that bothered my cat. | 2ª fala; quem fala: o chefe da fase; expressão irritada. |
| `S6Meet03` | Ei! Cadê a Illari? Ela estava bem aqui! | Hey! Where is Illari? She was right here! | 3ª fala; quem fala: Atli; expressão surpresa. |
| `S6Meet04` | Mandei para um lugar sossegado. Lutar com moça dormindo no meio é deselegante. | I sent her somewhere quiet. Fighting with a girl asleep in the middle is unseemly. | 4ª fala; quem fala: o chefe da fase; expressão neutra. |
| `S6Meet05` | Deselegante é sequestrar uma sacerdotisa. | Unseemly is kidnapping a priestess. | 5ª fala; quem fala: Naru (espírito da lamparina); expressão irritada. |
| `S6Meet06` | Está bem. Muito bem. Melhor do que você, pelo visto. | Very well. She is fine. Better than you, it seems. | 6ª fala; quem fala: o chefe da fase; expressão irritada. |
| `S6Meet07` | Devolve. | Give her back. | 7ª fala; quem fala: Atli; expressão neutra. |
| `S6Meet08` | Não. A noite fica. | No. The night stays. | 8ª fala; quem fala: o chefe da fase; expressão neutra. |
| `S6Meet09` | Ela não vai mudar de ideia. | She won't change her mind. | 9ª fala; quem fala: Naru (espírito da lamparina); expressão preocupada. |
| `S6Meet10` | Nem vocês vão me fazer mudar! | Not even you will make me change! | 10ª fala; quem fala: o chefe da fase; expressão irritada; GRITADA (a caixa treme). |

### Falas: Final da fase (depois da luta)

*Resumo, do código (em inglês): After the goddess: tired, she is talked round; she brings Illari back, who wakes and speaks with the three.*

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `S6End01` | Chega. Podem parar. ...Não muda nada. A noite fica. | Enough. You can stop. ...It changes nothing. The night stays. | 1ª fala; quem fala: o chefe da fase; expressão preocupada. |
| `S6End02` | Posso dizer uma coisa? Eu gosto mais da noite. | Can I say something? I like the night better. | 2ª fala; quem fala: Atli; expressão neutra. |
| `S6End03` | ...Continue. | ...Go on. | 3ª fala; quem fala: o chefe da fase; expressão neutra. |
| `S6End04` | O dia é muito quente. Eu suo. Eu queimo. Ninguém pediu isso. | The day is too hot. I sweat. I burn. Nobody asked for that. | 4ª fala; quem fala: Atli; expressão neutra. |
| `S6End05` | Então você entende! Que a noite fique! | Then you understand! Let the night stay! | 5ª fala; quem fala: o chefe da fase; expressão feliz. |
| `S6End06` | Mas pensa: se o dia não existisse, ninguém teria com o que comparar a noite. | But think: if the day did not exist, there would be nothing to compare the night with. | 6ª fala; quem fala: Atli; expressão neutra. |
| `S6End07` | Ninguém agradece o clima. | Nobody thanks the weather. | 7ª fala; quem fala: Naru (espírito da lamparina); expressão irritada. |
| `S6End08` | Aí as pessoas deixam de gostar da noite. Ela viraria só... como as coisas são. | Then people stop liking the night. It would be just... how things are. | 8ª fala; quem fala: Atli; expressão neutra. |
| `S6End09` | ...Ninguém agradece o clima. | ...Nobody thanks the weather. | 9ª fala; quem fala: o chefe da fase; expressão sem graça. |
| `S6End10` | E quem escreveria poemas sobre a noite? | And who would write poems about the night? | 10ª fala; quem fala: Atli; expressão feliz. |
| `S6End11` | ...Eu nunca tive um poema. Nenhum bom. | ...I never had a poem. Not a good one. | 11ª fala; quem fala: o chefe da fase; expressão sem graça. |
| `S6End12` | É o dia que faz as pessoas notarem a senhora. | It is the day that makes people notice you. | 12ª fala; quem fala: Atli; expressão feliz. |
| `S6End13` | ...Está bem. Levem a moça. E digam ao meu irmão que seja breve. Doze horas. | ...Very well. Take the girl. And tell my brother to be brief. Twelve hours. | 13ª fala; quem fala: o chefe da fase; expressão sem graça. |
| `S6End14` | Dez? | Ten? | 14ª fala; quem fala: Atli; expressão surpresa. |
| `S6End15` | ...Dez e meia. E uma brisa ao meio-dia. | ...Ten and a half. And a breeze at noon. | 15ª fala; quem fala: o chefe da fase; expressão neutra. |
| `S6End16` | Atli? O que você está fazendo aqui? | Atli? What are you doing here? | 16ª fala; quem fala: Illari (a sacerdotisa); expressão surpresa. |
| `S6End17` | Vim te buscar! Você não estava presa? | I came to get you! Weren't you a prisoner? | 17ª fala; quem fala: Atli; expressão feliz. |
| `S6End18` | Presa? Eu estava tomando chá. Ela faz um chá excelente. | A prisoner? I was having tea. She makes an excellent tea. | 18ª fala; quem fala: Illari (a sacerdotisa); expressão feliz. |
| `S6End19` | Ela só não me deixava sair. Está sozinha, sabe. | She just wouldn't let me leave. She is lonely, you know. | 19ª fala; quem fala: Illari (a sacerdotisa); expressão preocupada. |
| `S6End20` | Eu não estou sozinha. | I am not lonely. | 20ª fala; quem fala: o chefe da fase; expressão sem graça. |
| `S6End21` | Vamos, Illari. O ritual está esperando. | Come on, Illari. The ritual is waiting. | 21ª fala; quem fala: Atli; expressão feliz. |
| `S6End22` | E o sol não vai nascer sozinho. | And the sun won't rise by itself. | 22ª fala; quem fala: Naru (espírito da lamparina); expressão feliz. |

---

## Cartas

Cada carta tem um nome (**Título**) e uma descrição curta do que ela faz (**Detalhe**), mostrados na escolha de carta e na HUD.

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `CardHomingWisp.Title` | Chama Guia | Homing Wisp | Nome da carta (HomingWisp). |
| `CardHomingWisp.Detail` | Uma chama segue você e caça o inimigo mais próximo. | A wisp of light follows your lantern and hunts the nearest foe. | Descrição da carta (HomingWisp). |
| `CardTwinFan.Title` | Leque Duplo | Twin Fan | Nome da carta (TwinFan). |
| `CardTwinFan.Detail` | Dois raios mais fracos se abrem a cada tiro. | Two lighter bolts fan out from every shot. | Descrição da carta (TwinFan). |
| `CardPiercingLight.Title` | Luz Perfurante | Piercing Light | Nome da carta (PiercingLight). |
| `CardPiercingLight.Detail` | Seus raios atravessam mais um inimigo. | Your bolts pass through one more foe. | Descrição da carta (PiercingLight). |
| `CardFervor.Title` | Fervor | Fervor | Nome da carta (Fervor). |
| `CardFervor.Detail` | +25% de dano em cada tiro. | +25% damage with every shot. | Descrição da carta (Fervor). |
| `CardQuickWick.Title` | Pavio Rápido | Quick Wick | Nome da carta (QuickWick). |
| `CardQuickWick.Detail` | +20% de cadência de tiro. | +20% fire rate. | Descrição da carta (QuickWick). |
| `CardBurstingBolts.Title` | Raios Explosivos | Bursting Bolts | Nome da carta (BurstingBolts). |
| `CardBurstingBolts.Detail` | Explodem ao acertar e ferem quem está perto. | Bolts burst on impact and hurt the foes around. | Descrição da carta (BurstingBolts). |
| `CardSpiritWard.Title` | Proteção do Espírito | Spirit Ward | Nome da carta (SpiritWard). |
| `CardSpiritWard.Detail` | Um escudo de luz bloqueia o próximo golpe. | A ward of light blocks the next hit. | Descrição da carta (SpiritWard). |
| `CardMendingWard.Title` | Escudo Vivo | Mending Ward | Nome da carta (MendingWard). |
| `CardMendingWard.Detail` | Um escudo bloqueia um golpe e se refaz a cada 45 segundos. | A ward blocks a hit, and mends itself every 45 seconds. | Descrição da carta (MendingWard). |
| `CardSlimWake.Title` | Casco Fino | Slim Wake | Nome da carta (SlimWake). |
| `CardSlimWake.Detail` | Seu barco é mais difícil de acertar: hitbox menor. | Your boat is harder to hit: smaller hitbox. | Descrição da carta (SlimWake). |
| `CardCalmSails.Title` | Velas Calmas | Calm Sails | Nome da carta (CalmSails). |
| `CardCalmSails.Detail` | O vento empurra você 40% menos. | The wind pushes you 40% less. | Descrição da carta (CalmSails). |
| `CardBrighterLight.Title` | Luz Mais Forte | Brighter Light | Nome da carta (BrighterLight). |
| `CardBrighterLight.Detail` | +20% de alcance da bomba. | +20% bomb radius. | Descrição da carta (BrighterLight). |
| `CardLongBurn.Title` | Chama Longa | Long Burn | Nome da carta (LongBurn). |
| `CardLongBurn.Detail` | As bombas duram 1 segundo a mais. | Bombs last 1 second longer. | Descrição da carta (LongBurn). |
| `CardSearingLight.Title` | Luz Ardente | Searing Light | Nome da carta (SearingLight). |
| `CardSearingLight.Detail` | +50% de dano da bomba. | +50% bomb damage. | Descrição da carta (SearingLight). |
| `CardSpareLantern.Title` | Bomba Reserva | Spare Bomb | Nome da carta (SpareLantern). |
| `CardSpareLantern.Detail` | Recarrega uma bomba. | Refills one bomb. | Descrição da carta (SpareLantern). |
| `CardHungryLantern.Title` | Bomba Faminta | Hungry Bomb | Nome da carta (HungryLantern). |
| `CardHungryLantern.Detail` | As luzes dão +50% de energia. | Lights give +50% energy. | Descrição da carta (HungryLantern). |
| `CardWideNet.Title` | Rede Larga | Wide Net | Nome da carta (WideNet). |
| `CardWideNet.Detail` | +40% de alcance para atrair as luzes. | +40% reach for pulling in lights. | Descrição da carta (WideNet). |
| `CardGoldenSpirits.Title` | Espíritos Dourados | Golden Spirits | Nome da carta (GoldenSpirits). |
| `CardGoldenSpirits.Detail` | +25% de pontos. | +25% score. | Descrição da carta (GoldenSpirits). |
| `CardSerpentFlame.Title` | Chama da Serpente | Serpent Flame | Nome da carta (SerpentFlame). Carta especial, deixada pelo chefe da fase 1. |
| `CardSerpentFlame.Detail` | +35% de dano, tiros explodem em brasas. | +35% damage, shots burst in embers. | Descrição da carta (SerpentFlame). Carta especial, deixada pelo chefe da fase 1. |
| `CardPartyFever.Title` | Febre da Festa | Party Fever | Nome da carta (PartyFever). Carta especial, deixada pelo chefe da fase 2. |
| `CardPartyFever.Detail` | +40% de pontos, luzes dão +30% de energia. | +40% score, lights give +30% energy. | Descrição da carta (PartyFever). Carta especial, deixada pelo chefe da fase 2. |
| `CardStormBolt.Title` | Raio da Tempestade | Storm Bolt | Nome da carta (StormBolt). Carta especial, deixada pelo chefe da fase 3. |
| `CardStormBolt.Detail` | Tiros atravessam +2 e voam 25% mais rápido. | Shots pierce 2 more and fly 25% faster. | Descrição da carta (StormBolt). Carta especial, deixada pelo chefe da fase 3. |
| `CardBearCloak.Title` | Manto de Urso | Bear Cloak | Nome da carta (BearCloak). Carta especial, deixada pelo chefe da fase 4. |
| `CardBearCloak.Detail` | Um escudo bloqueia o próximo golpe, e o vento empurra 30% menos. | A ward blocks the next hit, and the wind pushes you 30% less. | Descrição da carta (BearCloak). Carta especial, deixada pelo chefe da fase 4. |

---

## Outros

| Chave | Português (PT-BR) | Inglês (EN) | Contexto |
| --- | --- | --- | --- |
| `Boss6Music` | ♪ A lua prateada reflete sob o lago | ♪ The Silver Moon Reflects on the Lake | Texto sem contexto catalogado ainda. |
| `MenuMusic` | ♪ Águas quietas - entrada e menu | ♪ Quiet waters - opening and menu | Texto sem contexto catalogado ainda. |
| `SadEndingMusic` | ♪ Noite sem fim - final ruim | ♪ Endless night - bad ending | Texto sem contexto catalogado ainda. |
| `HappyEndingMusic` | ♪ Aurora - final | ♪ Daybreak - ending | Texto sem contexto catalogado ainda. |
| `MusicRoomTitle` | Sala de música | Music Room | Texto sem contexto catalogado ainda. |
| `RestartsLeft` | Reinícios restantes: | Restarts left: | Texto sem contexto catalogado ainda. |
| `GameOver` | Game Over | Game Over | Texto sem contexto catalogado ainda. |
| `GameOverMusic` | ♪ Luz que se apaga - game over | ♪ Light going out - game over | Texto sem contexto catalogado ainda. |
| `DifficultyStages` | ({0} Fases) | ({0} Stages) | Texto sem contexto catalogado ainda. |
| `AttackCatPlay` | Brincadeira da Gata: Novelos que Quicam | The Cat at Play: Bouncing Yarn | Texto sem contexto catalogado ainda. |
| `AttackKillaCrescent` | Lua Crescente: Os Chifres de Prata | Crescent Moon: The Horns of Silver | Texto sem contexto catalogado ainda. |
| `AttackKillaTide` | Maré Lunar: O Rio do Céu | Lunar Tide: The River of the Sky | Texto sem contexto catalogado ainda. |
| `AttackKillaEclipse` | Eclipse Total: O Sol Coberto | Total Eclipse: The Covered Sun | Texto sem contexto catalogado ainda. |
| `AttackKillaNewMoon` | Lua Nova: O Céu Sem Lua | New Moon: The Moonless Sky | Texto sem contexto catalogado ainda. |
| `AttackKillaFullMoon` | Lua Cheia: O Olho de Prata | Full Moon: The Eye of Silver | Texto sem contexto catalogado ainda. |
| `AttackKillaConstellations` | Constelações: O Mapa das Estrelas | Constellations: The Map of the Stars | Texto sem contexto catalogado ainda. |
| `AttackKillaSilverRain` | Chuva de Prata: Os Raios da Lua | Silver Rain: The Rays of the Moon | Texto sem contexto catalogado ainda. |
| `AttackKillaWaning` | Lua Minguante: A Sombra Cresce | Waning Moon: The Shadow Grows | Texto sem contexto catalogado ainda. |
| `AttackKillaPentagram` | Pentagrama Lunar: A Estrela de Cinco Pontas | Lunar Pentagram: The Five-Pointed Star | Texto sem contexto catalogado ainda. |
| `AttackKillaEternalNight` | Noite Eterna | Eternal Night | Texto sem contexto catalogado ainda. |
| `BadEnd1` | Barrados na fronteira, Atli e Ukhumo voltaram ao rio, e a noite não terminou. | Turned back at the border, Atli and Ukhumo returned to the river, and the night did not end. | Texto sem contexto catalogado ainda. |
| `BadEnd2` | Atli: Faz quanto tempo que a gente está parado aqui, Ukhumo? | Atli: How long have we been sitting here, Ukhumo? | Texto sem contexto catalogado ainda. |
| `BadEnd3` | Ukhumo: Desde que o sol sumiu. E os peixes continuam sem morder. | Ukhumo: Since the sun vanished. And the fish still won't bite. | Texto sem contexto catalogado ainda. |
| `BadEnd4` | Atli: Pelo menos ninguém precisa acordar cedo. E a lua é bonita, vai. | Atli: At least nobody has to wake up early. And the moon is pretty, come on. | Texto sem contexto catalogado ainda. |
| `BadEnd5` | Ukhumo: Bonita e fria. Sem sol, o milho não cresce, e eu estou com fome. | Ukhumo: Pretty and cold. Without the sun the corn won't grow, and I'm hungry. | Texto sem contexto catalogado ainda. |
| `BadEnd6` | Ukhumo: Você pesca pouco e reclama muito, Atli. Passa a isca. | Ukhumo: You fish little and complain a lot, Atli. Pass the bait. | Texto sem contexto catalogado ainda. |
| `BadEnd7` | E o sol nunca mais nasceu. | And the sun never rose again. | Texto sem contexto catalogado ainda. |

