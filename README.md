# Aurora Engine
![Aurora](Aurora-icon.png)

Engine experimental para Windows, escrita em C# com WPF. Inclui editor de cenas, scripts C#, Academy com dez aulas e o jogo de exemplo Coin Run.

## Executar e compilar

Requer Windows x64 com .NET Framework 4.8 e componentes WPF. O build usa o compilador C# do Framework instalado no Windows. Não usa o SDK dotnet nem pacotes NuGet.

Abra PowerShell na pasta do repositório e execute:

~~~powershell
.\build.ps1
.\verify.ps1
.\bin\AuroraEngine.exe
~~~

Se a política local bloquear scripts, revise os arquivos e execute com PowerShell -ExecutionPolicy Bypass -File .\build.ps1. Feche a engine antes de recompilar.

O executável gerado fica em bin e não deve ser enviado como código-fonte. Para distribuir o programa compilado, use uma Release do GitHub.

## Primeiro projeto

O Hub abre diretamente. Informe nome, escolha um template e clique Create project. O diretório padrão é Games, na raiz desta pasta. Projetos pessoais dentro de Games são ignorados pelo Git.

Para testar o jogo pronto, abra Demos/CoinRun/project.aurora e clique Play, depois Jogar. Mova com WASD ou setas. A interface mostra a pontuação e oferece reinício.

## Recursos

- Hierarquia, Inspector, viewport 3D, câmera ortográfica e primitivas.
- Transformações, seleção, undo/redo e cenas JSON versionadas.
- Múltiplas cenas, scripts C# compilados no Play e logs.
- HUD de texto e botão, ações de teclado configuráveis, câmera e troca de cenas.
- Colisores simples e gatilhos, importação OBJ e texturas PNG.
- Exportação Windows pelo menu File para uma pasta vazia.
- Dez aulas da Academy com progresso local e recompensas únicas.

## Limitações da alpha

Colisão usa caixas alinhadas aos eixos e esferas em objetos da raiz. O HUD oferece uma área de texto e um botão configurados por script. OBJ não carrega materiais MTL. O renderer é voltado a cenas pequenas. Scripts C# executam com as permissões do editor, portanto abra apenas projetos confiáveis.

A primeira aula valida a criação de um cubo. As outras nove são concluídas por perguntas, com exercícios sugeridos. O jogo exportado precisa de .NET Framework/WPF no destino. A validação em outro computador ainda está pendente.

## Estrutura

- Editor — interface, Academy, controles e player.
- Engine — cenas, projetos, runtime e persistência.
- Rendering — viewport e leitura visual de assets.
- Physics — gravidade e contatos básicos.
- Scripting — API e compilação de scripts.
- Assets — inventário e importação de arquivos.
- Demos — Coin Run com scripts e assets.
- Games — projetos pessoais locais.
- docs — formato de dados e guia de gameplay.
- bin — saída de compilação e validação.

## Testes

verify.ps1 executa testes de persistência, hierarquia, scripts, fluxo do Coin Run, assets, exportação e progressão da Academy. Também cria uma janela WPF e renderiza uma imagem do editor. Os relatórios são gerados em bin. Isso não substitui uma validação interativa completa nem o teste em outro Windows.

[Guia de gameplay](docs/GAMEPLAY.md) · [Formato de dados](docs/FORMAT.md)

## Licença

A Aurora Engine é disponibilizada sob a **GNU General Public License, versão 3 (GPL-3.0-only)**. O texto completo está em [LICENSE](LICENSE).

Você pode usar, estudar, modificar e compartilhar o projeto. Ao distribuir versões modificadas abrangidas pela licença, mantenha a GPLv3 e disponibilize o código-fonte correspondente conforme seus termos. A licença permite uso comercial. O software é fornecido sem garantia, nos termos da GPLv3.

Esta licença abrange o código, a documentação e os assets originais incluídos neste repositório, incluindo a demo Coin Run. Componentes de terceiros continuam sujeitos às respectivas licenças.

Ao publicar executáveis em uma Release, inclua o arquivo LICENSE e disponibilize o código-fonte correspondente àquela versão, com os scripts necessários para compilá-la. O player exportado incorpora código da engine e sua distribuição também deve respeitar a GPLv3. O exportador atual não monta automaticamente um pacote de conformidade com licença e código-fonte.
