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
        stream.ReadByte () |> printfn "Connected (%i)"
        new BinaryReader(stream), new BinaryWriter(stream)

    let mutable last = None

    let setMotors (reader: BinaryReader, writer: BinaryWriter) (left: float) (right: float) =
        let left' = (left + 1.0) * 100. |> byte
        let right' = (right + 1.0) * 100. |> byte
        writer.Write [| 109uy; right'; left'; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy |]
        reader.ReadBytes 9 |> ignore
        last <- reader.ReadBytes 11 |> Some

    type IR = { Left: bool; Right: bool }
    type Light = { Left: float; Middle: float; Right: float }
    type Line = { Left: bool; Right: bool }
    type Sensors = { IR: IR; Light: Light; Line: Line; Stall: bool }

    let getSensors (reader: BinaryReader, writer: BinaryWriter) =
        let sensors = match last with
                      | Some last -> last
                      | None -> writer.Write [| 65uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy |]
                                reader.ReadBytes 9 |> ignore
                                reader.ReadBytes 11
        let asBool i = sensors.[i] = 1uy
        let asFloat i = (((sensors.[i] |> int) << 8 ||| (sensors.[i + 1] |> int)) |> float) / 65535.
        printfn "SENSORS: %A" sensors
        { IR    = { Left = asBool 0; Right = asBool 1 }
          Light = { Left = asFloat 2; Middle = asFloat 4; Right = asFloat 6 }
          Line  = { Left = asBool 8; Right = asBool 9 }
          Stall = asBool 10 }
