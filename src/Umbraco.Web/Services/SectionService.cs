﻿using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models.Trees;
using Umbraco.Core.Services;
using Umbraco.Web.Trees;

namespace Umbraco.Web.Services
{
    internal class SectionService : ISectionService
    {
        private readonly IUserService _userService;
        private readonly BackOfficeSectionCollection _sectionCollection;

        public SectionService(
            IUserService userService,
            BackOfficeSectionCollection sectionCollection)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _sectionCollection = sectionCollection ?? throw new ArgumentNullException(nameof(sectionCollection));
        }

        /// <summary>
        /// The cache storage for all applications
        /// </summary>
        public IEnumerable<IBackOfficeSection> GetSections()
            => _sectionCollection;

        /// <inheritdoc />
        public IEnumerable<IBackOfficeSection> GetAllowedSections(int userId)
        {
            var user = _userService.GetUserById(userId);
            if (user == null)
                throw new InvalidOperationException("No user found with id " + userId);

            return GetSections().Where(x => user.AllowedSections.Contains(x.Alias));
        }

        /// <inheritdoc />
        public IBackOfficeSection GetByAlias(string appAlias)
            => GetSections().FirstOrDefault(t => t.Alias == appAlias);
    }
}
