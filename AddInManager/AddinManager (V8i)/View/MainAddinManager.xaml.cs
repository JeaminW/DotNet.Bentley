using AddinManager.Helper;
using AddinManager.Model;
using AddinManager.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AddinManager.View
{
    /// <summary>
    /// MainAddinManager.xaml 的交互逻辑
    /// </summary>
    public partial class MainAddinManager : Window
    {
        public MainAddinManager()
        {
            InitializeComponent();
            this.DataContext = new AddInViewModel();
            MVVM.Messenger.Default.Register<AddInViewModel>(this, "Closed.Token", m => this.Close());
        }

        private void OnClosed(object sender, EventArgs e)
        {
            if (this.DataContext is AddInViewModel vmodel)
            {
                if (vmodel.Models.Count == 0)
                {
                    File.WriteAllText(Helper.GlobalHelper.AddInManagerAssemblyFile, "");
                    return;
                }

                try
                {
                    var models = AddInAssemblyModel.Converter(vmodel.Models);

                    AddInAssemblyModel.WriteModels(models);
                }
                catch (Exception ex)
                {

                }
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            GlobalHelper.DeleteTemp();

            if (this.DataContext is AddInViewModel vmodel)
            {
                if (!File.Exists(GlobalHelper.AddInManagerAssemblyFile))
                {
                    return;
                }

                var models = AddInAssemblyModel.ReadModels();

                if (models == null || models.Count == 0)
                {
                    return;
                }

                vmodel.Models = AddInAssemblyModel.Converter(models);
            }
        }
    }
}
