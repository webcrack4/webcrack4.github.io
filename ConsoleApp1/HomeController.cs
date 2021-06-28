using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using Newtonsoft.Json;

namespace com.dauntless_autobuilder.Controllers
{
    public class HomeController
    {
        private IDeserializer deserializer;
        private ISerializer serializer;
        private static List<Skill> perks;
        private static Dictionary<string, Dictionary<string,Effect>> effectList;
        private static List<KeyValuePair<Skill, int>> availableSkillList;
        private static DauntlessVersion dauntlessVersion;
        private static Dictionary<string, List<Weapon>> weapons;
        private static List<Lantern> lanterns;
        private static List<Cell> cells;           
        private static List<Armour> armours;
        private static Dictionary<string, List<Equip>> classifiedEquips;       //防具按照部位区分
        private static Dictionary<string, List<Equip>> virtualEquips;
        private static Dictionary<string, List<string>> skillList;
        private static readonly string[] Parts = {"Head", "Torso", "Arms", "Legs" };
        private static readonly string[] CellTypes = {"Defence", "Mobility", "Power", "Technique", "Utility" };
        public static readonly int MAX_SEARCH_NUM = 30;
        public static readonly int MAX_SKILL_SLOT_NUM = 12;
        public static int maxRecordNum = 0;
        public static List<string> extraCellSlots;
        public static List<Set> list;
        public static Dictionary<string, int> cellsSlotRequirements;
        public static List<KeyValuePair<Skill, int>> moreSkillList;


        private void dataInit()
        {
            deserializer = new DeserializerBuilder().Build();
            serializer = new SerializerBuilder().JsonCompatible().Build();
            perks = new List<Skill>();
            effectList = new Dictionary<string, Dictionary<string, Effect>>();
            availableSkillList = new List<KeyValuePair<Skill, int>>();
            classifiedEquips = new Dictionary<string, List<Equip>>();
            virtualEquips = new Dictionary<string, List<Equip>>();
            skillList = new Dictionary<string, List<string>>();
            dauntlessVersion = deserializer.Deserialize<DauntlessVersion>(System.IO.File.ReadAllText(Environment.CurrentDirectory + "/data/misc.yml"));

            foreach (string file in Directory.EnumerateFiles(Environment.CurrentDirectory + "/data/perks/", "*.yml"))
            {
                var json = serializer.Serialize(deserializer.Deserialize(new StringReader(System.IO.File.ReadAllText(file))));
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

            lanterns = new List<Lantern>();
            foreach (string file in Directory.EnumerateFiles(Environment.CurrentDirectory + "/data/lanterns/", "*.yml", SearchOption.AllDirectories))
            {
                var json = serializer.Serialize(deserializer.Deserialize(new StringReader(System.IO.File.ReadAllText(file))));
                var lantern = JsonConvert.DeserializeObject<Lantern>(json);
                lanterns.Add(lantern);
            }

            cells = new List<Cell>();
            foreach (string file in Directory.EnumerateFiles(Environment.CurrentDirectory + "/data/cells/", "*.yml", SearchOption.AllDirectories))
            {
                var json = serializer.Serialize(deserializer.Deserialize(new StringReader(System.IO.File.ReadAllText(file))));
                var cell = JsonConvert.DeserializeObject<Cell>(json);
                cells.Add(cell);
            }

            armours = new List<Armour>();
            foreach (string file in Directory.EnumerateFiles(Environment.CurrentDirectory + "/data/armours/", "*.yml", SearchOption.AllDirectories))
            {
                var json = serializer.Serialize(deserializer.Deserialize(new StringReader(System.IO.File.ReadAllText(file))));
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

        public dynamic Search( RequestBody requestBody)
        {
            list = new List<Set>();
            var requirement = GetRequirement(requestBody.keyValueList);
            if (requirement.Count() is 0)
            {
                return list;
            }
            extraCellSlots = new List<string>();
            var totalSlotsRequirement = getSlotsNumberOfTheRestRequired(requirement);
            if(totalSlotsRequirement > MAX_SKILL_SLOT_NUM)
            {
                return list;
            }
            maxRecordNum = MAX_SEARCH_NUM;
            if (!(requestBody.weaponType is null) && !(requestBody.weaponName is null))
            {
                if (weapons.ContainsKey(requestBody.weaponType))
                {
                    var selectWeapon = weapons[requestBody.weaponType].Find(match => match.name.Equals(requestBody.weaponName));
                    if (selectWeapon != null && selectWeapon.cells != null)
                    {
                        extraCellSlots.AddRange(selectWeapon.cells);
                    }
                }
            }
            if (requestBody.hasLantern)
            {
                extraCellSlots.Add("Utility");
            }
            RecursiveSearch(requirement, SearchSetsOrderByParts);
            return list;
        }

        public static void RecursiveSearch(Dictionary<Skill, int> requirement, Action<string[], Dictionary<string, List<Equip>>, Dictionary<Skill, int>, int, Dictionary<string, Equip>> action)
        {
            GetPermutation(Parts, 0, Parts.Length - 1, requirement, action);
        }

        private static void GetPermutation(string[] parts, int startIndex, int endIndex, Dictionary<Skill, int> requirement, Action<string[], Dictionary<string, List<Equip>>,Dictionary<Skill,int>,  int, Dictionary<string, Equip>> action)
        {
            if (list.Count >= maxRecordNum) return;
            if (startIndex == endIndex)
            {
                var equips = new Dictionary<string, Equip>();
                action(parts, classifiedEquips, requirement, 0, equips);
            }
            else
            {
                for (int i = startIndex; i <= endIndex; i++)
                {
                    Swap(ref parts[startIndex], ref parts[i]);
                    GetPermutation(parts, startIndex + 1, endIndex, requirement, action);
                    Swap(ref parts[startIndex], ref parts[i]);
                }
            }
        }

        public static void SearchSetsOrderByParts(string[] parts, Dictionary<string, List<Equip>> restArmours, Dictionary<Skill, int> requirement, int i, Dictionary<string, Equip> equips)
        {
            if (list.Count >= maxRecordNum) return;
            if (requirement.Count == 0) return;
            if (list.Exists(match => match.equips.OrderBy(r => r.Key).SequenceEqual(equips.OrderBy(r => r.Key))))
                return;

            var requireMentForCellSearch = requirement.ToDictionary(x => x.Key, y => y.Value);
            putCellsInMatchedSet(equips, requireMentForCellSearch);
            if (i >= parts.Length)
                return;

            var matchedArmours = MatchArmour(restArmours[parts[i]], requirement);
            if(matchedArmours.Count == 0)
            {
                return;
            }
            var newRestArmours = restArmours.Where(x => !x.Key.Equals(parts[i])).ToDictionary(x => x.Key, y => y.Value);
            foreach (var matchedArmour in matchedArmours)
            {
                var newRestRequirement = requirement.ToDictionary(x => x.Key, y => y.Value);
                if (matchedArmour.skill != null && newRestRequirement.ContainsKey(matchedArmour.skill))
                {
                    if (0 >= (newRestRequirement[matchedArmour.skill] -= 3))
                    {
                        newRestRequirement.Remove(matchedArmour.skill);
                    }
                }
                var tempEquips = equips.ToDictionary(x => x.Key, y => y.Value);
                tempEquips.Add(parts[i], matchedArmour);
                if (newRestRequirement.Count == 0)
                {
                    var set = new Set() {cells = new List<Perk>(), equips = tempEquips };
                    addSetToList(set);
                    if (list.Count >= maxRecordNum) return;
                    continue;
                }
                SearchSetsOrderByParts(parts, newRestArmours, newRestRequirement, i + 1, tempEquips);
            }
        }

        public static Equip getVirtualEquipsMatchRestRequirement(List<Equip> virtualEquipForSearch)
        {
            var foundEquip = virtualEquipForSearch.Find(match => cellsSlotRequirements.Keys.Any(x => match.getCells().Contains(x)));
            var cellSlotType = foundEquip.getCells().FirstOrDefault();
            if (cellsSlotRequirements.GetValueOrDefault(cellSlotType) == 1)
            {
                cellsSlotRequirements.Remove(cellSlotType);
            }
            else
            {
                cellsSlotRequirements[cellSlotType] -= 1;
            }
            return foundEquip;
        }

        public static bool putCellsInMatchedSet(Dictionary<string, Equip> equips, Dictionary<Skill, int> requirement)
        {
            var set = new Set();
            set.equips = equips.ToDictionary(x=>x.Key, y=>y.Value);
            set.cells = new List<Perk>();
            var cellsSlotForSearch = new List<string>(extraCellSlots);
            foreach (var keyValuePair in equips)
            {
                cellsSlotForSearch.AddRange(keyValuePair.Value.getCells());
            }
            var restRequirement = requirement.ToDictionary(x => x.Key, y => y.Value);
            if (matchCell(cellsSlotForSearch,ref restRequirement, set))
                return true;

            set.cells = new List<Perk>();
            int slotNumberOfTheRestRequired = getSlotsNumberOfTheRestRequired(restRequirement);
            if (equips.Count<4 && Parts.Length - equips.Count >= slotNumberOfTheRestRequired)
            {
                var emptyParts = Parts.Except(equips.Values.Select(x => x.type));
                foreach(var emptyPart in emptyParts)
                {
                    if(cellsSlotRequirements.Count is 0)
                    {
                        break;
                    }
                    var virtualEquipOfEmptyPart = getVirtualEquipsMatchRestRequirement(virtualEquips[emptyPart]);
                    set.equips.Add(emptyPart, virtualEquipOfEmptyPart);
                    cellsSlotForSearch.AddRange(virtualEquipOfEmptyPart.getCells());
                }
            }
            if (matchCell(cellsSlotForSearch, ref requirement, set))
                return true;
            return false;
        }

        private static int getSlotsNumberOfTheRestRequired(Dictionary<Skill, int> requirement)
        {
            cellsSlotRequirements = new Dictionary<string, int>();
            int numOfSlots = 0;
            foreach(var keyValuePair in requirement)
            {
                int numOfCurrentTypeOfSlot = (keyValuePair.Value % 3 == 0 ? keyValuePair.Value / 3 : keyValuePair.Value / 3 + 1);
                numOfSlots += numOfCurrentTypeOfSlot;
                if (cellsSlotRequirements.ContainsKey(keyValuePair.Key.type))
                    cellsSlotRequirements[keyValuePair.Key.type] += numOfCurrentTypeOfSlot;
                else
                    cellsSlotRequirements.Add(keyValuePair.Key.type, numOfCurrentTypeOfSlot);
            }
            return numOfSlots;
        }

        public static bool matchCell(List<string> cellSlotsForMatch, ref Dictionary<Skill, int> requirement, Set set)
        {
            foreach (var cellSlot in cellSlotsForMatch)
            {
                var skillMatchCell = requirement.Keys.Where(x => x.type.Equals(cellSlot)).ToArray();
                if (skillMatchCell.Any())
                {
                    var firstSkillOfTheMatch = skillMatchCell.First();
                    var slotValue = requirement[firstSkillOfTheMatch] > 3
                        ? "3"
                        : "" + requirement[firstSkillOfTheMatch];
                    set.cells.Add(new Perk() { name = firstSkillOfTheMatch.name, value = slotValue });
                    if (0 >= (requirement[firstSkillOfTheMatch] -= 3))
                    {
                        requirement.Remove(firstSkillOfTheMatch);
                    }
                    if (requirement.Count == 0)
                    {
                        addSetToList(set);
                        return true;
                    }
                }
            }
            return false;
        }

        public static void addSetToList(Set set)
        {
            if (list.Exists(match => match.equips.OrderBy(r => r.Key).SequenceEqual(set.equips.OrderBy(r => r.Key))))
                return;
            list.Add(set);
        }

        public static void Swap(ref string a, ref string b)
        {
            var temp = a;
            a = b;
            b = temp;
        }

        private Dictionary<Skill, int> GetRequirement(List<KeyValuePair<string, int>> keyValueList)
        {
            var requirement = new Dictionary<Skill, int>();
            foreach (var keyValue in keyValueList)
            {
                if (keyValue.Value == 0 || keyValue.Value > 6 ) continue;
                requirement.Add(perks.Find(match => match.name.Equals(keyValue.Key)), keyValue.Value);
            }
            return requirement;
        }

        //private static List<Equip> MatchArmour(List<Equip> forSearchArmours, Dictionary<Skill, int> restRequirement)
        //  => forSearchArmours.FindAll(match => match.perks != null && match.skill != null && (restRequirement.ContainsKey(match.skill) || restRequirement.Keys.Any(x=> match.getCells().Contains(x.type))));

        private static List<Equip> MatchArmour(List<Equip> forSearchArmours, Dictionary<Skill, int> restRequirement)
            => forSearchArmours.FindAll(match => match.perks != null && match.skill != null && restRequirement.ContainsKey(match.skill));








        public dynamic MoreSkill( RequestBody requestBody)
        {
            moreSkillList = new List<KeyValuePair<Skill, int>>();
            var requirement = GetRequirement(requestBody.keyValueList);
            if (requirement.Count() is 0)
            {
                return moreSkillList;
            }
            extraCellSlots = new List<string>();
            var totalSlotsRequirement = getSlotsNumberOfTheRestRequired(requirement);
            if (totalSlotsRequirement > MAX_SKILL_SLOT_NUM)
            {
                return moreSkillList;
            }

            extraCellSlots = new List<string>();
            if (!(requestBody.weaponType is null) && !(requestBody.weaponName is null))
            {
                if (weapons.ContainsKey(requestBody.weaponType))
                {
                    var selectWeapon = weapons[requestBody.weaponType].Find(match => match.name.Equals(requestBody.weaponName));
                    if (selectWeapon != null && selectWeapon.cells != null)
                    {
                        extraCellSlots.AddRange(selectWeapon.cells);
                    }
                }
            }
            if (requestBody.hasLantern)
            {
                extraCellSlots.Add("Utility");
            }
            maxRecordNum = 1;
            foreach (var avaiableSkill in availableSkillList)
            {
                if(requirement.Any(x => x.Key.name.Equals(avaiableSkill.Key.name) && x.Value >= avaiableSkill.Value))
                {
                    continue;
                }
                var newRequirement = requirement.ToDictionary(x => x.Key, y => y.Value);
                if (newRequirement.ContainsKey(avaiableSkill.Key))
                {
                    newRequirement[avaiableSkill.Key] = avaiableSkill.Value;
                }
                else
                {
                    newRequirement.Add(avaiableSkill.Key, avaiableSkill.Value);
                }
                list = new List<Set>();
                RecursiveSearch(newRequirement, SearchSetsOrderByParts);
                if(list.Count > 0)
                    moreSkillList.Add(avaiableSkill);
            }
            return moreSkillList;
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

    public class Weapon:Equip
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


