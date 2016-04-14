module Test

open System
open BriefRobotics

let protocol port =
    let robot = Scribbler.connect port
    let sensors () = Scribbler.getSensors robot
    let motors = Scribbler.setMotors robot

    sensors () |> printfn "Sensors: %A"
    Console.ReadLine() |> ignore

    sensors () |> printfn "Sensors: %A"
    Console.ReadLine() |> ignore

    sensors () |> printfn "Sensors: %A"
    Console.ReadLine() |> ignore

    printfn "Motors 1 1"
    motors 1. 1.
    Console.ReadLine() |> ignore

    printfn "Motors 0.5 0.5"
    motors 0.5 0.5
    Console.ReadLine() |> ignore

    printfn "Motors 0 0"
    motors 0. 0.
    Console.ReadLine() |> ignore

    printfn "Motors -0.5 -0.5"
    motors -0.5 -0.5
    Console.ReadLine() |> ignore

    printfn "Motors -1 -1"
    motors -1. -1.
    Console.ReadLine() |> ignore

    printfn "Motors 0 0"
    motors 0. 0.
    Console.ReadLine() |> ignore

    printfn "Done"