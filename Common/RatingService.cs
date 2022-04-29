using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public interface IRated
    {
        int Rate { get; }
        int GameScore { get; }
    }

    public class RateOjbect : IRated
    {
        public int GameScore { get; set; }
        public int Rate { get; set; }

        public RateOjbect(int rate, int gameScore)
        {
            GameScore = gameScore;
            Rate = rate;  
        }

        public override string ToString()
        {
            return $"{Rate}: {GameScore}";
        }
    }

    public class RatingService
    {
        public int K = 16;

        public Dictionary<IRated, int> Calc(params IRated[] rated)
        {
            var maxScore = rated.Max(r => r.GameScore);
            var minScore = rated.Min(r => r.GameScore);

            var matrix = new Dictionary<IRated, List<double>>();
            foreach (var rate in rated) matrix[rate] = new List<double>();


            for (int i = 0; i < rated.Length; i++)
            {
                var first = rated[i];
                for (int j = i + 1; j < rated.Length; j++)
                {
                    var second = rated[j];
                    var f = Calc(first, second, minScore, maxScore);
                    var s = Calc(second, first, minScore, maxScore);
                    matrix[first].Add(f);
                    matrix[second].Add(s);
                    Console.WriteLine($"{first}->{f}----{second}->{s}");
                }
            }

            var result = matrix.ToDictionary(v => v.Key, v => AggregateRate(v.Value));

            return result;
        }

        public int AggregateRate(List<double> rateds)
        {
            return (int)(rateds.Sum() / rateds.Count);
        }

        public double Calc(IRated rated, IRated opponent, int minScore, int maxScore)
        {
            var p = (opponent.Rate - rated.Rate) / 400.0;
            var Ea = 1.0 / (1 + Math.Pow(10, p));

            var Sa = CalcSA(rated, minScore, maxScore);
            var a = rated.Rate + K * (Sa - Ea);

            return a;
        }

        public double CalcSA(IRated rated, int minScore, int maxScore)
        {
            return rated.GameScore / (double)(maxScore);
        }
    }
}
