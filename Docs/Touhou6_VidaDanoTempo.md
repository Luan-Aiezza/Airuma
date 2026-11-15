# Touhou 6 × Airumã — vida dos bosses, dano e duração das lutas

Pedido: comparar o sistema de vida, dano e tempo de luta do Touhou 6 (decompilação em `/Users/luan/Downloads/th06-master-2`) com o nosso, do boss 1 ao 6, e ver onde divergem.

## 0. O que a decompilação tem e o que NÃO tem (leia primeiro)

* **Tem:** o motor — como o dano do jogador é calculado, os limites e redutores de dano, as tabelas de tiro do jogador por nível de poder (`BulletData.cpp`), o contador de spellcards por fase (`g_SpellcardScore`), as regras de tempo esgotado.
* **NÃO tem:** a **vida de cada fase do boss** e os **tempos limite de cada ataque**. Isso fica nos scripts dos inimigos (`.ecl`, instruções `ENEMY_LIFE_SET` e `TIMER_CALLBACK_THRESHOLD`) que estão nos arquivos de dados do jogo original; a decompilação só traz o interpretador dessas instruções. (A cópia `th06-master-2` é idêntica à `th06-master` que usei antes.)
* **Consequência:** **não consigo, só com esse repositório, dizer "a luta do boss N dura X segundos no Touhou 6"**. Abaixo comparo o que dá para medir (o modelo de dano) e deixo os números de duração do Touhou como **lacuna a preencher** (seção 6). Tudo que for lembrança minha está marcado como *não verificado*.

## 1. O modelo de dano do Touhou 6 (extraído do código)

### 1.1 Regras (`EnemyManager.cpp`, `Player.cpp`)

| Regra | Valor |
|---|---|
| Teto de dano por quadro em um inimigo | **70** (soma de todas as balas que acertam no mesmo quadro) |
| Durante um **spellcard**, dano de tiro normal | **÷ 7** (inteiro; mínimo 1 se houve acerto) |
| Dano de bomba, em geral | cada bala de tiro vale **⅓** enquanto a bomba está ativa |
| Durante spellcard, bomba usada **depois** do início | **÷ 3** |
| Durante spellcard, bomba usada **antes** do início | **0** (e anula também o tiro naquele quadro — comentado no código como bug) |
| Contato do corpo do jogador com inimigo comum | tira 10 da vida (não do boss) |
| Tempo esgotado no spellcard | passa de fase, **sem captura**, balas viram itens |
| Bônus de captura | `valor do spellcard × (1 + segundos restantes ÷ 10)`, só se não morreu nem usou bomba |

### 1.2 Dano do jogador por nível de poder (`BulletData.cpp`)

"DPS bruto" = soma de `dano × 60 ÷ quadros entre tiros` de todas as balas do nível, supondo que **todas acertem** (balas retas acertam bem num boss parado; as teleguiadas/lasers, menos). Os níveis vão de 1 (poder 0) a 9 (poder 128 = máximo).

| Nível (poder) | Reimu A | Reimu B | Marisa A | Marisa B |
|---|---|---|---|---|
| 1 (0) | 576 | 576 | 576 | 576 |
| 2 (8) | 632 | 672 | 504 | 387 |
| 3 (16) | 776 | 912 | 488 | 386 |
| 4 (32) | 992 | 900 | 584 | 530 |
| 5 (48) | 1032 | 996 | 600 | 530 |
| 6 (64) | 1044 | 1146 | 656 | 482 |
| 7 (80) | 1032 | 1288 | 624 | 602 |
| 8 (96) | 1104 | 1624 | 708 | 602 |
| 9 (127+) | **1390** | **1904** | **744** | **890** |

Leitura:
* No Touhou 6 o poder multiplica o DPS por **~1,3 a 3,3** (Reimu A 2,4×), e **o nível mais baixo já tem 576/s** (as personagens "mais fracas" em poder alto, como Marisa A, ficam em ~1,3×).
* Em **spellcard**, o DPS efetivo é o **bruto ÷ 7**: ≈ **82/s** (nível 1) a ≈ **200/s** (Reimu A nível 9). É isso que faz os spellcards **durarem várias vezes mais** que as fases sem spellcard para a mesma vida.
* A vida de boss do Touhou 6 é grande (a ordem de grandeza dos milhares), porque o jogador causa centenas de pontos por segundo.

## 2. O nosso modelo de dano (código do Airumã)

| Item | Nosso jogo |
|---|---|
| Tiro reto, nível 1 / 2 / 3 | 10 / 20 / 30 de dano por segundo (10 salvas/s × 1, 2 ou 3 balas × dano 1) |
| Com foco | × 1,4 (14 / 28 / 42) |
| Tiro teleguiado | 1/3 do reto (3,3 / 6,7 / 10 por segundo, com acerto garantido) |
| Redução de dano em ataques "especiais" | **nenhuma** — todo ataque recebe o mesmo dano |
| Bomba | machuca o boss; não zera nem divide o dano |
| Teto de dano por quadro | não há |
| Poder (1→3) multiplica o DPS por | **3,0×** |
| Cartas | multiplicam dano, perfuração, estouro etc. |
| Tempo esgotado | passa de ataque sem bônus, balas somem (igual ao Touhou) |

## 3. As lutas do Airumã hoje (bot ideal, Normal, arma reta, nível 3)

Medido com o harness (`sweep`): tempo que o bot ideal leva para vencer cada ataque, e o limite de tempo.

| Boss | Ataques | Vida total | Tempo do bot ideal (soma) | Soma dos limites de tempo | Pico de balas |
|---|---|---|---|---|---|
| 1 Iryssh | 5 | 1.583 | **~75 s** | 240 s | 103 |
| 2 Baylo | 5 | 1.285 | **~72 s** | 235 s | 189 |
| 3 Thaqai | 6 | 1.689 | **~70 s** | 250 s | 209 |
| 4 Ukhumo | 7 | 2.900 | **~107 s** | 345 s | 183 |
| 5 Kuychu | 8 | 3.587 | **~130 s** | 430 s | 272 |
| 6 Killa | 10 | 3.768 | **~154 s** | 520 s | 321 |

(Um jogador real é bem mais lento que o bot: errar tiro, desviar, tirar o tiro do boss. Uma regra prática que já usamos: o limite de tempo é ~3× o tempo do bot ideal.)

## 4. Onde divergem (e o efeito na duração)

1. **Sem redutor de dano nos ataques-spellcard (÷ 7).** É a maior diferença estrutural. No Touhou 6 os ataques "de carta" são longos porque o dano cai a ⅐; no nosso jogo todo ataque cai na velocidade do tiro cheio. Resultado: nossas lutas têm ataques de **7 a 20 s** de tiro cheio (bot ideal), o que está longe de fazer o jogador ver o cronômetro correr.
2. **Duração total.** Nosso bot ideal faz o boss 1 em ~75 s e o boss 6 em ~154 s; a progressão é **plana demais no começo** (os bosses 1, 2 e 3 têm quase o mesmo tempo, ~70–75 s) e só cresce a partir do 4. No Touhou 6 a duração costuma crescer **a cada fase** *(não verificado: depende dos dados do `.ecl`)*.
3. **Vida entre os bosses.** A vida total sobe de 1.285 (boss 2) a 3.768 (boss 6), mas o boss 1 (1.583) passou o boss 2 e o 3 (1.689) por pouco: a curva não é monótona.
4. **Escala do poder.** O nosso poder triplica o DPS (3,0×); no Touhou 6 a escala é de ~1,3 a 3,3× conforme a personagem e o nível começa a 576/s (ou seja, **o jogador já começa forte**, e o poder pouco muda a duração). Nós começamos em 10/s (nível 1): um jogador que perde poder (morre) muda **muito** o tempo de luta, até 3×.
5. **Teto de dano por quadro (70).** Não temos teto; com cartas fortes (dano, perfuração, estouro) um jogador pode derreter o boss, como a jogadora relatou ("buildei pra dano, os spells não duravam"). No Touhou 6 o teto de 70/quadro (÷ 7 em spell) limita o DPS máximo em ~600/s por mais que se empilhe.
6. **Bombas.** No Touhou 6 a bomba **não ajuda** a derrubar o boss (⅓ do dano; zero antes do spell). Em nós a bomba é arma.
7. **Densidade/limite de tempo.** Os nossos limites de tempo (30–70 s por ataque) existem, mas o jogador nunca os alcança por causa do item 1; no Touhou 6 o tempo é parte do desenho (a luta é "sobreviver até acabar o relógio ou derrubar a barra").
8. **Quantidade de ataques.** Pelo contador de spellcards do Touhou 6 (3, 4, 7, 9\*, 8, 11) os bosses têm de **3 a 11 spellcards**, mais as fases sem carta; o nosso 5, 5, 6, 7, 8, 10. A forma é parecida (cresce 1 por fase), **boss 1 e 2 estão acima** do que o Touhou 6 faz (3 e 4 spellcards). *(\*O contador do 4º tem 18 entradas por haver dois conjuntos de cartas, A e B.)*

## 5. O que teria que ser ajustado (proposta, por ordem de impacto)

1. **Redutor de dano em ataques de carta.** Marcar os ataques "especiais" (a segunda metade de cada boss, ou todos os ataques com nome de carta) com um **fator de dano** (ex.: 0,3–0,5) e **aumentar a vida na mesma proporção**: o ataque dura mais tempo de tiro, sem mudar a barra. Parâmetro: `DamageTakenFactor` por ataque (hoje 1,0).
2. **Teto de dano por quadro e/ou por segundo** no boss (ex.: dano máximo = vida do ataque ÷ tempo mínimo desejado), para que `build` de dano não torne os ataques instantâneos. A jogadora relatou isso.
3. **Curva de duração alvo por boss** (nova fonte de verdade): em vez de escolher a vida por boss, escolher o **tempo alvo** do bot ideal por boss (hoje: 75/72/70/107/130/154 s) e derivar a vida (`vida = tempo alvo × 29`, parado; `× 20`, em movimento).
4. **Nivelar a progressão** dos bosses 1 a 3 (hoje quase iguais) e o boss 1 > boss 2 da vida total.
5. **Bomba:** decidir se continua arma (como hoje) ou se passa a dar ⅓ do dano, como no Touhou 6.
6. **Poder:** decidir se o jogador começa mais forte (nível 1 a 15–20/s em vez de 10) e o poder escala menos, para a duração variar menos com a morte.
7. **Número de ataques:** boss 1 com 3–4 ataques e o boss 2 com 4 (hoje 5 e 5), se a meta for imitar o Touhou 6; senão manter e compensar o tempo por ataque.

## 6. Dados que faltam para a comparação número a número

Para fechar a tabela "duração por boss" do Touhou 6 eu preciso das **vidas de cada fase** e dos **tempos limite**: estão nos scripts `.ecl` do jogo (`stg1enm.ecl` … `stg6enm2.ecl`), dentro dos `ST*.DAT`. Formas de obtê-los:
* decompilar os `.ecl` com as ferramentas do projeto **thtk** (`thecl`) a partir da sua cópia do jogo, ou
* trazer a tabela de vidas/tempos de uma fonte (wiki/thpatch) e eu a coloco aqui.

Com esses números eu monto a tabela **boss a boss** (vida da barra, tempo limite, tempo de luta estimado com o DPS do nível de poder típico) e a curva alvo da seção 5 (item 3).

## 7. Perguntas para decidir o caminho

1. Você consegue extrair/me passar os números dos `.ecl` (vidas e tempos)? Sem eles, qualquer "alvo de duração" será por lembrança.
2. Quer o **redutor de dano nos ataques de carta** (item 1)? Esse é o ajuste que mais aproxima a duração do Touhou 6 e resolve o "buildei pra dano".
3. A bomba deve continuar machucando o boss?
4. Qual duração total você quer para o boss 1 e para o boss 6 (uma luta real, de jogador, não do bot)?
