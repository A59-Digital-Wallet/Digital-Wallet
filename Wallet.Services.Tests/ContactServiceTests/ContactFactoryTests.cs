using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Wallet.Data.Models;
using Wallet.DTO.Response;
using Wallet.Services.Factory;

namespace Wallet.Services.Tests
{
    [TestClass]
    public class ContactFactoryTests
    {
        private ContactFactory _contactFactory;

        [TestInitialize]
        public void SetUp()
        {
            _contactFactory = new ContactFactory();
        }

        [TestMethod]
        public void Map_ShouldMapContactsToContactResponseDTOs()
        {
            // Arrange
            var contacts = new List<Contact>
            {
                new Contact
                {
                    Id = 1,
                    ContactUser = new AppUser
                    {
                        Id = "contact1",
                        FirstName = "John",
                        LastName = "Doe",
                        ProfilePictureURL = "http://example.com/john.jpg"
                    }
                },
                new Contact
                {
                    Id = 2,
                    ContactUser = new AppUser
                    {
                        Id = "contact2",
                        FirstName = "Jane",
                        LastName = "Smith",
                        ProfilePictureURL = "http://example.com/jane.jpg"
                    }
                }
            };

            // Act
            var result = _contactFactory.Map(contacts);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);

            var contact1 = result.GetEnumerator();
            contact1.MoveNext();
            Assert.AreEqual("contact1", contact1.Current.Id);
            Assert.AreEqual("John", contact1.Current.FirstName);
            Assert.AreEqual("Doe", contact1.Current.LastName);
            Assert.AreEqual("http://example.com/john.jpg", contact1.Current.ProfilePictureURL);

            contact1.MoveNext();
            Assert.AreEqual("contact2", contact1.Current.Id);
            Assert.AreEqual("Jane", contact1.Current.FirstName);
            Assert.AreEqual("Smith", contact1.Current.LastName);
            Assert.AreEqual("http://example.com/jane.jpg", contact1.Current.ProfilePictureURL);
        }

        [TestMethod]
        public void Map_ShouldMapUserIdAndContactIdToContact()
        {
            // Arrange
            var userId = "user1";
            var contactId = "contact1";

            // Act
            var result = _contactFactory.Map(userId, contactId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(userId, result.UserId);
            Assert.AreEqual(contactId, result.ContactUserId);
        }
    }
}
