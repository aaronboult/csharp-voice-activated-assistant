using System;
using System.Speech.Recognition;

namespace Control{

    public class VoiceController{

        public void Listen(){

            Console.WriteLine("Starting...");

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

                    Console.WriteLine("Listener up");

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

            Console.WriteLine($"Recognized: {e.Result.Text}");

        }

    }

}