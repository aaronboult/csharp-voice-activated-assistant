using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Diagnostics;
using GUI;

namespace Managers{

    public static class ProgramManager{

        public static Dictionary<string, string> programMap { get; private set; }

        static ProgramManager(){

            try{

                XmlDocument doc = XmlManager.LoadDocument("programs.config.xml");

                programMap = XmlManager.GetMapFromDocument(doc.FirstChild.ChildNodes, "name", "path");

            }
            catch (FileNotFoundException){

                GenerateNewConfig();

            }

        }

        static void GenerateNewConfig(){

            string executablePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            programMap = new Dictionary<string, string>(){ 
                {"restart", $"{executablePath}\\main.exe"}
            };

            SaveConfig();

        }

        public static bool AddPathToConfig(string name, string path){

            bool success = false;

            if (!programMap.ContainsKey(name)){

                programMap.Add(name, path);

                SaveConfig();

                success = true;

            }

            return success;

        }

        public static bool RemovePathFromConfig(string name){

            bool success = false;

            if (programMap.ContainsKey(name)){

                programMap.Remove(name);

                SaveConfig();

                success = true;

            }

            return success;

        }

        static void SaveConfig(){

            string xmlBody = "";

            foreach (KeyValuePair<string, string> pair in programMap){

                xmlBody += $"<program name=\"{pair.Key}\" path=\"{pair.Value}\" />";

            }

            XmlDocument doc = new XmlDocument();

            string executablePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            doc.LoadXml($"<map>{xmlBody}</map>");

            doc.Save($"{executablePath}\\xmldocs\\programs.config.xml");

        }

        public static bool OpenProgram(string name){

            bool success = false;

            if (programMap.ContainsKey(name)){

                GUIController.LogOutput(programMap[name]);

                try{

                    Process.Start(programMap[name]);

                    success = true;

                }
                catch(System.ComponentModel.Win32Exception){

                    // Alert user of error?

                }

            }

            return success;

        }

    }

}