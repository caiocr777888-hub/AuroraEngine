using Aurora.Scripting;
public class CoinMenu : Script {
 public override void Start(){
  Game.Text="COIN RUN\nColete as cinco moedas douradas.";
  Game.ButtonLabel="Jogar";
  Game.ButtonAction=()=>Game.LoadScene("Scenes/Main.scene");
 }
 public override void Update(double deltaTime){}
}
