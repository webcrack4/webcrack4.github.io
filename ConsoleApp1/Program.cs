using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
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
        private static DauntlessVersion dauntlessVersion;
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

            fileCreate(new string[]{ fp1 , fp2, fp3, fp4});

            deserializer = new DeserializerBuilder().Build();
            serializer = new SerializerBuilder().JsonCompatible().Build();
            perks = new List<Skill>();
            effectList = new Dictionary<string, Dictionary<string, Effect>>();
            availableSkillList = new List<KeyValuePair<Skill, int>>();
            classifiedEquips = new Dictionary<string, List<Equip>>();
            virtualEquips = new Dictionary<string, List<Equip>>();
            skillList = new Dictionary<string, List<string>>();
            Console.Write(Environment.CurrentDirectory);

            FileStream fs = new FileStream(@Environment.CurrentDirectory + "\\data\\misc.yml", FileMode.Open, FileAccess.Read);
            TextReader tr = new StreamReader(fs, Encoding.Default);
            dauntlessVersion = deserializer.Deserialize<DauntlessVersion>(tr);
            File.WriteAllText(fp1, JsonConvert.SerializeObject(dauntlessVersion));

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

            File.WriteAllText(fp2, JsonConvert.SerializeObject(effectList));
            File.WriteAllText(fp4, JsonConvert.SerializeObject(skillList));

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

            cells = new List<Cell>();
            foreach (string file in Directory.EnumerateFiles(Environment.CurrentDirectory + "/data/cells/", "*.yml", SearchOption.AllDirectories))
            {
                var json = serializer.Serialize(deserializer.Deserialize(new StringReader(File.ReadAllText(file))));
                var cell = JsonConvert.DeserializeObject<Cell>(json);
                cells.Add(cell);
            }

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
        }

        public string DataTableToJSONWithStringBuilder(DataTable table)
        {
            var JSONString = new StringBuilder();
            if (table.Rows.Count > 0)
            {
                JSONString.Append("[");
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    JSONString.Append("{");
                    for (int j = 0; j < table.Columns.Count; j++)
                    {
                        if (j < table.Columns.Count - 1)
                        {
                            JSONString.Append("\"" + table.Columns[j].ColumnName.ToString() + "\":" + "\"" + table.Rows[i][j].ToString() + "\",");
                        }
                        else if (j == table.Columns.Count - 1)
                        {
                            JSONString.Append("\"" + table.Columns[j].ColumnName.ToString() + "\":" + "\"" + table.Rows[i][j].ToString() + "\"");
                        }
                    }
                    if (i == table.Rows.Count - 1)
                    {
                        JSONString.Append("}");
                    }
                    else
                    {
                        JSONString.Append("},");
                    }
                }
                JSONString.Append("]");
            }
            return JSONString.ToString();
        }
    }

    public class RequestBody
    {
        public List<KeyValuePair<string, int>> keyValueList { get; set; }
        public bool hasLantern { get; set; }
        public string weaponType { get; set; }
        public string weaponName { get; set; }
    }

    public class Skill
    {
        public string name { get; set; }
        public string type { get; set; }
        public Dictionary<string, Effect> effects { get; set; }
    }

    public class Effect
    {
        public object description { get; set; }
        public object value { get; set; }
    }


    public class DauntlessVersion
    {
        public string dauntless_version { get; set; }
        public string patchnotes_version_string { get; set; }
    }

    public class Weapon : Equip
    {
        public List<string> cells { get; set; }
        public List<UniqueEffect> unique_effects { get; set; }

        public override List<string> getCells()
        {
            return cells;
        }
    }

    public class Perk
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class UniqueEffect
    {
        public string name { get; set; }
        public string description { get; set; }
    }

    public class Lantern
    {
        public string name { get; set; }
        public string cells { get; set; }
    }

    public class Cell
    {
        public string name { get; set; }
        public string slot { get; set; }
        public Dictionary<string, Varaints> variants { get; set; }
    }

    public class Varaints
    {
        public Dictionary<string, int> perks { get; set; }
    }

    public class Armour : Equip
    {
        public string cells { get; set; }

        public override List<string> getCells()
        {
            return new List<string> { cells };
        }
    }

    public abstract class Equip
    {
        public string name { get; set; }
        public string type { get; set; }
        public List<Perk> perks { get; set; }
        public Skill skill { get; set; }
        public abstract List<string> getCells();
        public bool isVirtual { get; set; } = true;
    }

    public class Set
    {
        public Dictionary<string, Equip> equips { get; set; }
        public List<Perk> cells { get; set; }
    }

    public static class CacheKeys
    {
        public static string Skill { get { return "_Skill"; } }
        public static string Weapon { get { return "_Weapon"; } }
        public static string Armour { get { return "_Armour"; } }
        public static string Cell { get { return "_Cell"; } }
        public static string VirtualEquip { get { return "_VirtualEquip"; } }
        public static string Lantern { get { return "_Lantern"; } }
        public static string DauntlessVersion { get { return "_DauntlessVersion"; } }
        public static string ClasssifiedEquip { get { return "_ClasssifiedEquip"; } }
        public static string Perk { get { return "_Perk"; } }
        public static string AvailableSkillList { get { return "_AvailableSkillList"; } }
        public static string EffectList { get { return "_EffectList"; } }

    }
}
