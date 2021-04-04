using System;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using Parsing;
using GUI;

namespace Control{

    class VoiceController{

        private bool debug;

        private SpeechSynthesizer synthesiser;

        public VoiceController(bool debug = false){

            this.debug = debug;

        }

        public void Listen(){

            using (synthesiser = new SpeechSynthesizer())
            using (
                SpeechRecognitionEngine recognizer = new SpeechRecognitionEngine(
                    new System.Globalization.CultureInfo("en-GB")
                )
            ){
                synthesiser.SetOutputToDefaultAudioDevice();

                recognizer.LoadGrammar(new DictationGrammar());

                recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(SpeechRecognized);

                recognizer.SetInputToDefaultAudioDevice();

                recognizer.RecognizeAsync(RecognizeMode.Multiple);
                
                GUIController.LogOutput("Hello! How may I help?", bold: true, prefix: "");

                while (MainGUI.Opened){

                    string input = MainGUI.Reference.InputValue; // Value stored as its cleared once read

                    if (input != ""){

                        TryExecuteCommand(input);

                    }

                }

            }

        }

        private void SpeechRecognized(object sender, SpeechRecognizedEventArgs e){

            if (debug){

                GUIController.LogOutput($"Recognized: {e.Result.Text}\nComfidence: {e.Result.Confidence}");

            }

            TryExecuteCommand(e.Result.Text, true);

        }

        private void TryExecuteCommand(string commandText, bool voice = false){

            if (voice){

                if (!MainGUI.Reference.microphoneToggle){

                    return;

                }

                GUIController.LogOutput($"Voice Detected: {commandText}");

                if (commandText.Split(' ')[0].ToLower() != "cake"){

                    return; // If the command word does not match, return

                }

            }

            try{

                Command command = new Command(commandText, debug);

                string result = command.Execute();

                GUIController.LogOutput(result);

                if (MainGUI.Reference.voiceOutputToggle){

                    synthesiser.Speak(result);

                }

            }
            catch{

                GUIController.LogOutput("An error occurred when processing that request.");

            }

        }

    }

}