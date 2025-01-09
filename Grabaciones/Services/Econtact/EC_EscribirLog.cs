namespace Grabaciones.Services.Econtact
{
    public class EC_EscribirLog
    {
        #region Escribir archivo Log
        public static void EscribirLog(string Message)
        {
            string sLogFormat = System.DateTime.Now.ToShortDateString() + " " + System.DateTime.Now.ToLongTimeString() + " ==> ";
            StreamWriter sw = CreateLogFiles();
            sw.WriteLine(sLogFormat + " " + Message);
            sw.Flush();
            sw.Close();
        }
        #endregion

        #region Crear archivo Log
        private static StreamWriter CreateLogFiles()
        {
            StreamWriter sfile = null;
            string sYear = System.DateTime.Now.Year.ToString();
            string sMonth = System.DateTime.Now.Month.ToString().PadLeft(2, '0');
            string sDay = System.DateTime.Now.Day.ToString().PadLeft(2, '0');
            string sTime = sYear + sMonth + sDay;

            // Cambiar esta ruta a la ubicación deseada para guardar los archivos de registro.

            string sLogFile = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Log_" + sTime + ".txt");

            if (!File.Exists(sLogFile))
            {
                sfile = new StreamWriter(sLogFile);
                sfile.WriteLine("******************      Log   " + sTime + "       ******************");
                sfile.Flush();
                sfile.Close();
            }

            int NumberOfRetries = 3;
            int DelayOnRetry = 1000;

            for (int i = 1; i <= NumberOfRetries; ++i)
            {
                try
                {
                    // Hacer operaciones de archivo aquí
                    sfile = new StreamWriter(sLogFile, true);
                    break;
                }
                catch (IOException e)
                {
                    if (i == NumberOfRetries)
                        throw new Exception("Se ha producido un error en el método writelog()", e);

                    System.Threading.Thread.Sleep(DelayOnRetry);
                }
            }

            return sfile;
        }
        #endregion
    }
}
