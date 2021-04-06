using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;

namespace Managers{

    public static class ConfigManager{

        // The programMap is dynamic and entries can be added and removed
        public static Dictionary<string, string> programMap { get; private set; }

        // The general config is expected to be static and so no new entries will appear;
        // if not present they have been removed by the user and should be added back
        private static Dictionary<string, string> general;

        static ConfigManager(){

            try{

                XmlDocument doc = XmlManager.LoadDocument("config.xml");

                programMap = LoadConfigPortion(doc, "programs", "name", "path");
                
                general = LoadConfigPortion(doc, "general", "name", "value");

            }
            catch (FileNotFoundException){

                general = GenerateNewGeneralConfig();

                programMap = GenerateNewPathConfig();

                SaveConfig();

            }

        }

        private static Dictionary<string, string> LoadConfigPortion(XmlDocument doc, string title, string keyAttribute, string valueAttribute){

            (bool success, XmlNode node) programsNodeMatch = XmlManager.GetFirstLevelChild(title, ref doc);

            if (programsNodeMatch.success){

                if (programsNodeMatch.node.ChildNodes.Count > 0){

                    return XmlManager.GetMapFromDocument(
                        programsNodeMatch.node.ChildNodes, keyAttribute, valueAttribute
                    );

                }
                
            }

            return GenerateNewPathConfig();

        }

        public static void SaveConfig(){

            string xmlBody = "<xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\">";

            xmlBody += "<general>";

            foreach (KeyValuePair<string, string> pair in general){

                xmlBody += $"<entry name=\"{pair.Key}\" value=\"{pair.Value}\" />";

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

            if (!programMap.ContainsKey(name)){

                programMap.Add(name, path);

                SaveConfig();

                success = true;

            }

            return success;

        }

        public static bool RemovePathFromProgramMap(string name){

            bool success = false;

            if (programMap.ContainsKey(name)){

                programMap.Remove(name);

                SaveConfig();

                success = true;

            }

            return success;

        }

        public static bool UpdateProgramMapPath(string name, string path){

            bool success = false;

            if (programMap.ContainsKey(name)){

                programMap[name] = path;

                SaveConfig();

                success = true;

            }

            return success;

        }

        public static bool UpdateProgramMapName(string oldName, string newName){

            bool success = false;

            if (!programMap.ContainsKey(newName)){
                
                AddPathToProgramMap(newName, programMap[oldName]);

                RemovePathFromProgramMap(oldName);

                success = true;

            }

            return success || oldName == newName;

        }

        public static string GetGeneralConfigValue(string key){

            if (general.ContainsKey(key)){

                return general[key].ToLower();

            }

            return general[key] = "";

        }

        public static void UpdateGeneralConfig(string name, string value) => general[name] = value;

    }

}