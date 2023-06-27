namespace ForbiddenWords
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        private static Mutex? _mainMutex;
        [STAThread]
        static void Main()
        {
            bool _isNewCopy;
            _mainMutex = new Mutex(false, "MainApp",out _isNewCopy);
            if (!_isNewCopy) 
            {
                MessageBox.Show("It`s already running!");
                return;
            }
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}