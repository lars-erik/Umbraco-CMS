﻿using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Manifest;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Configuration.Grid
{
    internal class GridEditorsConfig : IGridEditorsConfig
    {
        private readonly ILogger _logger;
        private readonly IRuntimeCacheProvider _runtimeCache;
        private readonly DirectoryInfo _appPlugins;
        private readonly DirectoryInfo _configFolder;
        private readonly bool _isDebug;

        public GridEditorsConfig(ILogger logger, IRuntimeCacheProvider runtimeCache, DirectoryInfo appPlugins, DirectoryInfo configFolder, bool isDebug)
        {
            _logger = logger;
            _runtimeCache = runtimeCache;
            _appPlugins = appPlugins;
            _configFolder = configFolder;
            _isDebug = isDebug;
        }

        public IEnumerable<IGridEditorConfig> Editors
        {
            get
            {
                Func<List<GridEditor>> getResult = () =>
                {
                    // fixme - should use the common one somehow! + ignoring _appPlugins here!
                    var parser = new ManifestParser(_runtimeCache, Current.ManifestValidators, _logger);

                    var editors = new List<GridEditor>();
                    var gridConfig = Path.Combine(_configFolder.FullName, "grid.editors.config.js");
                    if (File.Exists(gridConfig))
                    {
                        var sourceString = File.ReadAllText(gridConfig);

                        try
                        {
                            editors.AddRange(parser.ParseGridEditors(sourceString));
                        }
                        catch (Exception ex)
                        {
                            _logger.Error<GridEditorsConfig>(ex, "Could not parse the contents of grid.editors.config.js into a JSON array '{Json}", sourceString);
                        }
                    }

                    // add manifest editors, skip duplicates
                    foreach (var gridEditor in parser.Manifest.GridEditors)
                    {
                        if (editors.Contains(gridEditor) == false)
                            editors.Add(gridEditor);
                    }

                    return editors;
                };

                //cache the result if debugging is disabled
                var result = _isDebug
                    ? getResult()
                    : _runtimeCache.GetCacheItem<List<GridEditor>>(
                        typeof(GridEditorsConfig) + "Editors",
                        () => getResult(),
                        TimeSpan.FromMinutes(10));

                return result;
            }

        }
    }
}
