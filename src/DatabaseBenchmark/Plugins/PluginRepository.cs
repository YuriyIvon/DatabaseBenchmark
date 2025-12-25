using DatabaseBenchmark.Common;
using DatabaseBenchmark.Plugins.Interfaces;
using DatabaseBenchmark.Plugins.TextEmbedding;

namespace DatabaseBenchmark.Plugins
{
    public class PluginRepository : IPluginRepository
    {
        private readonly Dictionary<string, IPlugin> _plugins = [];
        private readonly Dictionary<PluginType, IPluginTypeFactory> _factories = [];
        private readonly object _syncObject = new();

        public PluginRepository(string pluginsFilePath)
        {
            if (string.IsNullOrEmpty(pluginsFilePath))
            {
                throw new InputArgumentException("Plugins file path cannot be empty");
            }

            RegisterFactory(new TextEmbeddingModelFactory());
            LoadPlugins(pluginsFilePath);
        }

        private void RegisterFactory(IPluginTypeFactory factory)
        {
            _factories[factory.Type] = factory;
        }

        public T GetPlugin<T>(string name, PluginType type) where T : class, IPlugin
        {
            if (TryGetPlugin<T>(name, type, out var plugin))
            {
                return plugin;
            }

            throw new InputArgumentException($"Plugin \"{name}\" of type \"{type}\" not found");
        }

        public bool TryGetPlugin<T>(string name, PluginType type, out T plugin) where T : class, IPlugin
        {
            plugin = null;

            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            var key = GetPluginKey(name, type);

            lock (_syncObject)
            {
                if (_plugins.TryGetValue(key, out var existingPlugin))
                {
                    plugin = existingPlugin as T;
                    return plugin != null;
                }
            }

            return false;
        }

        private void LoadPlugins(string pluginsFilePath)
        {
            var pluginDefinitions = JsonUtils.DeserializeFile<PluginDefinitions>(pluginsFilePath);

            if (pluginDefinitions?.Plugins == null || !pluginDefinitions.Plugins.Any())
            {
                throw new InputArgumentException($"No plugins found in file \"{pluginsFilePath}\"");
            }

            foreach (var definition in pluginDefinitions.Plugins)
            {
                ValidatePluginDefinition(definition);

                if (!_factories.TryGetValue(definition.Type, out var factory))
                {
                    throw new InputArgumentException($"No factory registered for plugin type \"{definition.Type}\"");
                }

                var plugin = factory.Create(definition.Name, definition.Options);
                var key = GetPluginKey(definition.Name, definition.Type);

                lock (_syncObject)
                {
                    if (_plugins.ContainsKey(key))
                    {
                        throw new InputArgumentException(
                            $"Duplicate plugin definition for name \"{definition.Name}\" and type \"{definition.Type}\"");
                    }

                    _plugins[key] = plugin;
                }
            }
        }

        private static void ValidatePluginDefinition(PluginDefinition definition)
        {
            if (string.IsNullOrEmpty(definition.Name))
            {
                throw new InputArgumentException("Plugin name is missing");
            }

            if (definition.Options.ValueKind == System.Text.Json.JsonValueKind.Undefined ||
                definition.Options.ValueKind == System.Text.Json.JsonValueKind.Null)
            {
                throw new InputArgumentException($"Plugin \"{definition.Name}\" has no options defined");
            }
        }

        private static string GetPluginKey(string name, PluginType type) => $"{type}:{name}";
    }
}
