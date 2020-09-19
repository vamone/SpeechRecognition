using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeachRecognition
{
    public class VoiceGrammarActionXref
    {
        public VoiceGrammarActionXref()
        {
        }

        public VoiceGrammarActionXref(Action action, params string[] keywords)
        {
            this.Action = action;
            this.KeyWords = keywords.ToList();
        }

        public IEnumerable<string> KeyWords { get; set; }

        public Action Action { get; set; }

        public VoiceGrammarActionXref YesConfirmation { get; set; }

        public VoiceGrammarActionXref NoConfirmation { get; set; }

        public string StringFormatMessageOnMatch { get; set; }
    }
}