using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;

namespace Managers{

    public static class ConfigManager{

        public static Dictionary<string, string> programMap { get; private set; }

        public static Dictionary<string, string> general { get; private set; }

        static ConfigManager(){

            try{

                XmlDocument doc = XmlManager.LoadDocument("config.xml");

                programMap = LoadConfigPortion(doc, "programs", "name", "path");
                
                general = LoadConfigPortion(doc, "general", "name", "value");

            }
            catch (FileNotFoundException){

                programMap = GenerateNewGeneralConfig();

                general = GenerateNewPathConfig();

                SaveConfig();

            }

        }

        private static Dictionary<string, string> LoadConfigPortion(XmlDocument doc, string title, string keyAttribute, string valueAttribute){

            (bool success, XmlNode node) programsNodeMatch = XmlManager.GetFirstLevelChild(title, ref doc);

            Dictionary<string, string> loaded;

            if (programsNodeMatch.success){

                loaded = XmlManager.GetMapFromDocument(
                    programsNodeMatch.node.ChildNodes, keyAttribute, valueAttribute
                );
                
            }
            else{

                loaded = GenerateNewPathConfig();

            }

            return loaded;

        }

        private static void SaveConfig(){

            string xmlBody = "<xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\">";

            xmlBody += "<general>";

            foreach (KeyValuePair<string, string> pair in general){

                xmlBody += $"<entry name=\"{pair.Key}\" path=\"{pair.Value}\" />";

            }

            xmlBody += "</general>";

            xmlBody += "<programs>";

            foreach (KeyValuePair<string, string> pair in programMap){

                xmlBody += $"<program name=\"{pair.Key}\" path=\"{pair.Value}\" />";

            }

            xmlBody += "</programs>";

            xmlBody += "</xml>";

            XmlDocument doc = new XmlDocument();

            string executablePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            doc.LoadXml(xmlBody);

            doc.Save($"{executablePath}\\xmldocs\\config.xml");

        }

        private static Dictionary<string, string> GenerateNewGeneralConfig(){

            var newGeneral = new Dictionary<string, string>(){
                {"microphone", "false"},
                {"speakers", "false"},
            };

            return newGeneral;

        }

        private static Dictionary<string, string> GenerateNewPathConfig(){

            string executablePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            var newProgramMap = new Dictionary<string, string>(){ 
                {"restart", $"{executablePath}\\main.exe"}
            };

            return newProgramMap;

        }

        public static bool AddPathToProgramMap(string name, string path){

            bool success = false;

            if (!ConfigManager.programMap.ContainsKey(name)){

                ConfigManager.programMap.Add(name, path);

                SaveConfig();

                success = true;

            }

            return success;

        }

        public static bool RemovePathFromProgramMap(string name){

            bool success = false;

            if (ConfigManager.programMap.ContainsKey(name)){

                ConfigManager.programMap.Remove(name);

                SaveConfig();

                success = true;

            }

            return success;

        }

    }

}