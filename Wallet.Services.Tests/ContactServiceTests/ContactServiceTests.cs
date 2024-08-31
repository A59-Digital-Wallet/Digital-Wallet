using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wallet.Common.Exceptions;
using Wallet.Data.Models;
using Wallet.Data.Repositories.Contracts;
using Wallet.DTO.Response;
using Wallet.Services.Contracts;
using Wallet.Services.Implementations;
using Wallet.Services.Factory.Contracts;

namespace Wallet.Services.Tests
{
    [TestClass]
    public class ContactServiceTests
    {
        private Mock<IContactsRepository> _contactsRepositoryMock;
        private Mock<IContactFactory> _contactFactoryMock;
        private Mock<IUserRepository> _userRepositoryMock;
        private ContactService _contactService;

        [TestInitialize]
        public void SetUp()
        {
            _contactsRepositoryMock = new Mock<IContactsRepository>();
            _contactFactoryMock = new Mock<IContactFactory>();
            _userRepositoryMock = new Mock<IUserRepository>();

            _contactService = new ContactService(
                _contactsRepositoryMock.Object,
                _contactFactoryMock.Object,
                _userRepositoryMock.Object);
        }

        [TestMethod]
        public async Task GetContactsAsync_ShouldReturnContacts_WhenContactsExist()
        {
            // Arrange
            var userId = "user1";
            var contacts = new List<Contact>
            {
                new Contact { Id = 1, UserId = userId, ContactUserId = "contact1" },
                new Contact { Id = 2, UserId = userId, ContactUserId = "contact2" }
            };

            var contactDTOs = new List<ContactResponseDTO>
            {
                new ContactResponseDTO { Id = "contact1", FirstName = "John", LastName = "Doe" },
                new ContactResponseDTO { Id = "contact2", FirstName = "Jane", LastName = "Smith" }
            };

            _contactsRepositoryMock.Setup(repo => repo.GetContactsAsync(userId))
                .ReturnsAsync(contacts);
            _contactFactoryMock.Setup(factory => factory.Map(contacts))
                .Returns(contactDTOs);

            // Act
            var result = await _contactService.GetContactsAsync(userId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public async Task AddContactAsync_ShouldAddContact_WhenContactDoesNotExist()
        {
            // Arrange
            var userId = "user1";
            var contactId = "contact1";
            var contactUser = new AppUser { Id = contactId };

            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(contactId))
                .ReturnsAsync(contactUser);
            _contactsRepositoryMock.Setup(repo => repo.GetContactAsync(userId, contactId))
                .ReturnsAsync((Contact)null); // Contact does not exist

            var contact = new Contact { UserId = userId, ContactUserId = contactId };
            _contactFactoryMock.Setup(factory => factory.Map(userId, contactId))
                .Returns(contact);

            // Act
            await _contactService.AddContactAsync(userId, contactId);

            // Assert
            _contactsRepositoryMock.Verify(repo => repo.AddContactAsync(It.IsAny<Contact>()), Times.Once);
        }

        [TestMethod]
        public async Task AddContactAsync_ShouldThrowEntityNotFoundException_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = "user1";
            var contactId = "contact1";

            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(contactId))
                .ReturnsAsync((AppUser)null); // Contact does not exist

            // Act & Assert
            await Assert.ThrowsExceptionAsync<EntityNotFoundException>(() => _contactService.AddContactAsync(userId, contactId));
        }

        [TestMethod]
        public async Task AddContactAsync_ShouldThrowInvalidOperationException_WhenContactAlreadyExists()
        {
            // Arrange
            var userId = "user1";
            var contactId = "contact1";
            var contactUser = new AppUser { Id = contactId };
            var existingContact = new Contact { UserId = userId, ContactUserId = contactId };

            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(contactId))
                .ReturnsAsync(contactUser);
            _contactsRepositoryMock.Setup(repo => repo.GetContactAsync(userId, contactId))
                .ReturnsAsync(existingContact); // Contact already exists

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _contactService.AddContactAsync(userId, contactId));
        }

        [TestMethod]
        public async Task RemoveContactAsync_ShouldRemoveContact_WhenContactExists()
        {
            // Arrange
            var userId = "user1";
            var contactId = "contact1";
            var existingContact = new Contact { UserId = userId, ContactUserId = contactId };

            _contactsRepositoryMock.Setup(repo => repo.GetContactAsync(userId, contactId))
                .ReturnsAsync(existingContact);

            // Act
            var result = await _contactService.RemoveContactAsync(userId, contactId);

            // Assert
            Assert.IsTrue(result);
            _contactsRepositoryMock.Verify(repo => repo.RemoveContactAsync(existingContact), Times.Once);
        }

        [TestMethod]
        public async Task RemoveContactAsync_ShouldThrowEntityNotFoundException_WhenContactDoesNotExist()
        {
            // Arrange
            var userId = "user1";
            var contactId = "contact1";

            _contactsRepositoryMock.Setup(repo => repo.GetContactAsync(userId, contactId))
                .ReturnsAsync((Contact)null); // Contact does not exist

            // Act & Assert
            await Assert.ThrowsExceptionAsync<EntityNotFoundException>(() => _contactService.RemoveContactAsync(userId, contactId));
        }

        [TestMethod]
        public async Task GetContactAsync_ShouldReturnContact_WhenContactExists()
        {
            // Arrange
            var userId = "user1";
            var contactId = "contact1";
            var contact = new Contact { Id = 1, UserId = userId, ContactUserId = contactId };

            _contactsRepositoryMock.Setup(repo => repo.GetContactAsync(userId, contactId))
                .ReturnsAsync(contact);

            // Act
            var result = await _contactService.GetContactAsync(userId, contactId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(contactId, result.ContactUserId);
        }

        [TestMethod]
        public async Task GetContactAsync_ShouldReturnNull_WhenContactDoesNotExist()
        {
            // Arrange
            var userId = "user1";
            var contactId = "contact1";

            _contactsRepositoryMock.Setup(repo => repo.GetContactAsync(userId, contactId))
                .ReturnsAsync((Contact)null); // Contact does not exist

            // Act
            var result = await _contactService.GetContactAsync(userId, contactId);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task SearchForContactsAsync_ShouldReturnContacts_WhenContactsExist()
        {
            // Arrange
            var userId = "user1";
            var searchQuery = "John";
            var contacts = new List<Contact>
            {
                new Contact { Id = 1, UserId = userId, ContactUserId = "contact1" }
            };

            var contactDTOs = new List<ContactResponseDTO>
            {
                new ContactResponseDTO { Id = "contact1", FirstName = "John", LastName = "Doe" }
            };

            _contactsRepositoryMock.Setup(repo => repo.SearchForContactsAsync(userId, searchQuery))
                .ReturnsAsync(contacts);
            _contactFactoryMock.Setup(factory => factory.Map(contacts))
                .Returns(contactDTOs);

            // Act
            var result = await _contactService.SearchForContactsAsync(userId, searchQuery);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }
    }
}
