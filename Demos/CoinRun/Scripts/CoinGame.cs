using System;
using Aurora.Engine;
using Aurora.Scripting;

public class CoinGame : Script {
 int score;
 bool restartHeld;
 public override void Start(){
  Game.TriggerEntered+=Collect;
  Game.ButtonLabel="Reiniciar";
  Game.ButtonAction=Reset;
  ShowScore();
 }
 void ShowScore(){Game.Text=score==5?"Você venceu!\n5 / 5 moedas":"COIN RUN\nMoedas: "+score+" / 5\nWASD / setas · R reinicia";}
 void Reset(){
  score=0;
  Entity.X=0;Entity.Z=4;Entity.Color="#65B8FF";
  foreach(var coin in Scene.Entities)if(coin.Name.StartsWith("Coin "))coin.Visible=true;
  ShowScore();
 }
 void Collect(Entity coin,Entity body){
  if(body!=Entity||!coin.Name.StartsWith("Coin ")||!coin.Visible||score==5)return;
  coin.Visible=false;
  score++;
  if(score==5)Entity.Color="#69F0AE";
  ShowScore();
 }
 public override void Update(double dt){
  bool pressed=Game.IsDown("Restart");
  if(pressed&&!restartHeld)Reset();
  restartHeld=pressed;
  foreach(var coin in Scene.Entities)if(coin.Name.StartsWith("Coin "))coin.RY+=120*dt;
  if(score==5)return;
  double x=(Game.IsDown("Right")?1:0)-(Game.IsDown("Left")?1:0);
  double z=(Game.IsDown("Down")?1:0)-(Game.IsDown("Up")?1:0);
  double length=Math.Sqrt(x*x+z*z);
  if(length==0)return;
  Entity.X+=x*4*dt/length;Entity.Z+=z*4*dt/length;
 }
}
