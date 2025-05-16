namespace Grabaciones.Services.Econtact
{
    public class EC_EscribirLog
    {
        // Semáforo para sincronizar acceso al archivo de log desde múltiples hilos
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        #region Escribir archivo Log (versión sincrónica)
        public static void EscribirLog(string message)
        {
            string sLogFormat = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " ==> ";

            // Usar la versión asincrónica pero ejecutar de forma sincrónica
            EscribirLogAsync(message).GetAwaiter().GetResult();
        }
        #endregion

        #region Escribir archivo Log (versión asincrónica)
        public static async Task EscribirLogAsync(string message)
        {
            string sLogFormat = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " ==> ";
            string logEntry = sLogFormat + " " + message;

            // Usar semáforo para evitar problemas de concurrencia al escribir en el archivo
            await _semaphore.WaitAsync();

            try
            {
                using (StreamWriter sw = await CreateLogFilesAsync())
                {
                    await sw.WriteLineAsync(logEntry);
                    await sw.FlushAsync();
                    // No es necesario cerrar explícitamente ya que estamos usando 'using'
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
        #endregion

        #region Crear archivo Log (versión sincrónica)
        private static StreamWriter CreateLogFiles()
        {
            // Usar la versión asincrónica pero ejecutar de forma sincrónica
            return CreateLogFilesAsync().GetAwaiter().GetResult();
        }
        #endregion

        #region Crear archivo Log (versión asincrónica)
        private static async Task<StreamWriter> CreateLogFilesAsync()
        {
            StreamWriter sfile = null;
            string sYear = DateTime.Now.Year.ToString();
            string sMonth = DateTime.Now.Month.ToString().PadLeft(2, '0');
            string sDay = DateTime.Now.Day.ToString().PadLeft(2, '0');
            string sTime = sYear + sMonth + sDay;

            string projectDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

            // Crear el directorio si no existe
            if (!Directory.Exists(projectDirectory))
            {
                await Task.Run(() => Directory.CreateDirectory(projectDirectory));
            }

            string sLogFile = Path.Combine(projectDirectory, "Log_" + sTime + ".txt");

            // Crear el archivo si no existe
            if (!File.Exists(sLogFile))
            {
                using (sfile = new StreamWriter(sLogFile))
                {
                    await sfile.WriteLineAsync("******************      Log   " + sTime + "       ******************");
                    await sfile.FlushAsync();
                    // No necesitamos cerrar explícitamente ya que estamos usando 'using'
                }
            }

            // Implementar reintentos con backoff exponencial
            int numberOfRetries = 3;
            int delayOnRetry = 1000;

            for (int i = 1; i <= numberOfRetries; ++i)
            {
                try
                {
                    // StreamWriter en modo append
                    sfile = new StreamWriter(sLogFile, true);
                    break;
                }
                catch (IOException e)
                {
                    if (i == numberOfRetries)
                        throw new Exception("Se ha producido un error en el método writelog()", e);

                    await Task.Delay(delayOnRetry * i); // Backoff exponencial
                }
            }

            return sfile;
        }
        #endregion

        #region Registrar mensajes en batch (para mejor rendimiento)
        private static readonly int MaxBatchSize = 100;
        private static readonly TimeSpan BatchTimeout = TimeSpan.FromSeconds(5);
        private static readonly System.Collections.Concurrent.ConcurrentQueue<string> MessageQueue =
            new System.Collections.Concurrent.ConcurrentQueue<string>();
        private static Timer _batchTimer;
        private static bool _isBatchProcessing = false;
        private static readonly object _lockObject = new object();

        static EC_EscribirLog()
        {
            // Inicializar el temporizador para procesar mensajes en batch
            _batchTimer = new Timer(ProcessBatch, null, BatchTimeout, BatchTimeout);
        }

        /// <summary>
        /// Agrega un mensaje a la cola para ser registrado en batch
        /// </summary>
        public static void EnqueueLogMessage(string message)
        {
            string sLogFormat = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " ==> ";
            MessageQueue.Enqueue(sLogFormat + " " + message);

            // Si hay suficientes mensajes, procesar el batch
            if (MessageQueue.Count >= MaxBatchSize)
            {
                TriggerBatchProcessing();
            }
        }

        private static void TriggerBatchProcessing()
        {
            lock (_lockObject)
            {
                if (!_isBatchProcessing)
                {
                    _isBatchProcessing = true;
                    Task.Run(async () => await ProcessBatchAsync());
                }
            }
        }

        private static void ProcessBatch(object state)
        {
            if (MessageQueue.Count > 0)
            {
                TriggerBatchProcessing();
            }
        }

        private static async Task ProcessBatchAsync()
        {
            try
            {
                if (MessageQueue.Count == 0)
                {
                    return;
                }

                await _semaphore.WaitAsync();

                try
                {
                    using (StreamWriter sw = await CreateLogFilesAsync())
                    {
                        // Procesar hasta MaxBatchSize mensajes
                        int count = 0;
                        string message;
                        while (count < MaxBatchSize && MessageQueue.TryDequeue(out message))
                        {
                            await sw.WriteLineAsync(message);
                            count++;
                        }

                        await sw.FlushAsync();
                    }
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            catch (Exception ex)
            {
                // Si hay un error al escribir en batch, intentar registrar directamente
                Console.WriteLine("Error al procesar batch de logs: " + ex.Message);
            }
            finally
            {
                lock (_lockObject)
                {
                    _isBatchProcessing = false;

                    // Si aún hay mensajes en la cola, procesar otro batch
                    if (MessageQueue.Count > 0)
                    {
                        TriggerBatchProcessing();
                    }
                }
            }
        }
        #endregion
    }
}
