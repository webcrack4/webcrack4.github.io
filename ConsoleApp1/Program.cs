using ConsoleApp1.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YamlDotNet.Serialization;

namespace ConsoleApp1
{
  class Program
  {
    static void Main(string[] args)
    {
      var p = new Program();
      p.ymlToJson();
      Console.Write("Hello World!");
    }

    private IDeserializer deserializer;
    private ISerializer serializer;
    private static List<Skill> perks;
    private static Dictionary<string, Dictionary<string, Effect>> effectList;
    private static List<KeyValuePair<Skill, int>> availableSkillList;
    private static Dictionary<string, List<Weapon>> weapons;
    private static List<Lantern> lanterns;
    private static List<Cell> cells;
    private static List<Armour> armours;
    private static Dictionary<string, List<Equip>> classifiedEquips;       //防具按照部位区分
    private static Dictionary<string, List<Equip>> virtualEquips;
    private static Dictionary<string, List<string>> skillList;
    private static readonly string[] Parts = { "Head", "Torso", "Arms", "Legs" };
    private static readonly string[] CellTypes = { "Defence", "Mobility", "Power", "Technique", "Utility" };

    private void fileCreate(string[] fileNames)
    {
      foreach (var fileName in fileNames)
      {
        if (!File.Exists(fileName))  // 判断是否已有相同文件 
        {
          FileStream fs1 = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);
          fs1.Close();
        }
      }
    }
    private void ymlToJson()
    {
      string fp1 = Environment.CurrentDirectory + "\\dauntless_version.json";
      string fp2 = Environment.CurrentDirectory + "\\effectList.json";
      string fp3 = Environment.CurrentDirectory + "\\weapons.json";
      string fp4 = Environment.CurrentDirectory + "\\skillList.json";
      string fp5 = Environment.CurrentDirectory + "\\virtualEquips.json";
      string fp6 = Environment.CurrentDirectory + "\\classifiedEquips.json";
      string fp7 = Environment.CurrentDirectory + "\\availableSkillList.json";
      string fp8 = Environment.CurrentDirectory + "\\lanterns.json";
      string fp9 = Environment.CurrentDirectory + "\\cells.json";
      string fp10 = Environment.CurrentDirectory + "\\armours.json";
      string fp11 = Environment.CurrentDirectory + "\\perks.json";

      fileCreate(new string[] { fp1, fp2, fp3, fp4, fp5, fp6, fp7, fp8, fp9, fp10, fp11 });

      deserializer = new DeserializerBuilder().Build();
      serializer = new SerializerBuilder().JsonCompatible().Build();
      perks = new List<Skill>();
      effectList = new Dictionary<string, Dictionary<string, Effect>>();
      availableSkillList = new List<KeyValuePair<Skill, int>>();
      classifiedEquips = new Dictionary<string, List<Equip>>();
      virtualEquips = new Dictionary<string, List<Equip>>();
      skillList = new Dictionary<string, List<string>>();

      FileStream fs = new FileStream(@Environment.CurrentDirectory + "\\data\\misc.yml", FileMode.Open, FileAccess.Read);
      TextReader tr = new StreamReader(fs, Encoding.Default);
      File.WriteAllText(fp1, JsonConvert.SerializeObject(deserializer.Deserialize<DauntlessVersion>(tr)));

      foreach (string file in Directory.EnumerateFiles(Environment.CurrentDirectory + "/data/perks/", "*.yml"))
      {
        var json = serializer.Serialize(deserializer.Deserialize(new StringReader(File.ReadAllText(file))));
        var perk = JsonConvert.DeserializeObject<Skill>(json);
        perks.Add(perk);
        availableSkillList.Add(new KeyValuePair<Skill, int>(perk, 3));
        availableSkillList.Add(new KeyValuePair<Skill, int>(perk, 6));
        effectList.Add(perk.name, perk.effects);
        if (!skillList.ContainsKey(perk.type))
        {
          skillList.Add(perk.type, new List<string>() { perk.name });
        }
        else
        {
          skillList.GetValueOrDefault(perk.type).Add(perk.name);
        }
      }

      File.WriteAllText(fp11, JsonConvert.SerializeObject(perks));
      File.WriteAllText(fp2, JsonConvert.SerializeObject(effectList));
      File.WriteAllText(fp4, JsonConvert.SerializeObject(skillList));
      File.WriteAllText(fp7, JsonConvert.SerializeObject(availableSkillList));

      weapons = new Dictionary<string, List<Weapon>>();
      foreach (string file in Directory.EnumerateFiles(Environment.CurrentDirectory + "/data/weapons/", "*.yml", SearchOption.AllDirectories))
      {
        var json = serializer.Serialize(deserializer.Deserialize(new StringReader(File.ReadAllText(file))));
        var weapon = JsonConvert.DeserializeObject<Weapon>(json);
        if (weapon.perks != null)
        {
          weapon.skill = perks.Find(match => match.name.Equals(weapon.perks[0].name));
        }
        if (weapons.ContainsKey(weapon.type))
          weapons[weapon.type].Add(weapon);
        else
          weapons.Add(weapon.type, new List<Weapon>() { weapon });

      }

      File.WriteAllText(fp3, JsonConvert.SerializeObject(weapons));

      lanterns = new List<Lantern>();
      foreach (string file in Directory.EnumerateFiles(Environment.CurrentDirectory + "/data/lanterns/", "*.yml", SearchOption.AllDirectories))
      {
        var json = serializer.Serialize(deserializer.Deserialize(new StringReader(File.ReadAllText(file))));
        var lantern = JsonConvert.DeserializeObject<Lantern>(json);
        lanterns.Add(lantern);
      }


      File.WriteAllText(fp8, JsonConvert.SerializeObject(lanterns));

      cells = new List<Cell>();
      foreach (string file in Directory.EnumerateFiles(Environment.CurrentDirectory + "/data/cells/", "*.yml", SearchOption.AllDirectories))
      {
        var json = serializer.Serialize(deserializer.Deserialize(new StringReader(File.ReadAllText(file))));
        var cell = JsonConvert.DeserializeObject<Cell>(json);
        cells.Add(cell);
      }

      File.WriteAllText(fp9, JsonConvert.SerializeObject(cells));


      armours = new List<Armour>();
      foreach (string file in Directory.EnumerateFiles(Environment.CurrentDirectory + "/data/armours/", "*.yml", SearchOption.AllDirectories))
      {
        var json = serializer.Serialize(deserializer.Deserialize(new StringReader(File.ReadAllText(file))));
        var armour = JsonConvert.DeserializeObject<Armour>(json);
        if (armour.perks != null)
        {
          armour.skill = perks.Find(match => match.name.Equals(armour.perks[0].name));
          armour.isVirtual = false;
        }
        armours.Add(armour);
        if (classifiedEquips.ContainsKey(armour.type))
          classifiedEquips[armour.type].Add(armour);
        else
          classifiedEquips.Add(armour.type, new List<Equip>() { armour });

        if (armour.cells is null)
        {
          continue;
        }
      }

      File.WriteAllText(fp6, JsonConvert.SerializeObject(classifiedEquips));
      File.WriteAllText(fp10, JsonConvert.SerializeObject(armours));

      foreach (var part in Parts)
      {
        foreach (var cellType in CellTypes)
        {
          if (virtualEquips.ContainsKey(part))
          {
            virtualEquips[part].Add(new Armour() { name = part + " with " + cellType + " cell slot", cells = cellType, type = part });
          }
          else
          {
            virtualEquips.Add(part, new List<Equip>() { new Armour() { name = part + " with " + cellType + " cell slot", cells = cellType, type = part, isVirtual = true } });
          }
        }
      }


      File.WriteAllText(fp5, JsonConvert.SerializeObject(virtualEquips));
    }
  }
}
