import Object from "../WebSharper.StdLib/System.Object.js"
import { CubicInOut } from "./WebSharper.UI.Easings.js"
export default class Easing extends Object {
  transformTime;
  static get CubicInOut(){
    return CubicInOut();
  }
  static Custom(f){
    return new Easing(f);
  }
  TransformTime(t){
    return this.transformTime(t);
  }
  constructor(transformTime){
    super();
    this.transformTime=transformTime;
  }
}
