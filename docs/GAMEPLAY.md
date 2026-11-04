# Jogar, criar e exportar na Aurora

Abra `bin/AuroraEngine.exe`. O gerenciador de projetos abre direto. Para experimentar os recursos, abra `Demos/CoinRun/project.aurora`, clique em **Play** e depois em **Jogar**. Use WASD ou setas para coletar as cinco moedas. Pressione R ou **Reiniciar** para voltar ao início.

- **Interface e controles.** No script, configure `Game.Text`, `Game.ButtonLabel` e `Game.ButtonAction`. O HUD desta versão tem uma área de texto e um botão, definidos por código. Use `Game.IsDown("Left")` ou outra ação declarada no `input.json` na raiz do projeto. O Coin Run inclui um arquivo de exemplo.
- **Colisores.** Adicione `Body` e um `BoxCollider` ou `SphereCollider` ao objeto móvel. Dê um colisor ao obstáculo. Adicione `Trigger` para detectar a entrada em `Game.TriggerEntered` sem bloquear o corpo. O sistema usa objetos na raiz, caixas alinhadas aos eixos e bloqueio por restauração da posição. Rotação, corpos móveis entre si e detecção contínua ficam fora desta versão.
- **Assets.** Use **Assets → Import file** para copiar PNG e OBJ. Use **Add OBJ to scene** para inserir um modelo e **Apply PNG to selected** para aplicar uma imagem. A prévia aparece no viewport. Teste com `Assets/Marker.obj` do Coin Run. O OBJ aceita faces trianguladas ou convexas e coordenadas UV. Materiais MTL e outros formatos ainda não são lidos.
- **Câmera e cenas.** Adicione `Camera` a um Empty e ajuste sua transformação. A câmera olha para o eixo local −Z. O jogo usa a primeira câmera da cena e, se ela não existir, usa a vista padrão. Em scripts, `Game.LoadScene("Scenes/Main.scene")` troca a cena no próximo quadro. O menu do Coin Run mostra esse fluxo.
- **Exportação.** Use **File → Export Windows game** e escolha uma pasta vazia fora do projeto. Abra `Play.exe` dentro da pasta de exportação. Mantenha `Play.exe` e a pasta `Game` juntos ao compartilhar. O computador de destino precisa do .NET Framework com WPF.

Em **Academy → Abrir Academy**, a primeira aula detecta a criação do cubo. As nove seguintes apresentam uma explicação, um exercício no editor e uma pergunta com feedback. A resposta correta conclui a aula, libera a seguinte e concede 50 XP uma única vez. As dez aulas totalizam 500 XP.

Para recompilar no mesmo caminho, execute `build.ps1`. Use `bin/AuroraEngine.exe --test` para os testes e `--smoke` para validar a abertura e a renderização do editor.

