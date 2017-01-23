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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using HttpServer;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Reflection;

namespace AlmaAutoSensitize
{
    class AlmaAutoSensApplication : ApplicationContext
    {
        private static NotifyIcon notifyicon;
        private Webserver webserver;
        private static ISensitizer sensitizer;
        private static ResourceManager lang;

        /// <summary>
        /// Constructor
        /// </summary>
        public AlmaAutoSensApplication()
        {
            Log.WriteLog("Starting up");

            lang = new ResourceManager(string.Format("{0}.Languages.strings", typeof(AlmaAutoSensApplication).Namespace), Assembly.GetExecutingAssembly());
            if(string.IsNullOrEmpty(Properties.Settings.Default.Language))
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.InstalledUICulture;
                Thread.CurrentThread.CurrentUICulture = CultureInfo.InstalledUICulture;
            } else
            {
                CultureInfo ci = CultureInfo.GetCultureInfo(Properties.Settings.Default.Language);
                Thread.CurrentThread.CurrentCulture = ci;
                Thread.CurrentThread.CurrentUICulture = ci;
            }
            
            // set up tray icon
            MenuItem menu = new MenuItem(lang.GetString("exit"));
            menu.Click += Menu_Click;

            notifyicon = new NotifyIcon();
            notifyicon.Icon = Properties.Resources.iconDefault;
            notifyicon.ContextMenu = new ContextMenu(new MenuItem[]{ menu });
            notifyicon.Visible = true;
            
            // choose sensitizer
            if(Properties.Settings.Default.UseSensitizer == SensitizerType.Serial)
            {
                sensitizer = new SerialSensitizer(Properties.Settings.Default.CmdResens, Properties.Settings.Default.CmdDesens, Properties.Settings.Default.COMPort);
            }
            else
            {
                MessageBox.Show(lang.GetString("no_sensitizer"), lang.GetString("error"), MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, (MessageBoxOptions)0x40000);
                ExitApplication();
            }
            
            // initialize web server
            webserver = new Webserver(Properties.Settings.Default.ListenPort);
            try
            {
                webserver.Start(OnRequest);
            } catch(Exception ex)
            {
                MessageBox.Show(string.Format("{0}\n{1}", lang.GetString("could_not_start_webserver"), ex.Message), lang.GetString("error"), MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, (MessageBoxOptions)0x40000);
                ExitApplication();
            }
            

            Log.WriteLog("Start should be completed");
        }

        /// <summary>
        /// Terminates the whole application
        /// </summary>
        private void ExitApplication()
        {
            Log.WriteLog("Exiting");
            notifyicon.Visible = false;
            Application.Exit();
            Environment.Exit(0);
        }

        /// <summary>
        /// Handles click in tray icons "Exit" menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Menu_Click(object sender, EventArgs e)
        {
            ExitApplication();
        }
        
        /// <summary>
        /// Handles a HTTP request to the web server. We assume a request to a Bibliotheca Liber8 RFID tagger.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnRequest(object sender, RequestEventArgs e)
        {
            Log.WriteLog("Web request received");

            IHttpClientContext context = (IHttpClientContext)sender;
            
            Log.WriteLog("Processing web request: " + e.Request.Uri.ToString());

            // read request body if any (POST request)
            string body = "";
            int maxlength = 1024 * 1024;
            int length = 0;
            Stream input = e.Request.Body;
            length = e.Request.ContentLength;
            if (length > maxlength)
            {
                Log.WriteLog("Web request body too big: length=" + length + ", maxlength=" + maxlength);
                input.Close();
                return;
            }
            char[] inbuffer = new char[length];
            StreamReader reader = new StreamReader(input, Encoding.UTF8);
            reader.Read(inbuffer, 0, length);
            //reader.Close();
            //input.Close();
            body = new string(inbuffer);
            Log.WriteLog("Got web request body for " + e.Request.Uri.ToString(), body);

            Boolean success = true;

            // response body
            string strOutput = "";

            // check SOAP request
            string soapaction = e.Request.Headers.Get("SOAPAction");
            string contenttype = e.Request.Headers.Get("Content-Type");
            if (soapaction != null && soapaction == "\"SetSecurity\"" && contenttype != null && contenttype.Contains("text/xml"))
            {
                Log.WriteLog("requested SetSecurity");
                XmlDocument xml = new XmlDocument();
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml.NameTable);
                XmlNode node = null;
                try
                {
                    nsmgr.AddNamespace("soapenv", "http://schemas.xmlsoap.org/soap/envelope/");
                    nsmgr.AddNamespace("rfid", "http://liber8Connect.com/WCF/Bibliotheca/RFIDPadService");
                    xml.Load(new StringReader(body));

                    // get security flag; true = resensitize, false = desensitize
                    node = xml.SelectSingleNode("/soapenv:Envelope/soapenv:Body/rfid:SetSecurity/isSecure", nsmgr);
                }
                catch (Exception ex)
                {
                    Log.WriteLog("XML Exception: " + ex.Message);
                    Msgbox(string.Format("{0}\n{1}", lang.GetString("could_not_read_xml_from_alma"), ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (node != null)
                {
                    // request wants to set security bit
                    bool what;
                    string sensstatus;
                    if (node.InnerText == "true")
                    {
                        what = true;
                        sensstatus = lang.GetString("resensitize");
                        notifyicon.Icon = Properties.Resources.iconLocked;
                    }
                    else
                    {
                        what = false;
                        sensstatus = lang.GetString("desensitize"); ;
                        notifyicon.Icon = Properties.Resources.iconUnlocked;
                    }
                    Log.WriteLog(sensstatus);
                    if (Properties.Settings.Default.ShowTooltips)
                    {
                        notifyicon.ShowBalloonTip(3000, sensstatus, sensstatus, ToolTipIcon.Info);
                    }
                    try
                    {
                        checkAndStartBC3Intfc();
                        sensitizer.Sensitize(what);
                    }
                    catch (Exception ex)
                    {
                        checkAndStartBC3Intfc();
                        try
                        {
                            sensitizer.Sensitize(what);
                        }
                        catch (Exception ex2)
                        {
                            success = false;
                            Msgbox(string.Format("{0}\n{1}", string.Format(lang.GetString("could_not_sens"), sensstatus), ex2.Message), lang.GetString("error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }


            // answer to Alma
            Log.WriteLog("Making web response");
            IHttpResponse response = e.Request.CreateResponse(context);
            string origin = e.Request.Headers.Get("Origin");
            if (origin != null && Regex.IsMatch(origin, Properties.Settings.Default.AllowOriginRegex, RegexOptions.IgnoreCase))
            {
                // allow Alma website to access this web interface using HTTP CORS
                response.AddHeader("Access-Control-Allow-Origin", origin);
                response.AddHeader("Vary", "Origin");
                response.AddHeader("Access-Control-Allow-Methods", "POST, GET, OPTIONS");
                response.AddHeader("Access-Control-Allow-Headers", "Accept, Content-Type, SOAPAction");
            }

            strOutput = String.Format("<?xml version=\"1.0\" encoding=\"UTF-8\"?><root><SetSecurityResult>{0}</SetSecurityResult></root>", success ? "true" : "false");
            if(!success)
            {
                response.Status = System.Net.HttpStatusCode.InternalServerError;
            }

            Stream output = response.Body;
            byte[] outbuffer = Encoding.UTF8.GetBytes(strOutput);
            response.ContentLength = outbuffer.Length;
            output.Write(outbuffer, 0, outbuffer.Length);
            output.Flush();
            response.Send();
            //output.Close();
        }
        
        /// <summary>
        /// Show a non blocking MessageBox
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="buttons"></param>
        /// <param name="icons"></param>
        private static void Msgbox(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icons )
        {
            new Thread(() =>
            {
                MessageBox.Show(message, title, buttons, icons, MessageBoxDefaultButton.Button1, (MessageBoxOptions)0x40000);
            }).Start();
        }

        /// <summary>
        /// Check if BC3Intfc.exe is running. If not, start it.
        /// </summary>
        private static void checkAndStartBC3Intfc()
        {
            if(Properties.Settings.Default.BC3IntfcPath == "")
            {
                return;
            }
            Log.WriteLog("Checking for BC3Intfc...");
            Process[] pname = Process.GetProcessesByName("BC3Intfc");
            if (pname.Length == 0)
            {
                Log.WriteLog("BC3Intfc not running, starting...");
                Process.Start(Properties.Settings.Default.BC3IntfcPath);
                Thread.Sleep(500);
                Log.WriteLog("BC3Intfc should be started now");
            }
        }
    }
}
