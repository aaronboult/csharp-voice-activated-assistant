using System;
using System.Speech.Recognition;
using System.Speech.Synthesis;

namespace Control{

    class VoiceController{

        private bool debug;

        private bool terminate;

        private SpeechSynthesizer synthesiser;

        public bool speakOutput = false;

        public VoiceController(bool debug = false){

            this.debug = debug;

        }

        public void Listen(){

            if (debug){

                Console.WriteLine("Starting...");

            }

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

                if (debug){

                    Console.WriteLine("Listener up");

                }

                while (!terminate){

                    string input = Console.ReadLine();

                    TryExecuteCommand(input);

                }

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

                if (speakOutput){

                    synthesiser.Speak(result);

                }

            }
            catch{

                terminate = true;

            }

        }

    }

}