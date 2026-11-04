using System;
using System.IO;
namespace Aurora.Engine {
 public static class SceneFiles {
  public static string Resolve(Project project,string path) {
   var full=Path.GetFullPath(Path.IsPathRooted(path)?path:Path.Combine(project.Root,path));
   var root=Path.GetFullPath(project.Root).TrimEnd(Path.DirectorySeparatorChar)+Path.DirectorySeparatorChar;
   if(!full.StartsWith(root,StringComparison.OrdinalIgnoreCase)||!string.Equals(Path.GetExtension(full),".scene",StringComparison.OrdinalIgnoreCase))throw new Exception("Choose a .scene file inside this project.");
   return full;
  }
  public static void SetStartup(Project project,string path) {
   path=Resolve(project,path);Store.LoadScene(path);
   var previous=project.StartupScene;
   project.StartupScene=path.Substring(Path.GetFullPath(project.Root).TrimEnd(Path.DirectorySeparatorChar).Length+1).Replace('\\','/');
   try{Store.Save(Path.Combine(project.Root,"project.aurora"),project);}catch{project.StartupScene=previous;throw;}
  }
 }
}
