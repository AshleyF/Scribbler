module Main

open System
open System.Threading
open Microsoft.Psi
open Microsoft.Psi.FSharp
open Microsoft.Psi.Toys

printfn "Welcome to Ψπ Scribbler"

let run (behavior: Scribbler.Sensors -> (float * float * int)) =
    let robot = Scribbler.connect "/dev/ttyUSB0"
    let move (left, right, duration: int) =
        Scribbler.setMotors robot left right
        Thread.Sleep duration // TODO: state machine
    PsiFactory.CreateTimer("pull", 50u, fun _ _ -> Scribbler.getSensors robot)
    |> Stream.iter (printfn "Sensors : %A")
    |> Stream.map behavior
    |> Stream.iter move
    |> Psi.run

let forward speed duration =  speed,  speed, duration
let reverse speed duration = -speed, -speed, duration
let left    speed duration = -speed,  speed, duration
let right   speed duration =  speed, -speed, duration
let stop          duration =     0.,     0., duration

let followLine (sensors: Scribbler.Sensors) =
    match sensors.Line.Left, sensors.Line.Right with
    | false, false -> left    0.2 0
    | true,  false -> left   -0.1 0
    | false, true  -> right   0.1 0
    | true,  true  -> forward 0.3 0

let avoidObstacles (sensors: Scribbler.Sensors) =
    if sensors.Stall then reverse -3. 2000 else
    match sensors.IR.Left, sensors.IR.Right with
    | false, false -> stop        0
    | true,  false -> left   -0.3 1000
    | false, true  -> right   0.3 1000
    | true,  true  -> forward 0.3 0

let turtle (sensors: Scribbler.Sensors) =
    let len = ref 0
    let pattern () =
        len := !len + 10
        forward 1. !len
    if sensors.IR.Left || sensors.IR.Right
    then stop 0
    else pattern ()

run avoidObstacles
