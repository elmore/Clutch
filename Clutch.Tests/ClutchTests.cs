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
            Room room = new FluentClient("http://local.property.erm-api.com/v1/").Get<Room>(1).Result;

            Assert.IsNotNull(room);

            Assert.AreEqual("h234234872359283", room.Id);

            Assert.AreEqual("London", room.City);
        }

        [Test]
        public void RetreivesSubModel()
        {
            Room room = new FluentClient("http://local.property.erm-api.com/v1/").Find<User>(1).Get<Room>("h123123").Result;

        }
    }

    public class User
    {
        public string Id { get; set; }

        public int GenderId { get; set; }
        public int OccupationId { get; set; }

        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool ShowPhone { get; set; }
        public int LanguageCode { get; set; }
        public int Age { get; set; }
        public string AffiliateCode { get; set; }

        public string Ip { get; set; }
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
