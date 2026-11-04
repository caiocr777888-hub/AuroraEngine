using System.IO;
using System.Linq;
namespace Aurora.Assets {
 public static class AssetDatabase {
  public static string[] List(string root){return Directory.GetFiles(root,"*",SearchOption.AllDirectories).Where(x=>!x.EndsWith(".bak")&&!x.EndsWith(".tmp")).Select(x=>x.Substring(root.Length+1)).ToArray();}
  public static string Import(string root,string source){var destination=Path.Combine(root,"Assets",Path.GetFileName(source));Directory.CreateDirectory(Path.GetDirectoryName(destination));File.Copy(source,destination,false);return destination;}
 }
}
