﻿using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Clutch.Tests
{
    [TestFixture]
    public class ClutchTests
    {
        [Test]
        public void RetrievesModelFromRoot()
        {
            Room room = new FluentClient<Error>("http://local.property.erm-api.com/v1/").Get<Room>("H151006172656205").Result.Entity;

            Assert.IsNotNull(room);

            Assert.AreEqual("H151006172656205", room.Id);

            Assert.AreEqual("my city", room.City);
        }

        [Test]
        public void RetreivesSubModel()
        {
            var room = new FluentClient<Error>("http://local.property.erm-api.com/v1/").Find<User>(1).Get<Room>("h123123");
        }

        [Test]
        public void CreatesModel()
        {
            var model = new User
            {
                GenderId = 0,
                OccupationId = 0,
                Firstname = "",
                Lastname = "",
                Email = "",
                Phone = "",
                ShowPhone = false,
                LanguageCode = 1,
                Age = 18,
                AffiliateCode  = "",
                Ip = "",
            };

            FluentResponse<User, Error> result = new FluentClient<Error>("http://local.property.erm-api.com/v1/").Post<User>(model).Result;

            Assert.IsNotNull(result);

            Assert.IsNotNull(result.Entity);

            Assert.IsNotNullOrEmpty(result.Entity.Id);
        }

        [Test]
        public void CanUseClientForDifferentCalls()
        {
            var client = new FluentClient<Error>("http://local.property.erm-api.com/v1/");

            Room room1 = client.Get<Room>("H151006172656205").Result.Entity;

            Room room2 = client.Get<Room>("H151006172656205").Result.Entity;

            Assert.IsNotNull(room1);

            Assert.IsNotNull(room2);
        }

        [Test]
        public void HandlesNetworkFailure()
        {
            var client = new FluentClient<Error>("http://local.property.erm-api.com/v1/");

            // server set up to fail on this code
            FluentResponse<User, Error> room = client.Get<User>("forceerror").Result;

            Assert.IsNotNull(room);

            Assert.IsNull(room.Entity);

            Assert.AreEqual(401, (int)room.StatusCode);

            Assert.AreEqual("Authenticate", room.Error.Message);
        }


    }

    public class Error
    {
        public int Status { get; set; }
        public string Message { get; set; }
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
