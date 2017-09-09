using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using IamI.Lib.Basic.Log;

namespace IamI.Lib.WPF.Resource
{
    public static class ResourceHelp
    {
        /// <summary>
        /// 搜索指定类型，指定名称的资源。
        /// </summary>
        /// <typeparam name="T">资源的类型</typeparam>
        /// <param name="str">资源的标识符</param>
        /// <returns>所搜寻的资源</returns>
        public static T SearchResource<T>(string str)
        {
            try
            {
                var obj = Application.Current.TryFindResource(str);
                var st = (T) obj;
                return st;
            }
            catch (Exception ex)
            {
                Logger.Default.Error($"Can't find resource {str}");
                Logger.Default.Error(ex.ToString());
                return default(T);
            }
        }

        /// <summary>
        /// 以指定的位置，替换应用程序中的资源。
        /// </summary>
        /// <param name="application"></param>
        /// <param name="position">指定位置</param>
        /// <param name="dict">应用程序资源</param>
        public static void ReplaceResource(this Application application, int position, ResourceDictionary dict)
        {
            if (dict == null)
            {
                Logger.Default.Warning("Setting resource is null");
                return;
            }
            var merged_resource_dicrionaries = application.Resources.MergedDictionaries;
            if (position > merged_resource_dicrionaries.Count - 1)
                Logger.Default.Warning(
                    $"Trying to set {position}-th resource to another resource dict, but merged dict only contains {merged_resource_dicrionaries.Count} dicts. Nothing is done.");
            else if (position == merged_resource_dicrionaries.Count)
            {
                merged_resource_dicrionaries.Add(dict);
                Logger.Default.Info(
                    $"Added a resource dictionary to the application. Now it contains {merged_resource_dicrionaries.Count} resource dicts.");
            }
            else
            {
                Logger.Default.Info($"Setting the resource on {position} to another resource dict");
                merged_resource_dicrionaries[position] = dict;
            }
        }

        /// <summary>
        /// 应用程序与其文字索引的资源字典之间的对应关系
        /// </summary>
        public static Dictionary<Application, Dictionary<string, ResourceDictionary>> application_resource_dictionary_indecies = new Dictionary<Application, Dictionary<string, ResourceDictionary>>();

        /// <summary>
        /// 检索应用程序对应的资源字典的文字索引。
        /// </summary>
        /// <param name="application"></param>
        /// <returns>应用程序的资源字典的文字索引。</returns>
        public static Dictionary<string, ResourceDictionary> GetResourceIndexDictionary(this Application application)
        {
            if (application_resource_dictionary_indecies.TryGetValue(application,
                out Dictionary<string, ResourceDictionary> answer)) return answer;
            Logger.Default.Debug($"Builded indecies for an application.");
            answer = new Dictionary<string, ResourceDictionary>();
            application_resource_dictionary_indecies.Add(application, answer);
            return answer;
        }

        /// <summary>
        /// 根据给定名称，设置应用程序的资源字典的文字索引。
        /// </summary>
        /// <param name="application"></param>
        /// <param name="names">要设置的文字索引</param>
        public static void SetResourceDictionaries(this Application application, IEnumerable<string> names)
        {
            var indecies = application.GetResourceIndexDictionary();
            indecies.Clear();
            var merged_resouce_dictionaries = application.Resources.MergedDictionaries;
            var names_list = names.ToList();
            if (merged_resouce_dictionaries.Count != names_list.Count)
                Logger.Default.Warning($"Trying to index a {merged_resouce_dictionaries.Count} dictionary with {names_list.Count} names.");
            for (var i = 0; i < names_list.Count && i < merged_resouce_dictionaries.Count; i++)
                indecies.Add(names_list[i], merged_resouce_dictionaries[i]);
            Logger.Default.Info($"Set application indecies: [{string.Join(", ", names_list.ToArray())}]");
        }

        /// <summary>
        /// 以指定的文字索引，替换应用程序的资源。
        /// </summary>
        /// <param name="application"></param>
        /// <param name="key">对应应用程序的文字索引</param>
        /// <param name="dict">应用程序资源</param>
        public static void ReplaceResource(this Application application, string key, ResourceDictionary dict)
        {
            if (dict == null)
            {
                Logger.Default.Warning("Setting resource is null");
                return;
            }
            var indecies = application.GetResourceIndexDictionary();
            var merged_resouce_dictionaries = application.Resources.MergedDictionaries;
            if (indecies.Count != merged_resouce_dictionaries.Count)
                Logger.Default.Warning("Given indecies doesn't equal the application merged dictionaries. Make sure you set the correct index!");
            if (indecies.ContainsKey(key))
            {
                Logger.Default.Info($"Added dict {key} to the application resources.");
                indecies.Add(key, dict);
                merged_resouce_dictionaries.Add(dict);
            }
            else
            {
                Logger.Default.Info($"Setting the resource {key} to another dictionary.");
                indecies[key] = dict;
                var position = merged_resouce_dictionaries.IndexOf(indecies[key]);
                if (position < 0)
                {
                    Logger.Default.Warning($"Doesn't find resource dictionary named {key} in merged dictionary.");
                    merged_resouce_dictionaries.Add(dict);
                }
                else
                    merged_resouce_dictionaries[position] = dict;
            }
        }

        /// <summary>
        /// 根据给定的路径，从外部载入资源字典。
        /// </summary>
        /// <param name="path">资源字典路径</param>
        /// <returns>载入的资源字典</returns>
        public static ResourceDictionary LoadResourceDictionaryFromPath(string path)
        {
            var stream = new FileStream(path, FileMode.Open);
            var obj = System.Windows.Markup.XamlReader.Load(stream);
            stream.Close();
            Logger.Default.Debug($"Loaded ResourceDictionary {path}");
            return obj as ResourceDictionary;
        }

        /// <summary>
        /// 根据给定的流，从外部载入资源字典。
        /// </summary>
        /// <param name="stream">包含资源字典的流</param>
        /// <returns>载入的资源字典</returns>
        public static ResourceDictionary LoadResourceDictionaryFromStream(Stream stream)
        {
            return System.Windows.Markup.XamlReader.Load(stream) as ResourceDictionary;
        }

        /// <summary>
        /// 根据给定的路径，从内部载入资源字典。
        /// </summary>
        /// <param name="application">包含资源字典的应用程序</param>
        /// <param name="path">资源字典的内部路径</param>
        /// <returns>载入的资源字典</returns>
        public static ResourceDictionary LoadResourceDictionaryFromInner(this Application application, string path)
        {
            return Application.LoadComponent(new Uri(path)) as ResourceDictionary;
        }
    }
}