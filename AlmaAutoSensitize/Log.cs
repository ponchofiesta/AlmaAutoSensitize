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
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace AlmaAutoSensitize
{
    /// <summary>
    /// Log to Windows EventLog
    /// </summary>
    class Log
    {
        /// <summary>
        /// Write a log message
        /// </summary>
        /// <param name="text"></param>
        public static void WriteLog(string text)
        {
            WriteLog(text, null);
        }

        /// <summary>
        /// Write a log message including some extra data
        /// </summary>
        /// <param name="text"></param>
        /// <param name="data"></param>
        public static void WriteLog(string text, string data)
        {
            if(!Properties.Settings.Default.Debug)
            {
                return;
            }
            string source = AppDomain.CurrentDomain.FriendlyName;
            //string log = "Application";

            // Try to create the event source. Administrator privileges needed
            //try
            //{
            //    if (!EventLog.SourceExists(source))
            //    {
            //        EventLog.CreateEventSource(source, log);
            //    }
            //}
            //catch (Exception e)
            //{
            //    MessageBox.Show("Could not create event source. Maybe you need to run this once as administrator.\n" + e.Message, "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return;
            //}

            // Write a new event entry
            //try
            //{
            //    if (data == null)
            //    {
            //        EventLog.WriteEntry(source, text, EventLogEntryType.Information);
            //    } else
            //    {
            //        EventLog.WriteEntry(source, text, EventLogEntryType.Information, 1, 1, Encoding.UTF8.GetBytes(data));
            //    }
            //}
            //catch(Exception e)
            //{
            //    MessageBox.Show("Could not write event\n" + e.Message, "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}

            string temppath = Path.GetTempPath();
            try
            {
                StreamWriter sw = File.AppendText(temppath + source + ".log");
                sw.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "] (" + Thread.CurrentThread.ManagedThreadId + ") " + text);
                sw.Close();
            } catch(Exception ex)
            {
                
            }
            



        }
    }
}
