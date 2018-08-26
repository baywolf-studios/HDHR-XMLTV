﻿//  Fairfield Tek L.L.C.
//  Copyright (c) 2016, Fairfield Tek L.L.C.
//  
//  
// THIS SOFTWARE IS PROVIDED BY WINTERLEAF ENTERTAINMENT LLC ''AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR 
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL WINTERLEAF ENTERTAINMENT LLC BE LIABLE FOR ANY DIRECT, INDIRECT, 
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND 
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR 
// OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH 
// DAMAGE. 
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Xml;
using HdHomeRunEpgXml.Data;
using HdHomeRunEpgXml.Util;

namespace HdHomeRunEpgXml
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Argument expected: XML File Path.");
                Console.WriteLine("Example: ");
                Console.WriteLine("         HdHomeRunEpgXml " + '"' + "hdEpg.xml" + '"');
                Console.WriteLine("         HdHomeRunEpgXml " + '"' + "C:\\hdEpg.xml" + '"');
                Console.WriteLine("");
                Console.WriteLine("Press enter to exit.");
                Console.ReadLine();
                return;
            }

            List<XmlElement> tvShows = new List<XmlElement>();

            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "ISO-8859-1", null);
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);
            XmlElement eleTv = doc.CreateElement(string.Empty, "tv", string.Empty);
            doc.AppendChild(eleTv);
            List<string> processedChannel = new List<string>();

            //Fetch the devices registered.
            List<HdConnectDevice> devices = JsonCalls.GetHdConnectDevices();
            //For Each device.
            foreach (HdConnectDevice device in devices)
            {
                Console.WriteLine("Processing Device: " + device.DeviceID);
                //Get the Auth info
                HdConnectDiscover discover = device.GetHdConnectDiscover();
                //Get the channels
                List<HdConnectChannel> channels = discover.GetHdConnectChannels();
                //For each channel
                foreach (HdConnectChannel channel in channels)
                {
                    //If we already processed this channel, then skip
                    if (processedChannel.Contains(channel.GuideNumber))
                        continue;
                    //Process the channel
                    processedChannel.Add(channel.GuideNumber);
                    //Add the tv shows
                    tvShows.AddRange(doc.ProcessChannel(eleTv, channel, discover.DeviceAuth));
                }
            }

            //Append the shows to the list
            foreach (XmlElement element in tvShows)
                eleTv.AppendChild(element);

            try
            {
                //Save the file
                doc.Save(args[0]);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine("Finished.");
            Console.WriteLine("Epg file saved to: " + args[0]);
        }
    }
}