using GridTactics.Models;
using GridTactics.SceneObjects.Maps;
using GridTactics.SceneObjects.Shaders;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTactics.GameObjects.Maps
{
    public class WeatherController : Controller
    {
        private const float TIME_SCALE = 1.0f / 1;
        private const int HOURS_PER_DAY = 24;
        public const int MINUTES_PER_HOUR = 60;
        private const int MINUTES_PER_DAY = HOURS_PER_DAY * MINUTES_PER_HOUR;

        private const int DAWN_START = 4;
        private const int DAWN_MIDDLE = 6;
        private const int DAWN_END = 9;
        private const int DUSK_START = 18;
        private const int DUSK_MIDDLE = 20;
        private const int DUSK_END = 22;
        private static readonly Color MIDNIGHT = new Color(0.1f, 0.1f, 0.2f);
        private static readonly Color SUNRISE = new Color(0.5f, 0.2f, 0.4f);
        private static readonly Color MIDDAY = new Color(1.0f, 1.0f, 1.0f);
        private static readonly Color SUNSET = new Color(0.6f, 0.3f, 0.3f);

        private float worldTime;
        private int worldHour;
        private int worldMinute;

        DayNight dayNight;

        private Color ambientLight;
        private float bloom;

        private float timeScale;

        public WeatherController(DayNight iDayNight, float iTime, float iTimeScale, bool indoors)
            : base(PriorityLevel.GameLevel)
        {
            worldTime = iTime;
            dayNight = iDayNight;
            timeScale = iTimeScale;

            Indoors = indoors;
        }

        public override void PreUpdate(GameTime gameTime)
        {
            worldHour = (int)(worldTime / MINUTES_PER_HOUR);
            worldMinute = (int)(worldTime % MINUTES_PER_HOUR);

            if (!Indoors)
            {
                if (worldHour >= DUSK_END) ambientLight = MIDNIGHT;
                else if (worldHour >= DUSK_MIDDLE) ambientLight = Color.Lerp(SUNSET, MIDNIGHT, (worldTime / MINUTES_PER_HOUR - DUSK_MIDDLE) / (DUSK_END - DUSK_MIDDLE));
                else if (worldHour >= DUSK_START) ambientLight = Color.Lerp(MIDDAY, SUNSET, (worldTime / MINUTES_PER_HOUR - DUSK_START) / (DUSK_MIDDLE - DUSK_START));
                else if (worldHour >= DAWN_END) ambientLight = MIDDAY;
                else if (worldHour >= DAWN_MIDDLE) ambientLight = Color.Lerp(SUNRISE, MIDDAY, (worldTime / MINUTES_PER_HOUR - DAWN_MIDDLE) / (DAWN_END - DAWN_MIDDLE));
                else if (worldHour >= DAWN_START) ambientLight = Color.Lerp(MIDNIGHT, SUNRISE, (worldTime / MINUTES_PER_HOUR - DAWN_START) / (DAWN_MIDDLE - DAWN_START));
                else ambientLight = MIDNIGHT;
            }

            dayNight.Ambient = ambientLight.ToVector4();

            bloom = 1.0f;

            ProceedTime(gameTime.ElapsedGameTime.Milliseconds / 100.0f);
        }

        public void ProceedTime(float seconds)
        {
            if (!Indoors) worldTime += seconds * TIME_SCALE * timeScale;

            while (worldTime > MINUTES_PER_DAY) worldTime -= MINUTES_PER_DAY;
        }

        public static int ParseTime(string timeString)
        {
            string[] tokens = timeString.Split(':');
            return int.Parse(tokens[0]) * MINUTES_PER_HOUR + int.Parse(tokens[1]);
        }

        public string Date
        {
            get
            {
                int hour = worldHour;
                if (hour == 0) hour = 12;
                if (hour > 12) hour -= 12;

                return hour.ToString("D2") + ":" + worldMinute.ToString("D2") + (worldHour >= 12 ? "PM" : "AM");
            }
        }

        public float WorldTime { get => worldTime; }
        public int WorldHour { get => worldHour; }
        public int WorldMinute { get => worldMinute; }
        public bool IsDay { get => worldHour >= DAWN_MIDDLE && worldHour < DUSK_MIDDLE; }
        public bool IsNight { get => !IsDay; }
        public bool Indoors { get; private set; }

        public Color AmbientLight { get => ambientLight; set { ambientLight = value; if (dayNight != null) dayNight.Ambient = ambientLight.ToVector4(); } }
        public float Bloom { get => bloom; }
    }
}
