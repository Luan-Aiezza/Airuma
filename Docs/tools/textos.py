#!/usr/bin/env python3
"""
Textos do jogo <-> Docs/Textos.md

  python3 Docs/tools/textos.py export   # lê as tabelas do jogo e escreve Docs/Textos.md
  python3 Docs/tools/textos.py import   # lê Docs/Textos.md e grava de volta nas tabelas do jogo (só o que mudou)

As tabelas ficam em Scripts/Content/Text/Texts.PtBr.cs e Texts.En.cs. A chave (primeira coluna) nunca muda; só as colunas PT-BR e EN são lidas na importação.
"""
import json, re, sys, os

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
TEXT_DIR = os.path.join(ROOT, 'Scripts', 'Content', 'Text')
FILES = {'pt': os.path.join(TEXT_DIR, 'Texts.PtBr.cs'), 'en': os.path.join(TEXT_DIR, 'Texts.En.cs')}
DIALOGUE = os.path.join(TEXT_DIR, 'Dialogue.cs')
MD = os.path.join(ROOT, 'Docs', 'Textos.md')
ENTRY = re.compile(r'^(    \[")([^"]+)("\] = ")((?:[^"\\]|\\.)*)(",.*)$')


def load(path):
    table = {}
    for line in open(path, encoding='utf-8'):
        m = ENTRY.match(line.rstrip('\n'))
        if m:
            table[m.group(2)] = json.loads('"' + m.group(4) + '"')
    return table


def encode(text):
    return json.dumps(text, ensure_ascii=False)[1:-1].replace('♪', '\\u266A')


# ---------------------------------------------------------------- dialogue metadata (who says what, in which talk)

SPEAKERS = {'Atli': 'Atli', 'Airu': 'Airu (espírito da lamparina)', 'Boss': 'o chefe da fase', 'Ukhumo': 'Ukhumo', 'LeftSerpent': 'Serpente esquerda',
            'RightSerpent': 'Serpente direita', 'Illari': 'Illari (a sacerdotisa)'}
EMOTIONS = {'Neutral': 'neutra', 'Annoyed': 'irritada', 'Sheepish': 'sem graça', 'Surprised': 'surpresa', 'Happy': 'feliz', 'Worried': 'preocupada'}


def talk_stage(name):
    for words, n in (('StageSix', 6), ('StageFive', 5), ('StageFour', 4), ('StageThree', 3), ('StageTwo', 2), ('StageOne', 1), ('BeforeBoss', 1), ('AfterBoss', 1)):
        if words in name:
            return n
    return None


def parse_dialogues():
    src = open(DIALOGUE, encoding='utf-8').read()
    talks = []   # (stage, name, summary, [(key, speaker, emotion, shout)])
    for m in re.finditer(r'((?:\s*///[^\n]*\n)+)\s*public static IReadOnlyList<DialogueLine> (\w+) => new\[\]\s*\{(.*?)\n    \};', src, re.S):
        summary = ' '.join(re.sub(r'<[^>]+>', '', l.strip()[3:]).strip() for l in m.group(1).strip().split('\n'))
        lines = [(k, sp, em, bool(sh)) for sp, em, k, sh in re.findall(r'(?:new DialogueLine|L)\(DialogueSpeaker\.(\w+), Emotion\.(\w+), "(\w+)"(, shout: true)?\)', m.group(3))]
        talks.append((talk_stage(m.group(2)), m.group(2), summary, lines))
    return talks


# ---------------------------------------------------------------- the rest of the texts, by hand

INTERFACE = [
    ('Menu principal', [
        ('Title', 'Título do jogo, no alto do menu principal.'), ('MenuStart', 'Menu principal: começa uma partida (leva à escolha da dificuldade).'),
        ('MenuBossPractice', 'Menu principal: abre o treino (de fases ou de chefes).'), ('MenuScores', 'Menu principal: mostra os recordes.'),
        ('MenuMusicRoom', 'Menu principal: sala de músicas (ainda não existe; mostra o aviso "Em breve").'), ('MenuConfig', 'Menu principal: abre as opções.'),
        ('MenuQuit', 'Menu principal: fecha o jogo.'), ('MenuHint', 'Dica das teclas, no canto de baixo do menu.'),
        ('ComingSoon', 'Aviso que pisca ao escolher uma opção que ainda não existe.'), ('Back', 'Voltar, nos submenus.'), ('Menu', 'Botão "ir para o menu", na tela final da partida.'),
        ('DemoLabel', 'Aviso que pisca durante a demonstração (o jogo joga sozinho quando o menu fica 10 segundos parado).')]),
    ('Dificuldade, treino e recordes', [
        ('DifficultyTitle', 'Título da escolha de dificuldade.'), ('DifficultyEasy', 'Dificuldade fácil.'), ('DifficultyNormal', 'Dificuldade normal.'), ('DifficultyHard', 'Dificuldade difícil.'),
        ('PracticeTitle', 'Título da lista de treino de chefes.'), ('PracticeStages', 'Treino: opção "treinar uma fase inteira".'), ('PracticeBosses', 'Treino: opção "treinar só o chefe".'),
        ('PracticeStageTitle', 'Título da lista de treino de fases.'), ('LockedBoss', 'Nome de um chefe ainda não existente na lista de treino (aparece com um cadeado).'),
        ('ScoresTitle', 'Título da tela de recordes.'), ('NoScore', 'Linha vazia da tabela de recordes.'), ('Cleared', 'Marca ao lado do recorde de uma partida que chegou ao fim.')]),
    ('Opções', [
        ('CfgTitle', 'Título da tela de opções.'), ('CfgLives', 'Opções: vidas no início da partida (1 a 5).'), ('CfgBombs', 'Opções: bombas no início da partida (0 a 3).'),
        ('CfgMusic', 'Opções: volume da música.'), ('CfgSfx', 'Opções: volume dos efeitos sonoros.'), ('CfgMode', 'Opções: modo da tela.'), ('CfgFullscreen', 'Valor do modo de tela: tela cheia.'),
        ('CfgWindowed', 'Valor do modo de tela: janela.'), ('CfgControls', 'Opções: abre a tela de controles (também é o título dela).'), ('CfgDefaults', 'Opções e Controles: botão que restaura os padrões.'),
        ('CfgExit', 'Opções e Controles: botão que volta.'), ('CfgKeyUp', 'Controles: tecla de mover para cima.'), ('CfgKeyDown', 'Controles: tecla de mover para baixo.'),
        ('CfgKeyLeft', 'Controles: tecla de mover para a esquerda.'), ('CfgKeyRight', 'Controles: tecla de mover para a direita.'), ('CfgKeyShoot', 'Controles: tecla de atirar.'),
        ('CfgKeyBomb', 'Controles: tecla da bomba.'), ('CfgKeyFocus', 'Controles: tecla de foco (movimento lento e mostra a hitbox).'),
        ('CfgPressKey', 'Aviso ao escolher uma tecla para trocar.'), ('CfgHint', 'Dica das teclas, no rodapé das opções.')]),
    ('HUD (painel da direita)', [
        ('HiScore', 'HUD: r��tulo do maior recorde.'), ('Score', 'HUD: rótulo da pontuação atual.'), ('Lives', 'HUD: rótulo das vidas (corações).'), ('Lantern', 'HUD: rótulo das bombas (lamparinas).'),
        ('Power', 'HUD: rótulo do poder da lamparina (chamas).'), ('Wind', 'HUD: rótulo do vento (bússola).'), ('WindCalm', 'HUD: vento calmo.'), ('WindNone', 'HUD: sem vento.'),
        ('WindWeak', 'HUD: vento fraco.'), ('WindMedium', 'HUD: vento médio.'), ('WindStrong', 'HUD: vento forte.'), ('North', 'Direção do vento: norte.'), ('South', 'Direção do vento: sul.'),
        ('East', 'Direção do vento: leste.'), ('West', 'Direção do vento: oeste.'),
        ('FairyName', 'Nome do espírito da lamparina. NÃO aparece mais no painel; ainda é o nome mostrado nas falas dela (ela ainda vai ganhar outro nome).'),
        ('FairyRole', 'Título mostrado ao lado do retrato do espírito da lamparina, no painel (quebra em duas linhas).'), ('BoyName', 'Nome do menino (o jogador), nas falas.'),
        ('DialogueHint', 'Dica das teclas, no canto da caixa de diálogo.')]),
    ('Pausa e fim da partida', [
        ('Paused', 'Título do menu de pausa.'), ('Continue', 'Pausa: continuar.'), ('GiveUp', 'Pausa: desistir (volta ao menu).'), ('PauseHint', 'Dica das teclas, no menu de pausa.'),
        ('Victory', 'Tela final: título quando o jogador vence.'), ('Defeat', 'Tela final: título quando o jogador perde.'), ('Restart', 'Tela final: botão de jogar de novo.'),
        ('NewRecord', 'Tela final: aviso de novo recorde.')]),
    ('Escolha de cartas', [
        ('ChooseCard', 'Título da escolha de carta (genérico).'), ('ChooseCardStart', 'Título da escolha de carta no começo de uma fase.'), ('ChooseCardBoss', 'Título da escolha de carta depois de vencer um chefe.'),
        ('CardsHint', 'Dica das teclas, na escolha de carta.'), ('YourCards', 'Rótulo da lista das cartas que o jogador já tem.')]),
    ('Falas curtas do espírito da lamparina (caixa de fala da HUD)', [
        ('SayStart', 'Fala no começo da partida.'), ('SayHit', 'Fala quando o barco é atingido.'), ('SayBomb', 'Fala quando o jogador usa uma bomba.'), ('SayRage', 'Fala quando o chefe entra na segunda forma (fúria).'),
        ('SayEnraged', 'Fala a cada novo ataque do chefe enfurecido.'), ('SayVictory', 'Fala na tela de vitória.'), ('SayDefeat', 'Fala na tela de derrota.'), ('SayPowerUp', 'Fala quando o poder da lamparina sobe.'),
        ('SayFullPower', 'Fala quando o poder chega ao máximo.'), ('SayBombRefill', 'Fala quando uma bomba reserva é pega.'), ('SayLifeRefill', 'Fala quando uma vida reserva é pega.'),
        ('SayCard', 'Fala quando o jogador escolhe uma carta.'), ('SayWard', 'Fala quando o escudo de uma carta segura um golpe.')]),
    ('História contada fora das falas (abertura e fim)', [
        ('IntroHint', 'Dica no canto de baixo à direita, na abertura do jogo e na cena do fim da história (cada uma fecha sozinha em 10 segundos, ou com qualquer tecla).'), ('IntroPage1', 'Abertura do jogo: parágrafo 1 da história, escrito na escuridão, embaixo à esquerda, sobre a cena da noite sem fim (os quatro aparecem juntos).'), ('IntroPage2', 'Abertura do jogo: parágrafo 2 da história, escrito na escuridão, embaixo à esquerda, sobre a cena da noite sem fim (os quatro aparecem juntos).'), ('IntroPage3', 'Abertura do jogo: parágrafo 3 da história, escrito na escuridão, embaixo à esquerda, sobre a cena da noite sem fim (os quatro aparecem juntos).'),
        ('IntroPage4', 'Abertura do jogo: parágrafo 4 da história, escrito na escuridão, embaixo à esquerda, sobre a cena da noite sem fim (os quatro aparecem juntos).'), 
        ('S6Epi01', 'Fim do jogo: parágrafo 1 da história final, escrito embaixo da cena do nascer do sol (os quatro aparecem juntos).'), ('S6Epi02', 'Fim do jogo: parágrafo 2 da história final, escrito embaixo da cena do nascer do sol (os quatro aparecem juntos).'), ('S6Epi03', 'Fim do jogo: parágrafo 3 da história final, escrito embaixo da cena do nascer do sol (os quatro aparecem juntos).'), ('S6Epi04', 'Fim do jogo: parágrafo 4 da história final, escrito embaixo da cena do nascer do sol (os quatro aparecem juntos).'),
        ('TheEnd', 'Fim do jogo: escrito bem grande embaixo, no centro, na cena do nascer do sol que fecha a história.')]),
    ('Música', [
        ('StageMusic', 'Nome da música da fase 1, mostrado no título da fase.'), ('Stage2Music', 'Nome da música da fase 2, mostrado no título da fase: um quadro animado, com trompetes, saxofones e bateria.'), ('Stage3Music', 'Nome da música da fase 3, mostrado no título da fase: sombria, com trompetes e pianos.'), ('Boss3Music', 'Nome da música do chefe da fase 3 (Thaqai), em toda a luta: mais forte, com piano grave e trompetes.'), ('Stage4Music', 'Nome da música da fase 4, mostrado no título da fase: calma, com tambores lentos e trompetes.'), ('Boss4Music', 'Nome da música do chefe da fase 4 (Ukhumo), em toda a luta: violenta, animalesca, com tambores e trompetes.'), ('Stage5Music', 'Nome da música da fase 5, mostrado no título da fase: épica, com flautas, trompetes e saxofones (era a música do chefe).'), ('Boss5Music', 'Nome da música do chefe da fase 5 (Kuychu), em toda a luta: um hino divino, com trompetes e coros, que lembra a música católica.'), ('Stage6Music', 'Nome da música da fase 6, mostrado no título da fase: divina e cósmica, com órgão, piano, coros, canto gregoriano e trompetes.'), ('Boss2Music', 'Nome da música do chefe da fase 2 (Baylo), em toda a luta: intensa, com trompetes, saxofones e bateria.'), ('BossMusic', 'Nome da música do chefe da fase 1 (Iryssh), em toda a luta.')]),
]

STAGE_NAMES = {1: 'Iryssh, a mulher-serpente de fogo', 2: 'Baylo, o boto da festa', 3: 'Thaqai, a gata das estrelas', 4: 'Ukhumo, a ursa da caverna', 5: 'Kuychu, o guardião de duas cabeças', 6: 'Mama Killa, a Lua'}
STAGE_TITLE = {1: 'StageTitle', 2: 'Stage2Title', 3: 'Stage3Title', 4: 'Stage4Title', 5: 'Stage5Title', 6: 'Stage6Title'}
STAGE_PEOPLE = {1: ['IrysshName'], 2: ['BayloName'], 3: ['ThaqaiName'], 4: ['UkhumoName'], 5: ['KuychuName', 'SerpentLeftName', 'SerpentRightName'], 6: ['KillaName', 'IllariName']}
STAGE_ATTACKS = {
    1: ['AttackRingOfEmbers', 'AttackPiercingTongue', 'AttackTwinWhirlwinds', 'AttackBlazingBloom', 'AttackFuryOfTheRiver', 'AttackTideOfEmbers'],
    2: ['AttackDoubleStep', 'AttackConfettiRain', 'AttackSambaWheel', 'AttackDolphinLeap', 'AttackPartyWave'],
    3: ['AttackHailCurtain', 'AttackStarPentagram', 'AttackZigzagLightning', 'AttackFineWeave', 'AttackSkyFalls', 'AttackEclipse'],
    4: ['AttackClawRake', 'AttackRoar', 'AttackStomp', 'AttackBruteForce', 'AttackCrossingClaws', 'AttackCaveIn', 'AttackFeast'],
    5: ['AttackTwoHeads', 'AttackSevenArcs', 'AttackBorderQuake', 'AttackTide', 'AttackTrial', 'AttackBrokenRainbow', 'AttackTwoWorlds', 'AttackGuardian'],
    6: [],
}
ATTACK_NOTE = {'AttackSkyFalls': 'enfurecido', 'AttackEclipse': 'enfurecido', 'AttackBrokenRainbow': 'enfurecido', 'AttackTwoWorlds': 'enfurecido', 'AttackGuardian': 'enfurecido',
               'AttackBlazingBloom': 'enfurecido', 'AttackFuryOfTheRiver': 'enfurecido', 'AttackTideOfEmbers': 'enfurecido', 'AttackCrossingClaws': 'enfurecido', 'AttackCaveIn': 'enfurecido', 'AttackFeast': 'enfurecido'}
CARD_BOSS = {'CardSerpentFlame': 1, 'CardPartyFever': 2, 'CardStormBolt': 3, 'CardBearCloak': 4}


def row(key, pt, en, ctx):
    for t in (pt, en):
        assert '|' not in t and '\n' not in t, (key, t)
    return f'| `{key}` | {pt} | {en} | {ctx} |'


def export():
    pt, en = load(FILES['pt']), load(FILES['en'])
    assert pt.keys() == en.keys(), set(pt) ^ set(en)
    used, out = set(), []
    header = '| Chave | Português (PT-BR) | Inglês (EN) | Contexto |\n| --- | --- | --- | --- |'

    def table(rows):
        out.append(header)
        out.extend(rows)
        out.append('')

    def take(key, ctx):
        used.add(key)
        return row(key, pt[key], en[key], ctx)

    out.append('# Airumã: todos os textos do jogo\n')
    out.append('Este documento reúne **todos os textos que aparecem no jogo**, com o contexto de cada um, para você poder editá-los em um lugar só.\n')
    out.append('**Como editar:** mude apenas as colunas **Português** e **Inglês** (e, se quiser, acrescente observações na coluna Contexto: ela não é lida). '
               'Não mude a coluna **Chave**, não apague linhas e não use o caractere `|` dentro de um texto. Depois é só me pedir para ler o documento: '
               'eu aplico tudo de uma vez nas tabelas do jogo (`python3 Docs/tools/textos.py import`). Para gerar o documento de novo a partir do jogo: `python3 Docs/tools/textos.py export`.\n')
    out.append('**Detalhes do jogo:**\n')
    out.append('- Cada fala de diálogo cabe em até 3 linhas curtas da caixa (uns 36 letras por linha), então textos muito longos são cortados.')
    out.append('- Nas falas, o personagem "o chefe da fase" mostra o nome do chefe da fase (veja a tabela de nomes de cada fase).')
    out.append('- A fonte do jogo não tem alguns sinais, como parênteses, aspas e ponto e vírgula; prefira vírgula, ponto, dois pontos, traço, `!`, `?`, `/`, `+`, `%` e `...`.')
    out.append('- Os nomes dos ataques dos chefes aparecem como o título de uma carta de feitiço ("Tema: Nome") quando o ataque começa.')
    out.append('- O espírito da lamparina ainda se chama **Airu** nas falas (chave `FairyName`); no painel só aparece o título dela (`FairyRole`).\n')

    out.append('---\n\n## Interface\n')
    for title, keys in INTERFACE:
        out.append(f'### {title}\n')
        table([take(k, c) for k, c in keys])

    talks = parse_dialogues()
    for stage in range(1, 7):
        out.append(f'---\n\n## Fase {stage}: {STAGE_NAMES[stage]}\n')
        out.append('### Título e nomes\n')
        rows = [take(STAGE_TITLE[stage], f'Título da fase {stage}, mostrado quando ela começa.')]
        for k in STAGE_PEOPLE[stage]:
            rows.append(take(k, 'Nome de personagem da fase (mostrado no título do chefe e nas falas).'))
        table(rows)
        if STAGE_ATTACKS[stage]:
            out.append('### Ataques do chefe\n')
            table([take(k, f'Nome do ataque do chefe da fase {stage}' + (' (a segunda forma, enfurecida).' if k in ATTACK_NOTE else ' (a primeira forma, calma).')) for k in STAGE_ATTACKS[stage]])
        else:
            out.append('### Ataques do chefe\n')
            out.append('A Killa ainda usa os ataques da fase 3 (Thaqai); os nomes são os mesmos daquela fase.\n')
        for s, name, summary, lines in talks:
            if s != stage:
                continue
            if 'Intro' in name:
                title = 'Abertura da fase (antes dos inimigos)'
            elif 'Arrival' in name or name == 'BeforeBossOne':
                title = 'Chegada ao chefe (antes de ele aparecer)'
            elif 'Meeting' in name or name == 'BeforeBossTwo':
                title = 'Encontro com o chefe (antes da luta)'
            else:
                title = 'Final da fase (depois da luta)'
            out.append(f'### Falas: {title}\n')
            out.append(f'*Resumo, do código (em inglês): {summary}*\n')
            rows = []
            for i, (key, speaker, emotion, shout) in enumerate(lines, 1):
                ctx = f'{i}ª fala; quem fala: {SPEAKERS[speaker]}; expressão {EMOTIONS[emotion]}' + ('; GRITADA (a caixa treme)' if shout else '') + '.'
                rows.append(take(key, ctx))
            table(rows)

    out.append('---\n\n## Cartas\n')
    out.append('Cada carta tem um nome (**Título**) e uma descrição curta do que ela faz (**Detalhe**), mostrados na escolha de carta e na HUD.\n')
    cards = []
    for key in pt:
        m = re.match(r'(Card\w+)\.(Title|Detail)$', key)
        if m:
            cards.append((m.group(1), m.group(2), key))
    rows = []
    for card, kind, key in cards:
        extra = ''
        if card in CARD_BOSS:
            extra = f' Carta especial, deixada pelo chefe da fase {CARD_BOSS[card]}.'
        rows.append(take(key, ('Nome' if kind == 'Title' else 'Descrição') + f' da carta ({card[4:]}).' + extra))
    table(rows)

    missing = [k for k in pt if k not in used]
    if missing:
        out.append('---\n\n## Outros\n')
        table([take(k, 'Texto sem contexto catalogado ainda.') for k in missing])
    open(MD, 'w', encoding='utf-8').write('\n'.join(out) + '\n')
    print(f'{len(used)} textos escritos em {MD}')


def import_():
    cells = {}
    for line in open(MD, encoding='utf-8'):
        m = re.match(r'^\| `([^`]+)` \| (.*?) \| (.*?) \| (.*) \|\s*$', line.rstrip('\n'))
        if m:
            cells[m.group(1)] = (m.group(2).strip(), m.group(3).strip())
    changed = 0
    for lang, idx in (('pt', 0), ('en', 1)):
        path = FILES[lang]
        current = load(path)
        lines = open(path, encoding='utf-8').read().split('\n')
        for i, line in enumerate(lines):
            m = ENTRY.match(line)
            if not m or m.group(2) not in cells:
                continue
            new = cells[m.group(2)][idx]
            if not new:
                print('texto vazio ignorado:', m.group(2))
                continue
            if new != current[m.group(2)]:
                lines[i] = m.group(1) + m.group(2) + m.group(3) + encode(new) + m.group(5)
                changed += 1
        open(path, 'w', encoding='utf-8').write('\n'.join(lines))
    print(f'{changed} textos mudaram')


if __name__ == '__main__':
    {'export': export, 'import': import_}.get(sys.argv[1] if len(sys.argv) > 1 else '', lambda: print(__doc__))()
