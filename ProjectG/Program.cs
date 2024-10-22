using ProjectG.ApplicationLayer.Extensions;
using System.ComponentModel.Design;

namespace ProjectG.PresentationLayer
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            //var services = new ServiceContainer();
            //services.AddApplicationLayerServices();
            ApplicationConfiguration.Initialize();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            Application.Run(new PG(new ApplicationLayer.Services.MacroService()));
        }
    }
}