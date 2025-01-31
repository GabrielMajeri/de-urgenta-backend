﻿using System;
using System.Threading.Tasks;
using DeUrgenta.Backpack.Api.Commands;
using DeUrgenta.Backpack.Api.Validators;
using DeUrgenta.Domain;
using DeUrgenta.Domain.Entities;
using DeUrgenta.Tests.Helpers;
using Shouldly;
using Xunit;

namespace DeUrgenta.Backpack.Api.Tests.Validators
{
    [Collection(TestsConstants.DbCollectionName)]
    public class DeleteBackpackItemValidatorShould
    {
        private readonly DeUrgentaContext _dbContext;

        public DeleteBackpackItemValidatorShould(DatabaseFixture fixture)
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
            var sut = new DeleteBackpackItemValidator(_dbContext);

            // Act
            bool isValid = await sut.IsValidAsync(new DeleteBackpackItem(sub, Guid.NewGuid()));

            // Assert
            isValid.ShouldBeFalse();
        }

        [Fact]
        public async Task Invalidate_when_user_not_contributor_of_related_backpack()
        {
            // Arrange

            var sut = new DeleteBackpackItemValidator(_dbContext);

            var userSub = Guid.NewGuid().ToString();
            var contributorSub = Guid.NewGuid().ToString();
            var backpackItemId = Guid.NewGuid();
            var backpackId = Guid.NewGuid();

            var nonContributor = new User
            {
                FirstName = "NonContributor",
                LastName = "User",
                Sub = userSub
            };

            var contributor = new User
            {
                FirstName = "Contributor",
                LastName = "User",
                Sub = contributorSub
            };

            var backpack = new Domain.Entities.Backpack
            {
                Id = backpackId,
                Name = "A backpack"
            };

            await _dbContext.BackpackItems.AddAsync(new BackpackItem
            {
                Id = backpackItemId,
                Name = "test-backpack-item",
                Backpack = backpack
            });

            await _dbContext.Users.AddAsync(nonContributor);
            await _dbContext.Users.AddAsync(contributor);
            await _dbContext.Backpacks.AddAsync(backpack);
            await _dbContext.BackpacksToUsers.AddAsync(new BackpackToUser { Backpack = backpack, User = contributor });
            await _dbContext.SaveChangesAsync();

            // Act
            bool isValid = await sut.IsValidAsync(new DeleteBackpackItem(nonContributor.Sub, backpackItemId));

            // Assert
            isValid.ShouldBeFalse();
        }

        [Fact]
        public async Task Invalidate_request_when_item_does_not_exist()
        {
            // Arrange
            var sut = new DeleteBackpackItemValidator(_dbContext);

            var contributorSub = Guid.NewGuid().ToString();
            var backpackId = Guid.NewGuid();

            var contributor = new User
            {
                FirstName = "Contributor",
                LastName = "User",
                Sub = contributorSub
            };

            var backpack = new Domain.Entities.Backpack
            {
                Id = backpackId,
                Name = "A backpack"
            };

            await _dbContext.Users.AddAsync(contributor);
            await _dbContext.Backpacks.AddAsync(backpack);
            await _dbContext.BackpacksToUsers.AddAsync(new BackpackToUser { Backpack = backpack, User = contributor });
            await _dbContext.SaveChangesAsync();

            // Act
            bool isValid = await sut.IsValidAsync(new DeleteBackpackItem(contributorSub, Guid.NewGuid()));

            // Assert
            isValid.ShouldBeFalse();
        }

        [Fact]
        public async Task Validate_request_when_item_exists_and_user_contributor()
        {
            // Arrange
            var sut = new DeleteBackpackItemValidator(_dbContext);

            var contributorSub = Guid.NewGuid().ToString();
            var backpackId = Guid.NewGuid();
            var backpackItemId = Guid.NewGuid();

            var contributor = new User
            {
                FirstName = "Contributor",
                LastName = "User",
                Sub = contributorSub
            };

            var backpack = new Domain.Entities.Backpack
            {
                Id = backpackId,
                Name = "A backpack"
            };

            await _dbContext.BackpackItems.AddAsync(new BackpackItem
            {
                Id = backpackItemId,
                Name = "test-backpack-item",
                Backpack = backpack
            });

            await _dbContext.Users.AddAsync(contributor);
            await _dbContext.Backpacks.AddAsync(backpack);
            await _dbContext.BackpacksToUsers.AddAsync(new BackpackToUser { Backpack = backpack, User = contributor });
            await _dbContext.SaveChangesAsync();

            // Act
            bool isValid = await sut.IsValidAsync(new DeleteBackpackItem(contributorSub, backpackItemId));

            // Assert
            isValid.ShouldBeTrue();
        }
    }
}
