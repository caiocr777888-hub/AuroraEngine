namespace Aurora.Editor {
 internal sealed class AcademyLesson {
  public readonly string Title, Explanation, Exercise, Question;
  public readonly string[] Answers;
  public AcademyLesson(string title,string explanation,string exercise,string question,params string[] answers){
   Title=title;Explanation=explanation;Exercise=exercise;Question=question;Answers=answers;
  }
 }
 internal static class AcademyLessons {
  public static readonly AcademyLesson[] All={
   new AcademyLesson("Transformações","Posição desloca um objeto, rotação muda sua orientação e escala muda seu tamanho.","Crie um cubo e altere Scale X no Inspector para 2.","Qual propriedade muda o tamanho?","Escala","Posição","Rotação"),
   new AcademyLesson("Hierarquia","O filho herda a transformação do pai. Mover um pai move o conjunto sem alterar as coordenadas locais dos filhos.","Crie um Empty e escolha-o como Parent de um cubo. Mova o Empty.","O que acontece ao mover o pai?","O filho acompanha","O filho é apagado","Nada muda no filho"),
   new AcademyLesson("Cenas e salvamento","Cada cena guarda um conjunto de entidades. A cena inicial é a primeira aberta pelo jogo.","Use File → Save scene as e verifique a cópia no painel Project.","Qual comando cria uma cópia da cena?","Save scene as","Reload scene","Set as startup scene"),
   new AcademyLesson("Jogando e testando","Play trabalha numa cópia da cena. Pause congela o jogo. Stop descarta mudanças do runtime.","Adicione Spin, clique Play, pause e depois use Stop.","Qual comando retorna à cena editável?","Stop","Pause","Play"),
   new AcademyLesson("Modelos e texturas","Um modelo OBJ descreve a geometria. Uma imagem PNG pode virar a textura visível de um objeto.","Importe um PNG em Assets e use Apply PNG to selected em um plano.","Qual formato guarda a geometria nesta engine?","OBJ","PNG","JSON"),
   new AcademyLesson("O primeiro script","Start roda quando a instância começa. Update roda a cada quadro durante a execução. Use deltaTime para manter o movimento proporcional ao tempo.","Crie um script em Assets, associe a um cubo e teste o template em Play.","Qual método roda a cada quadro?","Update","Start","Stop"),
   new AcademyLesson("Controles e interface","Ações traduzem teclas em intenções como Left ou Restart. A interface mostra o estado do jogo em texto e botões.","Abra Coin Run e observe como a pontuação muda ao coletar moedas.","Qual arquivo permite configurar as teclas?","input.json","Main.scene","project.aurora"),
   new AcademyLesson("Colisões e gatilhos","Colisores sólidos bloqueiam um Body. Um Trigger detecta entrada sem bloquear o movimento.","No Coin Run, encoste numa parede e depois atravesse a posição de uma moeda.","Qual componente detecta contato sem bloquear?","Trigger","Camera","Spin"),
   new AcademyLesson("Câmera e entrega","O componente Camera define o ponto de vista do jogo. A exportação inclui o player Windows e os arquivos do projeto.","Exporte Coin Run para uma pasta vazia e abra Play.exe.","Qual arquivo inicia o jogo exportado?","Play.exe","input.json","Main.scene")
  };
 }
}
