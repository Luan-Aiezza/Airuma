# O que foi trazido do Touhou 6 (reconstrução th06)

Fonte analisada: `BulletManager.cpp/.hpp` e `EnemyShooter` (referência de leitura; nada foi copiado, só as ideias).

| th06 | No Airumã |
|---|---|
| `count1` (por camada) × `count2` (camadas), `angle1/angle2`, `speed1→speed2` | `Pat.Shoot(c, at, Aim, count, layers, angle1, angle2, speed1, speed2, shot)` |
| Modos FAN_AIMED, FAN, CIRCLE(_AIMED), OFFSET_CIRCLE(_AIMED), RANDOM_* | `enum Pat.Aim` |
| `EX_SPEEDUP` (a bala sai com um "estouro" e assenta) | `Shot.Popping(extra, seconds)` |
| `EX_ACCELERATION` / `EX_VELOCITY` por tempo limitado | `Shot.EasingOutAfter(seconds)` |
| `EX_ANGLE_ADD / PLAYER / SET` (desacelera até parar, vira e relança, N vezes) | `Shot.Turning(mode 0 soma · 1 mira no barco · 2 rumo absoluto, intervalo, vezes, rotação, velocidade)` + `BulletBehavior.Turn` |
| Laser: linha de aviso → feixe ativo → some, rotacionável | `Pat.Laser(...)` (usa `SlashData.Beam`, `Spin`, `Follow`) + `SlashEffect.DrawLaser` |
| Rank (contagem, velocidade ±0.5, intervalo ±20%) | não aplicado (já temos `Pressure` e dificuldade); sugestão futura |

Ataques novos do chefe 5 (Kuychu): `Content/Bosses/Attacks/Attacks.Rainbow.cs` (versão antiga em `Docs/Archive/DanmakuAttacks/Attacks.Rainbow.before_th06.cs.txt`).
