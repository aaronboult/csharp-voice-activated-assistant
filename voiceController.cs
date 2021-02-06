using System;
using System.Speech.Recognition;

namespace Control{

    public class VoiceController{

        bool debug;

        bool terminate;

        public VoiceController(bool debug = false){

            this.debug = debug;

        }

        public void Listen(){

            if (debug){

                Console.WriteLine("Starting...");

            }

            try{

                using (
                    SpeechRecognitionEngine recognizer = new SpeechRecognitionEngine(
                        new System.Globalization.CultureInfo("en-GB")
                    )
                ){
                    recognizer.LoadGrammar(new DictationGrammar());

                    recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(SpeechRecognized);

                    recognizer.SetInputToDefaultAudioDevice();

                    recognizer.RecognizeAsync(RecognizeMode.Multiple);

                    if (debug){

                        Console.WriteLine("Listener up");

                    }

                    while (!terminate){

                        string input = Console.ReadLine();

                        TryExecuteCommand(input);

                    }

                }

            }
            catch{

                Console.WriteLine("An error relating to the Speech assembly has occurred and the process will now terminate.");

            }

        }

        private void SpeechRecognized(object sender, SpeechRecognizedEventArgs e){

            if (debug){

                Console.WriteLine($"Recognized: {e.Result.Text}\nComfidence: {e.Result.Confidence}");

            }

            TryExecuteCommand(e.Result.Text, true);

        }

        private void TryExecuteCommand(string commandText, bool voice = false){

            if (commandText.Split(' ')[0].ToLower() != "cake" && voice){

                return;

            }

            Console.WriteLine($"Executing {commandText}");

            try{

                Command command = new Command(commandText, debug);

                string result = command.Execute();

                Console.WriteLine(result);

            }
            catch{

                terminate = true;

            }

        }

    }

}