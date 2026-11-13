# Padrões de bullets dos bosses — inventário detalhado

Documento para planejar alterações e substituições. Descreve, **ataque por ataque**, tudo o que o código faz hoje (`Scripts/Content/Bosses/Attacks/*.cs`): como o boss se move, quais padrões disparam ao mesmo tempo, com que intervalo, que figura, que bala, que velocidade, quantas balas em cada dificuldade, o que o jogador enxerga e quais são os pontos de pressão. No fim há um inventário de repetições e das figuras da biblioteca que ainda não foram usadas.

Os números foram lidos do código e (onde indicado) medidos com o harness headless (`/tmp/sim sweep`, bot ideal, dificuldade Normal, semente 7). **Nada aqui foi visto rodando no Godot.**

---

## 1. Como ler este documento

### 1.1 Um ataque é uma lista de padrões

Um ataque (`BossPhase`) é um conjunto de **padrões** que disparam ao mesmo tempo, cada um com seu relógio:

| Elemento | Significado |
|---|---|
| `Pat.Every(t, figura)` | a figura é disparada de `t` em `t` segundos |
| `.After(s)` | o primeiro disparo só acontece `s` segundos depois do início do ataque (defasa os padrões entre si) |
| `.Twin()` | o padrão sai do **segundo corpo** do boss (só Kuychu, que é duas pessoas) |
| `.OnQuake()` | só dispara enquanto a tela treme |
| `.NotOnEasy()` / `.OnlyOnHard()` | só vale a partir de Normal / só em Difícil |
| `.WithTheJet(atraso)` | só dispara quando o jato do Baylo é solto |
| `Pressured(p)` | peso do ataque: as contagens escaláveis de balas são multiplicadas por √p |
| `Faster(f)` | as balas do ataque voam `f` vezes mais rápido |
| `c.Count` | quantas vezes aquele padrão já disparou (gira a figura a cada volley) |
| `c.Aim` | ângulo da origem até o barco |

Contagens de balas (`c.N`): `n_efetivo = round(n × fator_dificuldade)` e depois `× √pressão`, com um mínimo. **Fator por dificuldade: Fácil 1,0 · Normal 1,35 · Difícil 1,7.** Ring, Arc, Shoot, Aimed, Rain, StopAndAimRing, PinwheelRing e PolarRing escalam; Wall (fileiras), Laser, Comb, Crescent, Line, Constellation e laços escritos à mão **não** escalam (a menos que o código chame `c.N`). Lasers duram menos tempo de aviso nas dificuldades altas (aviso × 1,0 / 0,9 / 0,8).

Vento: empurra as balas e o barco (fator 1,2 / 1,4 / 1,6); balas marcadas `Steady()` ignoram o vento.

### 1.2 Figuras (`Pat.*`)

| Figura | O que desenha |
|---|---|
| `Ring(n, start)` | `n` balas igualmente espaçadas num círculo, a primeira em `start`° (o `Count` gira o anel a cada volley) |
| `Arc / Aimed(n, spread)` | leque de `n` balas em `spread`° (Aimed = centrado no barco) |
| `Shoot(modo, count, layers, ang1, ang2, v1, v2)` | volley em camadas ao estilo dos scripts de Touhou 6: `FanAimed` (leque no barco), `CircleAimed`/`Circle` (anel, camadas giradas), velocidades de `v1` a `v2` por camada |
| `Wall(edge, slots, gap, largura, shot, mirror)` | cortina de `slots` balas numa borda, com uma brecha (ou duas, espelhadas) que se move com o tempo |
| `Rain(edge, n, spread)` | `n` balas em pontos aleatórios de uma borda, rejeitando as coladas (regra dos espaços) |
| `Comb` | fileira reta de balas, todas na mesma direção (parede inclinada com buracos) |
| `Crescent` | crescente que voa inteiro como uma figura só |
| `Line` | balas ao longo de um segmento, saindo no mesmo ângulo |
| `Constellation(n, skip)` | estrelas jogadas para fora, que freiam, ficam penduradas ligadas por fios finos que machucam, e depois caem |
| `PolarRing(n, ...)` | galáxia: balas em trilhos polares que giram (`omega` °/s) enquanto se afastam (`radialSpeed`) — volleys com `theta0` crescente desenham braços de espiral |
| `Laser(heading, largura, aviso, golpe, spin, follow, blink)` | raio: linha fina de aviso, depois o raio; pode girar em torno da origem, seguir o boss e "piscar" (transparente/inofensivo nos últimos 0,8 s de cada 2 s) |
| `StopAndAimRing` (helper) | anel que explode (`Popping`), freia até parar e depois **cada bala voa no barco** (a posição do barco é lida na hora de partir) |
| `PinwheelRing` (helper) | anel que freia e vira `turn`° e parte de novo: vira cata-vento (2 viradas em sentidos opostos = flor) |
| `AdvancingPentagram` (helper) | pentagrama de estrelas que avança 2 s como uma figura rígida e depois "estoura" (linhas saem do centro; cada ponta vira anel de estrelinhas) |
| **ainda não usadas** | `Layers`, `Spokes` (moinho), `Rose` (flor), `Polygon`, `Star` (polígono estrelado), `Lattice` (malha), `Along` (fileira em qualquer borda) — ver §9 |

### 1.3 Modificadores de bala (`Shot`)

`Sized(x)` tamanho · `Zoomed(x)` aumenta a figura · `Hitting(r)` raio de hitbox próprio · `Steady()` imune ao vento · `Curving(°/s)` curva · `EasingOutAfter(s)` a curva acaba depois de `s` s · `Dancing(curva)` balança no ritmo (meio segundo) · `Zigzagging(curva)` zigue-zague · `Popping(extra, s)` explode saindo rápido e freia · `Turning(modo, intervalo, vezes, rotação, velocidade)` para e vira (modo 3 = mira o barco) · `Splitting(fuse, n, estilo, vel)` quebra em `n` filhas no ar · `Scattering` estoura a partir de um centro · `Bouncing(n)` quica nas bordas · `Heavy()` não perde velocidade · `Constellation(freio)` estrela de constelação · `Pulled` sofre gravidade.

### 1.4 Regra dos espaços

Entre as balas de um padrão sempre deve haver vão pro barco (hitbox 8 px): mínimo 16 px, confortável 24 px; centros a ≥ 30 px. Anéis/leques precisam abrir (≥ 16° entre vizinhos), camadas com velocidades bem distintas, galáxias com `radialSpeed × intervalo ≥ 30 px`, chuva rejeita gotas coladas. Lasers longos usam `blink: 2`.

### 1.5 Tempo e pontuação de cada ataque

* **Limite de tempo** (arma reta; a teleguiada tem ×1,5): `5 × ⌈3 × HP / (29 se parado, senão 20) / 5⌉`, entre 30 e 65, **+5 s**.
* **Bônus:** `base × (1 + segundos_restantes / 10)`; zera se o jogador morrer, for atingido ou usar bomba no ataque, e vale 0 se o tempo acabar. Bases por boss: 7.000 · 7.500 · 9.000 · 10.500 · 11.500 · 13.000.
* Em cada ataque abaixo: **HP** = vida do ataque, **mata** = tempo que o bot ideal levou (Normal), **pico** = máximo de balas ativas na tela medido com o bot.

---

## 2. Visão geral — 40 ataques

| # | Boss (fase) | Forma calma | Forma furiosa | Hitbox | Vida base | Bônus base |
|---|---|---|---|---|---|---|
| 1 | **Iryssh** — mulher de fogo, rio | 2 | 2 | 32 | 300 (+10 % furiosa) | 7.000 |
| 2 | **Baylo** — golfinho dançarino | 3 | 2 | 26 | 330 (+15 %) | 7.500 |
| 3 | **Thaqai** — gata das estrelas, raios | 4 | 2 | 32 | 360 (+20 %) | 9.000 |
| 4 | **Ukhumo** — ursa da caverna | 4 | 3 | 32 | 390 (+25 %) | 10.500 |
| 5 | **Kuychu** — guardião das duas serpentes | 5 | 3 | 44 | 420 (+30 %) | 11.500 |
| 6 | **Killa** — Mama Killa, a Lua | 6 | 4 | 32 | 450 (+35 %) | 13.000 |

Todos os bosses têm `speed = 1` (nenhum multiplica a velocidade das balas, exceto o ataque `Guardian`, ×1,5). A vida efetiva de um ataque = vida escrita × 0,48 × (2 se parado; 0,7 se varredura; 1 nos demais).

Resumo dos 40 ataques (HP / mata / pico em balas):

| Boss | # | Ataque | Movimento | HP | mata | pico | limite |
|---|---|---|---|---|---|---|---|
| Iryssh | 1 | Anel de Brasas | Patrol | 144 | 7 s | 102 | 35 s |
| | 2 | Flor Flamejante | Patrol | 144 | 7 s | 71 | 35 s |
| | 3 | Fúria do Rio Incandescente | Parado | 316 | 11 s | 86 | 40 s |
| | 4 | Investida da Grande Serpente | Dash | 158 | 6 s | 58 | 35 s |
| Baylo | 1 | Passo Duplo | Roam | 158 | 7 s | 187 | 35 s |
| | 2 | Chuva de Notas | Roam | 158 | 7 s | 88 | 35 s |
| | 3 | Roda de Pétalas | Roam | 158 | 7 s | 68 | 35 s |
| | 4 | Grande Mergulho | Dive | 182 | 6 s | 134 | 35 s |
| | 5 | Disparo do Rio Profundo | Sweep | 127 | 11 s | 48 | 35 s |
| Thaqai | 1 | Cortina Branca | Parado | 345 | 12 s | 206 | 45 s |
| | 2 | Pentagrama | Roam | 172 | 7 s | 67 | 35 s |
| | 3 | Zigue-zague | Cross | 172 | 7 s | 54 | 35 s |
| | 4 | Brincadeira da Gata | Shuttle | 172 | 7 s | 81 | 35 s |
| | 5 | O Céu Cai | Parado | 414 | 14 s | 144 | 50 s |
| | 6 | A Lua Coberta (Eclipse) | Parado | 414 | 14 s | 191 | 50 s |
| Ukhumo | 1 | Garras: Três Cortes | Parado | 374 | 13 s | 293 | 45 s |
| | 2 | Rugido | Parado | 374 | 13 s | 130 | 45 s |
| | 3 | Pisada | Parado | 374 | 13 s | 76 | 45 s |
| | 4 | Força Bruta | Parado | 374 | 13 s | 119 | 45 s |
| | 5 | Garras Cruzadas | Parado | 468 | 16 s | 168 | 55 s |
| | 6 | Desabamento Feroz | Parado | 468 | 16 s | 298 | 55 s |
| | 7 | Grande Banquete | Parado | 468 | 16 s | 177 | 55 s |
| Kuychu | 1 | Duas Cabeças | Parado (dança) | 403 | 16 s | 290 | 50 s |
| | 2 | Sete Arcos | Parado (dança) | 403 | 16 s | 240 | 50 s |
| | 3 | A Fronteira Treme | Parado (dança) | 403 | 14 s | 281 | 50 s |
| | 4 | Maré: Muralhas d'Água | Parado | 403 | 14 s | 173 | 50 s |
| | 5 | Prova de Valor | Parado (dança) | 403 | 16 s | 235 | 50 s |
| | 6 | Arco-Íris Partido | Parado, fundido | 524 | 18 s | 264 | 60 s |
| | 7 | Dois Mundos | Parado, fundido | 524 | 18 s | 175 | 60 s |
| | 8 | Guardião dos Dois Reinos | Parado, fundido | 524 | 18 s | 176 | 60 s |
| Killa | 1 | Lua Nova | Roam | 216 | 11 s | 312 | 40 s |
| | 2 | Lua Crescente | Roam | 216 | 9 s | 76 | 40 s |
| | 3 | Maré Lunar | Parado | 432 | 15 s | 206 | 50 s |
| | 4 | Lua Cheia | Roam | 216 | 11 s | 201 | 40 s |
| | 5 | Constelações | Roam + gravidade | 216 | 10 s | 100 | 40 s |
| | 6 | Chuva de Prata | Parado | 432 | 15 s | 120 | 50 s |
| | 7 | Lua Minguante | Parado | 583 | 20 s | 226 | 70 s |
| | 8 | Eclipse Total | Parado | 583 | 20 s | 304 | 70 s |
| | 9 | Pentagrama Lunar | Roam | 291 | 11 s | 183 | 50 s |
| | 10 | Noite Eterna | Parado | 583 | 20 s | 320 | 70 s |

(Movimentos: **Patrol** vai de estação em estação; **Roam** passeia por todo o campo; **Dive** mergulha e emerge sobre uma sombra; **Sweep** varre o campo carregando o jato; **Cross** cruza em diagonais; **Shuttle** alterna entre dois cantos descansando ~10 s em cada; **Dash** desce em investida por uma de três pistas.)

---

## 3. Boss 1 — Iryssh (fogo e serpente)

Cores: vermelho, laranja e amarelo; balas `Fireball`, `FireSerpent`, `LotusPetal`, `MagmaOrb`, `Ember` e orbes vermelho/laranja/amarelo (`FireOrbs`). Todos os ataques têm `Pressured` baixo (0,5–1,4): é o boss de entrada.

### 3.1 Anel de Brasas — *calmo, Patrol, HP 144, pico 102, pressão 0,51*
**O jogador vê:** três braços de fogo giram em espiral enquanto ela anda de estação em estação; de tempos em tempos um anel lento de orbes explode, para e vem todo junto no barco.

| Padrão | Intervalo | Atraso | Figura | Bala | Vel. | n (F/N/D) |
|---|---|---|---|---|---|---|
| Espiral | 0,3 s | — | `Ring` de 3 balas, início = `Count × 8,5°` (gira 8,5° por volley) | `Fireball` ×0,9 | 135 | 2/3/4 |
| Anel parar-e-mirar | 3,4 s | 1,2 s | `StopAndAimRing`, 12 orbes (vermelho, laranja, amarelo em rotação), início `Count × 15°` | orbes ×0,95; explode (`Popping`), freia em 0,9 s, depois parte a 165 | 150 → 165 | 9/11/14 |

Ponto de atenção: a espiral é contínua (3,3 balas/s por braço); o anel que mira é o único elemento "leitura de barco".

### 3.2 Flor Flamejante — *calmo, Patrol, HP 144, pico 71, pressão 1,37*
**Vê:** um lótus: pétalas explodem em anel, freiam e disparam no barco; entre os anéis, um leque em camadas de brasas.

| Padrão | Int. | Atraso | Figura | Bala | Vel. | n (F/N/D) |
|---|---|---|---|---|---|---|
| Lótus | 2,6 s | — | `Shoot` Circle, 14 balas, 1 camada, início `Count × 13°`, vel. 250→250 | `LotusPetal` ×1,1; `Popping`, `Turning(3, 0,9 s, ... , 190)` | 250 → 190 | 16/22/28 |
| Leque de brasas | 2,6 s | 1,3 s | `Shoot` FanAimed, 3 por camada × 2 camadas, 20° entre vizinhos, vel. 220→150 | `Fireball` ×0,9, `Popping` | 220 / 150 | 4/5/6 por camada |

### 3.3 Fúria do Rio Incandescente — *furioso, PARADO em (300,150), HP 316, pico 86, pressão 1,2*
**Vê:** orbes de magma enormes em leque no barco; e uma espiral fina de serpentes de fogo que se curvam.

| Padrão | Int. | Figura | Bala | Vel. | n (F/N/D) |
|---|---|---|---|---|---|
| Magma | 2,4 s | `Arc` de 3 balas, centrado no barco, 40° | `MagmaOrb` ×2,2, hitbox 16 | 190 | 3/4/5 |
| Serpentes | 0,34 s | `Ring` de 4 balas, início `Count × 11°` | `FireSerpent` ×0,9; curva ±30°/s (alterna por volley), acaba após 1,5 s | 140 | 4/5/8 |

### 3.4 Investida da Grande Serpente — *furioso, Dash, HP 158, pico 58, pressão 0,5*
**Vê:** a serpente desce a toda a velocidade por uma de três pistas (esquerda/centro/direita), reaparece no topo em outra pista, deixa um rastro de brasas e, de tempos em tempos, um anel que para e mira. *Mecânica própria:* espera 3,6 s se aproximando, investe a 2,5× a velocidade do boss.

| Elemento | Int. | Atraso | Figura | Bala | n (F/N/D) |
|---|---|---|---|---|---|
| Rastro (emissor antigo) | 0,52 s | — | 8 balas em 360° girando (`SpinRate 15`) | `Ember` ×0,5, vel. 100 | ×0,6 / ×1 / ×1,35 (as brasas descem um degrau de dificuldade) |
| Anel | 3,6 s | 1,5 s | `StopAndAimRing` em ponto aleatório (x = meio ± 120, y = 150), 10 orbes, início `Count × 17°` | orbes, vel. 40, freia em 0,9 s, parte a 160 | 7/10/12 |

---

## 4. Boss 2 — Baylo (a festa)

Cores: verde, amarelo, rosa, ciano. Balas `VenomOrb`, **notas musicais** (`NoteGold/Pink/Cyan`), `LotusPetal`, contas coloridas, `GiantBlue/White`. O ritmo é o **beat de 0,5 s**: muitas balas `Dancing` balançam no beat. Roam em todo o campo.

### 4.1 Passo Duplo — *calmo, Roam, HP 158, pico 187 (o mais denso do boss), pressão 1,0*
**Vê:** dois anéis de orbes giram em sentidos opostos com vizinhas balançando uma contra a outra; no meio, leque em camadas de notas no barco.

| Padrão | Int. | Atraso | Figura | Bala | Vel. | n (F/N/D) |
|---|---|---|---|---|---|---|
| Anel dançante | 1,0 s (2 beats) | — | `Ring` 10 balas (alternate), início `Count×14°` (sentido troca a cada volley) | `VenomOrb` ×0,9, `Dancing(100)` | 130 | 10/14/17 |
| Leque de notas | 2,5 s (5 beats) | 1,0 s | `Shoot` FanAimed 4×2 camadas, 17°, vel. 220→160 | nota (cicla dourado/rosa/ciano), `Popping` | 220 / 160 | 4/5/7 por camada |

### 4.2 Chuva de Notas — *calmo, Roam, HP 158, pico 88, pressão 1,24*
**Vê:** serpentinas (linhas de contas) jogadas em leque, giradas a cada vez; e notas caindo do topo balançando.

| Padrão | Int. | Figura | Bala | Vel. | n (F/N/D) |
|---|---|---|---|---|---|
| Serpentinas | 2,0 s (4 beats) | 6 `Line` de 3 contas e 72 px, rumos de 10° a 170° (gira 9° em volleys alternados) | contas coloridas ×0,9 | 140 | 7/9/11 linhas |
| Notas | 0,5 s | `Rain` Top, 1 nota, espalha 16° | nota ×1,0, `Dancing(50)` | 115 | 1/1/2 |

### 4.3 Roda de Pétalas — *calmo, Roam, HP 158, pico 68, pressão 1,0*
**Vê:** uma galáxia de pétalas que gira, uma por beat, e anéis de notas que param e voam no barco.

| Padrão | Int. | Atraso | Figura | Bala | n (F/N/D) |
|---|---|---|---|---|---|
| Galáxia | 1,0 s | — | `PolarRing` 7 braços, raio inicial 12, v. radial 42, ω = ±24°/s (muda de sentido a cada 4 volleys) | `LotusPetal` ×1,0 | 7/9/12 |
| Anel de notas | 3,4 s | 1,2 s | `StopAndAimRing` 10, freia em 0,9 s, parte a 170 | notas | 10/14/17 |

### 4.4 Grande Mergulho — *furioso, Dive, HP 182, pico 134, pressão 2,51*
**Vê:** o golfinho mergulha; uma **sombra** no chão segue o barco e fica parada; ele emerge sobre a sombra. Enquanto emerge e na superfície: anéis de bolhas balançando.

| Padrão | Int. | Atraso | Figura | Bala | n (F/N/D) |
|---|---|---|---|---|---|
| Bolhas | 1,0 s | — | `Ring` 9 (alternate), `Count×13°` | `VenomOrb` ×1,0, `Dancing(90)`, vel. 120 | 14/19/24 |
| Leque de notas (**só Difícil**) | 2,4 s | 1,0 s | `FanAimed` 3×2, 18° | `NoteCyan` `Popping` | 5/6/8 |

### 4.5 Disparo do Rio Profundo — *furioso, Sweep, HP 127, pico 48, pressão 1,0*
**Vê:** o jato d'água. Ele vai à borda esquerda, carrega, varre o jato até a direita (parando antes da borda para o barco ter onde ficar), cruza, carrega, varre de volta, volta ao meio e descansa alguns segundos. **Não há balas pequenas.** A cada vez que o jato é solto sai um anel de esferas gigantes.

| Padrão | Disparo | Figura | Bala | Vel. |
|---|---|---|---|---|
| Esferas | quando o jato é solto | 6 esferas gigantes (azul/branco alternados), início 0°+15° | `GiantBlue/White`, `Zoomed(1,5)`, hitbox 54, `Steady` | 105 |
| Esferas (2º anel) | 0,9 s depois, mesmo gatilho | os mesmos 6, defasados 30° | idem | 105 |

---

## 5. Boss 3 — Thaqai (tempestade e céu noturno)

Cores: branco/azul, amarelo-raio, violeta, estrelas. Balas `Hail` (granizo grande), `Spark`, `Bolt*` (raios), `Star*`, `OrbViolet/Pink`, `Claw`. Lasers de relâmpago.

### 5.1 Cortina Branca — *calmo, PARADO (300,140), HP 345, pico 206, pressão 1,0*
**Vê:** fileiras lentas de granizo caem atravessando todo o campo com uma **brecha larga que oscila**; entre as fileiras, um leque de faíscas que zigue-zagueiam até o barco.

| Padrão | Int. | Atraso | Figura | Bala | Vel. | Notas |
|---|---|---|---|---|---|---|
| Cortina | 1,3 s | — | `Wall` Top, 24 casas, brecha centrada em `0,5 + 0,13·sen(t/8 s)`, largura 170 px | `Hail` ×1,9 (hitbox grande p/ não passar entre duas) | 100 | brecha varre ±13 % do campo |
| Faíscas | 2,4 s | 0,5 s | `Aimed` 3 balas, 26° | `Spark` ×0,9 `Zigzagging(55)` | 165 | 3/4/5 |

### 5.2 Pentagrama — *calmo, Roam, HP 172, pico 67, pressão 2,5*
**Vê:** pentagramas enormes de estrelas saem do boss e **avançam como figura rígida** por 2 s (o vento não os desmancha); depois estouram: as estrelas das linhas voam para fora do centro e cada ponta (estrela maior) quebra num anel de estrelinhas. Entre eles, constelações (Normal e Difícil).

| Padrão | Int. | Atraso | Figura | Bala | Vel. |
|---|---|---|---|---|---|
| Pentagrama | 4,6 s | — | `AdvancingPentagram`: raio 105, 3 estrelas por aresta (2 internas + a ponta), mira o barco (+28° em volleys alternados), gira 17°/volley | linhas: `Star`/`StarBlue`; pontas: `StarGold`/`StarViolet` ×1,5 hitbox 10, quebra em 5 `StarGold` a 140 | 85; estouro 150 |
| Constelação | 4,6 s | 2,3 s | `Constellation` 5 estrelas, pulando 2 (pentagrama), gira 23°/volley | `Star` v. 175, freio 250 | **Normal/Difícil** |

### 5.3 Zigue-zague — *calmo, Cross, HP 172, pico 54, pressão 2,5*
**Vê:** ela cruza o campo em diagonais; leque de raios (balas) que zigue-zagueiam; e **relâmpagos**: depois de uma linha de aviso, três raios caem do céu — um no barco e dois afastados.

| Padrão | Int. | Atraso | Figura | Bala/Laser |
|---|---|---|---|---|
| Leque | 1,8 s | — | `Shoot` FanAimed 3×2, 20°, v. 220→160 | `BoltYellow`/`BoltBlue` alternando, `Zigzagging(30–50)`, `Popping` — 5/6/8 por camada |
| Relâmpagos | 3,4 s | 1,0 s | 3 `Laser` verticais (±8°), x = barco + 2 sorteados a ≥ 170 px | largura 26, aviso 1,1 s, golpe 0,5 s, amarelo/azul em volleys alternados |

### 5.4 Brincadeira da Gata — *calmo, Shuttle, HP 172, pico 81, pressão 2,5*
**Vê:** ela alterna entre o canto superior esquerdo e o direito, descansando ~10 s em cada. Novelos que **quicam** nas paredes e marcas de garra em pentes que saem para todos os lados.

| Padrão | Int. | Atraso | Figura | Bala | Vel. | n (F/N/D) |
|---|---|---|---|---|---|---|
| Novelos | 1,8 s | — | `Aimed` 5 bolas, 100° de abertura | `OrbViolet`/`OrbPink` ×1,2, `Bouncing(3)` | 190 | 8/11/14 |
| Garras | 2,2 s | 0,9 s | 5 `Comb` de 4 balas (30 px entre dentes), saindo da origem a 34 px, em 5 direções (72° entre elas), gira 17°/volley | `Claw` | 170 | fixo (20 balas) |

### 5.5 O Céu Cai — *furioso, PARADO (300,Centre), HP 414, pico 144, pressão 1,0*
**Vê:** cortinas de granizo com **duas brechas espelhadas**, e dos lados muitos raios lentos.

| Padrão | Int. | Figura | Bala | Vel. |
|---|---|---|---|---|
| Cortina | 1,4 s | `Wall` Top 24, brecha `0,27 + 0,08·sen(t/7)` e espelho, largura 150, espelhada | `Hail` ×1,9 | 105 |
| Raios laterais | 1,3 s | `Rain` Left/Right alternando, 3 raios, espalha 34° | `BoltYellow`/`BoltBlue` | 75 (1/1/2 a 3/4/5) |

### 5.6 A Lua Coberta (Eclipse) — *furioso, PARADO (300,150), HP 414, pico 191, pressão 1,0*
**Vê:** a cortina com uma brecha oscilante, raios lentos dos dois lados e anéis de estrelas que param e voam no barco.

| Padrão | Int. | Atraso | Figura | Bala | Vel. |
|---|---|---|---|---|---|
| Cortina | 1,3 s | — | `Wall` 24 casas, brecha `0,5 + 0,13·sen(t/7)`, 170 px | `Hail` ×1,9 | 105 |
| Raios | 1,4 s | — | `Rain` Left/Right alternando, 2 raios, 36° | `BoltYellow`/`BoltRed` | 65 |
| Anel de estrelas | 3,6 s | 1,4 s | `StopAndAimRing` 12, `Count×17°` | `StarGold`/`StarBlue` ×0,8; freia em 0,8 s, parte a 150 | 130 → 150 |

> Nota: `FineWeave` (trama fina de granizo com emissor antigo) existe no código mas **nenhum boss usa**.

---

## 6. Boss 4 — Ukhumo (caverna, garras e força bruta)

Cores: vermelho (garras), verde musgo, pedra. Mecânicas **fora das balas**: cortes de garra com aviso (`SlashSpec`), rugido que empurra o barco (`RoarSpec`), terremoto com estalactites (`QuakeSpec`). Ela nunca sai da plataforma. Balas: `Claw`, `MossBoulder`, `GiantStalactite`, `Stalactite`, `Rock`, `OrbGreen/BeadGreen`, `Bite`.

### 6.1 Garras: Três Cortes — *calmo, PARADO, HP 374, pico 293, pressão 1,2*
**Vê:** três cortes vermelhos cruzam o campo na direção do barco (aviso fino 0,95 s, golpe 0,22 s, largura 34, 155 px entre eles, a cada 2,6 s); ao redor dela, anéis de marcas de garra girando e alternando sentido.

| Padrão | Int. | Figura | Bala | Vel. | n (F/N/D) |
|---|---|---|---|---|---|
| Anéis de garra | 0,9 s | `Ring` 14, início `Count×9°` com sinal alternado | `Claw` ×1,0 | 140 (volleys pares) / 105 (ímpares) | 15/21/26 |

### 6.2 Rugido — *calmo, PARADO, HP 374, pico 130, pressão 1,8*
**Vê:** a cada 3,6 s ela ruge e o barco é **empurrado** (240 px/s por 0,55 s) para dentro dos anéis; de vez em quando, três pedras gigantes cobertas de musgo.

| Padrão | Int. | Atraso | Figura | Bala | Vel. |
|---|---|---|---|---|---|
| Anéis | 1,6 s | — | `Ring` 16, início `Count×11°` | `Claw`, `Popping(160, 0,2 s)` | 120 — 21/30/36 |
| Pedras | 4,2 s | 1,0 s | 3 balas em -42°, 0°, +42° do barco | `MossBoulder` hitbox 100, `Steady`, `Heavy` | 95 (320+ px de separação ao alcançar o barco) |

### 6.3 Pisada — *calmo, PARADO, HP 374, pico 76, pressão 2,0*
**Vê:** a tela treme (terremoto a cada 3,2 s, dura 3,4 s) e **estalactites gigantes** (64×160 px) caem do teto; fora disso, uma bala verde grande que mira o barco e se parte no ar em um anel de miúdas.

| Padrão | Int. | Figura | Bala | Vel. |
|---|---|---|---|---|
| Estalactites | 0,42 s (só no terremoto) | `Rain` Top 2, espalha 4° | `GiantStalactite`, hitbox 26 | 170 |
| Bala que racha | 1,6 s | 1 bala no barco ±14° | `OrbGreen` ×2,2, hitbox 18; após 0,9 s quebra em 8 `BeadGreen` a 120 | 160 — filhas: 11/16/20 |

### 6.4 Força Bruta — *calmo, PARADO, HP 374, pico 119, pressão 2,0*
**Vê:** um único corte pesado (aviso 0,9 s, golpe 0,25 s, largura 46) vindo de qualquer direção a cada 2,8 s; a linha cortada solta um rastro de balas vermelhas; longe dos cortes, anel lento de garras que para e mira. **Sem pedras.**

| Padrão | Int. | Atraso | Figura | Bala | Vel. | n |
|---|---|---|---|---|---|---|
| Anel | 3,6 s | 1,4 s | `StopAndAimRing` 10, `Count×17°` | `Claw`, freia 0,9 s, parte a 150 | 130 → 150 | 12/17/21 |

### 6.5 Garras Cruzadas — *furioso, PARADO, HP 468, pico 168, pressão 1,8*
**Vê:** cortes triplos (3 paralelos, 150 px entre eles, ângulo 30°, de qualquer direção, a cada 2,4 s) sobre leques em camadas e anéis.

| Padrão | Int. | Atraso | Figura | Bala | n |
|---|---|---|---|---|---|
| Leque | 2,0 s | — | `FanAimed` 4×2, 17°, v. 230→165 | `Claw` `Popping` | 5/7/9 |
| Anel | 1,3 s | 0,6 s | `Ring` 14 `Count×8°` alternando | `Claw` ×1,0, v. 115 | 19/25/32 |

### 6.6 Desabamento Feroz — *furioso, PARADO, HP 468, pico 298, pressão 3,0*
**Vê:** a tela treme quase sem parar (terremoto a cada 1,0 s, dura 4,4 s, intensidade 1,4), estalactites caem em multidão, pedras grandes caem e se partem em anéis, chuva de pedrinhas; de vez em quando um rugido empurra o barco.

| Padrão | Int. | Figura | Bala | Vel. |
|---|---|---|---|---|
| Estalactites | 0,16 s (terremoto) | `Rain` Top 1, 4° | `Stalactite` ×1,3 | 300 |
| Pedras que racham | 0,9 s (terremoto) | `Rain` Top 1 | `Rock` ×2,0 hitbox 16; após 0,9 s quebra em `N(9,6)` `Rock` a 115 | 180 — 16/21/26 filhas |
| Pedrinhas | 0,28 s (terremoto) | `Rain` Top 2, 10° | `Rock` ×1,0 | 220 |

Rugido: a cada 5,0 s, empurra 210 por 0,5 s.

### 6.7 Grande Banquete de Garras — *furioso, PARADO, HP 468, pico 177, pressão 2,0*
**Vê:** tudo junto — mordidas enormes no barco, anéis de garras, leque de garras, **quatro cortes inclinados** (2,3 s), terremotos curtos (a cada 4,5 s, dura 2,2 s), um rugido (a cada 7 s). Sem pedras.

| Padrão | Int. | Atraso | Figura | Bala | n |
|---|---|---|---|---|---|
| Mordidas | 2,4 s | 0,6 s | `Arc` 3 balas, 36°, centro no barco | `Bite` hitbox 30 | 4/6/7 |
| Anéis | 1,4 s | — | `Ring` 14 `Count×9°` alternando, v. 115 | `Claw` | 20/27/34 |
| Leque | 2,8 s | 1,6 s | `FanAimed` 4×2, 17°, v. 225→160 | `Claw` `Popping` | 5/7/9 |

---

## 7. Boss 5 — Kuychu (as duas serpentes, o arco-íris)

Kuychu é **duas pessoas que são um ser** (corpo esquerdo = boss; corpo direito = gêmeo). Na primeira metade (ataques 1–5) cada um atira de onde está e **se revezam passeando pelo campo** (`dance`); a partir do ataque 6 os dois **se fundem** e atiram das duas metades de um corpo parado (`LeftHalf`/`RightHalf`, ±34 px). Cores: o arco-íris (7 cores) — orbes, contas, kunai, e lasers coloridos. Usa `StopAndAimRing` e `PinwheelRing` à exaustão.

### 7.1 Duas Cabeças — *calmo, dança, HP 403, pico 290, pressão 0,78*
**Vê:** cada cabeça com suas cores (esq.: vermelho/laranja/amarelo; dir.: verde/ciano/azul): anéis que explodem, param e voam no barco, e leques em camadas de arroz; revezam-se a cada 0,8 s.

| Padrão | Corpo | Int. | Atraso | Figura | Bala | n (F/N/D) |
|---|---|---|---|---|---|---|
| Anel quente | esq. | 1,6 s | — | `StopAndAimRing` 14, `Count×11°`, v. 170, freia 0,9 s, parte a 185 | `Orb` R/O/Y | 12/17/21 |
| Anel frio | dir. | 1,6 s | 0,8 s | idem, `Count×-11°` | `Orb` G/C/B | 12/17/21 |
| Leque amarelo | esq. | 1,6 s | 0,4 s | `FanAimed` 4×2, 17°, v. 240→170 | `RiceYellow` `Popping` | 4/4/6 |
| Leque violeta | dir. | 1,6 s | 1,2 s | idem | `RiceViolet` | 4/4/6 |

### 7.2 Arco-Íris: Sete Arcos — *calmo, dança, HP 403, pico 240, pressão 1,0*
**Vê:** de cada cabeça, sete correntes de bolas nas sete cores se curvam em grandes arcos (a curva dura 1,6 s e depois voam retas), girando em sentidos opostos; e anéis em camadas de kunai que estouram no barco.

| Padrão | Corpo | Int. | Atraso | Figura | Bala | n |
|---|---|---|---|---|---|---|
| Arcos | esq. | 0,5 s | — | `Ring` 7 cores, `Count×3,4°`, curva +42°/s por 1,6 s, `Popping` | `Orb` ×0,95, v. 150 | 7/9/12 |
| Arcos | dir. | 0,5 s | — | idem, `Count×-3,4°`, curva -42°/s | idem | idem |
| Kunai azul | esq. | 3,0 s | 0,5 s | `Shoot` CircleAimed 18×2 camadas, v. 200→140 | `KunaiBlue` `Popping` | 18/24/31 por camada |
| Kunai verde | dir. | 3,0 s | 2,0 s | idem, +10° | `KunaiGreen` | idem |

### 7.3 A Fronteira Treme — *calmo, dança, HP 403, pico 281, pressão 2,0*
**Vê:** terremoto (a cada 3,0 s, dura 3,6 s); gotas das cachoeiras caem em multidão; as duas cabeças revezam **leques de 5 lasers** no barco.

| Padrão | Int. | Atraso | Figura | Bala/Laser |
|---|---|---|---|---|
| Gotas | 0,09 s (terremoto) | — | `Rain` Top 3, 5° | `BeadBlue` ×0,9, v. 300 — 4/6/7 por tiro |
| Lasers esq. | 4,6 s | 0,8 s | 5 `Laser`, -48°…+48° em torno do barco (24° entre eles) | largura 32, aviso 1,1 s, golpe 0,55 s, cores do arco-íris |
| Lasers dir. | 4,6 s | 3,1 s | idem, deslocados +12° | idem (cores deslocadas) |

### 7.4 Maré: Muralhas d'Água — *calmo, PARADO, sem dança, HP 403, pico 173, pressão 4,0*
**Vê:** ondas de água cruzam o campo **na horizontal** (cortes largos: 2 por vez, 200 px entre eles, ângulo 84°, aviso 1,1 s, golpe 0,45 s, largura 44, a cada 2,7 s) e, do topo, **colunas de gotas** em pontos aleatórios (≥ 220 px entre colunas). Em Difícil, cata-ventos de 7 cores.

| Padrão | Int. | Atraso | Figura | Bala | Vel. |
|---|---|---|---|---|---|
| Colunas | 1,7 s | 0,6 s | 2 colunas de 6 bolas (38 px entre elas) caindo do topo | orbes arco-íris ×0,95, `Steady` | 150 / 165 / 185 |
| Cata-ventos | 3,4 s | 1,6 s | `PinwheelRing`, 8/8/12 bolas, `Count×8°`, v. 90, freia 0,8 s, vira ±60° | orbes arco-íris | **só Difícil** |

### 7.5 Prova de Valor — *calmo, dança, HP 403, pico 235, pressão 1,0*
**Vê:** os dois juntos, frente a frente: grandes anéis nas 7 cores que explodem e viram cata-vento (cada um para o seu lado, **duas viradas** = flor), mais leque de kunai no barco.

| Padrão | Corpo | Int. | Atraso | Figura | Bala | n |
|---|---|---|---|---|---|---|
| Cata-vento | esq. | 1,4 s | — | `PinwheelRing` 21, `Count×8°`, v. 150, freia 0,7 s, vira +90° ×2 | orbes arco-íris | 21/28/36 |
| Cata-vento | dir. | 1,4 s | 0,7 s | idem, `Count×-8°`, -90° | idem | idem |
| Kunai | esq. | 2,8 s | 1,4 s | `FanAimed` 5×2, 16°, v. 250→170 | `KunaiBlue` `Popping` | 5/7/9 |

### 7.6 Arco-Íris Partido — *furioso, FUNDIDO, HP 524, pico 264, pressão 0,6*
**Vê:** sete **lasers** (um de cada cor) giram devagar em torno do corpo como raios de uma roda (deixando brechas), "piscando" (inofensivos nos últimos 0,8 s de cada 2 s); uma galáxia de 7 cores gira de cada metade; cata-ventos.

| Padrão | Int. | Atraso | Figura | Bala/Laser |
|---|---|---|---|---|
| Lasers | 9,5 s | 0,8 s | 7 `Laser` a 360/7° um do outro, giram ±11°/s (sentido alterna), largura 30, aviso 1,6 s, golpe 6,4 s, `follow`, `blink 2` | cores do arco-íris |
| Galáxia esq. | 0,95 s | — | `PolarRing` 7, `Count×13°`, raio 12, v. radial 34, ω +20°/s | `Orb` ×0,95 — 5/7/9 |
| Galáxia dir. | 0,95 s | 0,48 s | idem, `Count×-13°`, ω -20°/s | idem |
| Cata-vento | 2,0 s | 0,6 s | `PinwheelRing` 24, `Count×7°`, v. 40, freia 0,8 s, vira ±75° | orbes arco-íris — 19/25/32 |

### 7.7 Dois Mundos: O Abalo — *furioso, FUNDIDO, HP 524, pico 175, pressão 2,5*
**Vê:** o corpo pisa, a tela treme sem pausa (terremoto a cada 1,0 s, dura 4,2 s, intensidade 1,4) e chovem gotas; **colunas de balas vêm do céu e da água** (alternando), param no meio e viram no barco; leques de 7 cores.

| Padrão | Int. | Atraso | Figura | Bala | Vel. |
|---|---|---|---|---|---|
| Gotas | 0,24 s (terremoto) | — | `Rain` Top 2, 5° | `BeadBlue` ×0,9 | 300 |
| Colunas | 3,8 s | 0,5 s | 8 balas em x uniforme (±14 px), de cima (-14) ou de baixo (fundo+14) alternando; param no meio (freio 1,1 s do céu / 1,3 s da água) e depois mirando o barco | orbes arco-íris ×0,95, `Steady` | céu: 2·230/1,1 ≈ 418 → 210; água: 2·330/1,3 ≈ 508 → 190 |
| Leque | 2,6 s | 1,2 s | `FanAimed` 5×2, 16°, v. 240→170 | `OrbRed` ×0,95 `Popping` | 5/7/9 |

### 7.8 Guardião dos Dois Reinos — *furioso, FUNDIDO, HP 524, pico 176, pressão 0,6, **×1,5 velocidade***
**Vê:** tudo do arco-íris junto, de cada metade. Lasers saem de cada metade para um lado e para o outro, cata-ventos e flores, galáxias, abalo curto (a cada 5 s, 2,2 s).

| Padrão | Int. | Atraso | Figura | Bala/Laser |
|---|---|---|---|---|
| Lasers | 7,2 s | 0,8 s | 5 `Laser` de cada metade (72° entre eles; esq. gira +12°/s, dir. -12°/s), aviso 1,2 s, golpe 4,4 s, `blink 2` | cores do arco-íris |
| Galáxia esq. | 1,0 s | — | `PolarRing` 7, `Count×11°`, v. radial 48, ω +27° | `Orb` ×0,95 |
| Galáxia dir. | 1,0 s | 0,5 s | idem, sentido oposto | idem |
| Cata-vento esq. | 3,2 s | 0,5 s | `PinwheelRing` 16, v. 57, freia 0,8 s, vira +80° ×2 | orbes arco-íris — 12/17/21 |
| Cata-vento dir. | 3,2 s | 2,1 s | idem, -80° | idem |

---

## 8. Boss 6 — Killa (as fases da Lua)

Cores da noite: prata, azul-pálido, violeta, ouro das estrelas. Ela fica **de pé na linha do horizonte** e nunca sobe: só vai para os lados e para baixo. Em quase todos os ataques há "textura": estrelas lentas que caem (`FallingStars`).

### 8.1 Lua Nova — *calmo, Roam, HP 216, pico 312 (o maior da forma calma), pressão 2,0*
**Vê:** não há lua; só estrelas lentas caindo (a cada 0,4 s, 1 estrela, 105 de velocidade) e anéis de prata que param e voam todos juntos no barco.

| Padrão | Int. | Figura | Bala | n |
|---|---|---|---|---|
| Estrelas caindo | 0,4 s | `Rain` Top 1, 8° | `StarBlue/Gold/Violet` ×0,95, v. 105 | 1/1/2 |
| Anel | 2,4 s | `StopAndAimRing` 16, `Count×9°`, v. 150, freia 0,9 s, parte a 165, alterna prata (branco/ciano) e violeta (violeta/azul) | orbes | 23/31/38 |

### 8.2 Lua Crescente — *calmo, Roam, HP 216, pico 76, pressão 1,6*
**Vê:** dois arcos de contas (chifres de prata) voam como figura rígida no barco, de lados alternados; leques de kunai em camadas.

| Padrão | Int. | Atraso | Figura | Bala | Vel. |
|---|---|---|---|---|---|
| Crescente | 1,8 s | — | `Crescent` raio 120, 6 contas por arco, rumo = barco ±14° | `BeadWhite` ×0,85 | 150 |
| Kunai | 2,6 s | 0,9 s | `FanAimed` 4×2, 17°, v. 230→165 | `KunaiBlue` `Popping` | 5/6/9 por camada |

### 8.3 Maré Lunar — *calmo, PARADO (300,140), HP 432, pico 206, pressão 1,0*
**Vê:** o mesmo princípio da Cortina Branca da Thaqai: fileiras de granizo com brecha larga oscilante (mais rápido: a cada 0,9 s), e contas ciano no barco.

| Padrão | Int. | Atraso | Figura | Bala | Vel. |
|---|---|---|---|---|---|
| Cortina | 0,9 s | — | `Wall` 24, brecha `0,5 + 0,13·sen(t/8)`, 170 px | `Hail` ×1,9 | 100 |
| Contas | 2,2 s | 0,5 s | `Aimed` 3, 24° | `BeadCyan` ×0,95 | 170 — 3/4/5 |

### 8.4 Lua Cheia — *calmo, Roam, HP 216, pico 201, pressão 3,0*
**Vê:** orbes de prata gigantes e lentos em anéis que giram; entre eles, anéis de raiozinhos de prata.

| Padrão | Int. | Atraso | Figura | Bala | Vel. | n |
|---|---|---|---|---|---|---|
| Gigantes | 3,4 s | — | `Ring` 6, `Count×19°`, **sem escala** (fixo) | `GiantWhite` | 85 | 6 |
| Raios | 1,8 s | 0,9 s | `Ring` 18, `Count×-7°` | `BeadWhite` ×0,9 | 125 | 31/42/54 |

### 8.5 Constelações — *calmo, Roam + **gravidade**, HP 216, pico 100, pressão 2,0*
**Vê:** estrelas voam, são ligadas por fios finos e ficam penduradas; uma **LUA** desce até o meio do campo (300,420) em 5 s e, a cada 5,5 s, incha, avisa por 1,2 s e **puxa o barco** (230 px/s por 2,6 s, raio 30) — o barco precisa fugir a toda velocidade; a lua machuca se tocada. Cata-ventos azul/violeta giram ao redor.

| Padrão | Int. | Atraso | Figura | Bala |
|---|---|---|---|---|
| Constelação | 4,6 s | — | `Constellation` 5 estrelas, pula 2, `Count×23°` | `StarBlue`/`StarGold` alternando, v. 175, freio 250 |
| Cata-vento | 3,0 s | 1,5 s | `PinwheelRing` 14, `Count×11°`, v. 130, freia 0,8 s, vira ±80° | `MoonOrbs` — 20/27/34 |

### 8.6 Chuva de Prata — *calmo, PARADO (300,140), HP 432, pico 120, pressão 3,0*
**Vê:** leques de 5 lasers de luz da lua no barco (que ficam transparentes a cada 2 s) e estrelas lentas caindo.

| Padrão | Int. | Atraso | Figura | Laser |
|---|---|---|---|---|
| Lasers | 6,4 s | 0,6 s | 5 `Laser`, ±52° em torno do barco (26° entre eles) | largura 30, aviso 1,2 s, golpe 3,4 s, `blink 2`, branco/azul/violeta |
| Estrelas | 0,3 s | — | `Rain` Top 1 | `Star*`, v. 105 — 1/1/2 |

### 8.7 Lua Minguante — *furioso, PARADO, HP 583, pico 226, pressão 3,0*
**Vê:** colunas de orbes entram pelos lados (esquerda/direita alternando), param num ponto aleatório do campo e **caem juntas como uma cortina**; cata-ventos prateados; estrelas.

| Padrão | Int. | Figura | Bala | Detalhe |
|---|---|---|---|---|
| Colunas | 2,2 s | 6 linhas (4 min.) nas alturas 230, 300, 370…, entrando por um lado e freando em `stopAt` (x aleatório entre 120 e 120 das bordas); intervalo ≥ 1,2 s, velocidade nunca > ~430 px/s ao começar; ao parar, giram 90° e partem a 150 | `MoonOrbs` ×0,95, `Steady`, `Turning(2, …)` | 10/14/17 linhas |
| Cata-vento | 2,8 s (atraso 1,0 s) | `PinwheelRing` 16, `Count×9°`, v. 120, freia 0,8 s, vira ±75° | prata (branco/ciano) — 28/38/47 |
| Estrelas | 0,5 s | `Rain` Top | `NightStars`, v. 105 |

### 8.8 Eclipse Total — *furioso, PARADO, HP 583, pico 304, pressão 2,6*
**Vê:** sete lasers de luz da lua giram devagar em torno dela como a coroa de um eclipse, piscando a cada 2 s; galáxia de prata e violeta; anéis que param e voam no barco.

| Padrão | Int. | Atraso | Figura | Bala/Laser |
|---|---|---|---|---|
| Lasers | 7,2 s | 0,8 s | 7 `Laser`, 360/7° entre eles, giram ±10°/s, largura 28, aviso 1,2 s, golpe 4,4 s, `follow`, `blink 2` | branco/azul/violeta |
| Galáxia | 1,0 s | — | `PolarRing` 6, `Count×12°`, raio 12, v. radial 42, ω +22°/s | `OrbWhite` ×0,95 + `MoonOrbs` — 10/13/16 |
| Anel | 3,2 s | 1,2 s | `StopAndAimRing` 12, `Count×15°`, v. 140, freia 0,8 s, parte a 160 | violeta (violeta/azul) — 19/26/32 |

### 8.9 Pentagrama Lunar — *furioso, Roam, HP 291, pico 183, pressão 3,0*
**Vê:** pentagramas de estrelas que avançam rígidos e estouram (como o da Thaqai, mas com 2 estrelas por aresta), constelações (Normal+) e uma galáxia lenta.

| Padrão | Int. | Atraso | Figura | Bala |
|---|---|---|---|---|
| Pentagrama | 4,6 s | — | `AdvancingPentagram` raio 110, mira o barco (+24° em alternados), `Count×17°`, 2 estrelas por aresta | linhas `StarBlue`/`StarViolet`, pontas `StarGold` |
| Constelação | 4,6 s | 1,9 s | `Constellation` 5, pula 2, `Count×29°` | `Star`, v. 175 — **Normal/Difícil** |
| Galáxia | 1,6 s | — | `PolarRing` 5, `Count×14°`, raio 12, v. radial 46, ω ±20°/s alternando a cada volley | `OrbCyan` ×0,95 + `MoonOrbs` — 9/12/16 |

### 8.10 Noite Eterna — *furioso, PARADO, HP 583, pico 320 (o mais denso de todos), pressão 1,1*
**Vê:** cortinas de granizo com **duas brechas espelhadas** que oscilam; uma galáxia gira em cada mão dela (sentidos opostos); cata-ventos do coração; estrelas lentas.

| Padrão | Int. | Atraso | Figura | Bala |
|---|---|---|---|---|
| Cortina | 1,0 s | — | `Wall` 24, brecha `0,27 + 0,08·sen(t/7)` e espelho, 150 px | `Hail` ×1,9, v. 105 |
| Galáxia mão esq. | 1,1 s | — | `PolarRing` 5 em `(-48,-34)` da origem, `Count×13°`, v. radial 40, ω +20°/s | `OrbWhite`/`SilverOrbs` — 5/7/9 |
| Galáxia mão dir. | 1,1 s | 0,55 s | idem, `(+48,-34)`, `Count×-13°`, ω -20°/s | `OrbViolet`/`VioletOrbs` |
| Cata-vento | 3,4 s | 1,3 s | `PinwheelRing` 16, `Count×8°`, v. 110, freia 0,8 s, vira ±80° | `MoonOrbs` — 17/23/28 |
| Estrelas | 0,6 s | — | `Rain` Top 1 | `NightStars` v. 95 |

---

## 9. Inventário e pontos para planejar substituições

### 9.1 Quanto cada figura aparece (40 ataques)

| Figura / ideia | Quantos ataques | Onde |
|---|---|---|
| **`StopAndAimRing`** (anel que explode, para e mira) | 8 | Iryssh 1 e 4; Baylo 3; Thaqai 6; Ukhumo 4; Kuychu 1; Killa 1 e 8 |
| **`PinwheelRing`** (cata-vento / flor) | 7 | Kuychu 4, 5, 6, 8; Killa 5, 7, 10 |
| **`Wall` + brecha oscilante** | 5 | Thaqai 1, 5, 6; Killa 3, 10 |
| **`PolarRing`** (galáxia) | 6 | Baylo 3; Kuychu 6, 8; Killa 8, 9, 10 |
| **Leque `FanAimed` em camadas (`Shoot`)** | 10 | Iryssh 2; Baylo 1, 4; Thaqai 3; Ukhumo 5, 7; Kuychu 1, 5, 7; Killa 2 |
| **`Laser`** | 6 | Thaqai 3; Kuychu 3, 6, 8; Killa 6, 8 |
| **Pentagrama avançando** | 2 | Thaqai 2 e Killa 9 (mesma função) |
| **`Constellation`** | 3 | Thaqai 2; Killa 5 e 9 |
| **`Rain` (textura e colunas)** | 8 | Baylo 2; Thaqai 5, 6; Ukhumo 3, 6; Kuychu 3, 7; (Killa usa `FallingStars`, que é `Rain`, em 1, 6, 7, 10) |
| **`Ring` simples** | 10 | Iryssh 1 e 3; Baylo 1 e 4; Ukhumo 1, 2, 5, 7; Killa 4; Kuychu 2 |
| Cortes de garra / ondas (`SlashSpec`) | 5 | Ukhumo 1, 4, 5, 7; Kuychu 4 |
| Terremoto (`QuakeSpec`) | 6 | Ukhumo 3, 6, 7; Kuychu 3, 7, 8 |
| Rugido (`RoarSpec`) | 3 | Ukhumo 2, 6, 7 |
| `Comb`, `Crescent`, `Line`, `Bouncing` | 1 cada | Thaqai 4; Killa 2; Baylo 2; Thaqai 4 |

### 9.2 Repetições que enfraquecem a identidade dos bosses

1. **`StopAndAimRing`** é o "golpe de mira" de 8 ataques em cinco dos seis bosses (mesmos parâmetros: freia ~0,9 s, parte a ~150–185). Candidato a ser **substituído por figuras próprias de cada boss** (ex.: mira-cortada, anéis que viram espirais, leque que fecha).
2. **Cortina com brecha oscilante** é usada em Thaqai (3×) e Killa (2×) quase idêntica (24 casas, granizo ×1,9, brecha 170 px). Pode ganhar variações: brecha inclinada (`tilt`), duas fileiras desencontradas, ondas lado-a-lado, `Along` das laterais.
3. **Pentagrama avançando** é o mesmo código em Thaqai 2 e Killa 9 (só muda o raio e as estrelas por aresta).
4. **Kuychu**: quase todos os ataques usam o mesmo trio "anel/cata-vento + leque de kunai + chuva".
5. **A forma furiosa de vários bosses** é a calma + camada extra (Ukhumo 5–7; Thaqai 5–6), pouca diferença de figura.
6. **Killa 1 e 10** têm picos altos (312 e 320 balas) e Iryssh/Baylo baixos (50–190): a curva de densidade não é monotônica dentro de cada boss.

### 9.3 Figuras prontas da biblioteca que NÃO são usadas por nenhum ataque

Boas candidatas para os novos padrões, sem precisar de código novo no motor:

* **`Spokes`** — moinhos: `spokes` retas de `perSpoke` balas em velocidades crescentes; giradas entre volleys formam braços de moinho.
* **`Rose`** — flor: o anel tem velocidades moduladas pelo ângulo, e depois de um momento desenha uma rosa de `petals` pétalas.
* **`Layers`** — anéis concêntricos: o mesmo anel em várias velocidades, mantendo as brechas.
* **`Polygon`** / **`Star`** (polígono estrelado {n/k}, ex.: 7/3 heptagrama) — figuras armadas que depois se soltam.
* **`Lattice`** — malha de balas descendo e cruzando, com brechas em x e y (padrão "grade").
* **`Along`** — fileira de balas ao longo de qualquer borda, com `tilt` e `ripple` (onda).
* **`Launched` / `LaunchedOut`** — balas que ficam paradas e depois disparam; **`Pulled`** (gravidade); **`Blinking`**; **`Accelerating`** (balas que aceleram) — pouco explorados.
* **`Bouncing`** — só a Thaqai 4 usa.
* Emissores simples (`BulletEmitterComponent`): só a Iryssh 4 ainda usa um.

### 9.4 Mecânicas especiais que podem virar o eixo de um ataque novo

`SlashSpec` (cortes com aviso, em qualquer ângulo), `QuakeSpec` (terremoto + chuva), `RoarSpec` (empurrão), `GravitySpec` (lua que puxa), `Dive` (sombra que marca onde emerge), `Sweep` (jato varrendo), `Dash` (investida por pistas), `Shuttle`, `Cross`, e a **fusão/dança dos dois corpos** de Kuychu (`.Twin()`, `dance`, `fused`).

### 9.5 Parâmetros a manter ao substituir um padrão

* **HP, limite de tempo e bônus** dependem só do movimento (parado vs. móvel) e da vida do boss, não dos padrões. Trocar padrões não muda a economia de pontos, **mas** mudar o movimento de um ataque muda HP e limite.
* **Pressão** (`Pressured`) é o botão que iguala a densidade entre bosses; ao trocar um ataque, reajuste-a com `sim sweep` até o pico de balas ficar na faixa do boss (Iryssh ≈ 60–100; Baylo ≈ 50–190; Thaqai ≈ 55–205; Ukhumo ≈ 75–300; Kuychu ≈ 175–290; Killa ≈ 75–320).
* A **regra dos espaços** (§1.4) e os testes `SelfTest.Patterns` (contagem de ataques por boss 2+2, 3+2, 4+2, 4+3, 5+3, 6+4; cada boss enche mais a tela que o anterior; figuras mantêm a forma) precisam continuar passando.
* Os textos dos nomes dos ataques estão nas tabelas de `Loc` (PT/EN/ES) com a chave `Attack…`.

### 9.6 Perguntas para guiar o planejamento

1. Cada boss deveria ter **uma figura-assinatura exclusiva** (hoje a Iryssh só tem espiral + anéis; Baylo usa o beat; Thaqai o granizo; Ukhumo a mecânica; Kuychu os lasers/dança; Killa as fases)? Quais?
2. Substituir **todos** os `StopAndAimRing` ou só alguns?
3. A forma furiosa deve mudar de **figura** ou só de densidade/velocidade?
4. Quer trazer de volta os ataques arquivados em `Docs/Archive/DanmakuAttacks/` (3–12 padrões simultâneos) em vez de escrever novos?
5. Há mecânicas novas (ex.: bala que reflete na parede, balas que mudam de cor/estilo, ataques que dependem do poder do jogador) a incluir no motor antes?
