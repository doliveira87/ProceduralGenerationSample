using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpCity.Tools.RandomNumberGenerators
{
    public class UniformRandomGenerator : IRandomGenerator
    {
        // Yes, it's ironic we have to seed our random generator with a random number
        private MersenneTwister _rGen = new MersenneTwister((uint)new Random().Next());

        public int Next()
        {
            return _rGen.Next();
        }

        public double NextDouble()
        {
            return _rGen.NextDouble();
        }

        public int Next(int max)
        {
            return _rGen.Next(max);
        }

        public int Next(int min, int max)
        {
            return _rGen.Next(min, max);
        }
    }
}
