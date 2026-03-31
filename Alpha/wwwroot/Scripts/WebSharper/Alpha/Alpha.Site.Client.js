import { map, ofSeq } from "../WebSharper.StdLib/Microsoft.FSharp.Collections.ArrayModule.js"
import Var from "../WebSharper.UI/WebSharper.UI.Var.js"
import { Map, Sequence } from "../WebSharper.UI/WebSharper.UI.View.js"
import Doc from "../WebSharper.UI/WebSharper.UI.Doc.js"
import Attr from "../WebSharper.UI/WebSharper.UI.Attr.js"
import { sudokuTable, sudokuText, sudokuInput, sudokuCellBothThick, sudokuCellRightThick, sudokuCellBottomThick, sudokuCell } from "./Alpha.Site.Styles.js"
import { ofSeq as ofSeq_1 } from "../WebSharper.StdLib/Microsoft.FSharp.Collections.ListModule.js"
import { delay, map as map_1, collect } from "../WebSharper.StdLib/Microsoft.FSharp.Collections.SeqModule.js"
import { get } from "../WebSharper.StdLib/Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicFunctions.js"
import { Dynamic } from "../WebSharper.UI/WebSharper.UI.Client.Attr.js"
import { range } from "../WebSharper.StdLib/Microsoft.FSharp.Core.Operators.js"
import { IsNullOrWhiteSpace } from "../WebSharper.StdLib/Microsoft.FSharp.Core.StringModule.js"
export function renderInteractiveBoard(flatBoard){
  const vars=map((v) => Var.Create_1(v===0?"":String(v)), flatBoard);
  const allVarsView=Map(ofSeq, Sequence(map((v) => v.View, vars)));
  return Doc.Element("table", [Attr.Create("style", sudokuTable())], ofSeq_1(delay(() => map_1((r) => Doc.Element("tr", [], ofSeq_1(delay(() => collect((c) => {
    const idx=r*9+c;
    const dynClass=Map((currentBoard) => hasConflict(idx, currentBoard)?"sudoku-err":"", allVarsView);
    const cellContent=get(flatBoard, idx)!==0?Doc.Element("span", [Attr.Create("style", sudokuText())], [Doc.TextNode(String(get(flatBoard, idx)))]):Doc.Input([Attr.Create("maxlength", "1"), Attr.Create("style", sudokuInput())], get(vars, idx));
    return[Doc.Element("td", [Attr.Create("style", c===2||c===5?r===2||r===5?sudokuCellBothThick():sudokuCellRightThick():r===2||r===5?sudokuCellBottomThick():sudokuCell()), Dynamic("class", dynClass)], [cellContent])];
  }, range(0, 8))))), range(0, 8)))));
}
export function hasConflict(idx, boardState){
  let conflict;
  const v=get(boardState, idx);
  if(IsNullOrWhiteSpace(v))return false;
  else {
    const r=idx/9>>0;
    const c=idx%9;
    conflict=false;
    for(let cc=0, _1=8;cc<=_1;cc++){
      const i=r*9+cc;
      if(i!==idx&&get(boardState, i)==v)conflict=true;
    }
    for(let rr=0, _2=8;rr<=_2;rr++){
      const i_1=rr*9+c;
      if(i_1!==idx&&get(boardState, i_1)==v)conflict=true;
    }
    const br=(r/3>>0)*3;
    const bc=(c/3>>0)*3;
    for(let rr_1=br, _3=br+2;rr_1<=_3;rr_1++)for(let cc_1=bc, _4=bc+2;cc_1<=_4;cc_1++){
      const i_2=rr_1*9+cc_1;
      if(i_2!==idx&&get(boardState, i_2)==v)conflict=true;
    }
    return conflict;
  }
}
