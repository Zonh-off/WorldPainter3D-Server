using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Server.Configs;

public static class YamlConfig
{
    public static ServerConfig LoadOrCreate(string path)
    {
        var deserializer = new DeserializerBuilder()
           .WithNamingConvention(CamelCaseNamingConvention.Instance)
           .IgnoreUnmatchedProperties()
           .Build();

        var serializer = new SerializerBuilder()
           .WithNamingConvention(CamelCaseNamingConvention.Instance)
           .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults)
           .Build();

        if (!File.Exists(path))
        {
            var cfg = new ServerConfig();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, serializer.Serialize(cfg));
            return cfg;
        }

        var yaml = File.ReadAllText(path);
        var loaded = deserializer.Deserialize<ServerConfig>(yaml);

        return loaded ?? new ServerConfig();
    }
}