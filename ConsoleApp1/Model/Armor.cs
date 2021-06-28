using System.Collections.Generic;

namespace ConsoleApp1.Model
{
  public class Armour : Equip
  {
    public string cells { get; set; }

    public override List<string> getCells()
    {
      return new List<string> { cells };
    }
  }
}
