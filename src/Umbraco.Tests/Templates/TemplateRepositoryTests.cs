﻿using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Templates
{
    [TestFixture]
    public class TemplateRepositoryTests
    {
        private readonly Mock<CacheHelper> _cacheMock = new Mock<CacheHelper>();
        private readonly Mock<ITemplatesSection> _templateConfigMock = new Mock<ITemplatesSection>();
        private readonly IFileSystems _fileSystems = Mock.Of<IFileSystems>();
        private TemplateRepository _templateRepository;

        private readonly TestObjects _testObjects = new TestObjects(null);

        [SetUp]
        public void Setup()
        {
            var logger = Mock.Of<ILogger>();

            var accessorMock = new Mock<IScopeAccessor>();
            var scopeMock = new Mock<IScope>();
            var database = _testObjects.GetUmbracoSqlCeDatabase(logger);
            scopeMock.Setup(x => x.Database).Returns(database);
            accessorMock.Setup(x => x.AmbientScope).Returns(scopeMock.Object);

            var mvcFs = Mock.Of<IFileSystem>();
            var masterFs = Mock.Of<IFileSystem>();
            Mock.Get(_fileSystems).Setup(x => x.MvcViewsFileSystem).Returns(mvcFs);
            Mock.Get(_fileSystems).Setup(x => x.MasterPagesFileSystem).Returns(masterFs);

            _templateRepository = new TemplateRepository(accessorMock.Object, _cacheMock.Object, logger, _templateConfigMock.Object, _fileSystems);
        }

        [Test]
        public void DetermineTemplateRenderingEngine_Returns_MVC_When_ViewFile_Exists_And_Content_Has_Webform_Markup()
        {
            // Project in MVC mode
            _templateConfigMock.Setup(x => x.DefaultRenderingEngine).Returns(RenderingEngine.Mvc);

            // Template has masterpage content
            var templateMock = new Mock<ITemplate>();
            templateMock.Setup(x => x.Alias).Returns("Something");
            templateMock.Setup(x => x.Content).Returns("<asp:Content />");

            // but MVC View already exists
            Mock.Get(_fileSystems.MvcViewsFileSystem)
                .Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);

            var res = _templateRepository.DetermineTemplateRenderingEngine(templateMock.Object);
            Assert.AreEqual(RenderingEngine.Mvc, res);
        }

        [Test]
        public void DetermineTemplateRenderingEngine_Returns_WebForms_When_ViewFile_Doesnt_Exist_And_Content_Has_Webform_Markup()
        {
            // Project in MVC mode
            _templateConfigMock.Setup(x => x.DefaultRenderingEngine).Returns(RenderingEngine.Mvc);

            // Template has masterpage content
            var templateMock = new Mock<ITemplate>();
            templateMock.Setup(x => x.Alias).Returns("Something");
            templateMock.Setup(x => x.Content).Returns("<asp:Content />");

            // MVC View doesn't exist
            Mock.Get(_fileSystems.MvcViewsFileSystem)
                .Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);

            var res = _templateRepository.DetermineTemplateRenderingEngine(templateMock.Object);
            Assert.AreEqual(RenderingEngine.WebForms, res);
        }

        [Test]
        public void DetermineTemplateRenderingEngine_Returns_WebForms_When_MasterPage_Exists_And_In_Mvc_Mode()
        {
            // Project in MVC mode
            _templateConfigMock.Setup(x => x.DefaultRenderingEngine).Returns(RenderingEngine.Mvc);

            var templateMock = new Mock<ITemplate>();
            templateMock.Setup(x => x.Alias).Returns("Something");

            // but masterpage already exists
            Mock.Get(_fileSystems.MvcViewsFileSystem)
                .Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);
            Mock.Get(_fileSystems.MasterPagesFileSystem)
                .Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);

            var res = _templateRepository.DetermineTemplateRenderingEngine(templateMock.Object);
            Assert.AreEqual(RenderingEngine.WebForms, res);
        }

        [Test]
        public void DetermineTemplateRenderingEngine_Returns_Mvc_When_ViewPage_Exists_And_In_Webforms_Mode()
        {
            // Project in WebForms mode
            _templateConfigMock.Setup(x => x.DefaultRenderingEngine).Returns(RenderingEngine.WebForms);

            var templateMock = new Mock<ITemplate>();
            templateMock.Setup(x => x.Alias).Returns("Something");

            // but MVC View already exists
            Mock.Get(_fileSystems.MvcViewsFileSystem)
                .Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);
            Mock.Get(_fileSystems.MasterPagesFileSystem)
                .Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);

            var res = _templateRepository.DetermineTemplateRenderingEngine(templateMock.Object);
            Assert.AreEqual(RenderingEngine.Mvc, res);
        }

    }
}
