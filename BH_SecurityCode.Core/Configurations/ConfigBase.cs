using BH_SecurityCode.Core.Common;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Reflection;

namespace BH_SecurityCode.Core.Configurations
{
    public class ConfigBase
    {
        protected virtual void LoadConfig<T>(T instance)
        {
            string fileJson = Path.Combine(FilePathHelper.SettingRoot, instance.GetType().Name + ".json");
            if (File.Exists(fileJson))
            {
                string strJson = "";
                if (instance is IConfig iConfig && iConfig.IsEncryption)
                {
#if DEBUG
                    strJson = File.ReadAllText(fileJson);
#else
                    strJson = AES.Decrypt(File.ReadAllText(fileJson), Config.AesKey);
#endif
                }
                else
                {
                    strJson = File.ReadAllText(fileJson);
                }
                try
                {
                    T t = JsonConvert.DeserializeObject<T>(strJson);
                    foreach (PropertyInfo prop in instance.GetType().GetProperties())
                    {
                        if (prop.CanWrite)
                        {
                            prop.SetValue(instance, prop.GetValue(t));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"손상된 {instance.GetType().Name}파일 :: {ex.Message}");
                    ResetToDefault(instance);
                    SaveConfig(instance);
                }
            }
            else
            {
                ResetToDefault(instance);
                SaveConfig(instance);
            }
        }

        private static void ResetToDefault<T>(T instance)
        {
            foreach (PropertyInfo prop in instance.GetType().GetProperties())
            {
                if (prop.CanWrite)
                {
                    var attr = prop.GetCustomAttribute<DefaultValueAttribute>();
                    if (attr != null)
                    {
                        prop.SetValue(instance, attr.Value);
                    }
                }
            }
        }

        protected virtual void SaveConfig<T>(T instance)
        {
            string strJson = "";
            if (instance is IConfig iConfig && iConfig.IsEncryption)
            {
#if DEBUG
                strJson = JsonConvert.SerializeObject(instance);
#else
                strJson = AES.Encrypt(JsonConvert.SerializeObject(instance), Config.AesKey);
#endif
            }
            else
            {
                strJson = JsonConvert.SerializeObject(instance);
            }

            File.WriteAllText(Path.Combine(FilePathHelper.SettingRoot, instance.GetType().Name + ".json"), strJson);
        }
    }
}
