using ContactsConsoleAPI.Business;
using ContactsConsoleAPI.Business.Contracts;
using ContactsConsoleAPI.Data.Models;
using ContactsConsoleAPI.DataAccess;
using ContactsConsoleAPI.DataAccess.Contrackts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactsConsoleAPI.IntegrationTests.NUnit
{
    public class IntegrationTests
    {
        private TestContactDbContext dbContext;
        private IContactManager contactManager;

        [SetUp]
        public void SetUp()
        {
            this.dbContext = new TestContactDbContext();
            this.contactManager = new ContactManager(new ContactRepository(this.dbContext));
        }


        [TearDown]
        public void TearDown()
        {
            this.dbContext.Database.EnsureDeleted();
            this.dbContext.Dispose();
        }


        //positive test
        [Test]
        public async Task AddContactAsync_ShouldAddNewContact()
        {
            var newContact = new Contact()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH", //must be minimum 10 symbols - numbers or Upper case letters
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933779"
            };

            await contactManager.AddAsync(newContact);

            var dbContact = await dbContext.Contacts.FirstOrDefaultAsync(c => c.Contact_ULID == newContact.Contact_ULID);

            Assert.NotNull(dbContact);
            Assert.AreEqual(newContact.FirstName, dbContact.FirstName);
            Assert.AreEqual(newContact.LastName, dbContact.LastName);
            Assert.AreEqual(newContact.Phone, dbContact.Phone);
            Assert.AreEqual(newContact.Email, dbContact.Email);
            Assert.AreEqual(newContact.Address, dbContact.Address);
            Assert.AreEqual(newContact.Contact_ULID, dbContact.Contact_ULID);
        }

        //Negative test
        [Test]
        public async Task AddContactAsync_TryToAddContactWithInvalidCredentials_ShouldThrowException()
        {
            var newContact = new Contact()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH", //must be minimum 10 symbols - numbers or Upper case letters
                Email = "invalid_Mail", //invalid email
                Gender = "Male",
                Phone = "0889933779"
            };

            var ex = Assert.ThrowsAsync<ValidationException>(async () => await contactManager.AddAsync(newContact));
            var actual = await dbContext.Contacts.FirstOrDefaultAsync(c => c.Contact_ULID == newContact.Contact_ULID);

            Assert.IsNull(actual);
            Assert.That(ex?.Message, Is.EqualTo("Invalid contact!"));

        }

        [Test]
        public async Task DeleteContactAsync_WithValidULID_ShouldRemoveContactFromDb()
        {
            // Arrange
            var contact1 = new Contact()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH",
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933779"
            };

            await contactManager.AddAsync(contact1);
            // Act
            await contactManager.DeleteAsync(contact1.Contact_ULID);
            // Assert
            var deletedContact = await dbContext.Contacts.FirstOrDefaultAsync(x => x.Contact_ULID.Equals(contact1.Contact_ULID));
            Assert.IsNull(deletedContact);
        }

        [Test]
        public async Task DeleteContactAsync_TryToDeleteWithNullOrWhiteSpaceULID_ShouldThrowException()
        {
            //act
            var exception = Assert.ThrowsAsync<ArgumentException>(() => contactManager.DeleteAsync(" "));
            //assert
            Assert.That(exception.Message, Is.EqualTo("ULID cannot be empty."));
            
        }

        [Test]
        public async Task GetAllAsync_WhenContactsExist_ShouldReturnAllContacts()
        {
            // Arrange
            var contact1 = new Contact()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH", //must be minimum 10 symbols - numbers or Upper case letters
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933779"
            };

            await contactManager.AddAsync(contact1);

            var contact2 = new Contact()
            {
                FirstName = "DemoName",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1DDC23456HH",
                Email = "test2@gmail.com",
                Gender = "Male",
                Phone = "0888933779"
            };

            await contactManager.AddAsync(contact2);
            // Act
            var result = await contactManager.SearchByFirstNameAsync(contact2.FirstName);
            // Assert
            Assert.That(result.Count(), Is.EqualTo(1));
            var itemInDb = result.First();
            Assert.AreEqual(itemInDb.FirstName, contact2.FirstName);
            Assert.AreEqual(itemInDb.LastName, contact2.LastName);
            Assert.AreEqual(itemInDb.Address, contact2.Address);
            Assert.AreEqual(itemInDb.Contact_ULID, contact2.Contact_ULID);
            Assert.AreEqual(itemInDb.Email, contact2.Email);
            Assert.AreEqual(itemInDb.Gender, contact2.Gender);
            Assert.AreEqual(itemInDb.Phone, contact2.Phone);
        }

        [Test]
        public async Task GetAllAsync_WhenNoContactsExist_ShouldThrowKeyNotFoundException()
        {
            // Act
            var exception = Assert.ThrowsAsync<KeyNotFoundException>(() => contactManager.SearchByFirstNameAsync("NO_SUCH_KEY"));
            //Assert
            Assert.That(exception.Message, Is.EqualTo("No contact found with the given first name."));
            
        }

        [Test]
        public async Task SearchByFirstNameAsync_WithExistingFirstName_ShouldReturnMatchingContacts()
        {
            // Arrange
            var contact1 = new Contact()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH",
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933779"
            };

            await contactManager.AddAsync(contact1);
            // Act
            var result = await contactManager.SearchByFirstNameAsync("TestFirstName");
            // Assert
            Assert.That(result.Count(), Is.EqualTo(1));
            var contactInDb = result.FirstOrDefault();
            Assert.That(contactInDb.FirstName, Is.EqualTo(contact1.FirstName));
        }

        [Test]
        public async Task SearchByFirstNameAsync_WithNonExistingFirstName_ShouldThrowKeyNotFoundException()
        {
//Act
            var exception = Assert.ThrowsAsync<KeyNotFoundException>(() => contactManager.SearchByFirstNameAsync("NO_SUCH_KEY"));
            //Assert
            Assert.That(exception.Message, Is.EqualTo("No contact found with the given first name."));
        }

        [Test]
        public async Task SearchByLastNameAsync_WithExistingLastName_ShouldReturnMatchingContacts()
        {
            // Arrange
            var contact1 = new Contact()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastNamee",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH",
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933779"
            };

            await contactManager.AddAsync(contact1);

            var contact2 = new Contact()
            {
                FirstName = "TestFirstNamee",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH",
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933779"
            };

            await contactManager.AddAsync(contact2);
            // Act
            var result = await contactManager.SearchByLastNameAsync(contact1.LastName);
            // Assert
            Assert.That(result.Count(), Is.EqualTo(1));
            var contactInDb = result.First();
            Assert.That(contactInDb.LastName, Is.EqualTo(contact1.LastName));
            Assert.That(contactInDb.FirstName, Is.EqualTo(contact1.FirstName));
            Assert.That(contactInDb.Address, Is.EqualTo(contact1.Address));
            Assert.That(contactInDb.Email, Is.EqualTo(contact1.Email));
            Assert.That(contactInDb.Gender, Is.EqualTo(contact1.Gender));
            Assert.That(contactInDb.Phone, Is.EqualTo(contact1.Phone));
            Assert.That(contactInDb.Contact_ULID, Is.EqualTo(contact1.Contact_ULID));
        }

        [Test]
        public async Task SearchByLastNameAsync_WithNonExistingLastName_ShouldThrowKeyNotFoundException()
        {
            //Act
            var exception = Assert.ThrowsAsync<KeyNotFoundException>(() => contactManager.SearchByLastNameAsync("NO_SUCH_KEY"));
            //Assert
            Assert.That(exception.Message, Is.EqualTo("No contact found with the given last name."));
        }

        [Test]
        public async Task GetSpecificAsync_WithValidULID_ShouldReturnContact()
        {
            // Arrange
            var contact1 = new Contact()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastNamee",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH",
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933779"
            };

            await contactManager.AddAsync(contact1);

            var contact2 = new Contact()
            {
                FirstName = "TestFirstNamee",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH",
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933779"
            };

            await contactManager.AddAsync(contact2);
            // Act
            var result = await contactManager.GetSpecificAsync(contact1.Contact_ULID);
            // Assert
            Assert.NotNull(result);
            
            Assert.That(result.FirstName, Is.EqualTo(contact1.FirstName));
            
        }

        [Test]
        public async Task GetSpecificAsync_WithInvalidULID_ShouldThrowKeyNotFoundException()
        {
            // Arrange

            // Act
            var exception = Assert.ThrowsAsync<KeyNotFoundException>(() => contactManager.GetSpecificAsync("1ABC23456HH00000"));
            // Assert
            Assert.That(exception.Message, Is.EqualTo("No contact found with ULID: 1ABC23456HH00000"));
        }

        [Test]
        public async Task UpdateAsync_WithValidContact_ShouldUpdateContact()
        {
            // Arrange
            var contact1 = new Contact()
            {
                FirstName = "TestFirstName",
                LastName = "TestLastNamee",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH",
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933779"
            };

            await contactManager.AddAsync(contact1);

            var contact2 = new Contact()
            {
                FirstName = "TestFirstNamee",
                LastName = "TestLastName",
                Address = "Anything for testing address",
                Contact_ULID = "1ABC23456HH",
                Email = "test@gmail.com",
                Gender = "Male",
                Phone = "0889933779"
            };

            await contactManager.AddAsync(contact2);

            var updatedContact = contact1;
            updatedContact.FirstName = "Updated";

            // Act
            await contactManager.UpdateAsync(updatedContact);
            // Assert
            var itemInDb = await dbContext.Contacts.FirstOrDefaultAsync(x => x.Contact_ULID == updatedContact.Contact_ULID);
            Assert.IsNotNull(itemInDb);
            Assert.That(itemInDb.FirstName, Is.EqualTo(updatedContact.FirstName));
        }

        [Test]
        public async Task UpdateAsync_WithInvalidContact_ShouldThrowValidationException()
        {

            // Act
            var exception = Assert.ThrowsAsync<ValidationException>(() => contactManager.UpdateAsync(new Contact()));
            // Assert
            Assert.That(exception.Message, Is.EqualTo("Invalid contact!"));
        }
    }
}
