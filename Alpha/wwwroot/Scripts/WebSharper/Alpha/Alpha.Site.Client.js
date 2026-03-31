import { init } from "../WebSharper.StdLib/Microsoft.FSharp.Collections.ArrayModule.js"
import Var from "../WebSharper.UI/WebSharper.UI.Var.js"
import { get } from "../WebSharper.StdLib/Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicFunctions.js"
import Doc from "../WebSharper.UI/WebSharper.UI.Doc.js"
import Attr from "../WebSharper.UI/WebSharper.UI.Attr.js"
import { sudokuTable, sudokuInput, sudokuText, sudokuCellBothThick, sudokuCellRightThick, sudokuCellBottomThick, sudokuCell, buttonS, messageS } from "./Alpha.Site.Styles.js"
import { ofSeq } from "../WebSharper.StdLib/Microsoft.FSharp.Collections.ListModule.js"
import { delay, map, collect } from "../WebSharper.StdLib/Microsoft.FSharp.Collections.SeqModule.js"
import { range } from "../WebSharper.StdLib/Microsoft.FSharp.Core.Operators.js"
import { Handler } from "../WebSharper.UI/WebSharper.UI.Client.Attr.js"
import { TryParse } from "../WebSharper.StdLib/System.Int32.js"
export function SudokuGame(puzzle, solution){
  const inputs=init(81, (i) => Var.Create_1(get(puzzle, i)===0?"":String(get(puzzle, i))));
  const message=Var.Create_1("");
  return Doc.Element("div", [], [Doc.Element("table", [Attr.Create("style", sudokuTable())], ofSeq(delay(() => map((r) => Doc.Element("tr", [], ofSeq(delay(() => collect((c) => {
    const i=r*9+c;
    const cellContent=get(puzzle, i)===0?Doc.Input([Attr.Create("maxlength", "1"), Attr.Create("style", sudokuInput())], get(inputs, i)):Doc.Element("span", [Attr.Create("style", sudokuText())], [Doc.TextNode(String(get(puzzle, i)))]);
    return[Doc.Element("td", [Attr.Create("style", c===2||c===5?r===2||r===5?sudokuCellBothThick():sudokuCellRightThick():r===2||r===5?sudokuCellBottomThick():sudokuCell())], [cellContent])];
  }, range(0, 8))))), range(0, 8))))), Doc.Element("div", [Attr.Create("style", "text-align: center;")], [Doc.Element("button", [Attr.Create("style", buttonS()), Handler("click", () =>() => {
    let allCorrect=true;
    let isFull=true;
    for(let i=0, _1=80;i<=_1;i++)((() => {
      let o;
      if(get(puzzle, i)===0){
        const v=get(inputs, i).Get();
        if(v==""){
          isFull=false;
          return;
        }
        else {
          const m=(o=0,[TryParse(v, {get:() => o, set:(v_1) => {
            o=v_1;
          }}), o]);
          return m[0]?m[1]!==get(solution, i)?void(allCorrect=false):null:void(allCorrect=false);
        }
      }
      else return null;
    })());
    if(!allCorrect)message.Set("\u274c Hibás szám van a táblán!");
    else!isFull?message.Set("\u26a0\ufe0f Jó eddig, de még nincs kész!"):message.Set("\u2705 Gratulálok, helyesen megoldottad!");
  })], [Doc.TextNode("\u2714\ufe0f Ellen\u0151rzés")]), Doc.Element("div", [Attr.Create("style", messageS())], [Doc.TextView(message.View)])])]);
}
