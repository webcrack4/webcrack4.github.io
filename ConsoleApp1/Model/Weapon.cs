using System.Collections.Generic;

namespace ConsoleApp1.Model
{
  public class Weapon : Equip
  {
    public List<string> cells { get; set; }
    public List<UniqueEffect> unique_effects { get; set; }

    public override List<string> getCells()
    {
      return cells;
    }
  }
}
