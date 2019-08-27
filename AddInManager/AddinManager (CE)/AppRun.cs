using AddinManager.Helper;
using AddinManager.Model;
using AddinManager.View;
using Bentley.MstnPlatformNET;
using Bentley.MstnPlatformNET.WPF;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AddinManager
{
    [AddIn(MdlTaskID = "AppRun")]
    public class AppRun : AddIn
    {
        private static Hashtable m_AddIns;

        protected AppRun(IntPtr mdlDescriptor)
            : base(mdlDescriptor)
        {
            m_AddIns = new Hashtable();
        }

        protected override int Run(string[] commandLine)
        {
            ResolveAddInFile(commandLine);

            return 0;
        }

        private void ResolveAddInFile(string[] commandLine)
        {
            var files = default(string[]);

            if (Directory.Exists(GlobalHelper.AddInPath))
            {
                files = Directory.GetFiles(GlobalHelper.AddInPath, "*.addin");
            }

            if (files == null)
            {
                return;
            }


            foreach (var item in files)
            {
                var model = AddInFileModel.Resolve(item);

                if (model == null)
                {
                    continue;
                }

                try
                {
                    if (model.AddInId == Guid.Empty)
                    {
                        MessageBox.Show(string.Format("{0} {1} , addin id is empty .", model.Name, model.Description));

                        continue;
                    }

                    if (m_AddIns.ContainsKey(model.AddInId))
                    {
                        MessageBox.Show(string.Format("{0} {1} , addin id is repeat .", model.Name, model.Description));

                        continue;
                    }

                    if (!File.Exists(model.Assembly))
                    {
                        MessageBox.Show(string.Format("{0} {1} , assembly file is not exists .", model.Name, model.Description));

                        continue;
                    }

                    var assembly = Assembly.LoadFrom(model.Assembly);

                    var addinType = assembly.GetTypes().FirstOrDefault(x => x.IsClass && !x.IsAbstract && x.IsSubclassOf(typeof(AddIn)));

                    if (addinType == null)
                    {
                        continue;
                    }

                    var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

                    var constructor = addinType.GetConstructor(flags, null, CallingConventions.HasThis, new Type[] { typeof(IntPtr) }, null);

                    var instance = (AddIn)constructor.Invoke(new object[] { GetMdlDescriptor() });

                    typeof(AddIn).InvokeMember("Run", flags | BindingFlags.InvokeMethod, Type.DefaultBinder, instance, new object[] { commandLine });

                    m_AddIns.Add(model.AddInId, "AddInId");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("{0} {1} {2} .", model.Name, model.Description, ex.Message));
                }
            }
        }
    }

    class AppRunKeyins
    {
        public static void ShowMain(string unparsed)
        {
            var main = new MainAddinManager();

            main.ShowDialog();
        }
    }
}
