using System;
using System.Speech.Recognition;

namespace Control{

    public class VoiceController{

        bool debug;

        public void Listen(bool debug = false){

            this.debug = debug;

            if (debug){

                Console.WriteLine("Starting...");

            }

            // try{

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

                    while (true){

                        Console.ReadLine();

                    }

                }

            // }
            // catch{

            //     Console.WriteLine("An error relating to the Speech assembly has occurred and the process will now terminate.");

            // }

        }

        private void SpeechRecognized(object sender, SpeechRecognizedEventArgs e){

            if (debug){

                Console.WriteLine($"Recognized: {e.Result.Text}");

            }

            Command command = new Command(e.Result.Text, debug);

            command.Execute();

        }

    }

}