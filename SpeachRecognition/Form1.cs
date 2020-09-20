using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Diagnostics;

namespace SpeachRecognition
{
    public partial class Form1 : Form
    {
        readonly SpeechRecognitionEngine _recognizer;

        readonly string _assistantName;

        readonly List<VoiceGrammarActionXref> _dictionaryOfActions;

        VoiceGrammarActionXref _currentAction;

        public Form1()
        {
            InitializeComponent();

            this.ShowInTaskbar = false;
            this.WindowState = FormWindowState.Minimized;

            this._recognizer = new SpeechRecognitionEngine();

            this._assistantName = "Computer";
            this._dictionaryOfActions = new List<VoiceGrammarActionXref>
            {
                //new VoiceGrammarActionXref(() => this.TurnOfPc(), "Shut down the pc", "Shut down pc", "Turn off the pc", "Turn off pc")
                //{
                //    YesConfirmation = new VoiceGrammarActionXref(() => this.Speak("Turning off pc"), "Yes", "Yes, please"),
                //    NoConfirmation = new VoiceGrammarActionXref(() => this.Speak("Okey"), "No", "No, thank you"),
                //    StringFormatMessageOnMatch = "Do you want me to {0}"
                //},
                new VoiceGrammarActionXref(() => this.LockScreen(), "Lock the screen", "Lock screen")
                {
                    YesConfirmation = new VoiceGrammarActionXref(() => this.Speak("Locking the screen"), "Yes", "Yes, please"),
                    NoConfirmation = new VoiceGrammarActionXref(() => this.Speak("Okey"), "No", "No, thank you"),
                    StringFormatMessageOnMatch = "Do you want me to {0}"
                }
            };
        }

        //https://www.codeproject.com/Articles/483347/Speech-recognition-speech-to-text-text-to-speech-a

        private void Form1_Load(object sender, EventArgs e)
        {
            var keyWords = this._dictionaryOfActions.SelectMany(x => x.KeyWords).ToArray();
            var yesKeyWords = this._dictionaryOfActions.Where(x => x.YesConfirmation != null).SelectMany(x => x.YesConfirmation.KeyWords).ToArray();
            var noKeyWords = this._dictionaryOfActions.Where(x => x.NoConfirmation != null).SelectMany(x => x.NoConfirmation.KeyWords).ToArray();

            this._recognizer.LoadGrammar(this.CreateGramma(true, keyWords));
            this._recognizer.LoadGrammar(this.CreateGramma(true, yesKeyWords));
            this._recognizer.LoadGrammar(this.CreateGramma(true, noKeyWords));

            this._recognizer.SetInputToDefaultAudioDevice();

            this._recognizer.RecognizeAsync(RecognizeMode.Multiple);
            this._recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(sr_SpeechRecognized);
            this._recognizer.RecognizeCompleted += new EventHandler<RecognizeCompletedEventArgs>(this.RecognizeCompleted);
        }

        void RecognizeCompleted(object sender, RecognizeCompletedEventArgs e)
        {
            //this._recognizer.RecognizeAsyncStop();
            //this._recognizer.RecognizeAsync(RecognizeMode.Single);
        }

        private Grammar CreateGramma(bool isStartsWithAssistantName, params string[] keyWords)
        {
            var gb = new GrammarBuilder();

            if (isStartsWithAssistantName)
            {
                gb.Append(this._assistantName);
            }

            gb.Append(new Choices(keyWords));

            return new Grammar(gb);
        }

        private void LockScreen()
        {
            Windows.LockWorkStation();
        }

        private void TurnOfPc()
        {
            var psi = new ProcessStartInfo("shutdown", "/s /t 10");
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            Process.Start(psi);
        }

        void sr_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            this.labelSpeachResult.Text = e.Result.Text;

            bool hasComputerName = e.Result.Text.Contains(this._assistantName);
            if (!hasComputerName)
            {
                return;
            }

            if (this._currentAction != null)
            {
                foreach (var keyWord in this._currentAction.YesConfirmation.KeyWords)
                {
                    bool hasYes = e.Result.Text.Contains(keyWord);
                    if (hasYes)
                    {
                        this._currentAction?.YesConfirmation?.Action?.Invoke();
                        this._currentAction?.Action?.Invoke();
                        this._currentAction = null;
                    }
                }
            }

            if (this._currentAction != null)
            {
                foreach (var keyWord in this._currentAction.NoConfirmation.KeyWords)
                {
                    bool hasNo = e.Result.Text.Contains(keyWord);
                    if (hasNo)
                    {
                        this._currentAction?.NoConfirmation?.Action?.Invoke();
                        this._currentAction = null;
                    }
                }
            }

            foreach (var item in this._dictionaryOfActions)
            {
                foreach (var keyWord in item.KeyWords)
                {
                    bool hasMatch = e.Result.Text.Contains(keyWord);
                    if (hasMatch)
                    {
                        bool hasConfirmationMessage = item.YesConfirmation.KeyWords.Any();
                        if(hasConfirmationMessage)
                        {
                            this._currentAction = item;
                            this.Speak(string.Format(item.StringFormatMessageOnMatch, keyWord));
                        }

                        break;
                    }
                }
            }
        }

        private void Speak(string text)
        {
            var builder = new PromptBuilder();

            builder.StartVoice(VoiceGender.Female, VoiceAge.Adult);
            builder.AppendText(text);
            builder.EndVoice();

            var synthesizer = new SpeechSynthesizer();
            synthesizer.Rate = 1;
            synthesizer.Speak(builder);
            synthesizer.Dispose();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            this.notifyIcon1.Visible = true;
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
        }
    }
}
