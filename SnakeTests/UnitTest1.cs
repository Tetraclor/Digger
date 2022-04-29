using Common;
using NUnit.Framework;

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
    }
}