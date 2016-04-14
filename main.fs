﻿module Main

open System
open BriefRobotics
open Microsoft.Psi
open Microsoft.Psi.FSharp

let port = "/dev/ttyUSB1"

// Test.protocol port

let robot = Scribbler.connect port

let sensors = PsiFactory.CreateTimer("pull", 100u, fun _ _ -> Scribbler.getSensors robot)
let actuate motors = motors |> Stream.iter (fun (left, right) -> Scribbler.setMotors robot left right)

let drive (sensors: Scribbler.Sensors) =
    if   sensors.IR.Left  then Some (1., -1.)
    elif sensors.IR.Right then Some (-1., 1.)
    else None

sensors
|> Stream.map drive
|> Stream.filter Option.isSome
|> Stream.map Option.get
|> actuate
|> Psi.run