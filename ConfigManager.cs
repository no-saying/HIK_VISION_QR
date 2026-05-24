using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace HIK_DEMON
{
    /// <summary>
    /// 配置文件读写（config.json），使用内置 JavaScriptSerializer，无需 NuGet。
    /// </summary>
    public static class ConfigManager
    {
        private static readonly string ConfigPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

        private static readonly JavaScriptSerializer _json = new JavaScriptSerializer();

        /// <summary>加载配置，文件不存在时返回默认配置。</summary>
        public static ProjectConfig Load()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                {
                    var defaults = new ProjectConfig();
                    defaults.EnsureDefaults();
                    return defaults;
                }

                string json = File.ReadAllText(ConfigPath, Encoding.UTF8);
                var cfg = _json.Deserialize<ProjectConfig>(json);
                if (cfg == null)
                {
                    cfg = new ProjectConfig();
                }
                cfg.EnsureDefaults();
                return cfg;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("加载配置失败: " + ex.Message);
                var fallback = new ProjectConfig();
                fallback.EnsureDefaults();
                return fallback;
            }
        }

        /// <summary>保存配置到 config.json。</summary>
        public static void Save(ProjectConfig config)
        {
            try
            {
                string json = _json.Serialize(config);
                File.WriteAllText(ConfigPath, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("保存配置失败: " + ex.Message);
            }
        }
    }
}
