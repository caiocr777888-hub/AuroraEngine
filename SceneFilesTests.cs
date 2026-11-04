using System;
using System.IO;
using Aurora.Engine;
namespace Aurora.Editor {
 public static class SceneFilesTests {
  public static int Run(){try{
   var root=Path.Combine(Path.GetTempPath(),"AuroraScenes-"+Guid.NewGuid());var p=Store.Create(root,"Project","Learning");
   var original=File.ReadAllBytes(p.ScenePath);var second=SceneFiles.Resolve(p,"Scenes/Level2.scene");
   var scene=new Scene{Name="Level2"};scene.Entities.Add(new Entity{Name="Player"});Store.Save(second,scene);
   SceneFiles.SetStartup(p,second);var reopened=Store.Open(Path.Combine(p.Root,"project.aurora"));
   if(Store.LoadScene(reopened.ScenePath).Entities[0].Name!="Player")throw new Exception("Startup scene persistence failed");
   if(Convert.ToBase64String(original)!=Convert.ToBase64String(File.ReadAllBytes(Path.Combine(p.Root,"Scenes/Main.scene"))))throw new Exception("Original scene changed");
   foreach(var invalid in new[]{"../outside.scene","Assets/file.txt"}){bool rejected=false;try{SceneFiles.Resolve(p,invalid);}catch{rejected=true;}if(!rejected)throw new Exception("Invalid scene path accepted");}
   var prior=p.StartupScene;try{SceneFiles.SetStartup(p,Path.Combine(p.Root,"Scenes/missing.scene"));}catch(FileNotFoundException){}
   if(prior!=p.StartupScene)throw new Exception("Failed change modified startup scene");
   File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"scene-tests.txt"),"PASS: multiple scenes, startup persistence, original scene preservation, invalid paths, failed startup change");return 0;
  }catch(Exception ex){File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"scene-tests.txt"),ex.ToString());return 1;}}
 }
}
