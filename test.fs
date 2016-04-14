module Test

open System
open BriefRobotics

let protocol port =
    let robot = Scribbler.connect port
    let sensors () = Scribbler.getSensors robot
    let motors = Scribbler.setMotors robot

    for i in 0 .. 1000 do
        sensors () |> printfn "Sensors %i\n%A" i
        Console.ReadLine() |> ignore

    [0. .. 0.1 .. 1.] @
    [1. .. -0.1 .. 0.]
    |> List.iter (fun speed ->
           printfn "Motors %f" speed
           motors -speed speed
           Console.ReadLine() |> ignore)

    printfn "Done"

let follow port =
    let robot = Scribbler.connect port
    let sensors () = Scribbler.getSensors robot
    let motors = Scribbler.setMotors robot

    // for i in 0 .. 1000 do
    while true do
        let s = sensors ()
        printfn "Sensors\n%A" s
        match s.Line.Left, s.Line.Right with
        | false, false ->
            printfn "None"
            motors -0.2 0.2 // spin left
        | true, false ->
            printfn "Left"
            motors -0.1 0.1 // vear left
        | false, true ->
            printfn "Right"
            motors 0.1 -0.1 // vear right
        | true, true ->
            printfn "Both"
            motors 0.3 0.3 // forward
        System.Threading.Thread.Sleep 33
        // motors 0. 0.
        // Console.ReadLine() |> ignore

    printfn "Done"
