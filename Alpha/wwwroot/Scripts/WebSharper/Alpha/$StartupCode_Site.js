import { sudokuCell } from "./Alpha.Site.Styles.js"
import { Lazy } from "../WebSharper.Core.JavaScript/Runtime.js"
let _c=Lazy((_i) => class $StartupCode_Site {
  static {
    _c=_i(this);
  }
  static globalCss;
  static sudokuText;
  static sudokuInput;
  static sudokuCellBothThick;
  static sudokuCellBottomThick;
  static sudokuCellRightThick;
  static sudokuCell;
  static sudokuTable;
  static footer;
  static h2s;
  static card;
  static mainS;
  static navLink;
  static nav;
  static titleS;
  static header;
  static page;
  static {
    this.page="font-family:'Segoe UI',sans-serif;background:#f0f4f8;min-height:100vh;margin:0;padding:0;";
    this.header="background:#2d3a8c;color:white;padding:18px 32px;display:flex;align-items:center;justify-content:space-between;";
    this.titleS="margin:0;font-size:1.8rem;letter-spacing:2px;";
    this.nav="display:flex;gap:16px;";
    this.navLink="color:#aec6ff;text-decoration:none;font-size:1rem;";
    this.mainS="max-width:700px;margin:40px auto;padding:0 16px;text-align:center;";
    this.card="background:white;border-radius:12px;box-shadow:0 2px 12px rgba(0,0,0,0.10);padding:32px;margin-bottom:24px;display:inline-block;";
    this.h2s="color:#2d3a8c;margin-top:0;";
    this.footer="text-align:center;color:#90a4ae;padding:24px;font-size:0.9rem;";
    this.sudokuTable="border-collapse:collapse;margin:24px auto;border:3px solid #2d3a8c;background:#fff;";
    this.sudokuCell="width:45px;height:45px;text-align:center;font-size:1.4rem;border:1px solid #ccd1d9;padding:0;";
    this.sudokuCellRightThick=sudokuCell()+"border-right:3px solid #2d3a8c;";
    this.sudokuCellBottomThick=sudokuCell()+"border-bottom:3px solid #2d3a8c;";
    this.sudokuCellBothThick=sudokuCell()+"border-right:3px solid #2d3a8c;border-bottom:3px solid #2d3a8c;";
    this.sudokuInput="width:100%;height:100%;border:none;text-align:center;font-size:1.4rem;color:#2d3a8c;background:#e8eaf6;outline:none;font-weight:bold;padding:0;box-sizing:border-box;";
    this.sudokuText="display:flex;align-items:center;justify-content:center;width:100%;height:100%;font-weight:bold;color:#333;";
    this.globalCss="\r\n            .sudoku-err { background-color: #ffcccc !important; }\r\n            .sudoku-err input { background-color: #ffcccc !important; color: #d32f2f !important; }\r\n            .sudoku-err span { color: #d32f2f !important; }\r\n        ";
  }
});
export default _c;
