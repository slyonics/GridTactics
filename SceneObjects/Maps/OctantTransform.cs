using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTactics.SceneObjects.Maps
{
    /// <summary>
    /// Immutable class for holding coordinate transform constants.  Bulkier than a 2D
    /// array of ints, but it's self-formatting if you want to log it while debugging.
    /// </summary>
    public class OctantTransform
    {
        public static OctantTransform[] s_octantTransform =
        {
            new OctantTransform( 1,  0,  0,  1 ),   // 0 E-NE
            new OctantTransform( 0,  1,  1,  0 ),   // 1 NE-N
            new OctantTransform( 0, -1,  1,  0 ),   // 2 N-NW
            new OctantTransform(-1,  0,  0,  1 ),   // 3 NW-W
            new OctantTransform(-1,  0,  0, -1 ),   // 4 W-SW
            new OctantTransform( 0, -1, -1,  0 ),   // 5 SW-S
            new OctantTransform( 0,  1, -1,  0 ),   // 6 S-SE
            new OctantTransform( 1,  0,  0, -1 ),   // 7 SE-E
        };

        public int xx { get; private set; }
        public int xy { get; private set; }
        public int yx { get; private set; }
        public int yy { get; private set; }

        public OctantTransform(int xx, int xy, int yx, int yy)
        {
            this.xx = xx;
            this.xy = xy;
            this.yx = yx;
            this.yy = yy;
        }

        public override string ToString()
        {
            // consider formatting in constructor to reduce garbage
            return string.Format("[OctantTransform {0,2:D} {1,2:D} {2,2:D} {3,2:D}]",
                xx, xy, yx, yy);
        }
    }
}
