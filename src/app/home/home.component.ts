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
  public searchResult = [];
  public moreSkillResult = [];
  public hasLantern: boolean = false;
  public hasWeapon: boolean = false;
  public showWeapon: boolean = false;
  public loading: boolean = false;
  public isShowSearchResult: boolean = false;
  public isShowMoreSkillResult: boolean = false;
  public selectedWeapon = {} as Weapon;
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
      this.selectedWeapon = {} as Weapon;
      this.showWeapon = false;
    }
    
    // let requestBody = {} as RequestBody;
    // requestBody.keyValueList = [];
    // for (let key in this.searchValue) {
    //   if (this.searchValue[key] != 0)
    //     requestBody.keyValueList.push({key: key, value: this.searchValue[key]});
    // }
    // this.selectSkill = requestBody.keyValueList;
    this.weaponDetail = this.selectedWeapon;
    // requestBody.hasLantern = this.hasLantern;
    // requestBody.weaponType = this.selectedWeapon.type;
    // requestBody.weaponName = this.selectedWeapon.name;
    this.weaponTypeForShow = this.selectedWeapon.type;
    this.weaponNameForShow = this.selectedWeapon.name;


    this.searchResult = []; //todo 搜索结果
    var requirement = new Map();

    var extraCellSlots = [];
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

    console.log(this.cellsSlotRequirements);
    this.loading = false;
    this.isShowSearchResult = true;
    this.isShowMoreSkillResult = false;
  }

  getTotoalSlotRequirement(requirement: Map<any, any>) {
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
