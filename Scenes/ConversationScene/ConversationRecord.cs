using GridTactics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTactics.Scenes.ConversationScene
{
    public class ConversationRecord
    {
        public string Name { get; set; }
        public string Background { get; set; }
        public DialogueRecord[] DialogueRecords { get; set; }
        public string[] EndScript { get; set; }
        public string Bounds { get; set; }
    }

    public class DialogueRecord
    {
        public string Speaker { get; set; }
        public string Text { get; set; }
        public string[] Script { get; set; }
    }
}
