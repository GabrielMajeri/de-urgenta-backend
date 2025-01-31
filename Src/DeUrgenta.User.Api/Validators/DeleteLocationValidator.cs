﻿using System.Linq;
using System.Threading.Tasks;
using DeUrgenta.Common.Validation;
using DeUrgenta.Domain;
using DeUrgenta.Domain.Entities;
using DeUrgenta.User.Api.Commands;
using Microsoft.EntityFrameworkCore;

namespace DeUrgenta.User.Api.Validators
{
    public class DeleteLocationValidator : IValidateRequest<DeleteLocation>
    {
        private readonly DeUrgentaContext _context;

        public DeleteLocationValidator(DeUrgentaContext context)
        {
            _context = context;
        }

        public async Task<bool> IsValidAsync(DeleteLocation request)
        {
            var locationExists = await _context.Users.AnyAsync(u => u.Sub == request.UserSub && Enumerable.Any<UserLocation>(u.Locations, l => l.Id == request.LocationId));

            return locationExists;
        }
    }
}