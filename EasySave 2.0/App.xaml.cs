using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using EasySave_2._0.Properties;
using SingleInstanceCore;

namespace EasySave_2._0
{
     
    /// Interaction logic for App.xaml
     
    public partial class App : Application, ISingleInstance
    {

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var langCode = Settings.Default.languageCode;
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(langCode);

            bool isFirstInstance = SingleInstance<App>.InitializeAsFirstInstance("EasySave");
            if (!isFirstInstance)
            {
                Current.Shutdown();
            }

            BaseWindow app = new BaseWindow();
            app.Show();

        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            SingleInstance<App>.Cleanup();
        }
        public void OnInstanceInvoked(string[] args)
        {
            throw new NotImplementedException();
        }

    }
}
