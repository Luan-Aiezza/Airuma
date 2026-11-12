# Airumã: diálogos entre os personagens

**Como editar:** mude apenas a coluna **Texto** (a coluna Quem fala só informa quem diz a frase; por enquanto ela não é lida). Não mude a coluna **Chave**, não apague linhas e não use o caractere `|` dentro de um texto. Depois é só me pedir para ler os documentos: eu aplico tudo de uma vez no jogo (`python3 Docs/tools/textos.py import`). Para gerar os documentos de novo a partir do jogo: `python3 Docs/tools/textos.py export`.

- Estes documentos são a tradução para o idioma ES: edite só a coluna Texto (ES); a coluna Referência (PT) é só para consulta. Cada fala cabe em 28 letras por 5 linhas.
- Cada fala cabe em até 5 linhas curtas na caixa de diálogo (uns 28 letras por linha), então textos muito longos são cortados.
- A fonte do jogo não tem alguns sinais, como aspas e ponto e vírgula; prefira vírgula, ponto, dois pontos, traço, parênteses, `!`, `?`, `/`, `+`, `%` e `...`.
- Nas falas, o personagem "o chefe da fase" mostra o nome do chefe da fase.
- O espírito da lamparina se chama **Naru** nas falas.

---

## Fase 1: Iryssh, a mulher-serpente de fogo

### Falas: Abertura da fase (antes dos inimigos)



| Chave | Texto (ES) | Referência (PT) | Quem fala |
| --- | --- | --- | --- |
| `Intro01` | Hmm... estoy seguro de que ya vi ese árbol. | Hmm... tenho certeza que já vi aquela árvore. | Naru |
| `Intro02` | Es porque ya vimos ese árbol. | É porque já vimos aquela árvore. | Atli |
| `Intro03` | ¿ESTAMOS PERDIDOS? | ESTAMOS PERDIDOS?! | Naru |
| `Intro04` | No estamos perdidos... solo cuesta ver en esta oscuridad. | Não estamos perdidos... só está difícil enxergar nesse escuro. | Atli |
| `Intro05` | Espera, ¿me estás culpando? ¡No es mi culpa, casi no me queda aceite! | Peraí, você tá me culpando?! A culpa não é minha, estou quase sem óleo! | Naru |
| `Intro06` | Perdón, perdón... te reabastezco más tarde. | Foi mal, foi mal... vou reabastecer mais tarde. | Atli |

### Falas: Chegada ao chefe (antes de ele aparecer)



| Chave | Texto (ES) | Referência (PT) | Quem fala |
| --- | --- | --- | --- |
| `BossA1` | ¡Un lago! ¡Logramos salir!! | Um lago! Conseguimos sair!! | Naru |
| `BossA2` | Hay muchos espíritus en esta región, no me da buena espina... | Tem muitos espíritos nessa região, isso não parece bom... | Atli |
| `BossA3` | ¡No importa! ¡Necesito aceite! | Não importa! Eu preciso de óleo! | Naru |
| `BossA4` | Espera... ¡algo está saliendo del agua! | Espere... tem alguma coisa saindo da água! | Atli |
| `BossA5` | *Temblores* | *Tremores* | Ninguém |

### Falas: Encontro com o chefe (antes da luta)



| Chave | Texto (ES) | Referência (PT) | Quem fala |
| --- | --- | --- | --- |
| `BossB1` | ¿Qué es todo este escándalo en mi territorio? | Que barulheira toda é essa no meu território?! | Iryssh |
| `BossB2` | ¿Quién se atreve a molestar mi sueño de belleza? | Quem ousa atrapalhar meu sono da beleza?! | Iryssh |
| `BossB3` | ¿Sueño de belleza? ¿Debajo del agua? | Sono da beleza? Embaixo da água? | Atli |
| `BossB4` | ¡Oye! ¡El agua dulce le hace bien al pelo y a la piel! | Ei! A água doce faz bem para o cabelo e para a pele! | Iryssh |
| `BossB5` | Ehhh... no parece estar funcionando muy bien... | Ehhh... não parece estar dando muito certo... | Naru |
| `BossB6` | ¿CÓMO TE ATREVES? ¡LO VAN A PAGAR!!! | COMO OUSA?! VOCÊS VÃO PAGAR POR ISSO!!! | Iryssh |
| `BossB7` | *Suspiros* Debería tirarte al agua, lámpara bocona... | *Suspiros* Eu deveria te jogar na água, lamparina desbocada... | Atli |

### Falas: Final da fase (depois da luta)



| Chave | Texto (ES) | Referência (PT) | Quem fala |
| --- | --- | --- | --- |
| `EndE1` | Listo, ¡ahora puedes tener tu sueño de belleza para siempre! | Pronto, agora você pode ter seu sono da beleza para sempre! | Atli |


---

## Fase 2: Baylo, o boto da festa

### Falas: Abertura da fase (antes dos inimigos)



| Chave | Texto (ES) | Referência (PT) | Quem fala |
| --- | --- | --- | --- |
| `S2Intro01` | Creo que encontramos un camino... | Acho que encontramos um caminho... | Atli |
| `S2Intro02` | ¿Esas son luces de fiesta...? | Isso são luzes de festa...? | Naru |
| `S2Intro03` | Eso parece... ¿pero una fiesta en medio de la nada? | É o que parece... mas uma festa no meio do nada? | Atli |
| `S2Intro04` | ¡Quizá encontremos a alguien que sepa algo! | Talvez encontremos alguém que saiba de algo! | Naru |
| `S2Intro05` | Quién sabe... | Quem sabe... | Atli |
| `S2Intro06` | Pero mejor tengamos cuidado. | Mas é melhor tomarmos cuidado. | Atli |

### Falas: Encontro com o chefe (antes da luta)



| Chave | Texto (ES) | Referência (PT) | Quem fala |
| --- | --- | --- | --- |
| `S2Meet01` | ¡Vaya, vaya! ¡Tenemos invitados, qué alegría! | Ora, ora! Temos convidados, que alegria! | Baylo |
| `S2Meet02` | ¿Qué les parece divertirse conmigo, jóvenes? | O que acham de se divertir comigo, jovens? | Baylo |
| `S2Meet03` | Gracias, pero es que tenemos algo de prisa... | Valeu, mas a gente meio que está com pressa... | Atli |
| `S2Meet04` | ¡Quien tiene prisa no tiene ritmo, amigo mío! | Quem tem pressa não tem ritmo, meu amigo! | Baylo |
| `S2Meet05` | ¿Qué? ¿Y qué tiene que ve... | O quê? Mas o que uma coisa tem a ve... | Naru |
| `S2Meet06` | ¡A BAILAR, MI GENTE!!! | VAMOS DANÇAR, MEU POVO!!! | Baylo |

### Falas: Final da fase (depois da luta)



| Chave | Texto (ES) | Referência (PT) | Quem fala |
| --- | --- | --- | --- |
| `S2End01` | ¡Ja, ja! ¡Hacía años que nadie me divertía así! | Ha ha! Faz anos que alguém não me divertia assim! | Baylo |
| `S2End02` | ¿Podemos irnos ya? Estamos en medio de algo importante... | Nós podemos ir agora? Estamos no meio de algo importante... | Atli |
| `S2End03` | ¿Pero ustedes siquiera saben adónde van? | Mas vocês sequer sabem para onde estão indo? | Baylo |
| `S2End04` | Ehhh... | Ehhh... | Atli |
| `S2End05` | ¡Jajaja! ¡Me lo imaginaba! | Hahaha! Eu imaginei! | Baylo |
| `S2End06` | Hmm, supongo que no hay problema en contarlo... | Hmm, acho que não tem problema contar... | Baylo |
| `S2End07` | Sigan la vena del río, allí encontrarán una pista de lo que está pasando. | Sigam pela veia do rio, lá encontrarão uma pista do que está acontecendo. | Baylo |
| `S2End08` | ¿Eh? Pero si ni sab... | Hã? Mas você nem sab... | Atli |
| `S2End09` | Si quieren descubrir por qué el cielo está detenido... | Se querem descobrir por que o céu está parado... | Baylo |
| `S2End10` | Bueno, no es que tengan mucha opción, ¡jaja! | Bom, não é como se vocês tivessem muita escolha, haha! | Baylo |
| `S2End11` | *Suspiros* Supongo que no hay remedio, vamos entonces... | *Suspiros* Acho que não tem jeito, vamos lá então... | Atli |


---

## Fase 3: Thaqai, a gata das estrelas

### Falas: Abertura da fase (antes dos inimigos)



| Chave | Texto (ES) | Referência (PT) | Quem fala |
| --- | --- | --- | --- |
| `S3Intro01` | ¿QUÉ TORMENTA ES ESTA?! | QUE TEMPESTADE É ESSA?!! | Atli |
| `S3Intro02` | ¡ESE MALDITO NOS ENGAÑÓ! | AQUELE MALDITO NOS ENGANOU! | Atli |
| `S3Intro03` | ¡¡WHAA!! ¡ESTÁ LLOVIENDO MUCHÍSIMO! ¡ME VOY A APAGAR! | WHAA!! TÁ CHOVENDO MUITO! EU VOU APAGAR! | Naru |

### Falas: Encontro com o chefe (antes da luta)



| Chave | Texto (ES) | Referência (PT) | Quem fala |
| --- | --- | --- | --- |
| `S3Meet01` | ¡Alto! ¡Nadie puede pasar por aquí! | Parados! Ninguém pode passar por aqui! | Thaqai |
| `S3Meet02` | ¿Qué...? ¿Por qué? | O que...? Por quê? | Atli |
| `S3Meet03` | ¡No es asunto tuyo! ¡Estoy aquí para que nadie estorbe los planes de mi maestra! | Isso não é da sua conta! Estou aqui para que ninguém atrapalhe os planos da minha mestra! | Thaqai |
| `S3Meet04` | ¿Planes? ¿Maestra? | Planos? Mestra? | Naru |
| `S3Meet05` | ¡Arghh, hablé de más! ¡Largo de aquí ya!!! | Arghh, eu falei demais! Caiam fora logo!!! | Thaqai |

### Falas: Final da fase (depois da luta)



| Chave | Texto (ES) | Referência (PT) | Quem fala |
| --- | --- | --- | --- |
| `S3End01` | ¿Yo... perdí? | Eu... perdi?! | Thaqai |
| `S3End02` | Ay no... ¡mi maestra me va a regañar! Debe estar observándome desde la luna... | Essa não... a mestra vai me dar uma bronca! Ela deve estar me observando pela lua... | Thaqai |
| `S3End03` | ¿La luna? Oye, ¿esto tiene que ver con esta noche sin fin? | Lua? Ei, isso tem a ver com essa noite sem fim?! | Atli |
| `S3End04` | ¡¡NOOO!! ¿POR QUÉ SIEMPRE ABRO LA BOCA? ¡NO DIJE NADA!! | NÃOOO!! POR QUE EU SEMPRE ABRO A BOCA? EU NÃO DISSE NADA!! | Thaqai |
| `S3End05` | ¡PERDÓNAME, MAESTRA! *Llanto* | ME PERDOE, MESTRA! *Choro* | Thaqai |
| `S3End06` | ¡OYE!... ¡Rayos, se escapó! | EI!... Droga, ela fugiu! | Atli |
| `S3End07` | La luna... hmm... Esa maestra suya debe ser alguien importante. | Lua... hmm... Essa mestra dela deve ser alguém importante. | Naru |
| `S3End08` | Si es así, ¡la Gran Serpiente debe saber algo! | Se for, a Grande Serpe deve saber de algo! | Naru |
| `S3End09` | ¿Quién es esa? | Quem é essa? | Atli |
| `S3End10` | ¡La Gran Serpiente! ¡La protectora del mundo espiritual, vive en la cima de los Andes! ¿No la conoces? | A Grande Serpe! A protetora do mundo espiritual, ela vive no topo dos Andes! Não a conhece?! | Naru |
| `S3End11` | ¿Los Andes? ¿Bromeas? ¡Eso queda lejísimos! | Andes?! Tá brincando?! Isso é muito longe! | Atli |
| `S3End12` | Sí, pues... | Pois é... | Naru |
| `S3End13` | ¿Tendremos aceite suficiente? | Será que temos óleo suficiente? | Naru |
| `S3End14` | ... | ... | Atli |


---

## Fase 4: Ukhumo, a ursa da caverna

### Falas: Abertura da fase (antes dos inimigos)



| Chave | Texto (ES) | Referência (PT) | Quem fala |
| --- | --- | --- | --- |
| `S4Intro01` | ¿Seguro que es por aquí? | Tem certeza que é por aqui? | Atli |
| `S4Intro02` | Ehh... este debería ser el atajo a los Andes, pero no recuerdo este lugar. | Ehh... esse deveria ser o atalho para os Andes, mas não me lembro daqui. | Naru |
| `S4Intro03` | No puede ser... ¿otra vez perdidos? | Não é possível... estamos perdidos de novo?! | Atli |
| `S4Intro04` | ¡¡¡No estamos perdidos!!! | Não estamos perdidos!!! | Naru |

### Falas: Encontro com o chefe (antes da luta)



| Chave | Texto (ES) | Referência (PT) | Quem fala |
| --- | --- | --- | --- |
| `S4Meet01` | ¡Oye! ¡Hay alguien allí! | Ei! Tem alguém ali! | Naru |
| `S4Meet02` | ¿Una chica...? | Uma garota...? | Atli |
| `S4Meet03` | Ugh... qué hambre tengo... | Ugh... tô com tanta fome... | Ukhumo |
| `S4Meet04` | Oye... ¿estás bien? | Ei... você tá bem?! | Naru |
| `S4Meet05` | *Snif* *Snif* | *Sniff* *Sniff* | Ukhumo |
| `S4Meet06` | Huelo... ¡carne!!! | Sinto cheiro... de carne!!! | Ukhumo |
| `S4Meet07` | ¡Serán el almuerzo de la GRAN UKHUMO!! | Vocês serão o almoço da GRANDE UKHUMO!! | Ukhumo |

### Falas: Final da fase (depois da luta)



| Chave | Texto (ES) | Referência (PT) | Quem fala |
| --- | --- | --- | --- |
| `S4End01` | Ugh... no puedo, tengo muchísima hambre... | Ugh... não dá, tô com muita fome... | Ukhumo |
| `S4End02` | Toma, come esto... | Aqui, coma esses... | Atli |
| `S4End03` | ¡¡¡PESCADO ASADO!!! ¿PUEDO? ¿EN SERIO? | PEIXE ASSADO!!! EU POSSO?! SÉRIO?! | Ukhumo |
| `S4End04` | No te preocupes, puedo pescar más después. | Não se preocupe, eu posso pescar mais depois. | Atli |
| `S4End05` | ...¡¡YUPIII!! *Ñam* *Ñam* | ...EBAAA!! *Nhac* *Nhac* | Ukhumo |
| `S4End06` | Está comiendo como si no hubiera mañana... | Ela tá comendo como se não houvesse amanhã... | Naru |
| `S4End07` | Sí... qué falta de modales... | Pois é... que falta de modos... | Atli |
| `S4End08` | ¡Delicioso!... Pensé que iba a morir de hambre, ¡muchas gracias! | Delicioso!... Achei que ia morrer de fome, muito obrigada! | Ukhumo |
| `S4End09` | No te preocupes... | Não se preocupe... | Atli |
| `S4End10` | Pero entonces, ¿por qué están en estas cuevas? No veo a mucha gente por aquí... | Mas então, por que estão nessas cavernas? Não vejo muitos por aqui... | Ukhumo |
| `S4End11` | Buscamos un camino a los Andes, ¡queremos encontrar a la Gran Serpiente! | Estamos procurando um caminho para os Andes, queremos encontrar a Grande Serpe! | Naru |
| `S4End12` | ¡AH! ¡YO SÉ DÓNDE QUEDA! ¡VAMOS! ¡LOS LLEVO HASTA ALLÁ! | AH! EU SEI ONDE FICA! VAMOS! VOU LEVAR VOCÊS ATÉ LÁ! | Ukhumo |


---

## Fase 5: Kuychu, o guardião de duas cabeças

### Falas: Abertura da fase (antes dos inimigos)



| Chave | Texto (ES) | Referência (PT) | Quem fala |
| --- | --- | --- | --- |
| `S5Intro01` | ¡Para llegar a los Andes, deben subir por la cascada inversa! | Para chegar aos Andes, vocês devem subir pela cachoeira reversa! | Ukhumo |
| `S5Intro02` | Es una cascada que sube en vez de bajar. | Ela é uma cachoeira que sobe em vez de descer. | Ukhumo |
| `S5Intro03` | Ya veo... | Estou vendo... | Atli |
| `S5Intro04` | No me gustan las alturas... así que me quedo aquí, ¡buena suerte! | Eu não curto lugares altos... então vou ficar por aqui, boa sorte! | Ukhumo |
| `S5Intro05` | ¡Gracias! | Obrigada! | Naru |
| `S5Intro06` | ¡Ah! ¡Y cuidado con los espíritus protectores! *Voz lejana* | Ah! E tomem cuidado com os espíritos protetores! *Voz distante* | Ukhumo |
| `S5Intro07` | Espera... ¿espíritus protectores? | Espera... espíritos protetores? | Atli |

### Falas: Encontro com o chefe (antes da luta)



| Chave | Texto (ES) | Referência (PT) | Quem fala |
| --- | --- | --- | --- |
| `S5Meet01` | ¿Quiénes son ustedes... | Quem são vocês... | Kuychu (esquerda) |
| `S5Meet02` | ...que osan pisar esta tierra sagrada? | Que ousam pisar nessa terra sagrada? | Kuychu (direita) |
| `S5Meet03` | ¡Quiero saber qué le pasa al cielo! ¿Por qué la noche no termina? | Eu quero saber o que está acontecendo com o céu! Por que a noite não tem fim?! | Atli |
| `S5Meet04` | ¡Más respeto! ¡Ella es la Gran Serpiente! | Olha os modos! Essa é a Grande Serpe! | Naru |
| `S5Meet05` | No es asunto tuyo, mortal, vete de estas tierras... | Não é da sua conta, mortal, vá embora dessas terras... | Kuychu (esquerda) |
| `S5Meet06` | O sufre las consecuencias... | Ou sofra as consequências... | Kuychu (direita) |
| `S5Meet07` | ¡No me iré de aquí sin respuestas! | Eu não vou sair daqui sem respostas! | Atli |
| `S5Meet08` | Entonces demuestra tu valor, ante la Gran Serpiente! | Então prove seu valor, perante a Grande Serpe! | Kuychu (esquerda) |
| `S5Meet09` | La guardiana de los espíritus... | O guardião dos espíritos... | Kuychu (direita) |

### Falas: Final da fase (depois da luta), caminho normal e difícil



| Chave | Texto (ES) | Referência (PT) | Quem fala |
| --- | --- | --- | --- |
| `S5End01` | Has demostrado tu valor, joven guerrero... | Você provou o seu valor, jovem guerreiro... | Kuychu |
| `S5End02` | ¿Me darán respuestas ahora? | Vão me dar respostas agora? | Atli |
| `S5End03` | Hmm... quizá ustedes puedan convencerla de detener esto. | Hmm... talvez vocês consigam convencê-la a parar com isso. | Kuychu |
| `S5End04` | ¿Convencerla? ¿De qué hablan? | Convencê-la? Do que estão falando? | Atli |
| `S5End05` | No puedo intervenir en los asuntos de los divinos... pero un mortal sí podría. | Eu não posso interferir nas atividades dos divinos... mas um mortal poderia. | Kuychu |
| `S5End06` | ¡Y esa diosa malcriada! Ha causado muchos problemas... | E aquela deusa mimada! Tem causado muitos problemas... | Kuychu |
| `S5End07` | Lo sabía, así que esto es culpa de una divinidad... | Sabia, então isso é culpa de uma divindade... | Naru |
| `S5End08` | Muy bien, está decidido, ¡los enviaré al reino de los espíritus! | Muito bem, está decidido, eu os mandarei ao reino dos espíritos! | Kuychu |
| `S5End09` | Si son fuertes, ¡demuestren su valor una vez más! | Se forem fortes, provem seu valor uma vez mais! | Kuychu |
| `S5End10` | ¿El reino de los espíritus?! | Reino dos espíritos?!! | Atli |
| `S5End11` | ¡Ve! ¡Trae el día de vuelta, niño! | Vá! Traga o dia novamente, criança! | Kuychu |
| `S5End12` | ¡OYE! ¡ESPERA!!!! | EI! ESPERA!!!! | Atli |
| `S5End13` | *Luz cegadora* | *Forte luz* | Kuychu |

### Falas: Final da fase (depois da luta), caminho fácil



| Chave | Texto (ES) | Referência (PT) | Quem fala |
| --- | --- | --- | --- |
| `S5Easy01` | Hmpf... no tienen valor. | Hmpf... vocês não têm valor. | Kuychu |
| `S5Easy02` | ¡Fuera de mi reino! | Saiam do meu reino! | Kuychu |


---

## Fase 6: Mama Killa, a Lua

### Falas: Abertura da fase (antes dos inimigos)



| Chave | Texto (ES) | Referência (PT) | Quem fala |
| --- | --- | --- | --- |
| `S6Intro01` | Esto es... ¿el cielo? ¿Estamos navegando bajo el cielo? | Isso é... o céu? Estamos velejando sob o céu? | Atli |
| `S6Intro02` | ¿Así que este es el reino de los espíritus? | Então esse é o reino dos espíritos? | Naru |
| `S6Intro03` | No sé... pero no te alejes. | Não sei... mas não saia de perto. | Atli |

### Falas: Chegada ao chefe (antes de ele aparecer)



| Chave | Texto (ES) | Referência (PT) | Quem fala |
| --- | --- | --- | --- |
| `S6Arr01` | Un lago... ¿y eso es la luna? ¡Qué grande! | Um lago... e aquilo é a lua?! Que grande! | Naru |
| `S6Arr02` | Tengo un mal presentimiento... | Estou com um pressentimento ruim... | Atli |
| `S6Arr03` | ¡Atli! ¡Viene alguien! | Atli! Tem alguém vindo! | Naru |

### Falas: Encontro com o chefe (antes da luta)



| Chave | Texto (ES) | Referência (PT) | Quem fala |
| --- | --- | --- | --- |
| `S6Meet01` | ¿Quiénes son ustedes? ¿Qué hacen en mis dominios? | Quem são vocês? O que fazem nos meus domínios? | Killa |
| `S6Meet02` | Ah... ustedes son el muchacho y el espíritu que molestaron a mi gata... | Ah... vocês são o garoto e o espírito que incomodaram minha gata... | Killa |
| `S6Meet03` | ¡Oye! ¿Eres tú quien causa todo esto? ¿Quién eres? | Ei! É você que está causando tudo isso? Quem é você? | Atli |
| `S6Meet04` | Qué muchacho tan descortés. Eso no es asunto tuyo... | Que garoto deselegante. Isso não é da sua conta... | Killa |
| `S6Meet05` | Ese aspecto... Atli, ¡ella es Killa... la diosa de la luna! | Essa aparência... Atli, ela é Killa... a deusa da lua! | Naru |
| `S6Meet06` | ¡No me importa si es una diosa o lo que sea, devuelve el día! | Não me interessa se é uma deusa ou o que for, traga o dia de volta! | Atli |
| `S6Meet07` | Fufu... ¿Por qué no vienes aquí a obligarme? | Fufu... Por que não vem até aqui me obrigar? | Killa |
| `S6Meet08` | ¡Atli, no! ¡Es muy fuerte! | Atli, não! Ela é muito forte! | Naru |
| `S6Meet09` | Ya lo creo que lo haré... | Pode apostar que vou... | Atli |

### Falas: Final da fase (depois da luta)



| Chave | Texto (ES) | Referência (PT) | Quem fala |
| --- | --- | --- | --- |
| `S6End01` | ¡Basta, deténganse! ¡La noche se queda! | Chega, podem parar! A noite fica! | Killa |
| `S6End02` | ¿Por qué quieres que sea siempre de noche? | Por que você quer que seja sempre noite? | Atli |
| `S6End03` | Yo... ¡¡¡odio el día!!! | Eu... odeio o dia!!! | Killa |
| `S6End04` | ¡A todos les gusta más el día que la noche! ¡La noche dura menos que el día! ¿Por qué? | Todos gostam mais do dia do que da noite! A noite dura menos que o dia! Por quê?! | Killa |
| `S6End05` | ¡No permitiré que la noche vuelva a ser menospreciada jamás! | Não vou permitir que a noite seja subestimada nunca mais! | Killa |
| `S6End06` | Pero... yo creo que la noche es hermosa... | Mas... eu acho a noite linda... | Atli |
| `S6End07` | ...Entonces, ¿por qué intentas detenerme? | ...Então por que está tentando me impedir? | Killa |
| `S6End08` | ¡Porque la noche solo es hermosa porque existe el día! | Porque a noite só é linda por existir o dia! | Atli |
| `S6End09` | ...¿Qué? | ...O quê? | Killa |
| `S6End10` | ¡Piensa! Si no existiera el día, la gente se cansaría de la noche... | Pense! Se não houver o dia, as pessoas vão se cansar da noite... | Atli |
| `S6End11` | ¡Justamente porque existe el día, a la gente le gusta la noche! | É justamente por existir o dia que as pessoas gostam da noite! | Atli |
| `S6End12` | El día es caluroso, ¡y no podemos admirar el cielo! Por eso me gusta la noche. | O dia é quente, e não podemos admirar o céu! Por isso eu gosto da noite. | Atli |
| `S6End13` | ... | ... | Killa |
| `S6End14` | ¡Y se duerme mejor después de un día agotador de trabajo! | E a gente dorme melhor depois de um dia cansativo de trabalho! | Naru |
| `S6End15` | *Pensativa* | *Pensativa* | Killa |
| `S6End16` | Está bien... me han convencido... | Certo... vocês me convenceram... | Killa |
| `S6End17` | Devolveré el día, ¡pero espero que sigan amando la noche! | Vou devolver o dia, mas espero que continuem amando a noite! | Killa |
| `S6End18` | Siempre... | Sempre... | Atli |
| `S6End19` | ¡Jamás la olviden! | Jamais se esqueçam dela! | Killa |
| `S6End20` | ¡Déjelo en nuestras manos, señora! | Pode deixar, senhora! | Naru |
