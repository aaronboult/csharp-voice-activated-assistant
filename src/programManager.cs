using System;
using System.Diagnostics;
using GUI;

namespace Managers{

    public static class ProgramManager{

        public static bool OpenProgram(string name){

            bool success = false;

            if (ConfigManager.programMap.ContainsKey(name)){

                GUIController.LogOutput(ConfigManager.programMap[name]);

                try{

                    Process.Start(ConfigManager.programMap[name]);

                    success = true;

                }
                catch(System.ComponentModel.Win32Exception){

                    GUIController.LogOutput($"Failed to open program: \"{name}\"", bold: true);

                }

            }

            return success;

        }

    }

}