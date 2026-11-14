# Fase 1 — inimigos: o que o th06 oferece e uma proposta para o Airumã

## 1. O que o `th06-master` realmente contém

**Importante:** o repositório é a *decompilação do executável* (código C++ do motor). **Os scripts de inimigos da Fase 1 (os `.ecl` com cada onda de fadas) não estão nele**: ficam nos arquivos de dados do jogo original, que a decompilação não inclui. Então não dá para "copiar" as ondas da Fase 1 de Touhou 6 — o que dá para extrair é o **repertório de comportamentos** que o motor deixa um inimigo fazer (`EclManager.hpp`, `EnemyEclInstr.cpp`, `Enemy.hpp`). As ondas abaixo combinam esse repertório com o que me lembro do estilo da Fase 1 de Touhou 6 (fadas em filas e em arcos, mira simples, "para-atira-sai"); **essa parte é memória minha, não verificada no código.**

### 1.1 Repertório de um inimigo (instruções da ECL)

| Categoria | Instruções | O que permite |
|---|---|---|
| Movimento | `MOVE_VELOCITY` / `MOVE_ANGULAR_VELOCITY` / `MOVE_ACCELERATION` | linha reta, curva suave, aceleração |
| | `MOVE_POSITION_INTERP_*` (linear, decelerate, accelerate, versões *fast*) | **entra rápido e freia** num ponto, ou sai acelerando |
| | `MOVE_TOWARDS_PLAYER`, `MOVE_RAND`, `MOVE_RAND_IN_BOUNDS` | persegue o jogador, passeia ao acaso |
| Tiro | `BULLET_FAN_AIMED`, `FAN`, `CIRCLE(_AIMED)`, `OFFSET_CIRCLE(_AIMED)`, `RANDOM_*` | leque no jogador, anéis, anéis deslocados, aleatório |
| | `SHOOT_INTERVAL`, `SHOOT_INTERVAL_DELAYED`, `SHOOT_NOW`, `SHOOT_OFFSET`, `SHOOT_DISABLE/ENABLE` | cadência, primeiro tiro atrasado, tiro de um ponto deslocado do corpo |
| | `BULLET_EFFECTS` (EX_*: speedup, accelerate, angle add/player/set) | "estouro" ao sair, curva por tempo, **para-e-vira-e-mira** |
| | `LASER_*` | laser (só chefes e inimigos grandes) |
| Vida / fases | `LIFE_CALLBACK_THRESHOLD`, `TIMER_CALLBACK_THRESHOLD`, `ENEMY_LIFE_SET` | o inimigo **muda de padrão** quando perde vida ou depois de X segundos |
| Fluxo | `ENEMY_CREATE`, `DROP_ITEMS`, `ENEMY_INTERRUPT` | um inimigo que chama outros; drops por inimigo |
| **Rank** | `bulletRankSpeedLow/High` ±0,5, `bulletRankAmount*` | mais balas e mais rápidas conforme o rank (nós: dificuldade) |

### 1.2 O que o Airumã já tem e o que falta

* **Já temos:** `Stream` (curva), `Sentries` (entra, fica parado atirando, sai), `SideColumn`, `RisingRow`, `Heavy`, mira simples (`Aimed`), `Fan`, `Ring`, `Spiral`, `Custom` (qualquer figura da biblioteca `Pat`).
* **Falta (aproximar do th06):** inimigo que **freia ao chegar** (`MOVE_POSITION_INTERP_DECELERATE`); **primeiro tiro atrasado** e **tiro único ao passar de uma linha**; inimigo que **muda de padrão** com o tempo; tiro que parte **deslocado** do corpo.

## 2. Diagnóstico da Fase 1 atual

* 5 ondas, quase todas de **"espíritos azuis que não atiram"** em colunas, linhas, V, anéis e pinças: só muda a **formação**, nunca o **tiro** nem o **comportamento**.
* Os "gunners" são poucos e só fazem **1 bala mirada** (a mesma em todas as ondas).
* Resultado: a fase é um tutorial de movimento; nada ensina uma ideia própria nem fala de **fogo/rio** (tema do boss).

## 3. Proposta: 7 ondas com ideia própria (tema: brasas e rio de fogo)

Regras da fase mantidas: só tipos 1 e 2, nada nasce abaixo de y = 230, inimigos só atiram acima de y = 330, regra dos espaços.
Cada onda ensina **uma** coisa que o boss depois usa.

| # | Nome | O que acontece | Comportamento (termo th06) | O que ensina / liga ao boss |
|---|---|---|---|---|
| 1 | **Faíscas** | Colunas e uma linha de espíritos azuis, sem tiro (como hoje, mas curta) | movimento linear / senoide | aquecer, mover |
| 2 | **Fogo de aviso** | De cada canto superior, 3 espíritos verdes entram em arco, **freiam** e cada um dá **1 tiro mirado lento**, depois **saem subindo** | `INTERP_DECELERATE` → `BULLET_AIMED` ×1 → `INTERP_ACCELERATE` p/ cima | ler tiro mirado e andar de lado |
| 3 | **Cortina de brasas** | Uma **fila** de espíritos azuis atravessa o topo e cada um solta **uma bala reta para baixo** no momento em que passa por uma linha (x diferentes: forma "pente" de balas) | tiro único ao passar de y (`SHOOT_NOW`) | passar **entre** colunas de balas (antecipa o *Wall* de Thaqai) |
| 4 | **Pinça com leque** | Dois grupos entram pelas laterais; cada um dá um **leque de 3** mirado quando chega ao meio e continua; no meio, V de espíritos azuis | `FAN_AIMED` 3 balas, abertura 26° | desviar de leque (antecipa os leques layered) |
| 5 | **Anel que espera** | Um espírito verde grande-parado no alto solta um **anel lento de 10–12 balas que para e mira** (versão simples do *stop & aim*) | `CIRCLE` + `EX_ANGLE_PLAYER` | **primeira vez** que se vê a figura que a Iryssh usa no ataque 1 |
| 6 | **Espiral de brasa** | Um espírito verde parado solta uma **espiral de 2 braços** por ~6 s, com espíritos azuis passando de lado | `SHOOT_INTERVAL` curto + ângulo somado | ensinar a **circular** com a espiral (ataque 1 da Iryssh) |
| 7 | **Revoada final** | Mistura curta: fila + 2 sentinelas (leque) + anel que espera, antes do boss | combina 2–5 | aquecimento do boss |

Número de balas simultâneas na tela alvo: ~15–45 (hoje a fase é quase vazia; a Iryssh chega a ~100).

### 3.1 Elementos de identidade (fogo e rio)

* **Espíritos azuis** viram **faíscas / fogos-fátuos** (continuam azuis só até o fim da onda 3; depois **laranja**: "o rio esquenta"). Ajuste só de cor, sem sprite novo.
* **Rastro de brasa:** os espíritos verdes deixam **1 brasa pequena** parada onde pararam (some em 2 s) — sem dano, só visual, para sentir a ligação com o fogo da Iryssh (opcional).
* **Música/cenário:** a fase vai "esquentando": o rio fica mais vermelho a partir da onda 5 (já existe `RageAmount` / tint de cena para o boss).

## 4. Implementação necessária (se aprovada)

| Item | Onde | Tamanho |
|---|---|---|
| `Hold`: inimigo entra, freia, atira N tiros e sai (como `Sentries` mas com *freada* e saída em aceleração) | `WaveBuilder` / `EnemySystem` | pequeno |
| Tiro único ao passar de uma linha y | `FirePattern` (novo `Kind`/campo `AtY`) | pequeno |
| Leque mirado de 3 ao chegar | já existe `FirePattern.Fan` | nenhum |
| Anel que para e mira por inimigo | já existe `Custom` + `StopAndAimRing` | nenhum |
| Espiral | já existe `FirePattern.Spiral` | nenhum |
| Reescrever `Stage1Waves.cs` com as 7 ondas | `Content/Stages` | médio |
| Testes de contagem e de regras da fase (`SelfTest.StageFlow`) | `Tests` | pequeno |

## 5. Perguntas para decidir

1. **Quantas ondas** quer na Fase 1: as 7 acima, ou manter 5 (a regra "cada fase um pouco maior que a anterior": ondas 5, 6, 7, 8, 9, 10 hoje)? Se for 7, a Fase 2 precisa ficar com ≥ 8.
2. Os **espíritos azuis passam a atirar** em alguma onda, ou continuam sem tiro (só os verdes atiram)?
3. Posso incluir a **mudança de cor** (azul → laranja) e o **rastro de brasa**, ou prefere só o comportamento/tiro?
4. A Fase 1 deve ficar **mais fácil que a 2** (hoje é assim); aceita a onda 6 (espiral) como a mais difícil da fase?

---

## 6. Implementado (decisões: 6 ondas · azuis continuam sem atirar, com variantes novas · cor e rastro · onda 6 a mais difícil)

* **6 ondas** em `Content/Stages/Stage1Waves.cs`: Faíscas · Fogo de aviso · Cortina de brasas · Pinça com leque · Anel que espera · Espiral de brasa.
* **Inimigos que não atiram** (todos do tipo 1, mudam só cor, tamanho, vida, formação e velocidade): **azul** (como antes), **faísca** (pequena, amarelada, rápida: ~250 px/s, em esquadrões que cruzam como um traço), **brasa** (laranja; os azuis "esquentam" a partir da onda 5), **cinza** (grande, vermelha, lenta: 80 px/s, aguenta 4 tiros).
* **Formações novas** (`WaveBuilder`): `Fan` (leque de espíritos a partir de um ponto), `Spiral` (pinwheel de espíritos que saem girando), `Slash` (traço diagonal de espíritos em fila), e `As(variante)` para dar a cor/tamanho/vida.
* **Gunners (tipo 2)**, um jeito de atirar por onda: tiro mirado lento · fila que cruza soltando balas retas (cortina de pentes) · leque de 3 · anel que para e mira · espiral de 2 braços.
* **Rastro de brasa:** *não* implementado (precisaria de um efeito visual novo sem dano); a cor laranja das brasas está feita.
* Balas simultâneas medidas (Normal): ondas 2–6 = 43 · 44 · 44 · 31 · 66; a onda 6 é a mais densa em Fácil, Normal e Difícil.
