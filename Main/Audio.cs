using FMOD;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GridTactics.Main
{
    public static class Audio
    {
        public static Dictionary<GameMusic, uint[]> MUSIC_LOOP_POINTS = new Dictionary<GameMusic, uint[]>()
        {
            // { GameMusic.Victory,  new uint[] { 3200, 28800 } }
        };

        private static readonly Dictionary<GameSound, SoundEffect> GAME_SOUNDS = new Dictionary<GameSound, SoundEffect>();
        private static readonly Dictionary<GameMusic, Sound> GAME_MUSIC = new Dictionary<GameMusic, Sound>();

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string dllName);

        private static FMOD.System fmodSystem;
        private static Channel fmodChannel;
        private static GameMusic currentMusic = GameMusic.None;

        private static float soundVolume = 1.0f;
        private static float musicVolume = 0.8f;

        public static void Initialize()
        {
            if (Environment.Is64BitProcess) LoadLibrary(Path.GetFullPath("fmod64.dll"));
            else LoadLibrary(Path.GetFullPath("fmod.dll"));

            Factory.System_Create(out fmodSystem);
            fmodSystem.setDSPBufferSize(1024, 10);
            fmodSystem.init(32, INITFLAGS.NORMAL, (IntPtr)0);

            List<Tuple<byte[], byte[]>> assetData = AssetCache.LoadAssetData("Sounds.jam");
            foreach (Tuple<byte[], byte[]> asset in assetData)
            {
                string soundName = Encoding.ASCII.GetString(asset.Item1);
                GAME_SOUNDS.Add((GameSound)Enum.Parse(typeof(GameSound), soundName), SoundEffect.FromStream(new MemoryStream(asset.Item2)));
            }

            assetData = AssetCache.LoadAssetData("Music.jam");
            foreach (Tuple<byte[], byte[]> asset in assetData)
            {
                string musicName = Encoding.ASCII.GetString(asset.Item1);
                GameMusic musicId = (GameMusic)Enum.Parse(typeof(GameMusic), musicName);

                Sound musicSound;
                CREATESOUNDEXINFO musicInfo = new CREATESOUNDEXINFO();
                musicInfo.cbsize = Marshal.SizeOf(musicInfo);
                musicInfo.length = (uint)asset.Item2.Length;

                fmodSystem.createSound(asset.Item2, MODE.OPENMEMORY, ref musicInfo, out musicSound);
                GAME_MUSIC.Add((GameMusic)Enum.Parse(typeof(GameMusic), musicName), musicSound);

                if (MUSIC_LOOP_POINTS.ContainsKey(musicId))
                {
                    uint[] loopPoints = Audio.MUSIC_LOOP_POINTS[musicId];
                    GAME_MUSIC[musicId].setLoopPoints(loopPoints[0], TIMEUNIT.MS, loopPoints[1], TIMEUNIT.MS);
                }
            }
        }

        public static void Deinitialize()
        {
            fmodSystem.release();
        }

        public static void ApplySettings()
        {
            SoundVolume = Settings.GetProgramSetting<int>("SoundVolume") / 100.0f;
            MusicVolume = Settings.GetProgramSetting<int>("MusicVolume") / 100.0f;
        }

        public static void PlayMusic(GameMusic musicType)
        {
            if (musicType == currentMusic) return;

            StopMusic();

            fmodSystem.playSound(GAME_MUSIC[musicType], null, false, out fmodChannel);
            if (fmodChannel != null) fmodChannel.setVolume(musicVolume);
            fmodChannel.setMode(MODE.LOOP_NORMAL);
            fmodChannel.setLoopCount(-1);

            currentMusic = musicType;
        }

        public static void PlayMusic(string[] scriptTokens)
        {
            PlayMusic((GameMusic)Enum.Parse(typeof(GameMusic), scriptTokens[1]));
        }

        public static void PauseMusic(bool pause)
        {
            fmodChannel.setPaused(pause);
        }

        public static void StopMusic()
        {
            bool isPlaying = false;
            if (fmodChannel != null) fmodChannel.isPlaying(out isPlaying);
            if (currentMusic != GameMusic.None && isPlaying) fmodChannel.stop();

            currentMusic = GameMusic.None;
        }

        public static SoundEffectInstance PlaySound(GameSound soundType, float volume = 0.5f, float pitch = 0.0f, float pan = 0.0f)
        {
            SoundEffectInstance soundEffectInstance = GAME_SOUNDS[soundType].CreateInstance();

            soundEffectInstance.Volume = soundVolume * volume;
            soundEffectInstance.Pitch = pitch;
            soundEffectInstance.Pan = pan;
            soundEffectInstance.Play();

            return soundEffectInstance;
        }

        public static void PlaySound(string[] scriptTokens)
        {
            float volume = 0.5f;
            float pitch = 0.0f;
            float pan = 0.0f;

            if (scriptTokens.Length > 2) volume = float.Parse(scriptTokens[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
            if (scriptTokens.Length > 3) pan = float.Parse(scriptTokens[3], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
            if (scriptTokens.Length > 4) pitch = float.Parse(scriptTokens[4], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
            PlaySound((GameSound)Enum.Parse(typeof(GameSound), scriptTokens[1]), volume, pan, pitch);
        }

        public static void PlaySoundSolo(string[] scriptTokens)
        {
            Audio.PauseMusic(true);
            PlaySound((GameSound)Enum.Parse(typeof(GameSound), scriptTokens[1]));
            Task.Delay(int.Parse(scriptTokens[2])).ContinueWith(t => Audio.PauseMusic(false));
        }

        public static float SoundVolume { set => soundVolume = value; get => soundVolume; }
        public static float MusicVolume
        {
            set
            {
                musicVolume = value;

                bool isPlaying = false;
                if (fmodChannel != null) fmodChannel.isPlaying(out isPlaying);
                if (currentMusic != GameMusic.None && isPlaying) fmodChannel.setVolume(musicVolume);
            }

            get => musicVolume;
        }

        public static GameMusic CurrentMusic { get => currentMusic; }
    }
}
