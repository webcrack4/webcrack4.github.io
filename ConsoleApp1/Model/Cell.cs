using System.Collections.Generic;

namespace ConsoleApp1.Model
{
  public class Cell
  {
    public string name { get; set; }
    public string slot { get; set; }
    public Dictionary<string, Varaints> variants { get; set; }
  }
}
