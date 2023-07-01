using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTactics.Main
{
    public static class PokeRng
    {
        private static Random random = new Random(System.DateTime.Now.Millisecond);

        public static List<byte> DestinedBytes { get; private set; } = new List<byte>();

        public static void Seed(int seedValue = -1)
        {
            if (seedValue == -1) random = new Random();
            else random = new Random(seedValue);

            DestinedBytes.Clear();
            for (int i = 0; i < 8; i++) DestinedBytes.Add((byte)random.Next(0, 256));
        }

        public static byte RandomByte()
        {
            byte result = DestinedBytes[0];
            DestinedBytes.RemoveAt(0);
            DestinedBytes.Add((byte)random.Next(0, 256));
            return result;
        }
    }

    public static class Rng
    {
        private static Random random = new Random(System.DateTime.Now.Millisecond);

        public static void Seed(int seedValue = -1)
        {
            if (seedValue == -1) random = new Random();
            else random = new Random(seedValue);
        }






        public static int RandomInt(int minimum, int maximum)
        {
            return random.Next(maximum - minimum + 1) + minimum;
        }

        public static int RandomInt(int minimum, int maximum, int[] countList)
        {
            int smallestCount = 42069;
            for (int i = minimum; i <= maximum; i++)
            {
                if (countList[i] < smallestCount) smallestCount = countList[i];
            }

            List<int> finalPool = new List<int>();
            for (int i = minimum; i <= maximum; i++)
            {
                if (countList[i] == smallestCount) finalPool.Add(i);
            }

            return finalPool[random.Next(finalPool.Count)];
        }

        public static int RandomSign()
        {
            return (random.Next(2) == 0) ? 1 : -1;
        }

        public static bool RandomBool()
        {
            return random.Next(2) == 0;
        }

        public static double RandomDouble(double minimum, double maximum)
        {
            return random.NextDouble() * (maximum - minimum) + minimum;
        }

        public static double GaussianDouble(double mean, double standardDeviation)
        {
            double u1 = random.NextDouble();
            double u2 = random.NextDouble();
            double randomStandardDeviationNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

            return mean + standardDeviation * randomStandardDeviationNormal;
        }

        public static T WeightedEntry<T>(Dictionary<T, double> entries)
        {
            double generatedWeight = RandomDouble(0.0, entries.Sum(x => x.Value));
            foreach (KeyValuePair<T, double> entry in entries)
            {
                generatedWeight -= entry.Value;
                if (generatedWeight < 0.0) return entry.Key;
            }

            return entries.Last().Key;
        }

        public static Color RandomColor()
        {
            return new Color(RandomInt(0, 255), RandomInt(0, 255), RandomInt(0, 255));
        }
    }
}
