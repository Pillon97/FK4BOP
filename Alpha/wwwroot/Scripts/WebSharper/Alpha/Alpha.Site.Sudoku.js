import { zeroCreate2D, set2D, get2D, get } from "../WebSharper.StdLib/Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicFunctions.js"
import { ofSeq, sortBy } from "../WebSharper.StdLib/Microsoft.FSharp.Collections.ArrayModule.js"
import { delay, collect, map, forall } from "../WebSharper.StdLib/Microsoft.FSharp.Collections.SeqModule.js"
import { range } from "../WebSharper.StdLib/Microsoft.FSharp.Core.Operators.js"
import Random from "../WebSharper.StdLib/System.Random.js"
import { copy } from "../WebSharper.StdLib/Microsoft.FSharp.Collections.Array2DModule.js"
export function boardFromString(s){
  const board=zeroCreate2D(9, 9);
  for(let i=0, _1=80;i<=_1;i++)set2D(board, i/9>>0, i%9, s[i].charCodeAt()-"0".charCodeAt());
  return board;
}
export function boardToString(board){
  return ofSeq(delay(() => collect((r) => map((c) => String.fromCharCode("0".charCodeAt()+get2D(board, r, c)), range(0, 8)), range(0, 8)))).join("");
}
export function generate(){
  return removeClues(generateFull());
}
export function removeClues(board){
  const rng=new Random();
  const puzzle=copy(board);
  const positions=ofSeq(delay(() => collect((r) => map((c) =>[r, c], range(0, 8)), range(0, 8))));
  const shuffled=sortBy(() => rng.Next_2(), positions);
  for(let i=0, _1=45-1;i<=_1;i++){
    const p=get(shuffled, i);
    set2D(puzzle, p[0], p[1], 0);
  }
  return puzzle;
}
export function generateFull(){
  const rng=new Random();
  const board=zeroCreate2D(9, 9);
  const nums=sortBy(() => rng.Next_2(), ofSeq(range(1, 9)));
  for(let c=0, _1=8;c<=_1;c++)set2D(board, 0, c, get(nums, c));
  solve(board);
  return board;
}
export function solve(board){
  let num;
  let found=false;
  let result=false;
  let ri=0;
  let ci=0;
  let stop=false;
  for(let r=0, _1=8;r<=_1;r++)for(let c=0, _2=8;c<=_2;c++)if(!stop&&get2D(board, r, c)===0){
    ri=r;
    ci=c;
    found=true;
    stop=true;
  }
  if(!found)return true;
  else {
    num=1;
    while(num<=9&&!result)
      {
        isValid(board, ri, ci, num)?(set2D(board, ri, ci, num),solve(board)?result=true:set2D(board, ri, ci, 0)):void 0;
        num=num+1;
      }
    return result;
  }
}
export function isValid(board, row, col, num){
  const rowOk=forall((c) => get2D(board, row, c)!==num, range(0, 8));
  const colOk=forall((r) => get2D(board, r, col)!==num, range(0, 8));
  const _1=(row/3>>0)*3;
  const bc=(col/3>>0)*3;
  return rowOk&&colOk&&forall((v) => v!==num, delay(() => collect((r) => map((c) => get2D(board, r, c), range(bc, bc+2)), range(_1, _1+2))));
}
