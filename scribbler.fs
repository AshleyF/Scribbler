namespace Microsoft.Psi.Toys

module Scribbler =

    open System
    open System.IO
    open System.IO.Ports
    open System.Threading

    let _connect port =
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

    let connect port =
        new BinaryReader(new MemoryStream(1024)), new BinaryWriter(new MemoryStream(1024))

    type IR = { Left: bool; Right: bool }
    type Light = { Left: float; Middle: float; Right: float }
    type Line = { Left: bool; Right: bool }
    type Sensors = { IR: IR; Light: Light; Line: Line; Stall: bool }

    let readSensors (reader: BinaryReader) =
        let readBool () = reader.ReadByte() = 1uy
        let readFloat () = (reader.ReadUInt16 () |> float) / 65535.
        { IR    = { Left = readBool (); Right = readBool () }
          Light = { Left = readFloat (); Middle = readFloat (); Right = readFloat () }
          Line  = { Left = readBool (); Right = readBool () }
          Stall = readBool () }

    let mutable last = None

    let setMotors (reader: BinaryReader, writer: BinaryWriter) (left: float) (right: float) =
        let left' = (left + 1.0) * 100. |> byte
        let right' = (right + 1.0) * 100. |> byte
        writer.Write [| 109uy; right'; left'; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy |]
        reader.ReadBytes 9 |> ignore
        last <- readSensors reader |> Some

    let getSensors (reader: BinaryReader, writer: BinaryWriter) =
        match last with
        | Some sensors -> last <- None; sensors
        | None -> writer.Write [| 65uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy |]
                  reader.ReadBytes 9 |> ignore
                  readSensors reader