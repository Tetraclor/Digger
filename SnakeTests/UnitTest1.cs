using Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SnakeTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void Test1()
        {
            var remotePlayer = new RemotePlayer();
           
        }

        [Test]
        public void NegativeRating()
        {
            var ratingService = new RatingService();

            var a1 = new RateOjbect(-10, 1000);
            var a2 = new RateOjbect(-100, 100);

            var dict = ratingService.Calc(a1, a2);

            ;
        }

        [Test]
        public void RatingTestTwo()
        {
            var ratingService = new RatingService();

            var a1 = new RateOjbect(1700, 1000);
            var a2 = new RateOjbect(1300, 100);

            var dict = ratingService.Calc(a2, a1);
            ;
        }

        [Test]
        public void RatingTest()
        {
            var ratingService = new RatingService();

            var a1 = new RateOjbect(1600, 1000);
            var a2 = new RateOjbect(1600, 100);
            var a3 = new RateOjbect(1600, 10);
            var a4 = new RateOjbect(1600, 1);

            var dict = ratingService.Calc(a1, a2, a3, a4);
            ;
        }

        [Test]
        public void RatingTest2()
        {
            var ratingService = new RatingService();
            ratingService.K = 15;

            var ratings = new List<int>() { 2100, 2000, 1900, 1800, 1700, 1600, 1500, 1400 };

            var playrs = ratings.Select(v => new RateOjbect(v, v * 10)).ToList();

            for (int i = 0; i < 1000; i++)
            {
                var dict = ratingService.Calc(playrs.ToArray());

                foreach (var player in playrs)
                {
                    player.Rate = (int)dict[player];
                }

               // Console.WriteLine(string.Join(" ", playrs));
            }

            Console.WriteLine(String.Join(" ", playrs.Select((v, i) => v.Rate - ratings[i])));
        }
    }
}