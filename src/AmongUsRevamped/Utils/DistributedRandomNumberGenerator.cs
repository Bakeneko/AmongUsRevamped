using System;
using System.Collections.Generic;
using System.Linq;

namespace AmongUsRevamped.Utils
{
    public class DistributedRandomNumberGenerator<T> where T : struct, IConvertible
    {
        private readonly Dictionary<T, float> Distributions = new();
        private double distSum = 0d;

        public DistributedRandomNumberGenerator() {}

        public int GetNumberCount()
        {
            return Distributions.Count;
        }

        public void AddNumber(T value, float distribution)
        {
            RemoveNumber(value);
            Distributions.Add(value, distribution);
            distSum += distribution;
        }

        public void RemoveNumber(T value)
        {
            Distributions.TryGetValue(value, out float stored);
            if (stored != default)
            {
                distSum -= stored;
            }
            Distributions.Remove(value);
        }

        public T GetDistributedRandomNumber()
        {
            double rand = AmongUsRevamped.Rand.NextDouble();
            double ratio = 1.0d / distSum;
            double tempDist = 0d;
            foreach (T key in Distributions.Keys)
            {
                tempDist += Distributions[key];
                if (rand / ratio <= tempDist)
                {
                    return key;
                }
            }
            // Should never happen
            return Distributions.Keys.FirstOrDefault();
        }

    }
}
