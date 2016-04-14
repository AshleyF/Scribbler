namespace BriefRobotics

module Scribbler =

    open System
    open System.IO
    open System.IO.Ports

    let _connect port =
        let baud, timeout = 38400, 1500
        let com = new SerialPort(port, baud)
        com.ReadTimeout <- timeout
        com.WriteTimeout <- timeout
        com.Open()
        let stream = com.BaseStream
        new BinaryReader(stream), new BinaryWriter(stream)

    let connect port =
        new BinaryReader(new MemoryStream()), new BinaryWriter(new MemoryStream())

    let setMotors (reader: BinaryReader, writer: BinaryWriter) (left: float) (right: float) =
        let left' = (left + 1.0) * 100. |> byte
        let right' = (right + 1.0) * 100. |> byte
        writer.Write [| 109uy; right'; left'; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy |]
        reader.ReadBytes 9 |> ignore

    type IR = { Left: bool; Right: bool }
    type Light = { Left: float; Middle: float; Right: float }
    type Line = { Left: bool; Right: bool }
    type Sensors = { IR: IR; Light: Light; Line: Line; Stall: bool }

    let getSensors (reader: BinaryReader, writer: BinaryWriter) =
        writer.Write [| 65uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy |]
        reader.ReadBytes 9 |> ignore
        let readBool () = reader.ReadByte() = 1uy
        let readFloat () = (reader.ReadInt16() |> float) / 255.
        { IR    = { Left = readBool (); Right = readBool () }
          Light = { Left = readFloat (); Middle = readFloat (); Right = readFloat () }
          Line  = { Left = readBool (); Right = readBool () }
          Stall = readBool () }