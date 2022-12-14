#time "on"

open System
open System.Collections.Generic
open System.Diagnostics
open System.IO
open System.Text.RegularExpressions

let testInput = [|
    "addx 15";
    "addx -11";
    "addx 6";
    "addx -3";
    "addx 5";
    "addx -1";
    "addx -8";
    "addx 13";
    "addx 4";
    "noop";
    "addx -1";
    "addx 5";
    "addx -1";
    "addx 5";
    "addx -1";
    "addx 5";
    "addx -1";
    "addx 5";
    "addx -1";
    "addx -35";
    "addx 1";
    "addx 24";
    "addx -19";
    "addx 1";
    "addx 16";
    "addx -11";
    "noop";
    "noop";
    "addx 21";
    "addx -15";
    "noop";
    "noop";
    "addx -3";
    "addx 9";
    "addx 1";
    "addx -3";
    "addx 8";
    "addx 1";
    "addx 5";
    "noop";
    "noop";
    "noop";
    "noop";
    "noop";
    "addx -36";
    "noop";
    "addx 1";
    "addx 7";
    "noop";
    "noop";
    "noop";
    "addx 2";
    "addx 6";
    "noop";
    "noop";
    "noop";
    "noop";
    "noop";
    "addx 1";
    "noop";
    "noop";
    "addx 7";
    "addx 1";
    "noop";
    "addx -13";
    "addx 13";
    "addx 7";
    "noop";
    "addx 1";
    "addx -33";
    "noop";
    "noop";
    "noop";
    "addx 2";
    "noop";
    "noop";
    "noop";
    "addx 8";
    "noop";
    "addx -1";
    "addx 2";
    "addx 1";
    "noop";
    "addx 17";
    "addx -9";
    "addx 1";
    "addx 1";
    "addx -3";
    "addx 11";
    "noop";
    "noop";
    "addx 1";
    "noop";
    "addx 1";
    "noop";
    "noop";
    "addx -13";
    "addx -19";
    "addx 1";
    "addx 3";
    "addx 26";
    "addx -30";
    "addx 12";
    "addx -1";
    "addx 3";
    "addx 1";
    "noop";
    "noop";
    "noop";
    "addx -9";
    "addx 18";
    "addx 1";
    "addx 2";
    "noop";
    "noop";
    "addx 9";
    "noop";
    "noop";
    "noop";
    "addx -1";
    "addx 2";
    "addx -37";
    "addx 1";
    "addx 3";
    "noop";
    "addx 15";
    "addx -21";
    "addx 22";
    "addx -6";
    "addx 1";
    "noop";
    "addx 2";
    "addx 1";
    "noop";
    "addx -10";
    "noop";
    "noop";
    "addx 20";
    "addx 1";
    "addx 2";
    "addx 2";
    "addx -6";
    "addx -11";
    "noop";
    "noop";
    "noop";
|]

let input = File.ReadAllLines("./Day10.txt") // testInput

let checkCycleCounts = [| 20; 60; 100; 140; 180; 220 |]

type MocpInstruction =
    | Noop
    | PrepAdd
    | Add of int

let (|Match|_|) pattern input =
    let m = Regex.Match(input, pattern) in
    if m.Success then Some ([| for g in m.Groups -> g.Value |]) else None

let decodeInstruction inp =
    match inp with
    | "noop" -> [| Noop |] 
    | Match "addx (-?\d+)" m ->
        let x = int (m[1])
        [| PrepAdd; Add(x) |]
    | _ -> raise(ArgumentOutOfRangeException(sprintf "Invalid instruction \"%s\"" inp))

let processInstruction state microIntr = 
    let (cycle, currX, nextX) = state
    let newCycle = cycle+1
    match microIntr with
    | Noop
    | PrepAdd -> (newCycle, nextX, nextX)
    | Add i -> (newCycle, nextX, nextX + i)

// Cycle count, current-X-register, next-x-register
let initialState = (0, 1, 1)

let res= input
                |> Seq.collect decodeInstruction
                |> Seq.scan processInstruction initialState
                |> Seq.map (fun (a, b, _) -> (a, b))
                //|> Seq.map (fun (c, x, nx) -> printfn "%d: %d (next %d)" c x nx; (c, x, nx))
                |> Seq.toArray

let part1Res = res
                |> Seq.where (fun (c, _) -> Array.contains c checkCycleCounts)
                |> Seq.map (fun (c, x) -> (c, x, c*x))
                |> Seq.toArray

let total = part1Res |> Seq.sumBy (fun (_, _, x) -> x)
printfn "Part 1: Sum of signal strength: %d" total

let horizontalLen = 40
let veritcalLen = 6

printfn "Part 2:"
for cycle in 1..(horizontalLen*veritcalLen) do
    let hPos = cycle % horizontalLen
    // Because mixing of 0 based and 1 based indexing
    let hPosAdj = if hPos = 0 then horizontalLen-1 else hPos-1

    if hPos = 1 then
        printf "%3d: " cycle

    let getCycleX c =
        if c < 0 then -100 else (snd res[c])

    let spritePos = getCycleX cycle
    if abs (spritePos - hPosAdj) <= 1 then
        printf "#"
    else
        printf "."

    if hPos = 0 then
        printfn ""

printfn ""
