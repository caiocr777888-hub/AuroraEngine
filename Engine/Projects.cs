using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
namespace Aurora.Engine {
 [DataContract] public class Project {
  [DataMember] public int Version=1;
  [DataMember] public string Name;
  [DataMember] public string Template;
  [DataMember] public string StartupScene="Scenes/Main.scene";
  public string Root;
  public string ScenePath { get { var p=Path.GetFullPath(Path.Combine(Root,StartupScene)); if(!p.StartsWith(Path.GetFullPath(Root)+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase)) throw new Exception("Scene outside project"); return p; } }
 }
 public static class Store {
  public static byte[] Encode<T>(T value) { using(var m=new MemoryStream()) { new DataContractJsonSerializer(typeof(T)).WriteObject(m,value); return m.ToArray(); } }
  public static T Decode<T>(byte[] bytes) { using(var m=new MemoryStream(bytes)) return (T)new DataContractJsonSerializer(typeof(T)).ReadObject(m); }
  public static void Save<T>(string path,T value) { Directory.CreateDirectory(Path.GetDirectoryName(path)); var tmp=path+".tmp"; File.WriteAllBytes(tmp,Encode(value)); if(File.Exists(path)) File.Replace(tmp,path,path+".bak"); else File.Move(tmp,path); }
  public static Scene LoadScene(string path) { var s=Decode<Scene>(File.ReadAllBytes(path)); s.Validate(); return s; }
  public static Project Open(string path) { var p=Decode<Project>(File.ReadAllBytes(path)); if(p.Version!=1) throw new Exception("Unsupported project version: "+p.Version); p.Root=Path.GetDirectoryName(Path.GetFullPath(path)); LoadScene(p.ScenePath); return p; }
  public static Project Create(string directory,string name,string template) {
   if(string.IsNullOrWhiteSpace(name)||name.IndexOfAny(Path.GetInvalidFileNameChars())>=0||name=="."||name=="..") throw new Exception("Invalid project name");
   var root=Path.Combine(directory,name); if(Directory.Exists(root)&&Directory.GetFileSystemEntries(root).Length>0) throw new Exception("Choose an empty project directory");
   var p=new Project{Name=name,Template=template,Root=root}; foreach(var d in new[]{"Scenes","Assets","Scripts","Materials"}) Directory.CreateDirectory(Path.Combine(root,d));
   var s=new Scene(); if(template=="Learning") {
    s.Entities.Add(new Entity{Name="Ground",Shape="Plane",SX=12,SZ=12,Color="#46566E",Y=-0.5});
    var e=new Entity{Name="Spinning cube",Y=0.5}; e.Components.Add(new Component()); s.Entities.Add(e);
    s.Entities.Add(new Entity{Name="Sphere",Shape="Sphere",X=2,Y=0.5,Color="#EAA35E"});
   }
   Save(p.ScenePath,s); Save(Path.Combine(root,"project.aurora"),p); return p;
  }
 }
 public static class Log {
  public static event Action<string> Message;
  public static void Write(string text) { var s=DateTime.Now.ToString("HH:mm:ss")+"  "+text; if(Message!=null) Message(s); try { var dir=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"AuroraEngine"); Directory.CreateDirectory(dir); File.AppendAllText(Path.Combine(dir,"editor.log"),s+Environment.NewLine); } catch(IOException) {} }
 }
}
