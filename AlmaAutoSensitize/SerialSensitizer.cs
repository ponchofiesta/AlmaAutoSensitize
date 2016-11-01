/*
Copyright 2016 Michael Richter, m.richter at tu-berlin.de

This file is part of AlmaAutoSensitize.

AlmaAutoSensitize is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

AlmaAutoSensitize is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Foobar.  If not, see<http://www.gnu.org/licenses/>.

Diese Datei ist Teil von AlmaAutoSensitize.

AlmaAutoSensitize ist Freie Software: Sie können es unter den Bedingungen
der GNU General Public License, wie von der Free Software Foundation,
Version 3 der Lizenz oder (nach Ihrer Wahl) jeder späteren
veröffentlichten Version, weiterverbreiten und/oder modifizieren.

AlmaAutoSensitize wird in der Hoffnung, dass es nützlich sein wird, aber
OHNE JEDE GEWÄHRLEISTUNG, bereitgestellt; sogar ohne die implizite
Gewährleistung der MARKTFÄHIGKEIT oder EIGNUNG FÜR EINEN BESTIMMTEN ZWECK.
Siehe die GNU General Public License für weitere Details.

Sie sollten eine Kopie der GNU General Public License zusammen mit diesem
Programm erhalten haben.Wenn nicht, siehe <http://www.gnu.org/licenses/>.
*/

using System;
using System.IO.Ports;

namespace AlmaAutoSensitize
{
    /// <summary>
    /// A Sensitizer using the serial port (e.g. COM1)
    /// </summary>
    class SerialSensitizer : ISensitizer
    {
        private string cmdResensitize;
        private string cmdDesensitize;
        private SerialPort port;

        private bool isWorking = false;

        /// <summary>
        /// Resensitzing command sent to the device
        /// </summary>
        public string CmdResensitize
        {
            get
            {
                return cmdResensitize;
            }

            set
            {
                cmdResensitize = value;
            }
        }

        /// <summary>
        /// Desensitzing command sent to the device
        /// </summary>
        public string CmdDesensitize
        {
            get
            {
                return cmdDesensitize;
            }

            set
            {
                cmdDesensitize = value;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SerialSensitizer() : this(null, null, null)
        {
        }
        public SerialSensitizer(string comPort) : this(null, null, comPort)
        {
        }
        public SerialSensitizer(string cmdResens, string cmdDesens) : this(cmdResens, cmdDesens, null)
        {
        }
        /// <summary>
        /// Constructor; initialize serial port
        /// </summary>
        /// <param name="cmdResens"></param>
        /// <param name="cmdDesens"></param>
        /// <param name="comPort"></param>
        public SerialSensitizer(string cmdResens, string cmdDesens, string comPort)
        {
            Log.WriteLog("Initializing sensitizer");
            if (cmdResens == null)
            {
                cmdResens = "RRR";
            }
            if (cmdDesens == null)
            {
                cmdDesens = "DDD";
            }
            if (comPort == null)
            {
                comPort = "COM1";
            }
            CmdResensitize = cmdResens;
            CmdDesensitize = cmdDesens;
            port = new SerialPort();
            port.PortName = comPort;
            port.BaudRate = 9600;
            port.DataBits = 8;
            port.StopBits = StopBits.One;
            port.Parity = Parity.None;
            port.Handshake = Handshake.XOnXOff;
        }

        /// <summary>
        /// Send resensitize command
        /// </summary>
        public void Resensitize()
        {
            Log.WriteLog("Resensitize");
            if (isWorking)
            {
                throw new SensitizeException("Sensitizer is already in use.");
            }
            isWorking = true;

            port.Open();
            port.Write(this.CmdResensitize);
            port.Close();

            isWorking = false;
        }

        /// <summary>
        /// Send desensitize command
        /// </summary>
        public void Desensitize()
        {
            Log.WriteLog("Desensitize");
            if (isWorking)
            {
                throw new SensitizeException("Sensitizer is already in use.");
            }
            isWorking = true;

            port.Open();
            port.Write(this.CmdDesensitize);
            port.Close();

            isWorking = false;
        }

        /// <summary>
        /// Re/DeSensitize
        /// </summary>
        /// <param name="what"></param>
        public void Sensitize(bool what)
        {
            if(what)
            {
                Resensitize();
            }
            else
            {
                Desensitize();
            }
        }
        
    }

    class SensitizeException : Exception
    {
        public SensitizeException() : base()
        {
            Log.WriteLog("SensitizerException");
        }
        public SensitizeException(string message) : base(message)
        {
            Log.WriteLog("SensitizerException", message);
        }

    }
}
