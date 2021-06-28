import { Component, Inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
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
  public weapons: { [index: string]: Weapon }[] = require("../../assets/weapons.json");;
  public perks: { [index: string]: Perk }[] = require("../../assets/effectList.json");
  public searchValue = [];
  public searchResult = [];
  public moreSkillResult = [];
  public httpClient: HttpClient;
  public baseUrl: string;
  public hasLantern: boolean;
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

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.httpClient = http;
    this.baseUrl = baseUrl;
    this.hasLantern = false;
    this.hasWeapon = false;
    console.log("weapon", this.weapons);
    console.log("perks", this.perks);
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
    let requestBody = {} as RequestBody;
    requestBody.keyValueList = [];
    for (let key in this.searchValue) {
      if (this.searchValue[key] != 0)
        requestBody.keyValueList.push({key: key, value: this.searchValue[key]});
    }
    this.selectSkill = requestBody.keyValueList;
    this.weaponDetail = this.selectedWeapon;
    requestBody.hasLantern = this.hasLantern;
    requestBody.weaponType = this.selectedWeapon.type;
    requestBody.weaponName = this.selectedWeapon.name;
    this.weaponTypeForShow = this.selectedWeapon.type;
    this.weaponNameForShow = this.selectedWeapon.name;
    const headers = new HttpHeaders().set('content-type', 'application/json');
    this.httpClient.post<any>(
      this.baseUrl + 'api/Home/Search', requestBody, { headers: headers }
    )
      .subscribe(result => {
        this.searchResult = result;
        this.loading = false;
        this.isShowSearchResult = true;
        this.isShowMoreSkillResult = false;
      }, error => console.error(error));
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
    const headers = new HttpHeaders().set('content-type', 'application/json');
    this.httpClient.post<any>(
      this.baseUrl + 'api/Home/MoreSkill', requestBody, { headers: headers }
    )
      .subscribe(result => {
        this.moreSkillResult = result;
        this.loading = false;
        this.isShowSearchResult = false;
        this.isShowMoreSkillResult = true;
      }, error => console.error(error));
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

interface DauntlessVersion {
  dauntless_version: string;
  patchnotes_version_string: string;
}

interface SkillList {
}

interface Perk {
}

interface Weapon {
  type: string;
  name: string;
}

interface HomeResult {
  item1: DauntlessVersion;
  item2: { [index: string]: SkillList }[];
  item3: { [index: string]: Weapon }[];
  item4: { [index: string]: Perk }[];
}

