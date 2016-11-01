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
using System.Net;
using HttpServer;
using HttpListener = HttpServer.HttpListener;

namespace AlmaAutoSensitize
{
    /// <summary>
    /// Simple web server
    /// </summary>
    class Webserver
    {
        private static HttpListener listener;
        private int listenPort;

        /// <summary>
        /// initialize web server
        /// </summary>
        /// <param name="listenPort"></param>
        public Webserver(int listenPort)
        {
            Log.WriteLog("Initializing web server on port "+listenPort);
            this.listenPort = listenPort;
            listener = HttpListener.Create(IPAddress.Loopback, this.listenPort);
        }

        public static HttpListener Listener
        {
            get
            {
                return listener;
            }

            set
            {
                listener = value;
            }
        }
        
        /// <summary>
        /// Start web server
        /// </summary>
        /// <param name="callback"></param>
        public void Start(EventHandler<RequestEventArgs> callback)
        {
            Log.WriteLog("Starting web server");
            listener.RequestReceived += callback;
            listener.Start(5);
        }

        /// <summary>
        /// Stop web server
        /// </summary>
        public void Stop()
        {
            Log.WriteLog("Stopping web server");
            //listener.Close();
        }
    }
}
