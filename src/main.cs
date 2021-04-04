using System;
using System.Threading;
using Managers;
using GUI;
using Testing;

namespace Control
{
    class Program{

        private static void Main(string[] args){
            
            Thread voiceThread = new Thread(new ThreadStart(BeginVoice));

            voiceThread.Start();

            GUIController.OpenMainGUI();

            voiceThread.Join();

        }

        private static void BeginVoice(){

            int millisecondsWaited = 0;

            // Either wait for the reference to be created or 5s to pass
            while (!MainGUI.Opened && millisecondsWaited < 5000){

                Thread.Sleep(1);

                millisecondsWaited++;

            }
    
            if (MainGUI.Opened){

                Console.WriteLine($"Awaited GUI instantiation.\nWaited: {millisecondsWaited}ms");

                VoiceController voice = new VoiceController();

                voice.Listen();

            }

            Console.WriteLine("Voice thread closing");

        }

    }
}