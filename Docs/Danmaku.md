# Padrões de bullets (danmaku)

Como os ataques dos bosses e dos espíritos são feitos a partir da fase 2 (a fase 1 usa os emissores simples antigos e não foi mexida).

## A ideia

Um **ataque** de boss (`BossPhase`) é uma lista de **padrões** (`PatternSpec`) que disparam ao mesmo tempo. Cada padrão diz de quanto em quanto tempo atira (`Pat.Every(intervalo, c => ...)`), depois de quanto tempo começa (`.After(s)`), se vem em rajadas (`.InBursts(liga, desliga)`), se sai do segundo corpo do boss (`.Twin()`), se só vale durante o terremoto (`.OnQuake()`) e em quais dificuldades vale (`.NotOnEasy()`, `.OnlyOnHard()`). O que atira é uma função do contexto (`PatternContext`: onde está o boss, onde está o barco, o campo, o tempo, quantas vezes já atirou), então a mesma figura sai igual sempre e dá para desenhá-la no papel.

As figuras estão em `Scripts/Ecs/Patterns/Pat.cs`: anéis (`Ring`, `Layers`), leques (`Arc`, `Aimed`), raios de moinho (`Spokes`), flores (`Rose`), polígonos e estrelas que se armam e depois se soltam (`Polygon`, `Star`), crescentes que voam inteiros (`Crescent`), pentes inclinados (`Comb`), paredes com brecha de qualquer lado (`Wall`), chuva e fileiras de qualquer borda (`Rain`, `Along`: cima, baixo, esquerda, direita), malhas (`Lattice`), galáxias e espirais (`PolarRing`) e constelações com linhas que machucam (`Constellation`). A bala em si é um `Shot` (estilo, velocidade, tamanho, hitbox própria, e o que faz depois: `Launched`, `LaunchedOut`, `Splitting`, `Bouncing`, `Pulled` (gravidade), `Dancing`, `Zigzagging`, `Zoomed`).

Tipos de bala novos: orbes (7x7), contas (5x5), arroz (3x7), kunai (5x9) e estrelas, em várias cores, e as **bolas gigantes** (25x25, hitbox de 34 px) em seis cores.

## Estado atual dos bosses

Os ataques dos bosses 2 a 5 voltaram a ser os **antigos** (emissores simples: `Main`/`Sub` de cada ataque). A Killa (boss 6) usa os seis ataques que já tinha emprestados da fase 3 e mais quatro ataques comuns (dois emissores cada, em `Attacks.Cosmos.cs`), para chegar a 10: 6 na primeira forma e 4 na segunda. A contagem de ataques por boss é 2+2, 3+2, 4+2, 4+3, 5+3, 6+4 (testada em `SelfTest.Patterns.cs`).

Os ataques feitos com padrões simultâneos (3 a 12 padrões por ataque, densidade crescente de ~300 a ~940 balas na tela, vento que não desmancha os caminhos) ficaram guardados como texto em `Docs/Archive/DanmakuAttacks/` para servir de referência e ser trazidos de volta, um a um, ao ajustar cada ataque: basta copiar o método para o `Attacks.<Tema>.cs` do boss. O motor (`Pat`, `Shot`, `PatternSpec`, `BossPhase.Patterns`, `Pressure`) continua no jogo e funciona junto com os emissores antigos no mesmo ataque.

## Espíritos

`FirePattern.Custom(intervalo, figura)` deixa um espírito atirar qualquer figura da biblioteca. Os conjuntos de tiros de cada fase estão em `Scripts/Content/Stages/SpiritFire.cs`. Para as ondas, além das formações antigas há `SideColumn` (fila que cruza a tela pelo lado), `RisingRow` (fila que sobe de baixo) e `Sentries` (espíritos que entram por um lado ou por baixo, ficam parados atirando e saem).

## Como testar sem o Godot

Os testes (`Scripts/Tests`, em especial `SelfTest.Patterns.cs`) conferem a curva de ataques, que cada boss enche mais a tela que o anterior, e que as figuras mantêm a forma. No Godot: `Godot --headless --path . res://Scenes/selftest.tscn`.
