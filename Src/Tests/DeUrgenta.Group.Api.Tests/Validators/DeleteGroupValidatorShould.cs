﻿using System;
using System.Threading.Tasks;
using DeUrgenta.Domain;
using DeUrgenta.Domain.Entities;
using DeUrgenta.Group.Api.Commands;
using DeUrgenta.Group.Api.Validators;
using DeUrgenta.Tests.Helpers;
using Shouldly;
using Xunit;

namespace DeUrgenta.Group.Api.Tests.Validators
{
    [Collection(TestsConstants.DbCollectionName)]
    public class DeleteGroupValidatorShould
    {
        private readonly DeUrgentaContext _dbContext;

        public DeleteGroupValidatorShould(DatabaseFixture fixture)
        {
            _dbContext = fixture.Context;
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("my-weird-sub")]
        public async Task Invalidate_request_when_no_user_found_by_sub(string sub)
        {
            // Arrange
            var sut = new DeleteGroupValidator(_dbContext);

            // Act
            bool isValid = await sut.IsValidAsync(new DeleteGroup(sub, Guid.NewGuid()));

            // Assert
            isValid.ShouldBeFalse();
        }

        [Fact]
        public async Task Invalidate_request_when_group_does_not_exists()
        {
            // Arrange
            string userSub = Guid.NewGuid().ToString();
            var user = new User
            {
                FirstName = "Integration",
                LastName = "Test",
                Sub = userSub
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
            var sut = new DeleteGroupValidator(_dbContext);

            // Act
            bool isValid = await sut.IsValidAsync(new DeleteGroup(userSub, Guid.NewGuid()));

            // Assert
            isValid.ShouldBeFalse();
        }

        [Fact]
        public async Task Invalidate_request_when_user_is_not_admin_of_group()
        {
            // Arrange
            string userSub = Guid.NewGuid().ToString();
            string adminSub = Guid.NewGuid().ToString();

            var user = new User
            {
                FirstName = "Integration",
                LastName = "Test",
                Sub = userSub
            };

            var adminUser = new User
            {
                FirstName = "Admin",
                LastName = "User",
                Sub = adminSub
            };

            var group = new Domain.Entities.Group
            {
                Admin = adminUser,
                Name = "a group"
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.Users.AddAsync(adminUser);
            await _dbContext.Groups.AddAsync(group);
            await _dbContext.SaveChangesAsync();

            var sut = new DeleteGroupValidator(_dbContext);

            // Act
            bool isValid = await sut.IsValidAsync(new DeleteGroup(userSub, group.Id));

            // Assert
            isValid.ShouldBeFalse();
        }

        [Fact]
        public async Task Validate_when_user_is_admin_of_requested_group()
        {
            // Arrange
            var sut = new DeleteGroupValidator(_dbContext);

            string userSub = Guid.NewGuid().ToString();
            var user = new User
            {
                FirstName = "Integration",
                LastName = "Test",
                Sub = userSub
            };

            var group = new Domain.Entities.Group
            {
                Admin = user,
                Name = "my group"
            };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.Groups.AddAsync(group);
            await _dbContext.SaveChangesAsync();

            // Act
            bool isValid = await sut.IsValidAsync(new DeleteGroup(userSub, group.Id));

            // Assert
            isValid.ShouldBeTrue();
        }
    }
}