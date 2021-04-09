using DLPEngineLibrary.Models;
using System;
using System.Collections.Generic;
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
            if (!File.Exists(pathToFile))
                File.Create(pathToFile).Close();

            if (object.Equals(@object, default(T)))
                return;

            writer(pathToFile, @object);
        }

        internal static bool CompareObjects<T>(T object1, T object2) where T : class
        {
            if (object.Equals(object1, object2))
                return true;

            if (object.Equals(object1, default(T)) || object.Equals(object2, default(T)))
                return false;

            Type type = typeof(T);

            foreach(System.Reflection.PropertyInfo property in type.GetProperties())
            {
                string object1Value = string.Empty;
                string object2Value = string.Empty;

                if (type.GetProperty(property.Name).GetValue(object1, null) != null)
                    object1Value = type.GetProperty(property.Name).GetValue(object1, null).ToString();

                if (type.GetProperty(property.Name).GetValue(object2, null) != null)
                    object2Value = type.GetProperty(property.Name).GetValue(object2, null).ToString();

                if (object1Value.Trim() != object2Value.Trim())
                    return false;
            }

            return true;
        }
    }
}
