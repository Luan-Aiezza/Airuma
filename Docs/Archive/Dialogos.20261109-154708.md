# Airumã: diálogos entre os personagens

**Como editar:** mude apenas a coluna **Texto** (a coluna Quem fala só informa quem diz a frase; por enquanto ela não é lida). Não mude a coluna **Chave**, não apague linhas e não use o caractere `|` dentro de um texto. Depois é só me pedir para ler os documentos: eu aplico tudo de uma vez no jogo (`python3 Docs/tools/textos.py import`). Para gerar os documentos de novo a partir do jogo: `python3 Docs/tools/textos.py export`.

- Os dois documentos são só em português. O inglês do jogo não é tocado; quando você terminar, me diga se quer que eu traduza o que mudou.
- Cada fala cabe em até 5 linhas curtas na caixa de diálogo (uns 28 letras por linha), então textos muito longos são cortados.
- A fonte do jogo não tem alguns sinais, como aspas e ponto e vírgula; prefira vírgula, ponto, dois pontos, traço, parênteses, `!`, `?`, `/`, `+`, `%` e `...`.
- Nas falas, o personagem "o chefe da fase" mostra o nome do chefe da fase.
- O espírito da lamparina se chama **Naru** nas falas.

---

## Fase 1: Iryssh, a mulher-serpente de fogo

### Falas: Abertura da fase (antes dos inimigos)



| Chave | Texto (PT-BR) | Quem fala |
| --- | --- | --- |
| `Intro01` | Hmm... tenho certeza que já vi aquela árvore. | Naru |
| `Intro02` | É porque já vimos aquela árvore. | Atli |
| `Intro03` | ESTAMOS PERDIDOS?! | Naru |
| `Intro04` | Não estamos perdidos... só está difícil enxergar nesse escuro. | Atli |
| `Intro05` | Peraí, você tá me culpando?! A culpa não é minha, estou quase sem óleo! | Naru |
| `Intro06` | Foi mal, foi mal... vou reabastecer mais tarde. | Atli |
|  |

### Falas: Chegada ao chefe (antes de ele aparecer)



| Chave | Texto (PT-BR) | Quem fala |
| --- | --- | --- |
| `BossA1` | Um lago! Conseguimos sair!! | Naru |
| `BossA2` | Tem muitos espíritos nessa região, isso não parece bom... | Atli |
| `BossA3` | Não importa! Eu preciso de óleo! | Naru |
| `BossA4` | Espere.. tem alguma coisa saindo da agua! | Atli |
| `BossA5` | *Tremores* | Ninguem |

### Falas: Encontro com o chefe (antes da luta)



| Chave | Texto (PT-BR) | Quem fala |
| --- | --- | --- |
| `BossB1` | Que barulheira toda e essa no meu territorio?! | Iryssh |
| `BossB2` | Quem ousa atrapalhar meu sono da beleza?! | Iryssh |
| `BossB3` | Sono da beleza? Embaixo da água? | Atli |
| `BossB4` | Ei! A água faz bem para o cabelo e para a pele! Um pirralho que nem você não entende nada de beleza! | Iryssh |
| `BossB5` | Ehhh.. não parece estar dando muito certo... | Naru |
| `BossB6` | COMO É?! VOCÊS VÃO PAGAR POR ISSO INVASORES!!! | Iryssh |
| `BossB7` | Eu deveria te jogar na agua, lamparina desbocada... | Atli |

### Falas: Final da fase (depois da luta)



| Chave | Texto (PT-BR) | Quem fala |
| --- | --- | --- |
| EndE2 | Pronto! agora voce pode dormir pra sempre! | Atli |
| 

---

## Fase 2: Baylo, o boto da festa

### Falas: Abertura da fase (antes dos inimigos)



| Chave | Texto (PT-BR) | Quem fala |
| --- | --- | --- |
| `S2Intro01` | Acho que encontramos um caminho... | Atli |
| `S2Intro02` | Isso são luzes de festa..? | Naru |
| `S2Intro03` | É o que parece... mas uma festa no meio do nada? | Atli |
| `S2Intro04` | Talvez encontremos alguém que saiba de algo! | Naru |
| `S2Intro05` | Quem sabe... | Atli |
| `S2Intro06` | Mas e melhor tomarmos cuidado     | Atli |

### Falas: Encontro com o chefe (antes da luta)



| Chave | Texto (PT-BR) | Quem fala |
| --- | --- | --- |
| `S2Meet01` | Ora, ora! Temos convidados! Que alegria! | Baylo |
| `S2Meet02` | O que acham de se divertir comigo? | Baylo |
| `S2Meet03` | Valeu, mas a gente meio que está com pressa... | Atli |
| `S2Meet04` | Quem tem pressa não tem ritmo meu amigo! | Baylo |
| `S2Meet05` | O quê? Mas o que uma coisa tem a ve... | Naru |
| `S2Meet06` | VAMOS DANÇAR, MEU POVO!!! | Baylo |

### Falas: Final da fase (depois da luta)



| Chave | Texto (PT-BR) | Quem fala |
| --- | --- | --- |
| `S2End01` | Ha ha! Faz anos que alguém não me acompanhava assim! | Baylo |
| `S2End02` | Nós podemos ir agora? Estamos no meio de algo importante... | Atli |
| `S2End03` | Mas vocês sequer sabem para onde estão indo? | Baylo |
| `S2End04` | Ehhh... | Atli |
| `S2End05` | Hahaha! Eu imaginei! | Baylo |
| `S2End06` | Hmm, acho que nao tem problema contar.. | Baylo |
|  |Sigam pela direita do rio, lá talvez encontrem uma pista do que procuram.   |  Baylo|
| `S2End07` | Hã? Mas você nem sab... | Atli |
| `S2End08` | Confie em mim... uma recompensa por terem me entretido. | Baylo |
| `S2End09` | Não é como se vocês tivessem muita escolha, haha! | Baylo |
| `S2End10` | *Suspiros* Acho que não tem jeito, vamos lá então... | Atli |


---

## Fase 3: Thaqai, a gata das estrelas

### Falas: Abertura da fase (antes dos inimigos)



| Chave | Texto (PT-BR) | Quem fala |
| --- | --- | --- |
| `S3Intro01` | QUE TEMPESTADE E ESSA?!!  | Atli |
| `S3Intro02` | AQUELE MALDITO NOS ENGANOU! | Atli |
| `S3Intro03` | WHAA!! EU VOU APAGAR! | Naru |

### Falas: Encontro com o chefe (antes da luta)



| Chave | Texto (PT-BR) | Quem fala |
| --- | --- | --- |
| `S3Meet01` | Parados! Ninguém pode passar por aqui! | Thaqai |
| `S3Meet02` | O que... porque? | Atli |
| `S3Meet03` | Isso não é da sua conta! Estou aqui para que ninguém atrapalhe os planos de minha mestra! | Thaqai |
| `S3Meet04` | Planos? Mestra?  | Naru |
| `S3Meet05` | Arghh, eu falei demais! Caim fora logo!!! | Thaqai |

### Falas: Final da fase (depois da luta)



| Chave | Texto (PT-BR) | Quem fala |
| --- | --- | --- |
| `S3End01` | Eu.. perdi?? | Thaqai |
| `S3End02` | Essa nao... a mestra vai me dar uma bronca! Tudo por culpa daquela sacerdotisa! | Thaqai |
| `S3End03` | Sacerdotisa?! Ei, voce sabe de algo nao e?!  | Atli |
| `S3End04` |  NAOOO!! PORQUE EU SEMPRE ABRO A BOCA? EU NAO DISSE NADA!! | Thaqai |
|  |ME  PERDOE MESTRA! (choro)  |  Thaqai|
| `S3End05` | EI!.. Droga ela fugiu!  | Atli |
| `S3End06` | Pra alguem como ela ter uma mestra, certamente deve ser obra de alguma divindade... | Naru |
| `S3End07` | Se for.. a grande serpe deve saber de algo! | Naru |
| `S3End08` | Quem e esse?  | Atli |
| `S3End09` | A grande serpe cuida das fronteiras entre o mundo espiritual e fisico, ela vive no topo dos Andes. | Naru |
| `S3End10` | Andes?! Ta brincando?! Vai levar uma vida pra chegar la! | Atli |
| `S3End11` | Poise... | Naru |
| `S3End12` | Espero que tenha oleo suficiente! | Naru |
|  |   ...| Atli|


---

## Fase 4: Ukhumo, a ursa da caverna

### Falas: Abertura da fase (antes dos inimigos)



| Chave | Texto (PT-BR) | Quem fala |
| --- | --- | --- |
| `S4Intro01` | Tem certeza que e por aqui?  | Atli |
| `S4Intro02` | Ehh... esse deveria ser o atalho pros Andes.. mas nao me lembro daqui | Naru |
| `S4Intro03` | Nao e possivel.. perdidos de novo?! | Atli |
|  S4Intro04|  Nao estamos perdidos!!! |  Naru|

### Falas: Encontro com o chefe (antes da luta)



| Chave | Texto (PT-BR) | Quem fala |
| --- | --- | --- |
| `S4Meet01` | Ei! Tem alguem ali! | Naru |
| `S4Meet02` | Uma garota..? | Atli |
| `S4Meet03` | Ugh.. To com tanta fome... | Ukhumo |
| `S4Meet04` | Ei... voce ta bem?! | Naru |
| `S4Meet05` | \*Sniff\* \*Sniff\* | Ukhumo |
|  S4Meet06|  Sinto cheiro.. de comida!!! |  Ukhumo|
|  S4Meet07|  VOU DEVORAR VOCES! |Ukhumo|

### Falas: Final da fase (depois da luta)



| Chave | Texto (PT-BR) | Quem fala |
| --- | --- | --- |
| `S4End01` | Ugh.. nao da, to com muita fome... | Ukhumo |
| `S4End02` | ..Aqui, eu tenho alguns peixes assados, coma | Atli |
| `S4End03` | ..OHHH SERIO?! SERIO?! | Ukhumo |
| `S4End04` | Sim, ta tudo bem, eu posso pescar mais depois. | Atli |
| `S4End05` | ..EBAAA!! (\*Nhac\* \*Nhac) | Ukhumo |
| `S4End06` | Ela ta comendo como se nao houvesse amanha... | Naru |
| `S4End07` | Poise.. e que falta de modos... | Atli |
| `S4End08` | Que delicia! achei que ia morrer de fome, muito obrigada! | Ukhumo |
| `S4End09` | Nao tem por onde..  | Atli |
| `S4End10` | Mas entao.. porque voce estao nessas cavernas? | Ukhumo |
| `S4End11` | Estamos procurando as cachoeiras para os Andes, queremos falar com a grande serpe! | Naru |
| `S4End12` | AH! EU SEI ONDE FICA! VAMOS! VAMOS! VOU LEVAR VOCES ATE LA! | Ukhumo |


---

## Fase 5: Kuychu, o guardião de duas cabeças

### Falas: Abertura da fase (antes dos inimigos)



| Chave | Texto (PT-BR) | Quem fala |
| --- | --- | --- |
| `S5Intro01` | Para chegar aos Andes, voces devem subir pela cachoeira reversa | Ukhumo |
| `S5Intro02` | Ela e uma cachoeira que sobe ao inves de descer | Ukhumo |
| `S5Intro03` | Estou vendo... | Atli |
| `S5Intro04` | Bom, daqui eu volto! e boa sorte! | Ukhumo |
| `S5Intro05` | Obrigada! | Naru |
|  S5Intro06|  Ah! E tomem cuidado com os espiritos protetores! (\*voz distante\*) |  Ukhumo|
|  S5Intro07|  Espera.. espiritos protetores? |Atli  |

### Falas: Encontro com o chefe (antes da luta)



| Chave | Texto (PT-BR) | Quem fala |
| --- | --- | --- |
| `S5Meet01` | Quem sao voces..  | Kuychu (esquerda) |
| `S5Meet02` | Que ousam pisar nessa terra sagrada? | Kuychu (direita) |
| `S5Meet03` | Estou a procura da Sacerdotisa que sumiu, e o que causou essa escuridao! | Atli |
| `S5Meet04` | Olha os modos! essa e a grande serpe! | Atli |
| `S5Meet05` | Isso nao cabe a voce mortal,  va embora dessas terras... | Kuychu (esquerda) |
|  S5Meet06|  Ou sofra as consquencias... |  Kuychu (Direita) |
| `S5Meet07 | Eu nao vou sair daqui sem respostas! | Atli |
| `S5Meet08 | Entao prove seu valor. | Kuychu (esquerda) |
|  S5Meet09|  Perante o guardiao dos espiritos... |  Kuychu (direita) |

### Falas: Final da fase (depois da luta) caminho normal e dificil



| Chave | Texto (PT-BR) | Quem fala |
| --- | --- | --- |
| `S5End01` | Você provou o seu valor jovem guerreio... | Kuychu |
| `S5End02` |  Vao me dar respostas agora? | Atli  |
| `S5End03` | Hmm.. Talvez voces consigam convence-la a parar com isso. | Kuychu  |
| `S5End04` | Convence-la? do que estao falando? | Atli  |
| `S5End05` | Eu nao posso interferir na decisao dos divindos... | Kuychu |
| `S5End06` | Mas aquela deusa mimada, trocando a noite pelo dia, inaceitavel! | Kuychu |
| `S5End07` | Entao realmente isso e culpa de uma divindade...  | Naru |
| `S5End08` | E a minha amiga Ariah? voce sabe onde ela esta?! | Atli |
| `S5End09` | Tudo converge no mesmo lugar... | Kuychu |
| `S5End10` | Lhes mandarei ao reino dos espiritos, se for forte, prove seu valor uma vez mais la! | Kuychu |
| `S5End11` | Reino dos espiritos?! Espera! | Atli |
| `S5End12` | Va, traga o dia novamente crianca! | Kuychu |
| `S5End13` | Espera..! | Atli |
| `S5End14` | (\*Forte luz\*) | Kuychu |

### Falas: Final da fase (depois da luta) caminho facil



| Chave | Texto (PT-BR) | Quem fala |
| --- | --- | --- |
| `S5Easy01` | Hmpf... vocês nao tem valor | Kuychu |
| `S5Easy02` | Saiam do meu reino! | Kuychu |
 |


---

## Fase 6: Mama Killa, a Lua

### Falas: Abertura da fase (antes dos inimigos)



| Chave | Texto (PT-BR) | Quem fala |
| --- | --- | --- |
| `S6Intro01` | Isso e.. o ceu? estamos velejando sob o ceu? | Atli |
| `S6Intro02` |  Entao esse o reino dos espiritos? | Naru |
| `S6Intro03` | Nao sei.. mas nao sai de perto. | Atli |

### Falas: Chegada ao chefe (antes de ele aparecer)



| Chave | Texto (PT-BR) | Quem fala |
| --- | --- | --- |
| `S6Arr01` | Aquela e.. Ariah! Ela está ali! Rapido! Precisamos salva-la | Naru |
| `S6Arr02` | Espera! tem alguem vindo... | Atli |
| `S6Arr03` | Que presenca e essa?! | Naru |

### Falas: Encontro com o chefe (antes da luta)



| Chave | Texto (PT-BR) | Quem fala |
| --- | --- | --- |
| `S6Meet01` | Quem sao voces, o que fazem no meu dominio? | Killa |
| `S6Meet02` | Ah... vocês são o velejador e o espirito que incomodaram minha gata... | Killa |
| `S6Meet03` | Ei! Cadê a Ariah?! E voce que esta causando tudo isso? quem e voce? | Atli |
| `S6Meet04` | Que garoto deselegante. isso nao e da sua conta... | Killa |
| `S6Meet05` | Essa aparencia.. Atli, ela e Killa.. a deusa da lua! | Naru |
| `S6Meet06` | Nao me interessa se ela e uma deusa ou o que for, nao vou sair daqui sem a Ariah!  | Atli |
| `S6Meet07` | Fufu.. entao porque nao vem ate aqui pega-la? | Killa |
| `S6Meet08` | Atli, nao! ela e muito forte! | Naru |
| `S6Meet09` | Pode apostar que vou... | Atli |

### Falas: Final da fase (depois da luta)



| Chave | Texto (PT-BR) | Quem fala |
| --- | --- | --- |
| `S6End01` | Chega, podem parar! ..Porque se esfoca tanto? a noite fica! | Killa |
| `S6End02` | Porque voce quer que sempre seja noite? porque sequestrar a Ariah? | Atli |
| `S6End03` | ..Sem a sacerdotisa, o rito de solsticio falha e o sol nao nascera. | Killa |
| `S6End04` | Mas porque voce quer isso?? | Atli |
| `S6End05` | Eu.. odeio o dia!!! | Killa |
| `S6End06` | Todos gostam mais do dia do que da noite! a noite dura menos que o dia! porque?! | Killa |
| `S6End07` | Nao vou permitir que a noite seja subestimada nunca mais! ela e mais linda que o dia | Killa |
| `S6End08` | Mas.. eu acho a noite linda... | Atli |
| `S6End09` | ..Entao porque esta lutando contra mim? | Killa |
| `S6End10` | Porque a noite so e linda por existir o dia.  | Atli |
| `S6End11` | ..O que? | Killa |
| `S6End12` | Pense! sem o dia as pessoas vao se cansar da noite! | Atli |
| `S6End13` | E justamente por existir o dia, que as pessoas amam a noite! | Atli |
| `S6End14` | O dia e quente, durante o dia trabalhamos, e nao podemos admirar o ceu! ansiamos pela noite durante o dia! | Atli |
| `S6End15` | ... | Killa |
| `S6End16` | (\*pensativa\*) | Killa |
| `S6End17` | Certo.. voce me convenceu. | Killa |
| `S6End18` |  Deixarei voces partirem, e espero que continuem amando a noite e nao se esquecam dela! | Killa |
| `S6End19` | Pode deixar senhora! | Naru |
| `S6End20` | ..ahn? Atli? e voce? onde estamos? o que esta acontecendo?  | Ariah |
| `S6End21` | Nao se preocupe.. e so uma bela noite | Atli |
| `S6End22` | Vamos, voce tem trabalho a fazer, Ariah! | Naru |
