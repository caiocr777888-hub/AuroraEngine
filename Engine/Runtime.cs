using System;
using System.Collections.Generic;
namespace Aurora.Engine {
 public class Runtime {
  public Scene Scene; readonly Aurora.Physics.PhysicsWorld physics=new Aurora.Physics.PhysicsWorld();
  readonly Aurora.Scripting.ScriptHost scripts;
  public readonly GameApi Game=new GameApi();
  readonly Aurora.Physics.Contacts contacts=new Aurora.Physics.Contacts();
  public Runtime(Scene source,string projectRoot=null) { Scene=Store.Decode<Scene>(Store.Encode(source)); if(projectRoot!=null)scripts=new Aurora.Scripting.ScriptHost(Scene,projectRoot,Game); Log.Write("Play: independent runtime scene started"); }
  public void Tick(double dt,double horizontal,double vertical) {
   dt=Math.Min(dt,0.05);
   contacts.Before(Scene);
   if(scripts!=null)scripts.Update(dt);
   foreach(var e in Scene.Entities) foreach(var c in e.Components) {
    Aurora.Scripting.BehaviorSystem.Step(e,c,dt,horizontal,vertical);
    if(c.Type=="Gravity") physics.Step(e,dt);
   }
   contacts.After(Scene,Game);
  }
 }
}
