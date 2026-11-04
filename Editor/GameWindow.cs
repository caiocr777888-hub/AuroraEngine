using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Aurora.Engine;
using Aurora.Rendering;

namespace Aurora.Editor {
 public sealed class GameInput {
  readonly Dictionary<string,string[]> bindings=new Dictionary<string,string[]>(StringComparer.OrdinalIgnoreCase);
  public GameInput(string root) {
   bindings["Up"]=new[]{"W","Up"};bindings["Down"]=new[]{"S","Down"};
   bindings["Left"]=new[]{"A","Left"};bindings["Right"]=new[]{"D","Right"};
   bindings["Restart"]=new[]{"R"};
   var path=Path.Combine(root,"input.json");
   if(!File.Exists(path))return;
   foreach(var entry in Store.Decode<InputBinding[]>(File.ReadAllBytes(path)))bindings[entry.Action]=entry.Keys;
  }
  public bool Down(string action){
   string[] keys;
   return bindings.TryGetValue(action,out keys)&&keys.Any(value=>{
    Key key;
    return Enum.TryParse(value,true,out key)&&key>Key.None&&key<=Key.OemClear&&Keyboard.IsKeyDown(key);
   });
  }
 }
 [System.Runtime.Serialization.DataContract]
 public class InputBinding {
  [System.Runtime.Serialization.DataMember] public string Action;
  [System.Runtime.Serialization.DataMember] public string[] Keys;
 }
 public class GameWindow:Window {
  public GameWindow(Project project){
   Title=project.Name;Width=1024;Height=700;WindowStartupLocation=WindowStartupLocation.CenterScreen;
   var view=new SceneView{Is2D=project.Template=="2D",ProjectRoot=project.Root};
   var session=new GameSession(Store.LoadScene(project.ScenePath),project.Root);
   var input=new GameInput(project.Root);
   Content=view;
   Action draw=()=>{view.Draw(session.Runtime.Scene,null,false);view.ShowGame(session.Runtime.Game);};
   var clock=Stopwatch.StartNew();double last=0;
   var timer=new DispatcherTimer{Interval=TimeSpan.FromMilliseconds(16)};
   timer.Tick+=(s,e)=>{double now=clock.Elapsed.TotalSeconds,delta=now-last;last=now;session.Tick(delta,a=>IsActive&&input.Down(a));draw();};
   Closed+=(s,e)=>timer.Stop();
   draw();timer.Start();
  }
 }
 public static class GameExport {
  public static void Write(Project project,string destination){
   destination=Path.GetFullPath(destination);
   var root=Path.GetFullPath(project.Root).TrimEnd(Path.DirectorySeparatorChar);
   if(destination.TrimEnd(Path.DirectorySeparatorChar).Equals(root,StringComparison.OrdinalIgnoreCase)||destination.StartsWith(root+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))
    throw new IOException("Choose a folder outside the project.");
   if(Directory.Exists(destination)&&Directory.GetFileSystemEntries(destination).Length!=0)throw new IOException("Choose an empty export folder.");
   var game=Path.Combine(destination,"Game");
   Directory.CreateDirectory(game);
   foreach(var directory in new[]{"Scenes","Scripts","Assets","Materials"})CopyFolder(Path.Combine(root,directory),Path.Combine(game,directory));
   foreach(var name in new[]{"project.aurora","input.json"}){
    var source=Path.Combine(root,name);if(File.Exists(source))File.Copy(source,Path.Combine(game,name));
   }
   Store.Open(Path.Combine(game,"project.aurora"));
   File.Copy(typeof(GameExport).Assembly.Location,Path.Combine(destination,"Play.exe"));
  }
  static void CopyFolder(string source,string destination){
   if(!Directory.Exists(source))return;
   Directory.CreateDirectory(destination);
   foreach(var file in Directory.GetFiles(source)){
    if(!file.EndsWith(".bak")&&!file.EndsWith(".tmp"))File.Copy(file,Path.Combine(destination,Path.GetFileName(file)));
   }
   foreach(var directory in Directory.GetDirectories(source)){
    if((File.GetAttributes(directory)&FileAttributes.ReparsePoint)==0)CopyFolder(directory,Path.Combine(destination,Path.GetFileName(directory)));
   }
  }
 }
}
