# AlmaAutoSensitize

AlmaAutoSensitize enables you to use automatic re- and desensitize books in Ex Libris Alma circulation using 3M-EM units.

## How it works

Alma doesn't support EM units anymore. But it supports RFID readers from Bibliotheca and 3M. The program simpulates a Bibliotheca RFID reader. It opens a HTTP port and waits for requests from Alma. On a request it reads the SOAP request from Alma and sends the re-/desensitize command over a serial port.

What Alma does with a supported RFID reader:

```
         +------------- PC --------------+
   Alma -|-> Webbrowser -> NGINX(Proxy) -|-> RFID reader
         +-------------------------------+
```
What it does with AlmaAutoSensitize and EM unit:
```
         +---------------- PC ----------------+
   Alma -|-> Webbrowser -> AlmaAutoSensitize -|-> EM unit
         +------------------------------------+
```

## Configuration

This app simulates the Bibliotheca RFID reader supported by Alma. You have to 
setup it first.
See: http://knowledge.exlibrisgroup.com/Alma/Product_Documentation/Alma_Online_Help_(English)/Integrations_with_External_Systems/040Fulfillment/RFID_Support
Choose "Bibliotheca - Liber8Connect". The Server URL is the path to your RFID 
reader. For example choose "http://localhost:5000/".

The configuration file is using .NET settings scheme. It is called 
AlmaAutoSensitize.exe.config near to the executable.

* `Debug: True | False`
True activates debug log via Windows event log. You need to run it as administrator once to setup AlmaAutoSensitize event source.

* `ListenPort: number`
The TCP/IP port number to listen for incoming HTTP requests from Alma. You have to use the same port as configured in Alma RFID settings (see above).

* `COMPort: number`
The COM port for your EM unit. On older 3M units it should be COM1 or COM2. If you're using a 3M Bookcheck Unit 942 using USB. You need to setup a virtual COM port using com0com. Rename the Port to COM8. You always have to run proxy app from 3M called "BC3Intfc.exe".

* `CmdResens: string`
The command send to EM unit for resensitizing. On 3M this is "RRR".

* `CmdDesens: string`
The command send to EM unit for desensitizing. On 3M this is "DDD".

* `UseSensitizer: string`
Only supported value is "Serial". I tried to add USB support for 3M Bookcheck unit 942 but .NET does not support that in an easy way.

* `AllowOriginRegex: string`
A regular expression string that matches the origin send by Alma in HTTP requests. For security reasons webbrowsers doesn't allow requests from one website to another. With this option we allow Alma to access this interface. Set something like: "alma\.exlibrisgroup\.com$" which matches alma.exlibrisgroup.com and all subdomains.

* ShowTooltips: True | False
AlmaAutoSensitize shows a tray icon to indicate the status. It changes the color on any operation. You can activate balloon tooltips as well.

## Requirements

AlmaAutoSensitize is written in C#. Therefor it needs .NET Framework 4.5 or 4.6 (tested with 4.6.1).

## Author

AlmaAutoSensitize is written by Michael Richter (m.richter at tu-berlin.de) for the library of Berlin Institute of Technology (Universitätsbibliothek, Technische Universität Berlin) in Germany.

## License
* AlmaAutoSensitize is lisenced under the GPL (see LICENSE_AlmaAutoSensitize.txt).
* The web server library HttpServer.dll is licensed under the Apache License 2.0 (see LICENSE_HttpServer.txt).
* The tray icons are lisenced under the GPL and copyright by Nick Roach (see LICENSE_AlmaAutoSensitize.txt).
