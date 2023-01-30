using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.SlimeSim
{
    public class ColorRange
    {
        public ColorRange(Color min, Color max)
        {
            Min = min;
            Max = max;
        }

        public Color Min { get; set; }
        public Color Max { get; set; }

        public Color getRandomColor()
        {
            return new Color(
                UnityEngine.Random.Range(Min.r, Max.r),
                UnityEngine.Random.Range(Min.g, Max.g),
                UnityEngine.Random.Range(Min.b, Max.b),
                UnityEngine.Random.Range(Min.a, Max.a)
            );
        }
    }
}
