using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.SpeechRecognition;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SmartApero
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Dictionary<int, string> Numbers = new Dictionary<int, string>();


        private List<Question> _questions = new List<Question>()
        {
            new Question { Key = "subject", AssociatedMark = "Hello. Que puis-je faire pour vous ?"},
            new Question { Key = "persons", AssociatedMark = "Ok Guillaume. Combien de personnes participeront à votre {0:subject} ?"},
            new Question { Key = "persons", AssociatedMark = "Ok, j'ai noté que {1:persons} personnes participeront à votre {0:subject}. Souhaitez-vous boire de l'alcool ?" },
            new Question { Key = "alcool", AssociatedMark = "" },
            new Question { Key = "budget", AssociatedMark = "" },
        };

        private int _questionsCount;
        private string _subject;
        private int _personsCount;
        private SpeechRecognizer speechRecognizer;

        // Speech events may originate from a thread other than the UI thread.
        // Keep track of the UI thread dispatcher so that we can update the
        // UI in a thread-safe manner.
        private CoreDispatcher dispatcher;

        private StringBuilder dictatedTextBuilder = new StringBuilder();

        private SpeechSynthesizer _speechSynthesizer;
        private VoiceInformation _voice;

        public MainPage()
        {
            this.InitializeComponent();
            Numbers = new Dictionary<int, string>();
            Numbers.Add(1, "un");
            Numbers.Add(2, "deux");
            Numbers.Add(3, "trois");
            Numbers.Add(4, "quatre");
            Numbers.Add(5, "cinq");
            Numbers.Add(6, "six");
            Numbers.Add(7, "sept");
            Numbers.Add(8, "huit");
            Numbers.Add(9, "neuf");
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            MediaElementCtrl.MediaEnded += MediaElementCtrl_MediaEnded;

            this.dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            this.speechRecognizer = new SpeechRecognizer();

            SpeechRecognitionCompilationResult result = await speechRecognizer.CompileConstraintsAsync();

            speechRecognizer.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;
            speechRecognizer.ContinuousRecognitionSession.Completed += ContinuousRecognitionSession_Completed;

            #region TTS
            try
            {
                _voice = (from voiceInformation
                            in Windows.Media.SpeechSynthesis.SpeechSynthesizer.AllVoices
                          select voiceInformation).First();

                _speechSynthesizer = new Windows.Media.SpeechSynthesis.SpeechSynthesizer();
                _speechSynthesizer.Voice = _voice;
            }
            catch (Exception exception)
            {
                var messageDialog = new Windows.UI.Popups.MessageDialog(exception.Message, "Exception");
                messageDialog.ShowAsync().GetResults();
            }
            #endregion

        }

        #region Continuous Recognition

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (speechRecognizer.State == SpeechRecognizerState.Idle)
            {
                await speechRecognizer.ContinuousRecognitionSession.StartAsync();
            }

            //StartRecognizing_Click(sender, e);
        }

        private async void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            var question = _questions[_questionsCount];

            //if (args.Result.Confidence == SpeechRecognitionConfidence.Medium ||
            //  args.Result.Confidence == SpeechRecognitionConfidence.High)
            //{
            dictatedTextBuilder.Append(args.Result.Text + " ");

            if (question.Key == "subject")
            {
                var words = args.Result.Text.Split(' ');
                for (int i = 0; i < words.Length; i++)
                {
                    if (words[i] == "un" || words[i] == "une")
                    {
                        question.Value = words[i + 1];
                    }
                }
            }
            else if (question.Key == "persons")
            {
                question.Value = RetrieveNumber(args.Result.Text);
            }

            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                DictationTextBox.Text = dictatedTextBuilder.ToString();
                BtnClearText.IsEnabled = true;
            });

            // Stop
            Button_Click_1(null, null);

            var txt = string.Format(new QuestionFormatter(), question.AssociatedMark, _questions.ToArray());

            Speak(txt);

            //}
            //else
            //{
            //    await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            //    {
            //        DictationTextBox.Text = dictatedTextBuilder.ToString();
            //    });
            //}
        }

        private string RetrieveNumber(string text)
        {
            var result = String.Empty;
            var words = text.Split(' ');

            if (words.Contains("entre") && words.Contains("et"))
            {
                var n1 = RetrieveNumber(text.Split(new[] { "entre" }, StringSplitOptions.None)[1]);
                var n2 = RetrieveNumber(text.Split(new[] { "et" }, StringSplitOptions.None)[1]);

                var n = Math.Round((decimal)((int.Parse(n1) + int.Parse(n2)) / 2));

                if (n == 15)
                    return "une quinzaine de";

                return "environ " + n.ToString();
            }

            // Find number
            for (int i = 0; i < words.Length; i++)
            {
                int res = 0;
                if (int.TryParse(words[i], out res))
                {
                    return res.ToString();
                }
            }

            // If not found, find the number in letter
            for (int i = 0; i < words.Length; i++)
            {
                if (Numbers.ContainsValue(words[i]))
                {
                    var number = Numbers.First(e => e.Value == words[i]);
                    return number.Key.ToString();
                }
            }

            return result;
        }

        private async void ContinuousRecognitionSession_Completed(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionCompletedEventArgs args)
        {
            if (args.Status != SpeechRecognitionResultStatus.Success)
            {
                if (args.Status == SpeechRecognitionResultStatus.TimeoutExceeded)
                {
                    await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        DictationButtonText.Text = " Continuous Recognition";
                        DictationTextBox.Text = dictatedTextBuilder.ToString();
                    });
                }
                else
                {
                    await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        DictationButtonText.Text = " Continuous Recognition";
                    });
                }
            }

            //// Stop
            //Button_Click_1(null, null);

            //var question = _questions[_questionsCount];
            //Speak(string.Format(new QuestionFormatter(), question.AssociatedMark, _questions));
        }

        private async void SpeechRecognizer_HypothesisGenerated(SpeechRecognizer sender, SpeechRecognitionHypothesisGeneratedEventArgs args)
        {
            string hypothesis = args.Hypothesis.Text;
            string textboxContent = dictatedTextBuilder.ToString() + " " + hypothesis + " ...";

            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                DictationTextBox.Text = textboxContent;
                BtnClearText.IsEnabled = true;
            });
        }

        private async void StartRecognizing_Click(object sender, RoutedEventArgs e)
        {
            // Create an instance of SpeechRecognizer.
            var speechRecognizer = new Windows.Media.SpeechRecognition.SpeechRecognizer();

            // Compile the dictation grammar by default.
            await speechRecognizer.CompileConstraintsAsync();

            // Start recognition.
            Windows.Media.SpeechRecognition.SpeechRecognitionResult speechRecognitionResult = await speechRecognizer.RecognizeWithUIAsync();

            // Do something with the recognition result.
            var messageDialog = new Windows.UI.Popups.MessageDialog(speechRecognitionResult.Text, "Text spoken");
            await messageDialog.ShowAsync();
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (speechRecognizer.State != SpeechRecognizerState.Idle)
            {
                await speechRecognizer.ContinuousRecognitionSession.StopAsync();
            }
        }

        #endregion

        private async Task Speak(string text)
        {
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                CortanaSpeakTxt.Text = text;

                var stream = await _speechSynthesizer.SynthesizeTextToStreamAsync(text);
                MediaElementCtrl.SetSource(stream, stream.ContentType);
                MediaElementCtrl.Play();

            });

        }

        private void MediaElementCtrl_MediaEnded(object sender, RoutedEventArgs e)
        {
            _questionsCount++;
            Button_Click(null, null);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            _questionsCount = 0;
            DictationTextBox.Text = String.Empty;
            Button_Click(null, null);
        }
    }
}

