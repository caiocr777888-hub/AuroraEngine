using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aurora.Scripting;

namespace Aurora.Engine {
 public class GameSession {
  readonly string root;
  readonly Scene initial;
  string pending;
  bool restart;
  public Runtime Runtime {get;private set;}
  public GameSession(Scene source,string root) {
   initial=source;
   this.root=root;
   Runtime=new Runtime(source,root);
  }
  public void Restart(){restart=true;}
  public void Tick(double dt,Func<string,bool> input) {
   var ui=Runtime.Game;
   ui.ReadInput=input;
   Runtime.Tick(dt,(input("Right")?1:0)-(input("Left")?1:0),(input("Down")?1:0)-(input("Up")?1:0));
   pending=ui.NextScene;
   if(restart||pending!=null) {
    restart=false;
    ui.NextScene=null;
    try {
     var next=pending==null?initial:Store.LoadScene(SceneFiles.Resolve(new Project{Root=root},pending));
     Runtime=new Runtime(next,root);
    } catch(Exception ex){Log.Write("ERROR: "+ex.Message);ui.Text=ex.Message;}
   }
  }
 }
 public class GameApi {
  public string Text {get;set;}
  public string ButtonLabel {get;set;}
  public Action ButtonAction {get;set;}
  internal string NextScene;
  internal Func<string,bool> ReadInput=action=>false;
  public bool IsDown(string action){return ReadInput(action);}
  public void LoadScene(string relativePath){NextScene=relativePath;}
  public event Action<Entity,Entity> TriggerEntered;
  internal void Enter(Entity trigger,Entity body){if(TriggerEntered!=null)TriggerEntered(trigger,body);}
 }
}
