using GridTactics.Models;
using GridTactics.SceneObjects.Controllers;
using GridTactics.SceneObjects.Maps;
using GridTactics.SceneObjects.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTactics.Scenes.MapScene
{
    public class EventController : ScriptController
    {
        private MapScene mapScene;

        public bool EndGame { get; private set; }
        public Actor ActorSubject { get; set; }

        public EventController(MapScene iScene, string[] script)
            : base(iScene, script, PriorityLevel.CutsceneLevel)
        {
            mapScene = iScene;
        }

        public override bool ExecuteCommand(string[] tokens)
        {
            switch (tokens[0])
            {
                case "EndGame": EndGame = true; break;
                case "ChangeMap": ChangeMap(tokens, mapScene); break;
                case "SetWaypoint": SetWaypoint(tokens); break;
                case "Conversation": Conversation(tokens, scriptParser); break;
                case "Encounter": Encounter(tokens, scriptParser, mapScene); break;
                case "Shop": Shop(tokens); break;
                case "GiveItem": GiveItem(tokens); break;
                case "Inn": Inn(tokens); break;
                case "RestoreParty": RestoreParty(); break;

                default: return false;
            }

            return true;
        }

        public override string ParseParameter(string parameter)
        {
            if (parameter.StartsWith("$SaveData."))
            {
                return GameProfile.GetSaveData<string>(parameter.Split('.')[1]).ToString();
            }
            else if (parameter[0] == '$')
            {
                switch (parameter)
                {
                    
                    default: return null;
                }
            }
            else return null;
        }

        public static void ChangeMap(string[] tokens, MapScene mapScene)
        {
            if (tokens.Length == 5) CrossPlatformGame.Transition(typeof(MapScene), tokens[1], int.Parse(tokens[2]), int.Parse(tokens[3]), (Orientation)Enum.Parse(typeof(Orientation), tokens[4]));
            else if (tokens.Length == 2) CrossPlatformGame.Transition(typeof(MapScene), tokens[1], mapScene.Tilemap.Name);
        }

        public static void SetWaypoint(string[] tokens)
        {
            /*
            mapScene.SetWaypoint(int.Parse(tokens[1]), int.Parse(tokens[2]));
            */
        }

        public static void Conversation(string[] tokens, ScriptParser scriptParser)
        {
            if (tokens.Length == 2)
            {
                ConversationScene.ConversationScene conversationScene = new ConversationScene.ConversationScene(tokens[1]);
                conversationScene.OnTerminated += new TerminationFollowup(scriptParser.BlockScript());
                CrossPlatformGame.StackScene(conversationScene);
            }
            else
            {
                var convoRecord = new ConversationScene.ConversationRecord()
                {
                    DialogueRecords = new ConversationScene.DialogueRecord[] { new ConversationScene.DialogueRecord() { Text = String.Join(' ', tokens.Skip(1)) } }
                };

                var convoScene = new ConversationScene.ConversationScene(convoRecord);
                convoScene.OnTerminated += new TerminationFollowup(scriptParser.BlockScript());
                CrossPlatformGame.StackScene(convoScene);
            }
        }

        public static void Encounter(string[] tokens, ScriptParser scriptParser, MapScene scene)
        {
            var unblock = new TerminationFollowup(scriptParser.BlockScript());
            BattleScene.BattleScene battleScene = null;

            Task.Run(() =>
            {
                battleScene = new BattleScene.BattleScene(tokens[1]);
                battleScene.OnTerminated += unblock;
            });

            TransitionController transitionController = new TransitionController(TransitionDirection.Out, 600);
            Pinwheel pinwheel = new Pinwheel(Color.Black, transitionController.TransitionProgress);
            transitionController.UpdateTransition += new Action<float>(t => pinwheel.Amount = t);
            scene.AddController(transitionController);
            CrossPlatformGame.TransitionShader = pinwheel;

            transitionController.OnTerminated += new TerminationFollowup(() =>
            {
                pinwheel.Terminate();
                CrossPlatformGame.StackScene(battleScene, true);
            });
        }

        public void Shop(string[] tokens)
        {
            ShopScene.ShopScene shopScene = new ShopScene.ShopScene(tokens[1]);
            shopScene.OnTerminated += new TerminationFollowup(scriptParser.BlockScript());
            CrossPlatformGame.StackScene(shopScene);
        }

        public void GiveItem(string[] tokens)
        {
            ItemRecord item = new ItemRecord(StatusScene.StatusScene.ITEMS.First(x => x.Name == string.Join(' ', tokens.Skip(1))));
            GameProfile.Inventory.Add(new ItemModel(item, 1));

            ConversationRecord conversationData = new ConversationRecord()
            {
                DialogueRecords = new DialogueRecord[]
                {
                    new DialogueRecord() { Text = "Found @" + item.Icon + " " + item.Name + "!"}
                }
            };

            ConversationScene.ConversationScene conversationScene = new ConversationScene.ConversationScene(conversationData);
            conversationScene.OnTerminated += new TerminationFollowup(scriptParser.BlockScript());
            CrossPlatformGame.StackScene(conversationScene);

            Audio.PlaySound(GameSound.GetItem);
        }

        public void Inn(string[] tokens)
        {
            ConversationScene.ConversationScene conversationScene = new ConversationScene.ConversationScene("NurseRespawn");
            conversationScene.OnTerminated += new TerminationFollowup(scriptParser.BlockScript());

            TransitionController transitionOutController = new TransitionController(TransitionDirection.Out, 600);
            ColorFade colorFadeOut = new SceneObjects.Shaders.ColorFade(Color.Black, transitionOutController.TransitionProgress);
            transitionOutController.UpdateTransition += new Action<float>(t => colorFadeOut.Amount = t);
            transitionOutController.FinishTransition += new Action<TransitionDirection>(t =>
            {
                transitionOutController.Terminate();
                colorFadeOut.Terminate();
                TransitionController transitionInController = new TransitionController(TransitionDirection.In, 600);
                ColorFade colorFadeIn = new SceneObjects.Shaders.ColorFade(Color.Black, transitionInController.TransitionProgress);
                transitionInController.UpdateTransition += new Action<float>(t => colorFadeIn.Amount = t);
                transitionInController.FinishTransition += new Action<TransitionDirection>(t =>
                {
                    colorFadeIn.Terminate();
                });
                mapScene.AddController(transitionInController);
                mapScene.SceneShader = colorFadeIn;

                CrossPlatformGame.StackScene(conversationScene);
            });

            mapScene.AddController(transitionOutController);
            mapScene.SceneShader = colorFadeOut;

            RestoreParty();
        }

        public static void RestoreParty()
        {
            foreach (var partyMember in GameProfile.PlayerProfile.ActiveRoster)
            {
                partyMember.Value.CurrentHealth.Value = partyMember.Value.Health.Value;
                partyMember.Value.Status.Value = StatusAilment.None;
                foreach (var ability in partyMember.Value.Techniques) ability.Value.ChargesLeft = ability.Value.MaxCharges;
                foreach (var ability in partyMember.Value.Repertoire) ability.Value.ChargesLeft = ability.Value.MaxCharges;
            }
        }
    }
}
