using EV3MessengerLib;
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
        public static EV3Messenger ev3Messenger = new EV3Messenger();
        private const string EV3_SERIAL_PORT = "COM37";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ev3Messenger.Connect(EV3_SERIAL_PORT);

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
