using System;
using System.Windows.Forms;

namespace Run4Fun
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Form form = new StartForm();
            form.ShowDialog();

            /*
            using (var game = new Run4FunGame())
                game.Run();
            */
        }
    }
#endif
}
