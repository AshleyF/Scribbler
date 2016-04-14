namespace BriefRobotics

module Scribbler =

    open System
    open System.IO
    open System.IO.Ports
    open System.Threading

    let connect port =
        printfn "Connecting to Scribbler on %s" port
        let baud, timeout = 38400, 15000
        let com = new SerialPort(port, baud)
        com.ReadTimeout <- timeout
        com.WriteTimeout <- timeout
        com.Open()
        Thread.Sleep 5000
        let stream = com.BaseStream
        new BinaryReader(stream), new BinaryWriter(stream)

    let _connect port =
        new BinaryReader(new MemoryStream()), new BinaryWriter(new MemoryStream())

    let mutable last = None

    let setMotors (reader: BinaryReader, writer: BinaryWriter) (left: float) (right: float) =
        let left' = (left + 1.0) * 100. |> byte
        let right' = (right + 1.0) * 100. |> byte
        writer.Write [| 109uy; right'; left'; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy |]
        reader.ReadBytes 9 |> ignore
        printfn "Read sensors 11"
        last <- reader.ReadBytes 11 |> Some

    type IR = { Left: bool; Right: bool }
    type Light = { Left: float; Middle: float; Right: float }
    type Line = { Left: bool; Right: bool }
    type Sensors = { IR: IR; Light: Light; Line: Line; Stall: bool }

    let getSensors (reader: BinaryReader, writer: BinaryWriter) =
        let sensors = match last with
                      | Some last -> printfn "Using last"
                                     last
                      | None -> printfn "Write sensor command"
                                writer.Write [| 65uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy |]
                                printfn "Read echo 9"
                                reader.ReadBytes 9 |> ignore
                                printfn "Read 11"
                                reader.ReadBytes 11
        let asBool i = sensors.[i] = 1uy
        let asFloat i = 0. // (((sensors.[i] |> int) << 8 ||| (sensors.[i + 1] |> int)) |> float) / 65535.
        printfn "SENSORS: %A" sensors
        { IR    = { Left = asBool 0; Right = asBool 1 }
          Light = { Left = asFloat 2; Middle = asFloat 4; Right = asFloat 6 }
          Line  = { Left = asBool 9; Right = asBool 10 }
          Stall = asBool 10 }
