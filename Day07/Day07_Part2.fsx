#time "on"

open System.Collections.Generic
open System.Diagnostics
open System.IO
open System.Text.RegularExpressions

open System.Text.RegularExpressions

let testInputs = [|
    "$ cd /";
    "$ ls";
    "dir a";
    "14848514 b.txt";
    "8504156 c.dat";
    "dir d";
    "$ cd a";
    "$ ls";
    "dir e";
    "29116 f";
    "2557 g";
    "62596 h.lst";
    "$ cd e";
    "$ ls";
    "584 i";
    "$ cd ..";
    "$ cd ..";
    "$ cd d";
    "$ ls";
    "4060174 j";
    "8033020 d.log";
    "5626152 d.ext";
    "7214296 k";
|]

type FolderNode = {
    Name: string;
    mutable Children: Node list
}
and Node =
    | File of string * int
    | Folder of FolderNode


let (|Match|_|) pattern input =
    let m = Regex.Match(input, pattern) in
    if m.Success then Some ([| for g in m.Groups -> g.Value |]) else None

let root = {Name = "/"; Children = []}

let rec getChildFolder (children: Node list) target =
    match children with
    | [] -> raise(InvalidDataException(sprintf "Failed to find child folder %s" target))
    | top :: rest ->
        match top with 
        | Folder f when f.Name = target ->
            f
        | _ -> getChildFolder rest target

let buildTree (input: string seq) =
    let mutable current = root
    let stack = Stack<FolderNode>()
    stack.Push(current)
    let mutable inFolderListing = false

    for inp in input do
        if inFolderListing then
            match inp with
            | Match "^\$" _ ->
                inFolderListing <- false
                //printfn "Listing end"

            | Match "dir (\w+)" m ->
                assert (m.Length = 2)
                //printfn "Found folder %s" m[1]
                let folder = Folder({Name = m[1]; Children = []})
                // Append isn't optimal, but more useful to preserve order
                current.Children <- current.Children @ [folder]

            | Match "(\d+) ([0-9.a-z]+)" m ->
                assert (m.Length = 3)
                let t = (m[2], int m[1])
                let f = File(t)
                //printfn "Foound file \"%s\" length %d" (fst t) (snd t)
                // Append isn't optimal, but more useful to preserve order
                current.Children <- current.Children @ [f]

            | _ -> ()
        // Do not use else, want to fall through
        if not inFolderListing then
            match inp with
            | "$ cd .." ->
                stack.Pop () |> ignore
                current <- stack.Peek ()
                //printfn "Move up to folder %s" current.Name

            | Match "^\\$ cd ([0-9.a-z]+)" m ->
                let fn = m[1]
                let f = getChildFolder current.Children fn
                stack.Push f
                current <- f
                //printfn "Move into folder \"%s\"" current.Name

            | "$ ls" -> 
                //printfn "List start"
                inFolderListing <- true
            | _ -> ()

//buildTree testInputs
buildTree (File.ReadAllLines("./Day07.txt"))
(*
let rec printFolder (prefix: string) (dir: FolderNode) =
    printfn "%s%s (dir)" prefix dir.Name
    for n in dir.Children do
        match n with
        | Folder f ->
            printFolder (prefix + "  ") f
        | File (n,s) ->
            printfn "%s  %s (file) %d" prefix n s

printfn "After parse:"
printFolder "  " root
*)

let empty = seq<string*int> Seq.empty

let rec folderSizes (dir: FolderNode) =
    let (size, bigFolders) = dir.Children
                            |> Seq.map (fun c ->
                                            match c with
                                            | File (n, s) -> (s, empty)
                                            | Folder f ->
                                                let (fs, ffs) = folderSizes f
                                                if fs >= 5_717_263 then
                                                    (fs, Seq.append ffs [| (f.Name, fs) |])
                                                else
                                                    (fs, ffs)
                                       )
                            |> Seq.fold (fun (accS, accFs) (s, fs)
                                            -> (accS+s, Seq.append accFs fs)) (0, empty)
    printfn "Folder \"%s\" recursive size %d" dir.Name size
    (size, bigFolders)

printfn "Finding recursive folder sizes:"
let (totalSize, bigFolders) = folderSizes root


printfn "Total size: %d" totalSize
printfn "Small Folders:"
for (n, s) in (bigFolders |> Seq.sortBy (fun (_, s) -> s)) do
    printfn "  %s: %d" n s
