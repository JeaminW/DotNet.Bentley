using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using AddinManager.Helper;
using System.Windows.Input;
using System.Reflection;
using Microsoft.Win32;
using System.IO;
using AddinManager.Model;
using Bentley.DgnPlatformNET;
using Bentley.MstnPlatformNET;

namespace AddinManager.ViewModel
{
    class AddInViewModel : AddinManager.MVVM.ObservableObject
    {
        private ObservableCollection<AddInModel> m_Models;

        private AddInModel m_SelectedModel;

        public ObservableCollection<AddInModel> Models
        {
            get
            {
                return m_Models;
            }

            set
            {
                m_Models = value;
                this.RaisePropertyChanged(nameof(Models));
            }
        }

        public AddInModel SelectedModel
        {
            get
            {
                return m_SelectedModel;
            }

            set
            {
                m_SelectedModel = value;
                this.RaisePropertyChanged(nameof(SelectedModel));
            }
        }

        public AddinManager.MVVM.RelayCommand Run { get; private set; }

        public AddinManager.MVVM.RelayCommand Load { get; private set; }

        public AddinManager.MVVM.RelayCommand Remove { get; private set; }

        public AddinManager.MVVM.RelayCommand Reload { get; private set; }

        public AddInViewModel()
        {
            this.Models = new ObservableCollection<AddInModel>();

            this.Run = new AddinManager.MVVM.RelayCommand(OnRun, () => SelectedModel != null && SelectedModel.Childs.Count == 0);
            this.Load = new AddinManager.MVVM.RelayCommand(OnLoad);
            this.Remove = new AddinManager.MVVM.RelayCommand(OnRemove, () => SelectedModel != null && SelectedModel.Childs.Count > 0);
            this.Reload = new AddinManager.MVVM.RelayCommand(OnReload, () => SelectedModel != null && SelectedModel.Parent == null);
        }

        private void OnRemove()
        {
            if (SelectedModel.Parent == null)
            {
                Models.Remove(SelectedModel);
            }
        }

        private void OnReload()
        {
            if (SelectedModel.Parent == null)
            {
                var path = SelectedModel.Path;
                Models.Remove(SelectedModel);
                loadModel(path);
            }
        }

        private void OnLoad()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "请选择要加载的程序集..."
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var file = openFileDialog.FileName;

                loadModel(file);
            }
        }

        private void OnRun()
        {
            const String CFG_ADDIN_DEPS_PATH = "MS_ADDIN_DEPENDENCYPATH";
            
            var file = SelectedModel.Path;
            var cfgAddinDepsPath = ConfigurationManager.GetVariable(CFG_ADDIN_DEPS_PATH, ConfigurationVariableLevel.User);
            var assemblyResolveHelper = default(AssemblyResolveHelper);

            var libDepsPath = Path.GetDirectoryName(file);
            if (!cfgAddinDepsPath.Contains(libDepsPath))
            {
                ConfigurationManager.DefineVariable(CFG_ADDIN_DEPS_PATH, $"{cfgAddinDepsPath};{libDepsPath}", ConfigurationVariableLevel.User);
            }

            try
            {
                assemblyResolveHelper = new AssemblyResolveHelper();

                var assembly = assemblyResolveHelper.Registered(file);

                if (assembly == null)
                {
                    System.Windows.MessageBox.Show("Assembly is resolve fail , please check if the dependencies are missing !");
                    return;
                }

                var type = assembly.GetType(SelectedModel.Name);

                if (type == null)
                {
                    System.Windows.MessageBox.Show(string.Format(" not found {0} type", SelectedModel.Name));
                    return;
                }

                var ctor = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder, new Type[0], null);

                if (ctor == null)
                {
                    System.Windows.MessageBox.Show(string.Format("{0} type not contain empty Constructor", SelectedModel.Name));
                    return;
                }

                var method = type.GetMethod("Execute", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                if (method == null)
                {
                    System.Windows.MessageBox.Show(string.Format("not found {0} Execute", method.Name));
                    return;
                }

                AddinManager.MVVM.Messenger.Default.Send(this, "Closed.Token");

                var instance = ctor.Invoke(null);

                method.Invoke(instance, new object[] { null });
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
            finally
            {
                assemblyResolveHelper?.UnRegistered();
            }
        }

        private void loadModel(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            var assemblyResolveHelper = default(AssemblyResolveHelper);

            try
            {
                assemblyResolveHelper = new AssemblyResolveHelper();

                var assembly = assemblyResolveHelper.Registered(path);

                if (assembly == null)
                {
                    System.Windows.MessageBox.Show("Assembly is resolve fail , please check if the dependencies are missing !");
                    return;
                }

                var types = assembly.GetTypes().Where(x => x.GetAttribue<AddInAttribute>() != null && x.GetInterface(typeof(ICommand).FullName) != null).ToList();

                if (types.Count == 0)
                {
                    return;
                }

                var addin = Models.FirstOrDefault(x => x.Name == assembly.GetName().Name);

                if (addin != null)
                {
                    Models.Remove(addin);
                }

                var model = new AddInModel() { Name = assembly.GetName().Name, Path = path };

                foreach (var item in types)
                {
                    model.Childs.Add(new AddInModel()
                    {
                        Name = item.FullName,
                        Path = path,
                        Parent = model
                    });
                }

                Models.Add(model);
            }
            finally
            {
                assemblyResolveHelper?.UnRegistered();
            }
        }
    }
}
