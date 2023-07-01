using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace GridTactics.Main
{
    public static class Graphics
    {
        public static Color ParseHexcode(string hexCode)
        {
            int red = int.Parse(hexCode.Substring(1, 2), NumberStyles.HexNumber);
            int green = int.Parse(hexCode.Substring(3, 2), NumberStyles.HexNumber);
            int blue = int.Parse(hexCode.Substring(5, 2), NumberStyles.HexNumber);
            int alpha = (hexCode.Length > 7) ? int.Parse(hexCode.Substring(7, 2), NumberStyles.HexNumber) : 255;

            return new Color(red, green, blue, alpha);
        }
    }
}
