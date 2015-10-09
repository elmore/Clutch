using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Clutch.Tests
{
    [TestFixture]
    public class ClutchTests
    {
        [Test]
        public void RetrievesModelFromRoot()
        {
            Room room = new FluentClient("http://local.property.erm-api.com/v1/").Get<Room>().Result;

            Assert.IsNotNull(room);

            Assert.AreEqual("h234234872359283", room.Id);

            Assert.AreEqual("London", room.City);
        }
    }

    public class Room
    {
        public string Id { get; set; }
        public string UserId { get; set; }

        public string User { get; set; }

        public int TypeId { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime DateAvailable { get; set; }

        public string DwellingCode { get; set; }
        public List<Amenity> Amenities { get; set; }

        public string MetroCode { get; set; }
        public string StreetName { get; set; }
        public string City { get; set; }
        public float MapX { get; set; }
        public float MapY { get; set; }

        public string Description { get; set; }
        public string Comment { get; set; }
    }

    public class Amenity
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
