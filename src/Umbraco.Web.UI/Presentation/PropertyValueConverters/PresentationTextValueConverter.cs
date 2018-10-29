using System;
using System.Linq;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web.UI.Presentation.PropertyValueConverters
{
    public class PresentationTextValueConverter : PropertyValueConverterBase
    {
        private readonly Lazy<IPublishedSnapshotService> snapshotService;
        private readonly IUmbracoContextAccessor contextAccessor;

        public PresentationTextValueConverter(Lazy<IPublishedSnapshotService> snapshotService, IUmbracoContextAccessor contextAccessor)
        {
            this.snapshotService = snapshotService;
            this.contextAccessor = contextAccessor;
        }

        private static readonly string[] PropertyTypeAliases =
        {
            "presentation.textbox"
        };

        public override bool IsConverter(PublishedPropertyType propertyType)
            => PropertyTypeAliases.Contains(propertyType.EditorAlias);

        public override Type GetPropertyValueType(PublishedPropertyType propertyType)
            => typeof(string);

        public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
            => PropertyCacheLevel.Element;

        public override object ConvertSourceToIntermediate(IPublishedElement owner, PublishedPropertyType propertyType, object source, bool preview)
        {
            // in xml a string is: string
            // in the database a string is: string
            // default value is: null
            return source;
        }

        public override object ConvertIntermediateToObject(IPublishedElement owner, PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            var cache = contextAccessor.UmbracoContext.ContentCache;

            // source should come from ConvertSource and be a string (or null) already
            return (inter ?? string.Empty).ToString().Split(' ');
        }

        public override object ConvertIntermediateToXPath(IPublishedElement owner, PublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            // source should come from ConvertSource and be a string (or null) already
            return inter;
        }

    }
}
