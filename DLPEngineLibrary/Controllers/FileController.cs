using DLPEngineLibrary.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace DLPEngineLibrary.Controllers
{
    internal static class FileController
    {
        internal delegate T FileReader<T>(string path);

        internal delegate void FileWriter<T>(string path, T @object);

        internal static void WriteLogInfo(string path, string info)
        {
            string pathTodir = Path.GetDirectoryName(path);
            if (!Directory.Exists(pathTodir))
                Directory.CreateDirectory(pathTodir);

            using (var stream = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write))
            {
                stream.Seek(0, SeekOrigin.End);
                StreamWriter writer = new StreamWriter(stream);
                var culture = CultureInfo.DefaultThreadCurrentCulture;
                var date = DateTime.Now;
                writer.Write($"{Environment.NewLine}{culture.Name} {date.ToString(culture)}: {info}");

                writer.Close();
                stream.Close();
            }

        }

        internal static string GetSId(WellKnownSidType wellKnownSidType)
        {
            return new SecurityIdentifier(wellKnownSidType, null).Translate(
                typeof(NTAccount)).Value;
        }

        /// <summary>
        /// Add directory security rules which is on the specified <paramref name="dirName"/>  
        /// </summary>
        /// <param name="dirName">Path to directory</param>
        /// <param name="account">The user/group to which the rules are applied.
        /// Example @"DomainName\UserName"</param>
        /// <param name="rights"></param>
        /// <param name="controlType"></param>
        internal static void AddDirectorySecurity(string dirName, string account,
            FileSystemRights rights, AccessControlType controlType)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(dirName);
            DirectorySecurity dirSecirity = dirInfo.GetAccessControl();

            #region 
            //AuthorizationRuleCollection accessRules = dirSecirity.GetAccessRules
            //    (true, true, typeof(System.Security.Principal.NTAccount));    
            //
            //
            //foreach(AccessRule rule in accessRules)
            //{
            //    if(rule.IdentityReference.Value == account.Value)
            //    {
            //        //bool value;
            //        //dirSecirity.PurgeAccessRules(rule.IdentityReference);
            //        //dirSecirity.ModifyAccessRule(AccessControlModification.RemoveAll, rule, out value);
            //        dirSecirity.AddAccessRule(new FileSystemAccessRule(rule.IdentityReference,
            //            rights, controlType));
            //        dirInfo.SetAccessControl(dirSecirity);
            //
            //        return;
            //    }
            //}
            #endregion

            dirSecirity.AddAccessRule(new FileSystemAccessRule(account, rights, InheritanceFlags.ContainerInherit |
                InheritanceFlags.ObjectInherit, PropagationFlags.None, controlType));
            dirInfo.SetAccessControl(dirSecirity);

        }

        /// <summary>
        /// Remove directory security rules which is on the specified <paramref name="dirName"/>  
        /// </summary>
        /// <param name="dirName">Path to directory</param>
        /// <param name="account">The user/group to which the rules are removed.
        /// Example @"DomainName\UserName"</param>
        /// <param name="rights"></param>
        /// <param name="controlType"></param>
        internal static void RemoveDirectorySecurity(string dirName, string account,
            FileSystemRights rights, AccessControlType controlType)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(dirName);
            DirectorySecurity dirSecurity = dirInfo.GetAccessControl();
            dirSecurity.RemoveAccessRule(new FileSystemAccessRule(account, rights, InheritanceFlags.ContainerInherit | 
                InheritanceFlags.ObjectInherit, PropagationFlags.None, controlType));
            dirInfo.SetAccessControl(dirSecurity);
        }

        ///<summary>
        ///Reads an object from a file in the <paramref name="pathToFile"/>
        ///via delegate <paramref name="reader"/> 
        ///otherwise creates a file in the <paramref name="pathToFile"/>
        ///</summary>
        ///<remarks>
        ///If the file does not exist or does not contain an object, then it creates a file at the specified <paramref name="pathToFile"/> 
        ///and returns a null
        ///</remarks>
        ///<returns>
        ///Return an object of type <typeparamref name="T"/>
        ///</returns>
        internal static T ReadObjectFromFile<T>(string pathToFile, FileReader<T> reader) where T : new()
        {
            string pathTodir = Path.GetDirectoryName(pathToFile);
            if (!Directory.Exists(pathTodir))
                Directory.CreateDirectory(pathTodir);

            if (!File.Exists(pathToFile))
            {
                File.Create(pathToFile).Close();
                return default(T);
            }
            else
            {
                using (var file = File.Open(pathToFile, FileMode.Open))
                    if (file.Length == 0)
                    {
                        file.Close();
                        return default(T);
                    }

                return reader(pathToFile);
            }
        }

        /// <summary>
        /// Write <paramref name="object"/> to file is on the <paramref name="pathToFile"/>
        /// via <paramref name="writer"/>
        /// </summary>
        /// <remarks>If the file does not exist, then it creates a file at the specified 
        /// <paramref name="pathToFile"/>
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="pathToFile"></param>
        /// <param name="object"></param>
        /// <param name="writer"></param>
        internal static void WriteObjectToFile<T>(string pathToFile, T @object, FileWriter<T> writer)
        {
            string pathTodir = Path.GetDirectoryName(pathToFile);
            if (!Directory.Exists(pathTodir))
                Directory.CreateDirectory(pathTodir);

            if (!File.Exists(pathToFile))
                File.Create(pathToFile).Close();

            if (object.Equals(@object, default(T)))
                return;

            writer(pathToFile, @object);
        }

        internal static void WriteBinaryFile(string pathToFile, byte[] data)
        {
            string pathTodir = Path.GetDirectoryName(pathToFile);
            if (!Directory.Exists(pathTodir))
                Directory.CreateDirectory(pathTodir);

            using (BinaryWriter writer = new BinaryWriter(File.Open(pathToFile, FileMode.Create)))
            {
                writer.Write(data);
                writer.Close();
            }
        }
    }
}
