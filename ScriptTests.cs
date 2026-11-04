using System;
using System.IO;
using Aurora.Engine;
namespace Aurora.Editor {
 public static class ScriptTests {
  public static int Run(){try{
   var learning=new LearningProgress();
   learning.CompleteExtra(0);
   learning.Complete();learning.Complete();
   learning.CompleteExtra(1);
   if(learning.XP!=50)throw new Exception("Academy locked lesson awarded XP");
   for(int i=0;i<9;i++){learning.CompleteExtra(i);learning.CompleteExtra(i);}
   var restored=Store.Decode<LearningProgress>(Store.Encode(learning));
   if(restored.XP!=500||!restored.Completed||restored.ExtraCompleted!=9)throw new Exception("Academy reward persistence or duplicate reward failure");
   var demo=Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"../Demos/CoinRun/project.aurora"));
   if(File.Exists(demo)){
    var project=Store.Open(demo);
    var assets=new Aurora.Rendering.RenderAssets();
    if(assets.Model(project.Root,"Assets/Marker.obj").TriangleIndices.Count!=12)throw new Exception("OBJ import failed");
    var texture=assets.Texture(project.Root,"Assets/Floor.png");
    if(texture==null)throw new Exception("PNG import failed");
    var session=new GameSession(Store.LoadScene(project.ScenePath),project.Root);
    session.Runtime.Game.ButtonAction();session.Tick(.016,a=>false);
    var game=session.Runtime;
    var player=game.Scene.Entities.Find(e=>e.Name=="Player");
    if(player==null)throw new Exception("Menu did not load the arena");
    double initialZ=player.Z;
    for(int i=0;i<20;i++)session.Tick(.05,a=>a=="Down");
    if(player.Z<=initialZ||player.Z>4.7)throw new Exception("Movement or wall collision failed");
    foreach(var coin in game.Scene.Entities.FindAll(e=>e.Name.StartsWith("Coin "))){
     player.X=coin.X;player.Z=coin.Z;game.Tick(.016,0,0);
     if(coin.Visible)throw new Exception("Coin collection failed");
    }
    if(player.Color!="#69F0AE"||!game.Game.Text.Contains("5 / 5"))throw new Exception("Win state failed");
    game.Game.ButtonAction();
    if(!game.Scene.Entities.Find(e=>e.Name=="Coin 1").Visible||!game.Game.Text.Contains("0 / 5"))throw new Exception("Restart failed");
    var export=Path.Combine(Path.GetTempPath(),"AuroraExport-"+Guid.NewGuid());
    GameExport.Write(project,export);
    if(!File.Exists(Path.Combine(export,"Play.exe"))||!File.Exists(Path.Combine(export,"Game","Scripts","CoinGame.cs")))throw new Exception("Export missing game files");
    Store.Open(Path.Combine(export,"Game","project.aurora"));
   }
   var root=Path.Combine(Path.GetTempPath(),"AuroraScriptTest-"+Guid.NewGuid());Directory.CreateDirectory(Path.Combine(root,"Scripts"));var file=Path.Combine(root,"Scripts","Motion.cs");
   File.WriteAllText(file,"using Aurora.Scripting; public class Motion : Script { public override void Start(){Entity.X=2;} public override void Update(double dt){Entity.X+=10*dt;} }");
   var scene=new Scene();var entity=new Entity();entity.Components.Add(new Component{Type="Script:Motion"});scene.Entities.Add(entity);
   var runtime=new Runtime(scene,root);runtime.Tick(.05,0,0);if(Math.Abs(runtime.Scene.Entities[0].X-2.5)>.00001||entity.X!=0)throw new Exception("Script execution or isolation failed");
   var saved=Store.Decode<Scene>(Store.Encode(scene));if(saved.Entities[0].Components[0].Type!="Script:Motion")throw new Exception("Script binding persistence failed");
   File.WriteAllText(file,"invalid C#");bool rejected=false;try{new Runtime(scene,root);}catch{rejected=true;}if(!rejected)throw new Exception("Compilation error not rejected");
   File.WriteAllText(file,"using Aurora.Scripting; public class Motion : Script { public override void Update(double dt){Entity.X++;throw new System.Exception(\"test\");} }");
   runtime=new Runtime(scene,root);runtime.Tick(.05,0,0);runtime.Tick(.05,0,0);if(runtime.Scene.Entities[0].X!=1)throw new Exception("Faulty script not disabled");
   File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"script-tests.txt"),"PASS: compilation, Start/Update, edit-scene isolation, binding persistence, compile error rejection, runtime exception containment");return 0;
  }catch(Exception ex){File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"script-tests.txt"),ex.ToString());return 1;}}
 }
}
