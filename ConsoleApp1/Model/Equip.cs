using System.Collections.Generic;

namespace ConsoleApp1.Model
{
  public abstract class Equip
  {
    public string name { get; set; }
    public string type { get; set; }
    public List<Perk> perks { get; set; }
    public Skill skill { get; set; }
    public abstract List<string> getCells();
    public bool isVirtual { get; set; } = true;
  }
}
