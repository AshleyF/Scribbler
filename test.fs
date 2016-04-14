open System
open System.Threading
open System.IO
open System.IO.Ports

printfn "Testing Scribbler Connection"

let port, baud = "/dev/ttyUSB1", 38400

let com = new SerialPort(port, baud)

let received (e: SerialDataReceivedEventArgs) =
  printfn "Received: %A (%A)" e e.EventType

let reader, writer =
  // let com = new SerialPort(port, baud)
  com.DataReceived.Add(received)
  // com.DtrEnable <- true
  // com.ReceivedBytesThreshold <- 1
  com.ReadTimeout <- 10000 // 1500
  com.WriteTimeout <- 10000 // 1500
  com.Open()
  com.DiscardInBuffer() // TODO: needed?
  com.DiscardOutBuffer() // TODO: needed?
  let stream = com.BaseStream
  new BinaryReader(stream), new BinaryWriter(stream)

let report message = printfn "%s: Read %i Write %i" message com.BytesToRead com.BytesToWrite

let write (bytes: byte array) =
  report (sprintf "  Writing %A" bytes)
  writer.Write bytes
  writer.Flush () // TODO: necessary?
  report "  Done"

let read () =
  Thread.Sleep(500)
  report "Reading"
  while com.BytesToRead > 0 do
    printf "%A " (reader.ReadByte ())
  printfn "."

let getInfo () =
  report "Get info"
  write [| 80uy; 32uy; 32uy; 32uy; 32uy; 32uy; 32uy; 32uy; 32uy |]
  read ()
  System.Threading.Thread.Sleep(100)
  report "Get info"
  write [| 80uy; 32uy; 32uy; 32uy; 32uy; 32uy; 32uy; 32uy; 32uy |]
  read ()

let translate () =
  report "Translating"
  write [| 0x6duy; 0x6euy; 0x6euy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy |]
  read ()

let stop () =
  report "Stopping"
  write [| 0x6cuy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy |]
  read ()

let getLights () =
  report "GetLight"
  write [| 0x46uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy |]
  read ()

let getIRs () =
  report "GetLight"
  write [| 0x49uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy |]
  read ()

let getSensors () =
  report "GetSensors"
  write [| 65uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy |]
  read ()

let setMotors (left: float) (right: float) =
  let left' = (left + 1.0) * 100. |> byte
  let right' = (right + 1.0) * 100. |> byte
  report (sprintf "SetMotors %f %f" left right)
  write [| 109uy; right'; left'; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy |]
  read ()

// getInfo ()
translate ()

Console.ReadLine() |> ignore

stop ()

Console.ReadLine() |> ignore

translate ()

Console.ReadLine() |> ignore

stop ()

Console.ReadLine() |> ignore

getSensors ()

Console.ReadLine() |> ignore

getSensors ()

Console.ReadLine() |> ignore

getSensors ()

Console.ReadLine() |> ignore

setMotors -1. -1.

Console.ReadLine() |> ignore

setMotors -0.5 -0.5

Console.ReadLine() |> ignore

setMotors 0. 0.

Console.ReadLine() |> ignore

setMotors 0.5 0.5

Console.ReadLine() |> ignore

setMotors 1. 1.

Console.ReadLine() |> ignore

setMotors 1. 0.

Console.ReadLine() |> ignore

setMotors 0. 1.

Console.ReadLine() |> ignore

stop ()


(*
0,1 IR
2,3 Light
4,5 Light
6,7 Light
8,9 Line
10  Stall

*)
