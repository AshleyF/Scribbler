namespace Microsoft.Psi.FSharp

open System
open Microsoft.Psi

module Convert =
    let toFunc1<'a, 'b> f = new Func<'a, 'b>(f)
    let toFunc2<'a, 'b, 'c> f = new Func<'a, 'b, 'c>(f)
    let toFunc3<'a, 'b, 'c, 'd> f = new Func<'a, 'b, 'c, 'd>(f)
    let toAction1<'a> a = new Action<'a>(a)
    let toAction2<'a, 'b> a = new Action<'a, 'b>(a)
    let toAction3<'a, 'b, 'c> a = new Action<'a, 'b, 'c>(a)

[<AutoOpen>]
module Utility =
    let curry f a b = f (a, b)
    let uncurry f (a, b) = f a b
    let flip f a b = f b a

type Stream<'a>  = IPsiStream<'a>
type Time<'a> = Timestamped<'a>

module Stream =
    let map    f   (s: Stream<'a>) = Convert.toFunc1   f |> s.Select
    let filter f   (s: Stream<'a>) = Convert.toFunc1   f |> s.Where
    let reduce f   (s: Stream<'b>) = Convert.toFunc2   f |> s.Reduce
    let scan   f t (s: Stream<'b>) = Convert.toFunc2   f |> (curry s.Aggregate) t
    let iter   f   (s: Stream<'a>) = Convert.toAction1 f |> s.Do // "do" is a keyword
    let itert  f   (s: Stream<'a>) = Convert.toAction1 f |> s.DoT

    let zip    (a: Stream<'a>) (b: Stream<'b>) = a.Join(b)
    let zipm f (a: Stream<'a>) (b: Stream<'b>) = zip a b |> map f

    let zipop f = uncurry f |> zipm

    let zipmin s = zipop min s
    let zipmax s = zipop max s

    let windowedm size (f: 'a seq -> 'b) (a: Stream<'a>) = a.BufferedSelect(size, Convert.toFunc1 f)
    let windowed size = windowedm size id // identity mapping TODO move to C#?

    let time (a: Stream<'a>) = a.LiftTime() |> map (fun x -> x.Data, x.OriginatingTime)

    let pairwise (a: Stream<'a>) =
        windowed 2 a
        |> map List.ofSeq
        |> filter (List.length >> (=) 2)
        |> map (function [x; x'] -> (x, x') | _ -> failwith "Expected pair")
    let pairwisem f (a: Stream<'a>) = a |> pairwise |> map f

    let inline sum (s: Stream<(^a)>) = scan (+) LanguagePrimitives.GenericZero s
    let inline sumBy f (s: Stream<'a>) = s |> map f |> sum

    let inline min (s: Stream<(^a)>) = reduce min s
    let inline max (s: Stream<(^a)>) = reduce max s

    let histogram binfn = // TODO: move to C#? Pretty sure it won't be 5 lines ;)
        let accumulate m k =
            let b = binfn k
            let c = match Map.tryFind b m with Some c -> c + 1 | None -> 1
            Map.add b c m
        scan accumulate Map.empty >> map (Map.toList >> (List.sortBy fst))

    let sample n (s: Stream<'a>) = s.Sample n

    let windowedMean size (s: Stream<double>) = windowedm size Seq.average s // TODO: more generic than Stream<double>

    let windowedSlope size (s: Stream<double>) = windowedm size (fun s -> (Seq.last s - Seq.head s) / float size) s // TODO: fit line to sequence!

[<AutoOpen>]
module StreamOperators =
    let (.|>) s f   = Stream.map    f   s
    let (?|>) s f   = Stream.filter f   s
    let (^|>) s f t = Stream.reduce f   s
    let (>|>) s f t = Stream.scan   f t s
    let (!|>) s f   = Stream.iter   f   s
    let (%|>) s f   = Stream.itert  f   s
    let (.+)  s     = Stream.zipop (+)  s
    let (.*)  s     = Stream.zipop ( *) s // TODO: more such operators

module Psi =

    let run (node: IPsiStream) =
        let now = DateTime.Now
        let replay = new ReplayDescriptor(now, now + TimeSpan.FromSeconds(10.))
        use graph = PsiFactory.CompileAndRun("run", node, replay)
        let completed = replay.Completed.WaitOne()
        printfn "Completed"