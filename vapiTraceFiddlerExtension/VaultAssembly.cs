using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Win32;

namespace vapiTraceFiddlerExtension
{
    internal static class VaultAssembly
    {
        static VaultAssembly()
        {
            FindVaultFiles();

            if (IsVaultXmlPresent)
                ParseInfo();
        }

        public static bool IsVaultDllPresent => !string.IsNullOrEmpty(_dllFile);

        public static bool IsVaultXmlPresent => !string.IsNullOrEmpty(_xmlFile);

        private static Assembly _instance;
        private static string _xmlFile;
        private static string _dllFile;
        private static readonly Dictionary<string, string> Info = new Dictionary<string, string>();
        private static string _html;

        public static Assembly Instance {
            get
            {
                if (_instance == null)
                {
                    var assemblyFile = _dllFile;
                    if (assemblyFile == null)
                        _instance = null;
                    else
                    {
                        try
                        {
                            _instance = Assembly.LoadFrom(assemblyFile);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            _instance = null;
                        }
                    }
                }

                return _instance;
            }
        }

        public static Type GetServiceType(string service)
        {
            var typeName = $"Autodesk.Connectivity.WebServices.{service}Service";
            if (typeName.EndsWith("ServiceService"))
                typeName = typeName.Replace("ServiceService", "Service");
            
            return Instance.GetType(typeName);
        }

        public static void FindVaultFiles()
        {
            var dlls = new List<string>();
            var paths = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
            };

            foreach (var path in paths)
            {
                dlls.AddRange(Directory.GetFiles(
                    Path.Combine(path, "Autodesk"),
                    "Autodesk.Connectivity.WebServices.dll", SearchOption.AllDirectories));
            }

            foreach (var dll in dlls)
            {
                var path = Path.GetDirectoryName(dll);
                if (path != null)
                {
                    var xml = Path.Combine(path, Path.GetFileNameWithoutExtension(dll) + ".xml");
                    if (File.Exists(xml))
                    {
                        _dllFile = dll;
                        _xmlFile = xml;
                        return;
                    }
                }
            }

            _dllFile = dlls.FirstOrDefault();
            if (_dllFile == null)
                _dllFile = GetAssemblyFile();
        }


        public static string GetAssemblyFile()
        {
            using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            using (var key = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"))
            {
                if (key != null)
                {
                    foreach (var subKeyName in key.GetSubKeyNames())
                    {
                        var subKey = key.OpenSubKey(subKeyName);
                        var displayName = subKey?.GetValue("DisplayName")?.ToString();
                        if (displayName != null && displayName.StartsWith("Autodesk Vault"))
                        {
                            var installLocation = subKey.GetValue("InstallLocation").ToString();

                            string[] files = Directory.GetFiles(installLocation,
                                "Autodesk.Connectivity.WebServices.dll", SearchOption.AllDirectories);
                            if (files.Length > 0)
                                return files.First();
                        }
                    }
                }
            }

            return null;
        }

        

        public static string Html(string h1, string h2, string content, string command)
        {
            if (_html == null)
            {
                var assembly = Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream("vapiTraceFiddlerExtension.Web.html"))
                {
                    if (stream != null)
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            _html = reader.ReadToEnd();
                        }
                    }
                    else
                    {
                        throw new FileNotFoundException("vapiTraceFiddlerExtension.Web.html");
                    }
                }
            }

            var html = _html;
            html = html.Replace("#H1#", h1);
            html = html.Replace("#H2#", h2);
            html = html.Replace("#CONTENT#", content);
            html = html.Replace("#COMMAND#", command);
            return html;
        }

        public static void ParseInfo()
        {
            var reader = new XmlTextReader(_xmlFile) { WhitespaceHandling = WhitespaceHandling.None };
            reader.MoveToContent();

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(reader);

            var rules = xmlDocument.GetElementsByTagName("member");
            foreach (XmlNode node in rules)
            {
                if (node.NodeType == XmlNodeType.Element && node.Name == "member")
                {
                    var name = node.Attributes?["name"]?.InnerText;
                    if (name == null)
                        continue;

                    if (name.Contains("("))
                        name = name.Substring(0, name.IndexOf("(", StringComparison.Ordinal));

                    Info[name] = node.InnerXml;
                }
            }
        }

        private static string GetKey(string type, string member)
        {
            var key = Regex.Replace(type, @"\[.*\]", string.Empty).Replace('+', '.');
            if (member != null)
                key += "." + member;
            return key;
        }

        public static string GetDocumentation(this Type type)
        {
            var key = "T:" + GetKey(type.FullName, null);
            Info.TryGetValue(key, out var info);
            return info;
        }

        public static string GetDocumentation(this PropertyInfo propertyInfo)
        {
            var key = "P:" + GetKey(propertyInfo.DeclaringType?.FullName, propertyInfo.Name);
            Info.TryGetValue(key, out var s);
            return s;
        }

        public static string GetDocumentation(this MemberInfo memberInfo)
        {
            var key = "M:" + GetKey(memberInfo.ReflectedType?.FullName, memberInfo.Name);
            Info.TryGetValue(key, out var s);
            return s;
        }

        public static string GetDocumentation(this ParameterInfo parameterInfo)
        {
            var memberDocumentation = parameterInfo.Member.GetDocumentation();
            if (memberDocumentation != null)
            {
                var regexPattern =
                    Regex.Escape(@"<param name=" + "\"" + parameterInfo.Name + "\"" + @">") +
                    ".*?" +
                    Regex.Escape(@"</param>");
                var match = Regex.Match(memberDocumentation, regexPattern);
                if (match.Success)
                {
                    return match.Value;
                }
            }
            return null;
        }
    }
}