# Demo do Airumã (itch.io)

A demo é um **projeto separado**, gerado do projeto completo por um script, que contém **só a Fase 1 e a Iryssh**: nenhuma outra fase, boss, música, diálogo ou texto do jogo completo vai no código nem nos arquivos da demo.

## Gerar e exportar

```
python3 tools/make_demo.py /Users/luan/Documents/GitHub/Airuma_Demo --build
cd /Users/luan/Documents/GitHub/Airuma_Demo
Godot --headless --import
Godot --headless --export-release "macOS"           ../Airuma_Demo_Builds/Mac/AirumaDemo.zip
Godot --headless --export-release "Windows Desktop" ../Airuma_Demo_Builds/Windows/AirumaDemo.exe
```

O script apaga a pasta de destino e a refaz do zero; rode de novo sempre que o jogo completo mudar. O projeto completo não é alterado.

## O que a demo tem de diferente
* Fases: só a 1 (6 ondas + Iryssh com 5 ataques). Ao vencer a boss e depois da conversa final: tela "Obrigado por testar / Disponível em breve na Steam" (PT, EN, ES), e volta ao menu.
* Treino de boss: lista de bosses (só a Iryssh), com cadeado até o jogador vencê-la numa partida; o treino usa as vidas e bombas das opções (não é mais infinito). Isso vale também no jogo completo.
* Sala de músicas: só menu, fase 1 e boss 1, com cadeado até vencer a fase 1.
* Sem a opção "Sair" no navegador; sem Web: o Godot 4 **não exporta projetos C# para Web**.

## Copyright
Todos os arquivos `.cs` levam o aviso de copyright e a proibição de descompilar/ler/usar em IA; o `.csproj` grava autor, empresa e copyright no assembly; os presets de exportação gravam o copyright nos aplicativos; o menu e a tela final mostram "(c) 2026 Luan Aiezza"; `COPYRIGHT.md` na raiz. É um aviso legal, não uma proteção técnica: o `.dll` ainda pode ser aberto por ferramentas de decompilação. Para dificultar de verdade, ofusque o assembly antes de publicar.
