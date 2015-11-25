﻿using SmartApero.Finders;
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

        private List<Question> _questions = new List<Question>()
        {
            new Question { Key = "subject", AssociatedMark = "Hello. Que puis-je faire pour vous ?"},
            new Question { Key = "persons", AssociatedMark = "Ok Guillaume. Combien de personnes participeront à votre {0:subject} ?", Finder = new PersonsFinder()},
            new Question { Key = "alcool", AssociatedMark = "Ok, j'ai noté que {1:persons} personnes participeront à votre {0:subject}. Souhaitez-vous boire de l'alcool ?", Finder = new GenericFinder() },
            new Question { Key = "diet", AssociatedMark = "Y-t-il des régimes particuliers à respecter: kacher, hallal, végétarien ?" , Finder = new GenericFinder()},
            new Question { Key = "budget", AssociatedMark = "Etes-vous limité à un budget ?" },
        };

        private Question _currentQuestion;

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

            StartConversation();
        }

        private void StartConversation()
        {
            AskQuestion(_questions[0]);
        }

        private void AskQuestion(Question question)
        {
            _currentQuestion = question;

            // Stop listening
            Button_Click_1(null, null);

            // Format question text
            var txt = string.Format(new QuestionFormatter(), question.AssociatedMark, _questions.ToArray());

            // Voice speaking
            Speak(txt);
        }

        private async void Start(object sender, RoutedEventArgs e)
        {
            if (speechRecognizer.State == SpeechRecognizerState.Idle)
            {
                await speechRecognizer.ContinuousRecognitionSession.StartAsync();
            }
        }

        #region Continuous Recognition

        private async void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            //if (args.Result.Confidence == SpeechRecognitionConfidence.Medium ||
            //  args.Result.Confidence == SpeechRecognitionConfidence.High)
            //{
            dictatedTextBuilder.Append(args.Result.Text + " ");

            if (_currentQuestion.Key == "subject")
            {
                var words = args.Result.Text.Split(' ');
                for (int i = 0; i < words.Length; i++)
                {
                    if (words[i] == "un" || words[i] == "une")
                    {
                        _currentQuestion.Value = words[i + 1];
                    }
                }
            }
            else 
            {
                _currentQuestion.Value = _currentQuestion.Finder.Resolve(args.Result.Text);
            }

            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                DictationTextBox.Text = dictatedTextBuilder.ToString();
                BtnClearText.IsEnabled = true;
            });

            _currentQuestion.HasBeenAsked = true;
            var nextQ = _questions.Where(e => !e.HasBeenAsked).First();

            AskQuestion(nextQ);

            //}
            //else
            //{
            //    await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            //    {
            //        DictationTextBox.Text = dictatedTextBuilder.ToString();
            //    });
            //}
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

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (speechRecognizer.State != SpeechRecognizerState.Idle)
            {
                await speechRecognizer.ContinuousRecognitionSession.StopAsync();
            }
        }

        #endregion

        /// <summary>
        /// Make the voice speak the text
        /// When the voice has ended, start listening
        /// </summary>
        /// <param name="text">Text to be spoken</param>
        /// <returns>nothing</returns>
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

        /// <summary>
        /// Start listening the user voice
        /// Increment the question count: a question has been asked, now listening for the user answer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MediaElementCtrl_MediaEnded(object sender, RoutedEventArgs e)
        {
            Start(null, null);
        }

        /// <summary>
        /// Reset the conversation at its beginning and start a new conversation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Reset(object sender, RoutedEventArgs e)
        {
            // Reset questions
            _questions.ForEach((q) =>
            {
                q.HasBeenAsked = false;
                q.Value = null;
            });

            DictationTextBox.Text = String.Empty;

            // Start the conversation
            StartConversation();
        }
    }
}

