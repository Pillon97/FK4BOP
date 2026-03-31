import Object from "./System.Object.js"
import { ajax } from "./WebSharper.Remoting.js"
export default class XhrProvider extends Object {
  Sync(url, headers, data){
    const res=[null];
    ajax(false, url, headers, data, (x) => {
      res[0]=x;
    }, (e) => {
      throw e;
    }, () => {
      ajax(false, url, headers, data, (x) => {
        res[0]=x;
      }, (e) => {
        throw e;
      }, void 0);
    });
    return res[0];
  }
  Async(url, headers, data, ok, err){
    ajax(true, url, headers, data, ok, err, () => {
      ajax(true, url, headers, data, ok, err, void 0);
    });
  }
}
