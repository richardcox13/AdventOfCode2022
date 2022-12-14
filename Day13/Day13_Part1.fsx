#time "on"

open System
open System.Collections.Generic
open System.Diagnostics
open System.IO
open System.Text.RegularExpressions

let rawTestInput = "
[1,1,3,1,1]
[1,1,5,1,1]

[[1],[2,3,4]]
[[1],4]

[9]
[[8,7,6]]

[[4,4],4,4]
[[4,4],4,4,4]

[7,7,7,7]
[7,7,7]

[]
[3]

[[[]]]
[[]]

[1,[2,[3,[4,[5,6,7]]]],8,9]
[1,[2,[3,[4,[5,6,0]]]],8,9]
"

let testInput = rawTestInput.Trim()

let inputString = File.ReadAllText("./Day13.txt") //testInput

type Packet = 
        | Value of int
        // Can have a zero length list...
        | Sequence of Packet list
        override this.ToString() =
                match this with
                | Value x -> x.ToString()
                | Sequence xs ->
                    let content = xs |> Seq.map (fun p -> p.ToString()) |> String.concat ","
                    "[" + content + "]"
        static member lessThan (left: Packet) (right: Packet) =
            let rec compSeq (ls: Packet list) (rs: Packet list) =
                let inner (ls: Packet list) (rs: Packet list) =
                    match (ls, rs) with
                    | ([], []) -> 0
                    | ([], _::_) -> -1
                    | (_::_, []) -> +1
                    | (lf::lr, rf::rr) ->
                        let c = comparePacket lf rf
                        if c <> 0 then
                            c
                        else
                            compSeq lr rr
                let c = inner ls rs
                //let conv s = s |> Seq.map (fun p -> p.ToString()) |> String.concat ","
                //printfn "    CompSeq %s to %s -> %d" (conv ls) (conv rs) c
                c

            and comparePacket (left: Packet) (right: Packet) =
                let rec inner (left: Packet) (right: Packet) =
                    match (left, right) with
                    | (Value l, Value r) ->
                        let c = l - r
                        if c < 0 then
                            -1
                        else if c = 0 then
                            0
                        else
                            +1
                    | (Sequence l, Sequence r) -> compSeq l r
                    | (Value _, Sequence r) -> compSeq [left] r
                    | (Sequence l, Value _) -> compSeq l [right]
                let c = inner left right
                //printfn "  Compare %s to %s -> %d" (left.ToString()) (right.ToString()) c
                c
            (comparePacket left right) <= 0

let (|PackeStart|_|) (input: string) =
    if input.Length > 0 && input[0] = '[' then
        Some (input.Substring(1))
    else
        None

let (|PacketEnd|_|) (input: string) =
    if input.Length > 0 && input[0] = ']' then
        Some (input.Substring(1).TrimStart(','))
    else
        None

let matchNumber = Regex("^(\\d+),?")

let (|Number|_|) (input: string) =
    if input.Length = 0 then
        None
    else
        let m = matchNumber.Match(input)
        if not m.Success then
            None
        else
            let g = m.Groups[1].Value
            // Use whole match for the skip, so will incoude the comma
            Some (int g, input.Substring(m.Value.Length))

let parsePacket (input: string) =
    let rec doParse input (currentSeq: Packet list) =
        //printfn "Matching: %s" input
        match input with
        | "" ->
            //printfn "Empty input: current seq: %s" (currentSeq |> Seq.map (fun p -> p.ToString()) |> String.concat "//")
            (currentSeq, "")
        | PackeStart rest ->
            let (innerSeq, rr) = doParse rest []
            doParse rr (currentSeq @ [Sequence(innerSeq)])
        | PacketEnd rest ->
             (currentSeq, rest)
        | Number (n, rest) ->
            doParse rest (currentSeq @ [Value(n)])
        | _ -> raise(UnreachableException(sprintf "Failed to match \"%s\"" input))

    // All inputs are surrounded by "[" and "]": strip them off...
    let inp = input.Substring(1, input.Length-2)
    let res = doParse inp []
    assert((snd res).Length = 0)
    Sequence(fst res)

let testParse input =
    let res = parsePacket input
    printfn "\"%s\" --> %s" input (res.ToString())


let input = inputString.Split("\r\n\r\n") 
                |> Array.mapi (fun idx s ->
                                let ss = s.Split("\r\n")
                                assert(ss.Length = 2)
                                (idx, ss[0], ss[1])
                             )

let doPair idx leftStr rightStr =
    let left = parsePacket leftStr
    let right = parsePacket rightStr
    printfn "Input #%d: %s & %s" (idx+1) leftStr rightStr
    printfn "          %s   %s" (left.ToString()) (right.ToString())
    let isLessThan = Packet.lessThan left right
    printfn "          Order is %s" (if isLessThan then "correct" else "incorrect")
    isLessThan

let mutable finalSum = 0
for (idx, leftStr, rightStr) in input do
    if doPair idx leftStr rightStr then
        finalSum <- finalSum + idx+1

printfn "Final sum = %d" finalSum
