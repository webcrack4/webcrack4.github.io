using System.Collections.Generic;

namespace ConsoleApp1.Model
{
  public class Skill
  {
    public string name { get; set; }
    public string type { get; set; }
    public Dictionary<string, Effect> effects { get; set; }
  }
}
