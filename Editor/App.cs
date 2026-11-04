using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using Microsoft.Win32;
using Aurora.Engine;
using Aurora.Rendering;
namespace Aurora.Editor {
 public class App : Application {
  [STAThread] public static int Main(string[] args) {
   if(args.Contains("--test")) return ScriptTests.Run()==0&&SceneFilesTests.Run()==0?Tests.Run():1;
   if(args.Contains("--smoke")) return Tests.Smoke();
   var app=new App(); app.DispatcherUnhandledException+=(s,e)=>{Log.Write("ERROR: "+e.Exception.Message);MessageBox.Show(e.Exception.Message,"Aurora — error");e.Handled=true;};
   app.Resources[SystemColors.ControlBrushKey]=new SolidColorBrush(Color.FromRgb(34,41,54));
   app.Resources[SystemColors.ControlTextBrushKey]=Brushes.Black;
   foreach(var type in new[]{typeof(TextBox),typeof(ComboBox),typeof(ComboBoxItem)}) {
    var inputStyle=new Style(type);
    inputStyle.Setters.Add(new Setter(Control.ForegroundProperty,Brushes.Black));
    inputStyle.Setters.Add(new Setter(Control.BackgroundProperty,Brushes.White));
    app.Resources[type]=inputStyle;
   }
   var style=new Style(typeof(Control));style.Setters.Add(new Setter(Control.FontFamilyProperty,new FontFamily("Segoe UI")));style.Setters.Add(new Setter(Control.FontSizeProperty,12.0));app.Resources[typeof(Control)]=style;
   if(args.Length==2 && args[0]=="--game") app.Run(new GameWindow(Store.Open(args[1])));
   else if(File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Game","project.aurora"))) app.Run(new GameWindow(Store.Open(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Game","project.aurora"))));
   else app.Run(new Hub());
   return 0;
  }
 }
 public static class UI {
  public static Brush Panel=new SolidColorBrush(Color.FromRgb(31,38,50));
  public static Button Button(string text,Action action){var b=new Button{Content=text,Margin=new Thickness(3),Padding=new Thickness(10,6,10,6),Background=new SolidColorBrush(Color.FromRgb(53,66,84)),Foreground=Brushes.White,BorderThickness=new Thickness(0)};
   // Own the template so Windows themes cannot replace dark fills with white.
   b.Template=(ControlTemplate)System.Windows.Markup.XamlReader.Parse(@"<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' TargetType='Button'>
    <Border x:Name='Surface' Background='{TemplateBinding Background}' CornerRadius='3' BorderThickness='1' BorderBrush='#65758C' Padding='{TemplateBinding Padding}'>
     <ContentPresenter x:Name='Label' HorizontalAlignment='Center' VerticalAlignment='Center' RecognizesAccessKey='True' TextElement.Foreground='#FFFFFF'/>
    </Border>
    <ControlTemplate.Triggers>
     <Trigger Property='IsMouseOver' Value='True'><Setter TargetName='Surface' Property='Background' Value='#465C78'/></Trigger>
     <Trigger Property='IsPressed' Value='True'><Setter TargetName='Surface' Property='Background' Value='#274B70'/></Trigger>
     <Trigger Property='IsKeyboardFocused' Value='True'><Setter TargetName='Surface' Property='BorderBrush' Value='#9CCFFF'/></Trigger>
     <Trigger Property='IsEnabled' Value='False'><Setter TargetName='Surface' Property='Background' Value='#29313E'/><Setter TargetName='Surface' Property='BorderBrush' Value='#465163'/><Setter TargetName='Label' Property='TextElement.Foreground' Value='#AEBBCC'/></Trigger>
    </ControlTemplate.Triggers>
   </ControlTemplate>");b.Click+=(s,e)=>action();return b;}
  public static TextBlock Text(string t){return new TextBlock{Text=t,Foreground=Brushes.LightSteelBlue,Margin=new Thickness(8),TextWrapping=TextWrapping.Wrap};}
  public static void Safe(Action a){try{a();}catch(Exception ex){Log.Write("ERROR: "+ex.Message);MessageBox.Show(ex.Message,"Aurora");}}
 }
 public class Hub:Window {
  TextBox name=new TextBox{Text="My Game",Margin=new Thickness(8)},folder=new TextBox{Text=Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"..","Games")),Margin=new Thickness(8)};
  ComboBox template=new ComboBox{Margin=new Thickness(8)};
  static string RecentFile=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"AuroraEngine","recent.txt");
  public Hub(){Title="Aurora • Project Hub";Width=720;Height=570;WindowStartupLocation=WindowStartupLocation.CenterScreen;Background=UI.Panel;var p=new StackPanel{Margin=new Thickness(28)};Content=p;p.Children.Add(new TextBlock{Text="A U R O R A",FontSize=30,Foreground=Brushes.White});p.Children.Add(UI.Text("ENGINE  /  Create worlds. Start small."));p.Children.Add(UI.Text("Project name"));p.Children.Add(name);p.Children.Add(UI.Text("Location"));p.Children.Add(folder);p.Children.Add(UI.Button("Browse directory…",()=>{using(var d=new System.Windows.Forms.FolderBrowserDialog()){if(d.ShowDialog()==System.Windows.Forms.DialogResult.OK)folder.Text=d.SelectedPath;}}));foreach(var t in new[]{"3D","2D","Learning"})template.Items.Add(t);template.SelectedIndex=2;p.Children.Add(template);var row=new StackPanel{Orientation=Orientation.Horizontal};row.Children.Add(UI.Button("Create project",()=>UI.Safe(()=>Launch(Store.Create(folder.Text,name.Text,(string)template.SelectedItem)))));row.Children.Add(UI.Button("Open project…",()=>UI.Safe(()=>{var d=new OpenFileDialog{Filter="Aurora project|*.aurora"};if(d.ShowDialog()==true)Launch(Store.Open(d.FileName));})));p.Children.Add(row);p.Children.Add(UI.Text("RECENT PROJECTS"));var recent=new ListBox{Height=110};if(File.Exists(RecentFile))foreach(var f in File.ReadAllLines(RecentFile).Where(File.Exists).Take(8))recent.Items.Add(f);recent.MouseDoubleClick+=(s,e)=>{if(recent.SelectedItem!=null)UI.Safe(()=>Launch(Store.Open((string)recent.SelectedItem)));};p.Children.Add(recent);}
  void Launch(Project p){Directory.CreateDirectory(Path.GetDirectoryName(RecentFile));var path=Path.Combine(p.Root,"project.aurora");var paths=new List<string>{path};if(File.Exists(RecentFile))paths.AddRange(File.ReadAllLines(RecentFile).Where(x=>x!=path).Take(7));File.WriteAllLines(RecentFile,paths);var editor=new EditorWindow(p);Application.Current.MainWindow=editor;editor.Show();Close();}
 }
 public class EditorWindow:Window {
  AcademyWindow academy;
  GameSession session;
  GameInput gameInput;
  Project project;Scene scene;Runtime runtime;bool paused,dirty,building;string selected;string mode="Move";string currentScenePath;
  SceneView view=new SceneView();TreeView hierarchy=new TreeView();StackPanel inspector=new StackPanel();ListBox assets=new ListBox(),console=new ListBox();TextBlock status=UI.Text("Ready"),viewLabel=UI.Text("SCENE");
  Stack<byte[]> undo=new Stack<byte[]>(),redo=new Stack<byte[]>();DispatcherTimer timer=new DispatcherTimer();Stopwatch time=new Stopwatch();double last;
  Point previous;bool orbit,pan,drag;int axis=-1;Entity dragEntity;Button play,pause,stop;
  Scene Active {get{return runtime==null?scene:runtime.Scene;}} Entity Selected {get{return Active.Entities.Find(x=>x.Id==selected);}}
  public EditorWindow(Project p){project=p;currentScenePath=p.ScenePath;scene=Store.LoadScene(currentScenePath);Title="Aurora • "+p.Name;Width=1440;Height=900;MinWidth=1000;MinHeight=650;Background=UI.Panel;WindowStartupLocation=WindowStartupLocation.CenterScreen;
   var shell=new DockPanel();Content=shell;var menu=new Menu{Background=UI.Panel,Foreground=Brushes.White};DockPanel.SetDock(menu,Dock.Top);shell.Children.Add(menu);
   Menu(menu,"File",new[]{"New scene…","Open scene…","Save scene","Save scene as…","Set as startup scene","Reload scene","Export Windows game…","Project hub","Exit"},new Action[]{NewScene,OpenSceneDialog,Save,SaveAs,SetStartup,Reload,ExportGame,()=>CloseToHub(),()=>Close()});
   Menu(menu,"Edit",new[]{"Undo","Redo","Duplicate","Delete"},new Action[]{Undo,Redo,Duplicate,Delete});
   Menu(menu,"Academy",new[]{"Abrir Academy"},new Action[]{()=>{if(academy==null){academy=new AcademyWindow(()=>scene,()=>runtime==null,()=>Activate()){Owner=this};academy.Closed+=(s,e)=>academy=null;academy.Show();}else academy.Activate();}});
   Menu(menu,"Assets",new[]{"Import file…","Add OBJ to scene…","Apply PNG to selected…","Create C# script…","Attach C# script…","Refresh"},new Action[]{Import,AddModel,ApplyTexture,CreateScript,AttachScript,RefreshAssets});
   Menu(menu,"GameObject",new[]{"Empty","Cube","Sphere","Plane","Cylinder","Sprite"},new Action[]{()=>Add("Empty"),()=>Add("Cube"),()=>Add("Sphere"),()=>Add("Plane"),()=>Add("Cylinder"),()=>Add("Sprite")});
   var components=new[]{"Spin","Controller","Gravity","Camera","Body","BoxCollider","SphereCollider","Trigger"};
   Menu(menu,"Component",components,components.Select<string,Action>(name=>()=>Component(name)).ToArray());
   Menu(menu,"Window",new[]{"Focus selected","Toggle 2D / 3D"},new Action[]{FocusSelected,()=>{view.Is2D=!view.Is2D;Draw();}});
   Menu(menu,"Tools",new[]{"Validate scene","Open project folder"},new Action[]{()=>{scene.Validate();Log.Write("Scene validation passed");},()=>Process.Start("explorer.exe",project.Root)});
   Menu(menu,"Help",new[]{"Controls / About"},new Action[]{()=>MessageBox.Show("Aurora Engine 0.1\n\nRight drag: orbit • Middle drag: pan • Wheel: zoom\nClick: select • F: focus • W/E/R: move/rotate/scale\nDrag colored axes to transform; Inspector provides exact values.\nCtrl+S: save • Ctrl+Z/Y: undo/redo • Delete: delete\nPlay: isolated scene • Controller component: arrow keys\n\n2D uses an orthographic camera and colored sprites.\nPrototype physics: gravity against Y=0.5 only.\nScript compiler, textures, mesh import and game export are future work.","Aurora controls")});
   var bar=new StackPanel{Orientation=Orientation.Horizontal,Margin=new Thickness(5)};DockPanel.SetDock(bar,Dock.Top);shell.Children.Add(bar);bar.Children.Add(UI.Text("AURORA  /  "+p.Name));foreach(var m in new[]{"Move","Rotate","Scale"}){var capture=m;bar.Children.Add(UI.Button(m,()=>{mode=capture;Draw();}));}bar.Children.Add(UI.Button("2D / 3D",()=>{view.Is2D=!view.Is2D;Draw();}));play=UI.Button("▶ Play",Play);pause=UI.Button("Ⅱ Pause",Pause);stop=UI.Button("■ Stop",Stop);bar.Children.Add(play);bar.Children.Add(pause);bar.Children.Add(stop);
   DockPanel.SetDock(status,Dock.Bottom);shell.Children.Add(status);
   var grid=new Grid();shell.Children.Add(grid);grid.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(230)});grid.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(5)});grid.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(1,GridUnitType.Star)});grid.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(5)});grid.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(300)});grid.RowDefinitions.Add(new RowDefinition{Height=new GridLength(1,GridUnitType.Star)});grid.RowDefinitions.Add(new RowDefinition{Height=new GridLength(5)});grid.RowDefinitions.Add(new RowDefinition{Height=new GridLength(220)});
   Put(grid,Panel("HIERARCHY",hierarchy),0,0);var vp=new DockPanel();DockPanel.SetDock(viewLabel,Dock.Top);vp.Children.Add(viewLabel);vp.Children.Add(view);Put(grid,vp,2,0);Put(grid,Panel("INSPECTOR",new ScrollViewer{Content=inspector,VerticalScrollBarVisibility=ScrollBarVisibility.Auto}),4,0);
   var tabs=new TabControl();tabs.Items.Add(new TabItem{Header="Project / Assets",Content=assets});tabs.Items.Add(new TabItem{Header="Console",Content=console});Put(grid,tabs,0,2,5);
   for(int i=1;i<=3;i+=2){var split=new GridSplitter{Width=5,HorizontalAlignment=HorizontalAlignment.Stretch,Background=UI.Panel};Put(grid,split,i,0);}
   Put(grid,new GridSplitter{Height=5,HorizontalAlignment=HorizontalAlignment.Stretch,Background=UI.Panel},0,1,5);
   foreach(var c in new Control[]{hierarchy,assets,console}){c.Background=UI.Panel;c.Foreground=Brushes.WhiteSmoke;c.BorderThickness=new Thickness(0);}
   hierarchy.SelectedItemChanged+=(s,e)=>{if(building)return;var item=hierarchy.SelectedItem as TreeViewItem;if(item!=null){selected=(string)item.Tag;Inspect();Draw();}};
   assets.MouseDoubleClick+=(s,e)=>{var a=assets.SelectedItem as string;if(a==null)return;UI.Safe(()=>{if(string.Equals(Path.GetExtension(a),".scene",StringComparison.OrdinalIgnoreCase))OpenScene(Path.Combine(project.Root,a));else MessageBox.Show(a+"\n\n"+new FileInfo(Path.Combine(project.Root,a)).Length+" bytes","Asset information");});};
   view.MouseDown+=MouseDownView;view.MouseMove+=MouseMoveView;view.MouseUp+=(s,e)=>{orbit=pan=drag=false;view.ReleaseMouseCapture();Inspect();};view.MouseWheel+=(s,e)=>{view.Distance=Math.Max(1,Math.Min(500,view.Distance*Math.Pow(.88,e.Delta/120.0)));view.CameraUpdate();};
   PreviewKeyDown+=Keys;Closing+=(s,e)=>{if(!CanClose())e.Cancel=true;};Closed+=(s,e)=>{timer.Stop();Log.Message-=LogLine;};Log.Message+=LogLine;
   view.Is2D=p.Template=="2D";view.ProjectRoot=p.Root;timer.Interval=TimeSpan.FromMilliseconds(16);timer.Tick+=(s,e)=>{double now=time.Elapsed.TotalSeconds,dt=now-last;last=now;if(runtime!=null&&!paused){var previous=runtime;session.Tick(dt,a=>IsActive&&gameInput.Down(a));runtime=session.Runtime;if(runtime!=previous){selected=null;Refresh();}else Draw();}};time.Start();timer.Start();Refresh();RefreshAssets();Log.Write("Opened "+p.Name+" • "+scene.Entities.Count+" entities");
  }
  static double KeyValue(Key k){return Keyboard.IsKeyDown(k)?1:0;}
  void LogLine(string s){console.Items.Add(s);while(console.Items.Count>500)console.Items.RemoveAt(0);console.ScrollIntoView(s);status.Text=s;}
  void Menu(Menu menu,string name,string[] items,Action[] actions){var m=new MenuItem{Header=name,Foreground=Brushes.White};menu.Items.Add(m);for(int i=0;i<items.Length;i++){var a=actions[i];var item=new MenuItem{Header=items[i],Foreground=Brushes.Black,Background=Brushes.WhiteSmoke};item.Click+=(s,e)=>UI.Safe(a);m.Items.Add(item);}}
  static void Put(Grid g,UIElement el,int col,int row,int span=1){Grid.SetColumn(el,col);Grid.SetRow(el,row);Grid.SetColumnSpan(el,span);g.Children.Add(el);}
  static DockPanel Panel(string title,UIElement body){var p=new DockPanel{Margin=new Thickness(4)};var label=UI.Text(title);DockPanel.SetDock(label,Dock.Top);p.Children.Add(label);p.Children.Add(body);return p;}
  bool Editable(){if(runtime==null)return true;Log.Write("Stop Play before editing the scene");return false;}
  void Snapshot(){undo.Push(Store.Encode(scene));redo.Clear();dirty=true;}
  void Refresh(){building=true;hierarchy.Items.Clear();BuildTree(null,hierarchy.Items);building=false;Inspect();Draw();play.IsEnabled=runtime==null;pause.IsEnabled=stop.IsEnabled=runtime!=null;}
  void BuildTree(string parent,ItemCollection collection){foreach(var e in Active.Entities.Where(x=>x.Parent==parent)){var n=new TreeViewItem{Header=e.Name+(e.Visible?"":" [hidden]"),Tag=e.Id,IsExpanded=true,IsSelected=e.Id==selected,Foreground=Brushes.WhiteSmoke};collection.Add(n);BuildTree(e.Id,n.Items);}}
  void Draw(){view.Draw(Active,selected,runtime==null);view.ShowGame(runtime==null?null:runtime.Game);viewLabel.Text=(runtime==null?"SCENE  /  "+mode:paused?"GAME  /  PAUSED":"GAME  /  RUNNING")+"    •    "+(view.Is2D?"ORTHOGRAPHIC":"PERSPECTIVE");Title="Aurora • "+project.Name+(dirty?" *":"");}
  void Inspect(){inspector.Children.Clear();var e=Selected;if(e==null){inspector.Children.Add(UI.Text("Select an object in the scene or hierarchy."));return;}inspector.Children.Add(UI.Text(e.Shape.ToUpperInvariant()+"  /  "+e.Id.Substring(0,8)));Field("Name",e.Name,v=>e.Name=v);var check=new CheckBox{Content="Visible",IsChecked=e.Visible,Foreground=Brushes.White,Margin=new Thickness(8),IsEnabled=runtime==null};check.Click+=(s,a)=>{Snapshot();e.Visible=check.IsChecked==true;Refresh();};inspector.Children.Add(check);
   Num("Position X",e.X,v=>e.X=v);Num("Position Y",e.Y,v=>e.Y=v);Num("Position Z",e.Z,v=>e.Z=v);Num("Rotation X",e.RX,v=>e.RX=v);Num("Rotation Y",e.RY,v=>e.RY=v);Num("Rotation Z",e.RZ,v=>e.RZ=v);Num("Scale X",e.SX,v=>e.SX=v);Num("Scale Y",e.SY,v=>e.SY=v);Num("Scale Z",e.SZ,v=>e.SZ=v);
   Field("Color #RRGGBB",e.Color,v=>{ColorConverter.ConvertFromString(v);e.Color=v;});inspector.Children.Add(UI.Text("Parent (local transform is preserved)"));var parents=new ComboBox{Margin=new Thickness(8),IsEnabled=runtime==null};parents.Items.Add(new ComboBoxItem{Content="Scene root",Tag=null});foreach(var p in scene.Entities.Where(x=>x.Id!=e.Id&&!Descendant(x,e.Id)))parents.Items.Add(new ComboBoxItem{Content=p.Name,Tag=p.Id});parents.SelectedIndex=0;foreach(ComboBoxItem item in parents.Items)if((string)item.Tag==e.Parent)parents.SelectedItem=item;parents.SelectionChanged+=(s,a)=>{Snapshot();e.Parent=(string)((ComboBoxItem)parents.SelectedItem).Tag;Refresh();};inspector.Children.Add(parents);
   inspector.Children.Add(UI.Text("COMPONENTS"));foreach(var c in e.Components.ToArray()){Num(c.Type+" value",c.Value,v=>c.Value=v);inspector.Children.Add(UI.Button("Remove "+c.Type,()=>{if(!Editable())return;Snapshot();e.Components.Remove(c);Inspect();}));}
   var row=new WrapPanel();foreach(var type in new[]{"Spin","Controller","Gravity"}){var t=type;row.Children.Add(UI.Button("+ "+t,()=>Component(t)));}inspector.Children.Add(row);
  }
  bool Descendant(Entity candidate,string ancestor){while(candidate.Parent!=null){if(candidate.Parent==ancestor)return true;candidate=scene.Entities.Find(x=>x.Id==candidate.Parent);if(candidate==null)break;}return false;}
  void Field(string label,string value,Action<string> set){var row=new Grid{Margin=new Thickness(8,3,8,3)};row.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(112)});row.ColumnDefinitions.Add(new ColumnDefinition());var l=UI.Text(label);l.Margin=new Thickness(0,3,4,0);row.Children.Add(l);var box=new TextBox{Text=value,IsEnabled=runtime==null,Padding=new Thickness(4)};Grid.SetColumn(box,1);row.Children.Add(box);inspector.Children.Add(row);Action commit=()=>{if(box.Text==value)return;try{var backup=Store.Encode(scene);set(box.Text);undo.Push(backup);redo.Clear();dirty=true;value=box.Text;Draw();}catch(Exception ex){box.Text=value;Log.Write("Invalid value: "+ex.Message);}};box.LostKeyboardFocus+=(s,a)=>commit();box.KeyDown+=(s,a)=>{if(a.Key==System.Windows.Input.Key.Enter){commit();a.Handled=true;}};}
  void Num(string label,double value,Action<double> set){Field(label,value.ToString("0.###",CultureInfo.InvariantCulture),s=>{double n;if(!double.TryParse(s.Replace(',','.'),NumberStyles.Float,CultureInfo.InvariantCulture,out n)||double.IsNaN(n)||double.IsInfinity(n)||Math.Abs(n)>1000000)throw new Exception("Use a finite number between -1000000 and 1000000");set(n);});}
  void Add(string shape){if(!Editable())return;Snapshot();var e=new Entity{Name=shape,Shape=shape};scene.Entities.Add(e);selected=e.Id;Refresh();}
  void Component(string type){if(!Editable()||Selected==null)return;if(Selected.Components.Any(x=>x.Type==type))return;Snapshot();Selected.Components.Add(new Component{Type=type,Value=type=="Controller"?3:45});Inspect();Log.Write("Added "+type);}
  void Delete(){if(!Editable()||Selected==null)return;Snapshot();scene.Delete(selected);selected=null;Refresh();}
  void Duplicate(){if(!Editable()||Selected==null)return;Snapshot();var e=Store.Decode<Entity>(Store.Encode(Selected));e.Id=Guid.NewGuid().ToString();e.Name+=" copy";e.X+=1;scene.Entities.Add(e);selected=e.Id;Refresh();}
  void Undo(){if(!Editable()||undo.Count==0)return;redo.Push(Store.Encode(scene));scene=Store.Decode<Scene>(undo.Pop());dirty=true;Refresh();}
  void Redo(){if(!Editable()||redo.Count==0)return;undo.Push(Store.Encode(scene));scene=Store.Decode<Scene>(redo.Pop());dirty=true;Refresh();}
  void Save(){scene.Validate();Store.Save(currentScenePath,scene);dirty=false;RefreshAssets();Log.Write("Saved "+currentScenePath);Draw();}
  void Reload(){OpenScene(currentScenePath);}
  string ChooseScenePath(string title){var d=new SaveFileDialog{Title=title,Filter="Aurora scene|*.scene",DefaultExt=".scene",InitialDirectory=Path.Combine(project.Root,"Scenes"),FileName="New Scene.scene"};return d.ShowDialog()==true?SceneFiles.Resolve(project,d.FileName):null;}
  void NewScene(){if(!Editable())return;var path=ChooseScenePath("Create scene");if(path==null||!CanClose())return;var fresh=new Scene{Name=Path.GetFileNameWithoutExtension(path)};Store.Save(path,fresh);ActivateScene(fresh,path);}
  void OpenSceneDialog(){if(!Editable())return;var d=new OpenFileDialog{Filter="Aurora scene|*.scene",InitialDirectory=Path.Combine(project.Root,"Scenes")};if(d.ShowDialog()==true)OpenScene(d.FileName);}
  void OpenScene(string path){if(!Editable())return;path=SceneFiles.Resolve(project,path);Store.LoadScene(path);if(!CanClose())return;ActivateScene(Store.LoadScene(path),path);}
  void ActivateScene(Scene next,string path){scene=next;currentScenePath=path;selected=null;dirty=false;undo.Clear();redo.Clear();Refresh();RefreshAssets();Log.Write("Opened scene: "+scene.Name);}
  void SaveAs(){if(!Editable())return;var path=ChooseScenePath("Save scene as");if(path==null)return;var copy=Store.Decode<Scene>(Store.Encode(scene));copy.Name=Path.GetFileNameWithoutExtension(path);copy.Validate();Store.Save(path,copy);ActivateScene(copy,path);}
  void SetStartup(){if(!Editable())return;Save();SceneFiles.SetStartup(project,currentScenePath);Log.Write("Startup scene: "+project.StartupScene);}
  bool CanClose(){if(!dirty)return true;var r=MessageBox.Show("Save scene changes?","Aurora",MessageBoxButton.YesNoCancel);if(r==MessageBoxResult.Cancel)return false;if(r==MessageBoxResult.Yes){try{Save();}catch(Exception e){MessageBox.Show(e.Message);return false;}}return true;}
  void CloseToHub(){if(!CanClose())return;dirty=false;var hub=new Hub();Application.Current.MainWindow=hub;hub.Show();Close();}
  void Play(){UI.Safe(()=>{if(runtime!=null)return;scene.Validate();gameInput=new GameInput(project.Root);session=new GameSession(scene,project.Root);runtime=session.Runtime;paused=false;Refresh();});}
  void ExportGame(){if(!Editable())return;using(var dialog=new System.Windows.Forms.FolderBrowserDialog{Description="Select an empty folder outside the project"}){if(dialog.ShowDialog()!=System.Windows.Forms.DialogResult.OK)return;Save();GameExport.Write(project,dialog.SelectedPath);Log.Write("Exported Windows game: "+Path.Combine(dialog.SelectedPath,"Play.exe"));}}
  string ChooseAsset(string extension){
   var dialog=new OpenFileDialog{Filter=extension.ToUpperInvariant()+" asset|*"+extension,InitialDirectory=Path.Combine(project.Root,"Assets")};
   if(dialog.ShowDialog()!=true)return null;
   var path=RenderAssets.Resolve(project.Root,dialog.FileName,extension);
   return path.Substring(Path.GetFullPath(project.Root).TrimEnd(Path.DirectorySeparatorChar).Length+1);
  }
  void AddModel(){
   if(!Editable())return;
   var path=ChooseAsset(".obj");if(path==null)return;
   view.Assets.Model(project.Root,path);
   Snapshot();var entity=new Entity{Name=Path.GetFileNameWithoutExtension(path),Shape="Model",Model=path};
   scene.Entities.Add(entity);selected=entity.Id;Refresh();
  }
  void ApplyTexture(){
   if(!Editable()||Selected==null)return;
   var path=ChooseAsset(".png");if(path==null)return;
   view.Assets.Texture(project.Root,path);
   Snapshot();Selected.Texture=path;Refresh();
  }
  void CreateScript(){if(!Editable())return;var d=new SaveFileDialog{Filter="C# script|*.cs",FileName="MyBehavior.cs",InitialDirectory=Path.Combine(project.Root,"Scripts")};if(d.ShowDialog()!=true)return;var name=Path.GetFileNameWithoutExtension(d.FileName);if(!System.Text.RegularExpressions.Regex.IsMatch(name,@"^[A-Za-z_][A-Za-z0-9_]*$"))throw new Exception("Use a C# class name, for example MyBehavior.");CheckScriptPath(d.FileName);File.WriteAllText(d.FileName,"using Aurora.Scripting;\n\npublic class "+name+" : Script\n{\n    public override void Start() { Print(\"Script started\"); }\n    public override void Update(double deltaTime)\n    {\n        Entity.RY += 60 * deltaTime;\n    }\n}\n");RefreshAssets();Log.Write("Created "+d.FileName);Process.Start("notepad.exe","\""+d.FileName+"\"");}
  void CheckScriptPath(string path){var root=Path.GetFullPath(Path.Combine(project.Root,"Scripts"))+Path.DirectorySeparatorChar;if(!Path.GetFullPath(path).StartsWith(root,StringComparison.OrdinalIgnoreCase))throw new Exception("Save scripts inside the project's Scripts directory.");}
  void AttachScript(){if(!Editable()||Selected==null){Log.Write("Select an object before attaching a script");return;}var d=new OpenFileDialog{Filter="C# script|*.cs",InitialDirectory=Path.Combine(project.Root,"Scripts")};if(d.ShowDialog()!=true)return;CheckScriptPath(d.FileName);Component("Script:"+Path.GetFileNameWithoutExtension(d.FileName));}
  void Pause(){if(runtime==null)return;paused=!paused;pause.Content=paused?"▶ Resume":"Ⅱ Pause";Draw();Log.Write(paused?"Runtime paused":"Runtime resumed");}
  void Stop(){if(runtime==null)return;runtime=null;session=null;paused=false;pause.Content="Ⅱ Pause";Refresh();Log.Write("Stopped • edit scene restored");}
  void RefreshAssets(){assets.Items.Clear();foreach(var path in Aurora.Assets.AssetDatabase.List(project.Root))assets.Items.Add(path);}
  void Import(){var d=new OpenFileDialog{Multiselect=true};if(d.ShowDialog()!=true)return;foreach(var f in d.FileNames){var dest=Path.Combine(project.Root,"Assets",Path.GetFileName(f));if(File.Exists(dest)){Log.Write("Skipped existing asset: "+dest);continue;}Aurora.Assets.AssetDatabase.Import(project.Root,f);Log.Write("Imported "+Path.GetFileName(f));}RefreshAssets();}
  void FocusSelected(){if(Selected!=null)view.Target=SceneView.World(Selected,Active).Transform(new Point3D());else view.Target=new Point3D();view.Distance=7;Draw();}
  void Keys(object sender,KeyEventArgs e){if(Keyboard.FocusedElement is TextBox||Keyboard.FocusedElement is ComboBox)return;if((Keyboard.Modifiers&ModifierKeys.Control)!=0){if(e.Key==System.Windows.Input.Key.S)UI.Safe(Save);if(e.Key==System.Windows.Input.Key.Z)Undo();if(e.Key==System.Windows.Input.Key.Y)Redo();return;}if(e.Key==System.Windows.Input.Key.Delete)Delete();if(e.Key==System.Windows.Input.Key.F)FocusSelected();if(e.Key==System.Windows.Input.Key.W)mode="Move";if(e.Key==System.Windows.Input.Key.E)mode="Rotate";if(e.Key==System.Windows.Input.Key.R)mode="Scale";}
  void MouseDownView(object sender,MouseButtonEventArgs e){Keyboard.ClearFocus();previous=e.GetPosition(view);orbit=e.ChangedButton==MouseButton.Right;pan=e.ChangedButton==MouseButton.Middle;if(orbit||pan){view.CaptureMouse();return;}if(e.ChangedButton!=MouseButton.Left||runtime!=null)return;string hit=null;axis=-1;VisualTreeHelper.HitTest(view.View,null,r=>{var mesh=r as RayMeshGeometry3DHitTestResult;if(mesh!=null){var model=mesh.ModelHit as GeometryModel3D;int a;if(model!=null&&view.Handles.TryGetValue(model,out a)){axis=a;return HitTestResultBehavior.Stop;}string id;if(model!=null&&view.Picks.TryGetValue(model,out id)){hit=id;return HitTestResultBehavior.Stop;}}return HitTestResultBehavior.Continue;},new PointHitTestParameters(previous));if(axis>=0&&Selected!=null){Snapshot();dragEntity=Selected;drag=true;view.CaptureMouse();}else{selected=hit;Refresh();}}
  void MouseMoveView(object sender,MouseEventArgs e){var p=e.GetPosition(view);double x=p.X-previous.X,y=p.Y-previous.Y;previous=p;if(orbit&&!view.Is2D){view.Yaw-=x*.4;view.Pitch=Math.Max(-85,Math.Min(85,view.Pitch+y*.4));view.CameraUpdate();}if(pan){var target=view.Target;target.X-=x*view.Distance/700;target.Y+=y*view.Distance/700;view.Target=target;view.CameraUpdate();}if(drag&&dragEntity!=null){double d=(axis==1?-y:x)*view.Distance/600;var a=dragEntity;if(mode=="Move"){if(axis==0)a.X+=d;if(axis==1)a.Y+=d;if(axis==2)a.Z+=d;}if(mode=="Rotate"){if(axis==0)a.RX+=d*40;if(axis==1)a.RY+=d*40;if(axis==2)a.RZ+=d*40;}if(mode=="Scale"){if(axis==0)a.SX=Math.Max(.01,a.SX+d);if(axis==1)a.SY=Math.Max(.01,a.SY+d);if(axis==2)a.SZ=Math.Max(.01,a.SZ+d);}Draw();}}
 }
}

