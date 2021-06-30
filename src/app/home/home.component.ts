import { Component, Inject } from '@angular/core';
import jquery = require("jquery");
const $: JQueryStatic = jquery;
declare var require: any;

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent {
  level = [0, 1, 2, 3, 4, 5, 6];
  parts = ["Head", "Torso", "Arms", "Legs"];
  weaponTypes = ["Sword", "Hammer", "Axe", "Chain Blades", "War Pike", "Repeater", "Aether Strikers"];
  selectedWeaponType = false;
  public types = ["Defence","Mobility","Power","Technique","Utility"];
  public dv = require("../../assets/dauntless_version.json");
  public skillList: { [index: string]: SkillList }[] = require("../../assets/skillList.json");
  public weapons: { [index: string]: Weapon }[] = require("../../assets/weapons.json");
  public perks: { [index: string]: any }[] = require("../../assets/perks.json");
  public searchValue = [];
  public searchResult: SetBuild[] = [];
  public moreSkillResult = [];
  public hasLantern: boolean = false;
  public hasWeapon: boolean = false;
  public showWeapon: boolean = false;
  public loading: boolean = false;
  public isShowSearchResult: boolean = false;
  public isShowMoreSkillResult: boolean = false;
  public selectedWeapon: any;
  public setDetail = {} as any;
  public cellDetail: { [key: string]: number; } = {};
  public weaponDetail = {};
  public selectSkill = [];
  public weaponTypeForShow = {};
  public weaponNameForShow = {};
  public isShowDetail = false;

  private classifiedEquips = require("../../assets/classifiedEquips.json");
  private virtualEquips = require("../../assets/virtualEquips.json");
  private effectList = require("../../assets/effectList.json");
  private availableSkillList = require("../../assets/availableSkillList.json");
  private lanterns = require("../../assets/lanterns.json");
  private cells = require("../../assets/cells.json");
  private armours = require("../../assets/armours.json");
  private MAX_SEARCH_NUM = 30;
  private MAX_SKILL_SLOT_NUM = 12;
  private cellsSlotRequirements: { [index: string]: number };
  private extraCellSlots = [];

  constructor() {
    this.hasLantern = false;
    this.hasWeapon = false;
  }

  ShowModal(set) {
    $('#myModal').on('shown.bs.modal', function () {
      $('#myInput').focus()
    })
    this.setDetail = set;
    this.isShowDetail = true;
    var celldetail = {} as { [key: string]: number; };
    this.setDetail.cells.forEach(function (cell) {
      if (celldetail[cell.name] == null) {
        celldetail[cell.name] = 0;
      }
      celldetail[cell.name] +=  +cell.value;
    });
    this.cellDetail = celldetail;
  }

  Reset() {
    this.searchValue = [];
    this.hasLantern = false;
    this.hasWeapon = false;
    this.selectedWeaponType = false;
    this.selectedWeapon = {} as Weapon;
  }

  addSkillIntoSearchList(skill)
  {
    this.searchValue[skill.key.name] = skill.value;
  }

  Search() {
    this.loading = true;
    if (this.hasWeapon && Object.keys(this.selectedWeapon).length != 0) {
      this.showWeapon = true;
    } else {
      this.selectedWeapon = {};
      this.showWeapon = false;
    }
    this.weaponTypeForShow = this.selectedWeapon.type;
    this.weaponNameForShow = this.selectedWeapon.name;
    this.searchResult = [];
    var requirement = new Map();

    this.extraCellSlots = [];
    for (let key in this.searchValue) {
      if (this.searchValue[key] != 0){
        var perk = this.perks.find((p)=>{
          return p.name == key;
        });
        requirement.set([perk], this.searchValue[key]);
      }
    }
    if(this.getTotoalSlotRequirement(requirement) > this.MAX_SKILL_SLOT_NUM){
      return;
    }

    if(this.selectedWeapon.cells != null){
      this.extraCellSlots.push(...this.selectedWeapon.cells);
    }
    if (this.hasLantern)
    {
        this.extraCellSlots.push("Utility");
    }
    this.recursiveSearch(requirement, this.searchSetsOrderByParts);
    this.loading = false;
    this.isShowSearchResult = true;
    this.isShowMoreSkillResult = false;
  }


  private getTotoalSlotRequirement(requirement: Map<any, any>) {
    this.cellsSlotRequirements = {};
    var numOfSlots = 0;
    requirement.forEach((v,k)=>{
      var numOfCurrentTypeOfSlot = v % 3 == 0 ? Math.floor(v/3) : (Math.floor(v/3) + 1);
      numOfSlots += numOfCurrentTypeOfSlot;
      if(this.cellsSlotRequirements[k[0].type] == null){
        this.cellsSlotRequirements[k[0].type] = numOfCurrentTypeOfSlot
      }else{
        this.cellsSlotRequirements[k[0].type] += numOfCurrentTypeOfSlot
      }
    });
    return numOfSlots;
  }

  private putCellsInMatchedSet = (equips: Map<string, any>, requirement: Map<any, number>): boolean  => {
    var setBuild = {equips : new Map<string, any>()} as SetBuild; 
    setBuild.equips = JSON.parse(JSON.stringify(equips));
    var cellsSlotForSearch = this.extraCellSlots;
    equips.forEach((v,k)=>{
      cellsSlotForSearch.push(...v.cells);
    });
    
    var restRequirement = new Map<any, number>();
    requirement.forEach((v,k)=>{
      restRequirement.set(k, v);
    });

    if (this.matchCell(cellsSlotForSearch, restRequirement, setBuild))
        return true;

    setBuild.cells = [];
    var slotNumberOfTheRestRequired = this.getSlotsNumberOfTheRestRequired(restRequirement);
    if (equips.size <4 && this.parts.length - equips.size >= slotNumberOfTheRestRequired)
    {
      //parts除去equips的部位
        var emptyParts = this.parts.filter((value, index, arr)=>{
          equips.forEach((v,k)=>{
            if(v.type == value){
              return false;
            }
          })
          return true;
        });
        emptyParts.forEach(emptyPart=>{
          // if(this.cellsSlotRequirements is 0)
          // {
          //     break;
          // }
          var virtualEquipOfEmptyPart = this.getVirtualEquipsMatchRestRequirement(this.virtualEquips[emptyPart]);
          setBuild.equips.set(emptyPart, virtualEquipOfEmptyPart);
          cellsSlotForSearch.push(...virtualEquipOfEmptyPart.cells);
        });
    }
    if (this.matchCell(cellsSlotForSearch, requirement, setBuild)){
      return true;
    }
    return false;
  }

  private getVirtualEquipsMatchRestRequirement = (virtualEquipForSearch: any[]): any => {
    // var foundEquip = virtualEquipForSearch.Find(match => cellsSlotRequirements.Keys.Any(x => match.getCells().Contains(x)));
    //         var cellSlotType = foundEquip.getCells().FirstOrDefault();
    //         if (cellsSlotRequirements.GetValueOrDefault(cellSlotType) == 1)
    //         {
    //             cellsSlotRequirements.Remove(cellSlotType);
    //         }
    //         else
    //         {
    //             cellsSlotRequirements[cellSlotType] -= 1;
    //         }
    //         return foundEquip;
  }

  private getSlotsNumberOfTheRestRequired = (requirement: Map<any, number>) =>{
    this.cellsSlotRequirements = {};
    var numOfSlots = 0;
    requirement.forEach((v,k)=>{
        var numOfCurrentTypeOfSlot = (v % 3 == 0 ? v / 3 : v / 3 + 1);
        numOfSlots += numOfCurrentTypeOfSlot;
        if (this.cellsSlotRequirements[k[0].type] == null){
          this.cellsSlotRequirements[k[0].type] += numOfCurrentTypeOfSlot;
        }
        else{
          this.cellsSlotRequirements[k[0].type] = numOfCurrentTypeOfSlot;
        }
    });
    return numOfSlots;
  }
  
  private matchCell = (cellSlotsForMatch: any[], requirement: Map<any, number>, setBuild: SetBuild): boolean => {
    cellSlotsForMatch.forEach(cellSlot => {
      var skillMatchCell = [];
      requirement.forEach((v,k) => {
        if(k[0].types == cellSlot){
          skillMatchCell.push(k);
        }
      });
      if(skillMatchCell.length > 0){
        var firstSkillOfTheMatch = skillMatchCell[0];
        var slotValue = requirement[firstSkillOfTheMatch] > 3 ? "3"  : "" + requirement[firstSkillOfTheMatch];
        setBuild.cells.push({name: firstSkillOfTheMatch.name, value : slotValue});
        if (0 >= (requirement[firstSkillOfTheMatch] -= 3))
        {
            requirement.delete(firstSkillOfTheMatch);
        }
        if (requirement.size == 0)
        {
            this.addSetToList(setBuild);
            return true;
        }
      }
    });
    
    return false;
  }

  private addSetToList = (setBuild: SetBuild) => {
    // Todo：思考如何去重
    // if (list.Exists(match => match.equips.OrderBy(r => r.Key).SequenceEqual(set.equips.OrderBy(r => r.Key))))
    //             return;
    this.searchResult.push(setBuild);
  }
  
  private searchSetsOrderByParts = (parts: string[], restArmours: Map<any,any>,requirement: Map<any, number>, i: number, equips: Map<string,any>) => {
    if (this.searchResult.length >= this.MAX_SEARCH_NUM) return;
    if (requirement.size == 0) return;

    // TODO: 改写，如果搜索列表跟装备结果全等，则直接返回
    // if (list.Exists(match => match.equips.OrderBy(r => r.Key).SequenceEqual(equips.OrderBy(r => r.Key))))
    //   return;
    
    ///////////// 用不上
    ///////////// var requireMentForCellSearch = JSON.parse(JSON.stringify(requirement))
    ///////////// console.log(requireMentForCellSearch);

    //函数putCellsInMatchedSet抽出来
    this.putCellsInMatchedSet(equips, requirement);
    if (i >= parts.length)
        return;

    // var matchedArmours = this.matchArmour(restArmours[parts[i]], requirement);
    // if(matchedArmours.length == 0)
    // {
    //     return;
    // }
    //         var newRestArmours = restArmours.Where(x => !x.Key.Equals(parts[i])).ToDictionary(x => x.Key, y => y.Value);
    //         foreach (var matchedArmour in matchedArmours)
    //         {
    //             var newRestRequirement = requirement.ToDictionary(x => x.Key, y => y.Value);
    //             if (matchedArmour.skill != null && newRestRequirement.ContainsKey(matchedArmour.skill))
    //             {
    //                 if (0 >= (newRestRequirement[matchedArmour.skill] -= 3))
    //                 {
    //                     newRestRequirement.Remove(matchedArmour.skill);
    //                 }
    //             }
    //             var tempEquips = equips.ToDictionary(x => x.Key, y => y.Value);
    //             tempEquips.Add(parts[i], matchedArmour);
    //             if (newRestRequirement.Count == 0)
    //             {
    //                 var set = new Set() {cells = new List<Perk>(), equips = tempEquips };
    //                 addSetToList(set);
    //                 if (list.Count >= maxRecordNum) return;
    //                 continue;
    //             }
    //             SearchSetsOrderByParts(parts, newRestArmours, newRestRequirement, i + 1, tempEquips);
    //         }
  }

  private matchArmour(forSearchArmours: any[], restRequirement: Map<any, number>) {
    return forSearchArmours.find(match=> {
      console.log(match);
      return false;
    });
    // return forSearchArmours.find(match=> match => match.perks != null && match.skill != null && restRequirement.has(match.skill));
  }

  private recursiveSearch(requirement: Map<any, any>, action: (parts: string[], restArmours: Map<any, any>, requirement: Map<any, any>, i: number, equips: Map<any, any>) => void) {
    this.getPermutation(this.parts, 0, this.parts.length - 1, requirement, action);
  }

  private getPermutation(parts: string[], startIndex: number, endIndex: number, requirement: Map<any, number>, action: (parts: string[], restArmours: Map<any, any>, requirement: Map<any, number>, i: number, equips: Map<string, any>) => void) {
    if (this.searchResult.length >= this.MAX_SEARCH_NUM) return;
    if (startIndex == endIndex)
    {
        var equips = new Map<string, any>();
        action(parts, this.classifiedEquips, requirement, 0, equips);
    }
    else
    {
        for (var i = startIndex; i <= endIndex; i++)
        {
            var temp = parts[startIndex];
            parts[startIndex] =  parts[i];
            parts[i] = temp;
            this.getPermutation(parts, startIndex + 1, endIndex, requirement, action);
            var temp2 = parts[startIndex];
            parts[startIndex] =  parts[i];
            parts[i] = temp2;
        }
    }
  }
  
  MoreSkill() {
    this.loading = true;
    if (this.hasWeapon && Object.keys(this.selectedWeapon).length != 0) {
      this.showWeapon = true;
    } else {
      this.selectedWeapon = {} as Weapon;
      this.showWeapon = false;
    }
    let requestBody = {} as RequestBody;
    requestBody.keyValueList = [];
    for (let key in this.searchValue) {
      if (this.searchValue[key] != 0)
        requestBody.keyValueList.push({ key: key, value: this.searchValue[key] });
    }
    requestBody.hasLantern = this.hasLantern;
    requestBody.weaponType = this.selectedWeapon.type;
    requestBody.weaponName = this.selectedWeapon.name;
    this.weaponTypeForShow = this.selectedWeapon.type;
    this.weaponNameForShow = this.selectedWeapon.name;


    this.moreSkillResult = [];  //todo: more skill 结果
    this.loading = false;
    this.isShowSearchResult = false;
    this.isShowMoreSkillResult = true;
  }

  Export(set) {
    var textString = "Skill List : \r\n";
    this.selectSkill.forEach(function (skill) {
      textString += (skill.key + " + " + skill.value+"\r\n");
    });
    textString += ("\r\n"+"Equips : \r\n");
    if (this.hasWeapon && Object.keys(this.selectedWeapon).length != 0) {
      textString += "Weapon Type: " + this.selectedWeapon.type + "\r\n";
      textString += "Weapon Name: " + this.selectedWeapon.name + "\r\n";
    }
    this.parts.forEach(function (part) {
      if (set.equips[part]!=null && Object.keys(set.equips[part]).length != 0) {
        textString+= (part+" : "+ set.equips[part].name + "\r\n");
      }
    });
    textString += ("Lantern : " + (this.hasLantern ? "yes" : "no") + "\r\n");
    textString += "\r\nCell list : \r\n";
    set.cells.forEach(function (cell) {
      textString += (cell.name +" + "+cell.value+"\r\n");
    });

    this.dyanmicDownloadByHtmlTag({
      fileName: "Dauntless_Set_" + Date.now(),
      text: textString
    })
  }

  private setting = {
    element: {
      dynamicDownload: null as HTMLElement
    }
  }

  private dyanmicDownloadByHtmlTag(arg: {
    fileName: string,
    text: string
  }) {
    if (!this.setting.element.dynamicDownload) {
      this.setting.element.dynamicDownload = document.createElement('a');
    }
    const element = this.setting.element.dynamicDownload;
    const fileType = arg.fileName.indexOf('.json') > -1 ? 'text/json' : 'text/plain';
    element.setAttribute('href', `data:${fileType};charset=utf-8,${encodeURIComponent(arg.text)}`);
    element.setAttribute('download', arg.fileName);

    var event = new MouseEvent("click");
    element.dispatchEvent(event);
  }
}


interface RequestBody {
  keyValueList: {key:string, value:string}[];
  hasLantern: boolean;
  weaponType: string;
  weaponName: string;
}

interface SkillList {
}


interface Weapon {
  type: string;
  name: string;
}

interface SetBuild{
  equips: Map<string, any>;
  cells: any[];
}

