using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Aurora.Engine;
namespace Aurora.Editor {
 [DataContract] public class LearningProgress {
  [DataMember] public bool Completed;
  [DataMember] public int XP;
  [DataMember] public int ExtraCompleted;
  public void Complete(){if(!Completed){Completed=true;XP+=50;}}
  public void CompleteExtra(int lesson){if(Completed&&lesson==ExtraCompleted&&lesson<AcademyLessons.All.Length){ExtraCompleted++;XP+=50;}}
 }
 public class AcademyWindow:Window {
  readonly Func<Scene> getScene; readonly Func<bool> editing; readonly Action returnToEditor;
  StackPanel content=new StackPanel{Margin=new Thickness(28)};LearningProgress progress=new LearningProgress();int step;
  int extra=-1;
  HashSet<string> baseline;Scene exerciseScene;DispatcherTimer timer=new DispatcherTimer();
  string path=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"AuroraEngine","academy.json");
  public AcademyWindow(Func<Scene> scene,Func<bool> canEdit,Action focus){getScene=scene;editing=canEdit;returnToEditor=focus;Title="Aurora Academy";Width=650;Height=650;MinWidth=460;MinHeight=450;Background=UI.Panel;Content=new ScrollViewer{Content=content,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};
   try{if(File.Exists(path))progress=Store.Decode<LearningProgress>(File.ReadAllBytes(path));}catch(Exception ex){Log.Write("Academy progress could not be read: "+ex.Message);}
   timer.Interval=TimeSpan.FromMilliseconds(250);timer.Tick+=(s,e)=>{if(step!=4||baseline==null||!editing())return;var active=getScene();if(!object.ReferenceEquals(active,exerciseScene)){exerciseScene=active;baseline=new HashSet<string>(active.Entities.Select(x=>x.Id));return;}if(active.Entities.Any(x=>x.Shape=="Cube"&&!baseline.Contains(x.Id))){timer.Stop();step=5;Render();Activate();}};Closed+=(s,e)=>timer.Stop();Render();
  }
  void Heading(string text){content.Children.Add(new TextBlock{Text=text,FontSize=24,Foreground=Brushes.White,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(8)});}
  void Next(){step++;Render();}
  void Render(){content.Children.Clear();Heading("AURORA  /  Academy");content.Children.Add(UI.Text(progress.XP+" XP  •  "+((progress.Completed?1:0)+progress.ExtraCompleted)+" / 10 aulas concluídas"+(progress.Completed?"  •  Criador Iniciante":"")));
   if(extra>=0){RenderExtra();return;}
   content.Children.Add(new ProgressBar{Minimum=0,Maximum=5,Value=step,Height=8,Margin=new Thickness(8),Foreground=Brushes.MediumPurple});
   if(step==0){
    Heading("Sua primeira aventura no desenvolvimento de jogos");
    content.Children.Add(UI.Button(progress.Completed?"1. Revisar primeira aula":"1. Começar primeira aula",Next));
    for(int i=0;i<AcademyLessons.All.Length;i++){
     int index=i;bool unlocked=progress.Completed&&i<=progress.ExtraCompleted;
     var button=UI.Button((i+2)+". "+AcademyLessons.All[i].Title+(unlocked?"":" · Bloqueada"),()=>{extra=index;Render();});
     button.IsEnabled=unlocked;content.Children.Add(button);
    }
   }
   if(step==1){Heading("1. O que é uma Game Engine?");content.Children.Add(UI.Text("Uma Game Engine é uma ferramenta que reúne recursos para criar jogos, como gráficos, objetos, programação, sons e física."));content.Children.Add(UI.Button("Continuar",Next));}
   if(step==2){Heading("2. Conhecendo o editor");var map=new Grid();map.ColumnDefinitions.Add(new ColumnDefinition());map.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(2,GridUnitType.Star)});map.ColumnDefinitions.Add(new ColumnDefinition());map.RowDefinitions.Add(new RowDefinition());map.RowDefinitions.Add(new RowDefinition());string[] text={"Hierarchy\nLista os objetos da cena","Scene\nMostra e permite editar o mundo","Inspector\nEdita propriedades do objeto","Project\nArquivos, cenas e scripts"};for(int i=0;i<4;i++){var box=new Border{Background=new SolidColorBrush(Color.FromRgb(44,57,78)),CornerRadius=new CornerRadius(6),Margin=new Thickness(3),Padding=new Thickness(8),Child=UI.Text(text[i])};Grid.SetColumn(box,i==3?0:i);Grid.SetRow(box,i==3?1:0);if(i==3)Grid.SetColumnSpan(box,3);map.Children.Add(box);}content.Children.Add(map);content.Children.Add(UI.Button("Continuar",Next));}
   if(step==3){Heading("3. Qual painel mostra os objetos presentes na cena?");var feedback=UI.Text("");foreach(var answer in new[]{"Inspector","Hierarchy","Console","Project"}){var choice=answer;content.Children.Add(UI.Button(answer,()=>{feedback.Text=choice=="Hierarchy"?"Correto! Hierarchy lista os objetos da cena.":"Ainda não. Procure o painel que organiza os objetos da cena.";feedback.Foreground=choice=="Hierarchy"?Brushes.LightGreen:Brushes.Salmon;if(choice=="Hierarchy"){step=4;var next=UI.Button("Continuar",Render);content.Children.Add(next);}}));}content.Children.Add(feedback);}
   if(step==4){Heading("4. Seu primeiro objeto");content.Children.Add(UI.Text("Volte ao editor e use GameObject → Cube para criar um cubo novo. Cubos que já existiam não contam. Saia do modo Play antes de criar. A aula detectará o objeto real automaticamente."));exerciseScene=getScene();baseline=new HashSet<string>(exerciseScene.Entities.Select(x=>x.Id));timer.Start();}
   if(step==5){Heading("Parabéns! Você concluiu sua primeira aula de desenvolvimento de jogos!");content.Children.Add(UI.Text("Recompensa: 50 XP\nConquista: Criador Iniciante\nRevisões não concedem XP adicional."));try{var updated=CopyProgress();updated.Complete();Store.Save(path,updated);progress=updated;content.Children.Add(UI.Text("Progresso salvo • "+progress.XP+" XP"));}catch(Exception ex){content.Children.Add(UI.Text("Não foi possível salvar: "+ex.Message));content.Children.Add(UI.Button("Tentar salvar novamente",Render));}content.BeginAnimation(OpacityProperty,new DoubleAnimation(0,1,TimeSpan.FromMilliseconds(500)));content.Children.Add(UI.Button("Voltar à trilha",()=>{step=0;Render();}));}
   content.Children.Add(UI.Button("Retornar ao editor",()=>{returnToEditor();}));
  }
  LearningProgress CopyProgress(){return new LearningProgress{Completed=progress.Completed,XP=progress.XP,ExtraCompleted=progress.ExtraCompleted};}
  void RenderExtra(){
   var lesson=AcademyLessons.All[extra];
   Heading((extra+2)+". "+lesson.Title);
   content.Children.Add(UI.Text(lesson.Explanation));
   content.Children.Add(UI.Text("Pratique no editor — "+lesson.Exercise));
   content.Children.Add(UI.Text(lesson.Question));
   var feedback=UI.Text("");
   for(int offset=0;offset<lesson.Answers.Length;offset++){
    int answer=(offset+extra)%lesson.Answers.Length;
    content.Children.Add(UI.Button(lesson.Answers[answer],()=>{
     if(answer!=0){feedback.Text="Ainda não. Releia a explicação e tente novamente.";return;}
     try{
      var updated=CopyProgress();updated.CompleteExtra(extra);
      bool earned=updated.XP>progress.XP;
      Store.Save(path,updated);progress=updated;
      feedback.Text=earned?"Correto! +50 XP.":"Correto! Aula já concluída.";
      feedback.Foreground=Brushes.LightGreen;
     }catch(Exception ex){feedback.Text="Não foi possível salvar: "+ex.Message;}
    }));
   }
   content.Children.Add(feedback);
   content.Children.Add(UI.Button("Retornar ao editor",returnToEditor));
   content.Children.Add(UI.Button("Voltar à trilha",()=>{extra=-1;step=0;Render();}));
  }
 }
}
