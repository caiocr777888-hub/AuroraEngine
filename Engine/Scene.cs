using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
namespace Aurora.Engine {
 [DataContract] public class Entity {
  [DataMember] public string Id=Guid.NewGuid().ToString();
  [DataMember] public string Parent;
  [DataMember] public string Name="Object";
  [DataMember] public string Shape="Cube";
  [DataMember] public bool Visible=true;
  [DataMember] public double X,Y,Z,RX,RY,RZ;
  [DataMember] public double SX=1,SY=1,SZ=1;
  [DataMember] public string Color="#70B5ED";
  [DataMember(EmitDefaultValue=false)] public string Texture;
  [DataMember(EmitDefaultValue=false)] public string Model;
  [DataMember] public List<Component> Components=new List<Component>();
 }
 [DataContract] public class Component {
  [DataMember] public string Type="Spin";
  [DataMember] public double Value=45;
 }
 [DataContract] public class Scene {
  [DataMember] public int Version=1;
  [DataMember] public string Name="Main";
  [DataMember] public List<Entity> Entities=new List<Entity>();
  public void Validate() {
   if(Version!=1) throw new Exception("Unsupported scene version: "+Version);
   if(Entities==null || Entities.Count>10000) throw new Exception("Invalid entity list");
   var ids=new HashSet<string>();
   foreach(var e in Entities) {
    if(e==null || string.IsNullOrEmpty(e.Id) || !ids.Add(e.Id)) throw new Exception("Invalid or duplicate entity ID");
    if(e.Components==null) e.Components=new List<Component>();
    foreach(double v in new[]{e.X,e.Y,e.Z,e.RX,e.RY,e.RZ,e.SX,e.SY,e.SZ}) if(double.IsNaN(v)||double.IsInfinity(v)) throw new Exception("Non-finite transform");
   }
   foreach(var e in Entities) {
    var visited=new HashSet<string>(); var p=e;
    while(p!=null) { if(!visited.Add(p.Id)) throw new Exception("Hierarchy cycle"); if(p.Parent!=null&&!ids.Contains(p.Parent)) throw new Exception("Missing parent"); p=Entities.Find(x=>x.Id==p.Parent); }
   }
  }
  public void Delete(string id) { foreach(var c in Entities.Where(x=>x.Parent==id).ToArray()) Delete(c.Id); Entities.RemoveAll(x=>x.Id==id); }
 }
}
