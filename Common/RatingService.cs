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
        public int K = 10;

        public Dictionary<IRated, double> Calc(params IRated[] rated)
        {
            var matrix = new Dictionary<IRated, List<double>>();
            foreach (var rate in rated) matrix[rate] = new List<double>();

            for (int i = 0; i < rated.Length; i++)
            {
                var first = rated[i];
                for (int j = i + 1; j < rated.Length; j++)
                {
                    var second = rated[j];
                    var f = Calc(first, second);
                    var s = Calc(second, first);
                    matrix[first].Add(f);
                    matrix[second].Add(s);
                }
                ;
            }

            var result = matrix.ToDictionary(v => v.Key, v => AggregateRate(v.Value));

            return result;
        }

        public static double AggregateRate(List<double> rateds)
        {
            return (rateds.Sum() / rateds.Count);
        }

        public double Calc(IRated rated, IRated opponent)
        {
            var p = (opponent.Rate - rated.Rate) / 400.0;
            var Ea = 1.0 / (1 + Math.Pow(10, p));

            var Sa = CalcSA(rated, opponent);
            var a = rated.Rate + K * (Sa - Ea);

            return a;
        }

        public static double CalcSA(IRated rated, IRated opponent)
        {
            return rated.GameScore > opponent.GameScore ? 1 :
                   rated.GameScore < opponent.GameScore ? 0 :
                   0.5;
        }
    }
}
