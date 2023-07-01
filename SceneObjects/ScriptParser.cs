using GridTactics.Main;
using GridTactics.Models;
using GridTactics.SceneObjects.Particles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GridTactics.SceneObjects
{
    public interface IScripted
    {
        bool ExecuteCommand(string[] tokens);
        string ParseParameter(string parameter);
    }

    public class ScriptParser
    {
        public delegate void UnblockFollowup();

        private class Blocker
        {
            private int blockers = 0;

            public void Block() { blockers++; }
            public void Unblock() { blockers--; }

            public bool Blocked { get => blockers > 0; }
        }

        private Scene parentScene;
        private IScripted scriptedController;

        private string[] latestScript;
        private Queue<string> scriptCommands;

        private Stack<Queue<string>> whileStack = new Stack<Queue<string>>();
        private Stack<string[]> whileConditional = new Stack<string[]>();
        private Blocker blocker;
        private int waitTimeLeft = 0;

        private List<Particle> childParticles = new List<Particle>();

        public ScriptParser(Scene iScene, IScripted iScriptedController)
        {
            parentScene = iScene;
            scriptedController = iScriptedController;
        }

        public void Update(GameTime gameTime)
        {
            if (scriptCommands == null) return;

            if (waitTimeLeft > 0) waitTimeLeft -= gameTime.ElapsedGameTime.Milliseconds;
            while (!blocker.Blocked && waitTimeLeft <= 0)
            {
                if (scriptCommands.Count > 0) ExecuteNextCommand();
                else
                {
                    scriptCommands = null;
                    break;
                }
            }
        }

        public void RunScript(string script)
        {
            blocker = new Blocker();
            waitTimeLeft = 0;

            latestScript = script.Trim().Split('\n');
            scriptCommands = new Queue<string>(latestScript);
        }

        public void RunScript(string[] script)
        {
            blocker = new Blocker();
            waitTimeLeft = 0;

            latestScript = script;
            scriptCommands = new Queue<string>(script);
        }

        public void EnqueueScript(string script)
        {
            if (scriptCommands == null) RunScript(script);
            else
            {
                string[] commands = script.Trim().Split('\n');
                foreach (string command in commands)
                    scriptCommands.Enqueue(command);
            }
        }

        public string DequeueNextCommand()
        {
            return scriptCommands.Dequeue().Trim();
        }

        public void AddParticle(Particle particle)
        {
            childParticles.Add(particle);
        }

        public UnblockFollowup BlockScript()
        {
            blocker.Block();
            return blocker.Unblock;
        }

        private void ExecuteNextCommand()
        {
            string[] originalTokens = scriptCommands.Dequeue().Trim().Split(' ');
            string[] tokens = (string[])originalTokens.Clone();
            for (int i = 1; i < tokens.Length; i++) tokens[i] = ParseParameter(tokens[i]);

            if (!scriptedController.ExecuteCommand(tokens))
            {
                switch (tokens[0])
                {
                    case "Terminate": waitTimeLeft = 0; scriptCommands.Clear(); break;
                    case "If": If(tokens); break;
                    case "ElseIf": SkipToNextEnd(); break; // TODO: needs to include a conditional
                    case "Else": SkipToNextEnd(); break;
                    case "Break": SkipToNextEnd(); break;
                    case "Wait": waitTimeLeft = int.Parse(tokens[1]); break;
                    case "Repeat": RunScript(latestScript); break;
                    case "While": While(tokens, originalTokens); break;
                    case "WEnd": Wend(tokens); break;
                    case "ClearParticles": foreach (Particle particle in childParticles) particle.Terminate(); childParticles.Clear(); break;
                    case "Particle": AddParticle(tokens); break;
                    case "Sound": Audio.PlaySound(tokens); break;
                    case "SoundSolo": Audio.PlaySoundSolo(tokens); break;
                    case "Music": Audio.PlayMusic(tokens); break;
                    case "StopMusic": Audio.StopMusic(); break;
                    case "SetFlag": SetFlag(tokens); break;
                    case "SetProperty": SetProperty(tokens); break;
                    case "AddView": AddView(tokens); break;
                    case "ChangeScene": ChangeScene(tokens); break;
                    case "StackScene": StackScene(tokens); break;
                    case "Switch": Switch(tokens); break;
                    case "AddMoney": GameProfile.PlayerProfile.Money.Value = GameProfile.PlayerProfile.Money.Value + int.Parse(tokens[1]); break;
                }
            }
        }

        public void EndScript()
        {
            scriptCommands.Clear();
        }

        private string ParseParameter(string parameter)
        {
            if (parameter[0] == '!' && parameter.Length > 1)
            {
                parameter = ParseParameter(parameter.Substring(1, parameter.Length - 1));
                if (parameter == "True") return "False";
                else if (parameter == "False") return "True";
                else throw new Exception();
            }
            else if (parameter.StartsWith("$Flag."))
            {
                return GameProfile.GetSaveData<bool>(parameter.Split('.')[1]).ToString();
            }
            else if (parameter[0] != '$') return parameter;
            else
            {
                string result = scriptedController.ParseParameter(parameter);
                if (result != null) return result;
                else
                {
                    switch (parameter)
                    {
                        case "$centerX": return (CrossPlatformGame.ScreenWidth / 2).ToString();
                        case "$centerY": return (CrossPlatformGame.ScreenHeight / 2).ToString();
                        case "$right": return CrossPlatformGame.ScreenWidth.ToString();
                        case "$bottom": return CrossPlatformGame.ScreenHeight.ToString();
                        case "$top": return "0";
                        case "$left": return "0";
                        case "$money": return GameProfile.PlayerProfile.Money.Value.ToString();
                        case "$selection": return GameProfile.GetSaveData<string>("LastSelection");
                        default:
                            if (parameter.Contains("$random"))
                            {
                                int start = parameter.IndexOf('(');
                                int middle = parameter.IndexOf(',');
                                int end = parameter.LastIndexOf(')');

                                int randomMin = int.Parse(parameter.Substring(start + 1, middle - start - 1));
                                int randomMax = int.Parse(parameter.Substring(middle + 1, end - middle - 1));

                                return Rng.RandomInt(randomMin, randomMax).ToString();
                            }
                            break;
                    }
                }
            }

            throw new Exception();
        }

        private void If(string[] tokens)
        {
            int nestLevel = 1;

            if (!EvaluateConditional(string.Join(' ', tokens.Skip(1))))
            {
                string skipLine;
                do
                {
                    skipLine = scriptCommands.Dequeue().Trim();
                    if (skipLine.StartsWith("If") || skipLine.StartsWith("Switch")) nestLevel++; // SkipToNextEnd();
                    if (skipLine == "End" || (nestLevel == 1 && (skipLine == "Else" || skipLine.StartsWith("ElseIf")))) nestLevel--;
                } while (nestLevel > 0);

                if (skipLine.Contains("ElseIf"))
                {
                    string[] originalTokens = skipLine.Split(' ');
                    string[] elseIfTokens = (string[])originalTokens.Clone();
                    for (int i = 1; i < elseIfTokens.Length; i++) elseIfTokens[i] = ParseParameter(elseIfTokens[i]);
                    If(elseIfTokens);
                }
            }
        }

        private void While(string[] tokens, string[] originalTokens)
        {
            if (!EvaluateConditional(string.Join(' ', tokens.Skip(1))))
            {
                string skipLine;
                do
                {
                    skipLine = scriptCommands.Dequeue().Trim();
                } while (skipLine != "WEnd");
            }
            else
            {
                whileConditional.Push(originalTokens);
                whileStack.Push(new Queue<string>(scriptCommands));
            }
        }

        private void Wend(string[] tokens)
        {
            string[] originalTokens = (string[])whileConditional.Peek().Clone();
            for (int i = 1; i < originalTokens.Length; i++) originalTokens[i] = ParseParameter(originalTokens[i]);

            if (EvaluateConditional(string.Join(' ', originalTokens)))
            {
                scriptCommands = new Queue<string>(whileStack.Peek());
            }
            else
            {
                whileConditional.Pop();
                whileStack.Pop();
            }
        }

        private bool EvaluateConditional(string conditions)
        {
            List<string> tokens = conditions.Trim().Split(' ').Select(x => ParseParameter(x)).ToList();

            while (true)
            {
                if (tokens.Count == 1) return bool.Parse(tokens[0]);
                if (tokens.Count == 2) throw new Exception();
                else
                {
                    switch (tokens[1])
                    {
                        case "<": return EvaluateConditional((int.Parse(tokens[0]) < int.Parse(tokens[2])).ToString() + " " + string.Join(' ', tokens.Skip(3)));
                        case ">": return EvaluateConditional((int.Parse(tokens[0]) > int.Parse(tokens[2])).ToString() + " " + string.Join(' ', tokens.Skip(3)));
                        case "=": return EvaluateConditional((int.Parse(tokens[0]) == int.Parse(tokens[2])).ToString() + " " + string.Join(' ', tokens.Skip(3)));
                        case "!=": return EvaluateConditional((int.Parse(tokens[0]) != int.Parse(tokens[2])).ToString() + " " + string.Join(' ', tokens.Skip(3)));
                        case "&&": return bool.Parse(tokens[0]) && EvaluateConditional(string.Join(' ', tokens.Skip(2)));
                        case "||": return bool.Parse(tokens[0]) || EvaluateConditional(string.Join(' ', tokens.Skip(2)));
                    }
                }
            }
        }

        private void SkipToNextEnd()
        {
            string skipLine;
            do
            {
                skipLine = scriptCommands.Dequeue().Trim();
            } while (skipLine != "End");
        }

        private void AddParticle(string[] tokens)
        {
            bool foreground = tokens.Length >= 5 && tokens[4] == "Foreground";
            Vector2 position = new Vector2(int.Parse(tokens[2]), int.Parse(tokens[3]));
            AnimationParticle particle = new AnimationParticle(parentScene, position, (AnimationType)Enum.Parse(typeof(AnimationType), tokens[1]), foreground);

            parentScene.AddParticle(particle);
            childParticles.Add(particle);
        }

        private void SetFlag(string[] tokens)
        {
            GameProfile.SetSaveData(tokens[1], bool.Parse(tokens[2]));
        }

        private void SetProperty(string[] tokens)
        {
            string propertyValue = string.Join(' ', tokens).Replace(tokens[0] + " " + tokens[1], "");
            (GameProfile.PlayerProfile.GetType().GetProperty(tokens[1]).GetValue(GameProfile.PlayerProfile) as ModelProperty<string>).Value = propertyValue;
        }

        private void AddView(string[] tokens)
        {
            Type viewModelType = Type.GetType(tokens[1] + "Model");
            ViewModel viewModel = (ViewModel)Activator.CreateInstance(viewModelType, new object[] { parentScene, "Views\\" + tokens[1].Split('.').Last() });
            parentScene.AddOverlay(viewModel);

            waitTimeLeft++;
        }

        private void StackScene(string[] tokens)
        {
            Type sceneType = Type.GetType(tokens[1]);
            if (tokens.Length == 2) CrossPlatformGame.StackScene((Scene)Activator.CreateInstance(sceneType));
            else if (tokens.Length == 3) CrossPlatformGame.StackScene((Scene)Activator.CreateInstance(sceneType, tokens[2]));
            else if (tokens.Length == 4) CrossPlatformGame.StackScene((Scene)Activator.CreateInstance(sceneType, tokens[2], tokens[3]));
        }

        private void ChangeScene(string[] tokens)
        {
            Type sceneType = Type.GetType(tokens[1]);
            if (tokens.Length == 2) CrossPlatformGame.Transition(sceneType);
            else if (tokens.Length == 3) CrossPlatformGame.Transition(sceneType, tokens[2]);
            else if (tokens.Length == 4) CrossPlatformGame.Transition(sceneType, tokens[2], tokens[3]);
        }

        private void Switch(string[] tokens)
        {
            string switchValue = ParseParameter(tokens[1]);
            string skipLine;
            do
            {
                skipLine = scriptCommands.Dequeue().Trim();
            } while (skipLine != "Case " + switchValue);
        }

        public bool Finished { get => scriptCommands == null; }
    }
}
