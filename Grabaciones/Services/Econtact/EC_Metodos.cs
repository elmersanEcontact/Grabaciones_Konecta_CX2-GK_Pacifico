using Grabaciones.Services.Interface;
using PureCloudPlatform.Client.V2.Model;
using System.Diagnostics;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Xml;
using System.Xml.Linq;


using ClosedXML.Excel;
using Grabaciones.Models;
using static ClosedXML.Excel.XLPredefinedFormat;
using Microsoft.Extensions.Hosting;
using FluentFTP;
using FluentFTP.Helpers;
using System;
using System.Net.Mail;
using System.Globalization;
using Newtonsoft.Json;
using DocumentFormat.OpenXml.Drawing;
using JsonSerializer = System.Text.Json.JsonSerializer;

using System.Formats.Asn1;
using CsvHelper.Configuration;
using CsvHelper;
using DocumentFormat.OpenXml.Wordprocessing;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using System.Threading.Tasks;

using System.Data;
using System.Data.SqlClient;
using Dapper;
using Microsoft.Win32;
using DocumentFormat.OpenXml.Office2016.Word.Symex;

using Amazon.S3;
using Amazon.S3.Transfer;
using System.Runtime.CompilerServices;
using Amazon.Runtime.Telemetry;
using Grabaciones.Services.GenesysCloud;
using DocumentFormat.OpenXml.Bibliography;
using PureCloudPlatform.Client.V2.Api;
using PureCloudPlatform.Client.V2.Client;


namespace Grabaciones.Services.Econtact
{
    public class EC_Metodos: IEC_Metodos
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly IAmazonS3 _s3Client;

        public EC_Metodos(IConfiguration config, HttpClient HttpClient, IAmazonS3 s3Client)
        {
            _config = config;
            _httpClient = HttpClient;
            _s3Client = s3Client;
        }

        #region Crear Directorio
        public Task<bool> CrearDirectorio(string Ruta)
        {
            string path = Ruta;

            return Task.Run(() =>
            {
                try
                {
                    // Determine whether the directory exists.
                    if (Directory.Exists(path))
                    {
                        Console.WriteLine("That path exists already.");
                        return false;
                    }
                    else
                    {
                        DirectoryInfo di = Directory.CreateDirectory(path);
                        //Console.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(path));
                        EC_EscribirLog.EscribirLog($"El directorio a sido creado de forma exitosa {0}. {Directory.GetCreationTime(path)}");
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: al momento de crear la carpeta" + e.ToString());
                    throw;
                }
                //return Respuesta = _respuesta == true ? "Nuevo" : "Existe";
            });
        }
        #endregion  

        #region Descargar Audio
        public async Task<bool> DownloadFileAsync(string audiomp3, string urlAudio)
        {
            EC_EscribirLog.EscribirLog($"Descarga de audio: {audiomp3}|{urlAudio}");

            try
            {
                HttpResponseMessage response = new HttpResponseMessage();
                response = await _httpClient.GetAsync(urlAudio);

                if (response.IsSuccessStatusCode)
                {
                    if (File.Exists(audiomp3))
                    {
                        // Opcional: Renombrar o sobrescribir
                        EC_EscribirLog.EscribirLog($"El archivo ya existe y será sobrescrito: {audiomp3}");
                        File.Delete(audiomp3);
                    }

                    using (HttpContent content = response.Content)
                    using (var fileStream = new FileStream(audiomp3, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await content.CopyToAsync(fileStream);
                    }

                    if (File.Exists(audiomp3))
                    {
                        EC_EscribirLog.EscribirLog($"Archivo descargado exitosamente: {audiomp3}");
                        return true;
                    }
                    else
                    {
                        EC_EscribirLog.EscribirLog($"Error: Archivo no encontrado después de la descarga: {audiomp3}");
                        return false;
                    }
                }
                else
                {
                    EC_EscribirLog.EscribirLog($"Error al descargar DownloadFileAsync: Código de estado {response.StatusCode} | {response.RequestMessage}");
                    return false ;
                }
            }
            catch (Exception ex)
            {
                EC_EscribirLog.EscribirLog("Error al descargar: " + ex.Message.ToString());
                return false ;
            }
        }
		#endregion

		#region Validar el telefono y reemplazar caracteres por vacio
		public string ReemplazarTelefonoxVacio(string telefonoxVacio)
		{
            int largNumero = telefonoxVacio.Length;
            int inicio = telefonoxVacio.Length - 9;
            string vTelefono = telefonoxVacio.Substring(inicio, 9);

            return vTelefono;
		}
        //405651976494542
        #endregion

        #region Validar si es celular 
        public string ValidarSiesCelular(string telefono)
		{
			if (!string.IsNullOrEmpty(telefono) && telefono.Length == 9 && telefono.StartsWith("9"))
			{
				return telefono;
			}
			else
			{
				return "";
			}
		}
		#endregion

		#region Validar si es celular o fijo
		public string ValidarSiesFijo(string telefono)
		{
			if (!string.IsNullOrEmpty(telefono) && telefono.Length != 9 && !telefono.StartsWith("9"))
			{
				return telefono;
			}
			else
			{
				return "";
			}
		}
		#endregion

		#region Convertir audios de mp3 a gsm
		public bool ConvertMp3ToGsm(string inputFile, string outputFile)
		{

            string ? appConversorAudio = _config.GetValue<string>("ConfiguracionAudio:RutaConversorAudio");

            if (!File.Exists(inputFile))
            {
                EC_EscribirLog.EscribirLog($"Error en ConvertMp3ToGsm: {inputFile} no existe el archivo");
                return false;
            }
            else
            {
               // Process cmd = new Process();

               
                    #region proceso antiguo para convertir audios de mp3 a gsm
                    //using (var cmd = new Process())
                    //{


                    //    cmd.StartInfo.FileName = "cmd.exe";
                    //    cmd.StartInfo.RedirectStandardInput = true;
                    //    cmd.StartInfo.RedirectStandardOutput = true;
                    //    cmd.StartInfo.RedirectStandardError = true;
                    //    cmd.StartInfo.CreateNoWindow = true;
                    //    cmd.StartInfo.UseShellExecute = false;

                    //    cmd.Start();

                    //    string? ejecutable = appConversorAudio;
                    //    cmd.StandardInput.WriteLine($"{ejecutable} -i \"{inputFile}\" -codec:a libmp3lame -qscale:a 2 -b:a 320000 \"{outputFile}\"");
                    //    cmd.StandardInput.Flush();
                    //    cmd.StandardInput.Close();
                    //    //cmd.WaitForExit();

                    //    if(!cmd.WaitForExit(300000))
                    //    {
                    //        EC_EscribirLog.EscribirLog($"Error en convertir de OPUS a GSM: Tiempo de espera exedido para {inputFile}");
                    //        cmd.Kill(); // Forzar la terminación del proceso de conversión de audio
                    //    //    return false;
                    //    }


                    //    string output = cmd.StandardOutput.ReadToEnd();
                    //    string error = cmd.StandardError.ReadToEnd();

                    //    if (!string.IsNullOrEmpty(error) && !File.Exists(outputFile))
                    //    {
                    //        EC_EscribirLog.EscribirLog($"Error en ConvertMp3ToGsm conversión: {error}");
                    //        return false;
                    //    }

                    //    if (File.Exists(outputFile))
                    //    {
                    //        EC_EscribirLog.EscribirLog($"Archivo convertido exitosamente en ConvertMp3ToGsm: {outputFile}");
                    //        return true;
                    //    }
                    //    else
                    //    {
                    //        EC_EscribirLog.EscribirLog($"Error en ConvertMp3ToGsm: El archivo de salida no se generó: {outputFile}");
                    //        return false;
                    //    }
                    //}
                    #endregion

                    #region Proceso para convertir audios de mp3 a gsm
                    using(var process = new Process())
                    {
                        // configurar el proceso
                        process.StartInfo.FileName = "cmd.exe";
                        process.StartInfo.Arguments = $"/c \"{appConversorAudio} -i \"{inputFile}\" -codec:a libmp3lame -qscale:a 2 -b:a 320000 \"{outputFile}\"\"";
                        process.StartInfo.UseShellExecute = false;  
                        process.StartInfo.CreateNoWindow= true;

                        //redigirigir salidas
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.RedirectStandardError = true;

                        //captar eventos asincronos
                        process.OutputDataReceived += (sender, e) =>
                        {
                            if (!string.IsNullOrEmpty(e.Data))
                            {
                                // porias registrar la salida si quieres
                                EC_EscribirLog.EscribirLog($"STDOUT: {e.Data}");
                            }
                        };

                        process.ErrorDataReceived += (sender, e) =>
                        {
                            if (!string.IsNullOrEmpty(e.Data))
                            {
                                // podras registrar la salida si quieres
                                EC_EscribirLog.EscribirLog($"STDERR: {e.Data}");
                            }
                        };

                        try
                        {
                            // Iniciar el proceso
                            process.Start();

                            // comenzar lectura asincrona
                            process.BeginOutputReadLine();
                            process.BeginErrorReadLine();

                            // esperar con un timeout (ej. 5 minutos)
                            bool exited = process.WaitForExit(120000); // 2 minutos
                            
                            //importante liberamos el procesos
                            process.Close();

                            if(!exited )
                            {
                                // se agotó el tiempo de espera, forzamos el cierre
                                process.Kill();
                                EC_EscribirLog.EscribirLog($"Error: FFmpeg excedió el tiempo de espera");
                                return false;
                            }
                            //validar si se genero el archivo
                            if (File.Exists(outputFile))
                            {
                            EC_EscribirLog.EscribirLog($"Éxito: El archivo de salida GSM {outputFile}, se genero de manera correcta");

                            for (int i = 0;i<3; i++)
                            {
                                try
                                {
                                    File.Delete(inputFile);
                                    EC_EscribirLog.EscribirLog($"Archivo {inputFile}, eliminado de forma correcta"); 
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    Thread.Sleep(500);
                                    EC_EscribirLog.EscribirLog($"Error al intentar eliminar el archivo mp3  | Mensaje de error: {ex.Message}");
                                }

                            }
                            return true;
                            }
                        else
                        {
                            EC_EscribirLog.EscribirLog($"Error: El archivo de salida no se generó de forma correcta.");
                            return false;
                        }

                        }
                        catch (Exception ex)
                        {
                            EC_EscribirLog.EscribirLog($"Error: {ex.Message.ToString()}");
                            return false;
                            // throw;
                        }

                }
                    #endregion
               
            }
		}
		#endregion

		#region Crear archivo Excel
		public async void CrearArchivoExcel(List<GC_LeerCsv> ArchivosCsvJuntos)
		{


            string _rutaFTP = @"upload";
            //string testingExcel = @"D:\Grabaciones\KPCx1\RS\KONECTA_SOAT_RIMAC_TELEMARKETING-Semana-1-julio\\KONECTA_SOAT_RIMAC_TELEMARKETING-Semana-1-julio.xlsx"; //ArchivosCsvJuntos[0].DirectorioExcel;
            string testingExcel = ArchivosCsvJuntos[0].DirectorioExcel;
            //string archivoExcel = @"KONECTA_SOAT_RIMAC_TELEMARKETING-Semana-1-julio.xlsx"; // ArchivosCsvJuntos[0].ArchivoExcel;
            string archivoExcel = ArchivosCsvJuntos[0].ArchivoExcel;

            // Define headers
            var headers = new List<string> { "Ruta", "Proveedor", "Producto", "Parte Disco", "Canal", "Sponsor", 
                                            "Fecha", "Año", "Mes", "Dia", "Hora", "Nombres y apellidos del titular", 
                                            "DNI del titular", "Nº PLACA", "Plan", "Prima", "Celular del cliente",
                                            "Fijo del cliente", "DNI del asesor", "Nombres y apellidos del asesor", 
                                            "Código", "Etiqueta", "Parte Grabación", "Dato del loguin del Asesor" };


            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Hoja1");

                // Add headers
                for (int i = 0; i < headers.Count; i++)
                {
                    worksheet.Cell(1, i + 1).Value = headers[i];
                }

                // Apply color and bold font to specific columns for the header
                var blueBackgroundColumns = new List<int> { 4, 7, 11, 14, 15, 16, 18, 19, 20, 21, 23, 24 };
                var YellowBackgroundColumns = new List<int> { 1, 2, 3, 5, 6, 8, 9, 10, 12, 13, 17, 22 };

                foreach (var col in blueBackgroundColumns)
                {
                    //var customBlueColor = XLColor.FromHtml("#4472C4");
                    var cell = worksheet.Cell(1, col);
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#4472C4");
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Font.Bold = true;
                }

                foreach (var col in YellowBackgroundColumns)
                {
                    var cell = worksheet.Cell(1, col);
                    cell.Style.Fill.BackgroundColor = XLColor.Yellow;
                    cell.Style.Font.FontColor = XLColor.Red;
                    cell.Style.Font.Bold = true;
                }


                // Populate data
                // Populate data
                for (int i = 0; i < ArchivosCsvJuntos.Count; i++)
                {
                    var item = ArchivosCsvJuntos[i];
                    worksheet.Cell(i + 2, 1).Value = item.Ruta;
                    worksheet.Cell(i + 2, 2).Value = item.Proveedor;
                    worksheet.Cell(i + 2, 3).Value = item.Producto;
                    worksheet.Cell(i + 2, 4).Value = item.ParteDisco;
                    worksheet.Cell(i + 2, 5).Value = item.Canal;
                    worksheet.Cell(i + 2, 6).Value = item.Sponsor;
                    worksheet.Cell(i + 2, 7).Value = item.Fecha;
                    worksheet.Cell(i + 2, 8).Value = item.Anio;
                    worksheet.Cell(i + 2, 9).Value = item.Mes;
                    worksheet.Cell(i + 2, 10).Value = item.Dia;
                    worksheet.Cell(i + 2, 11).Value = item.Hora;
                    worksheet.Cell(i + 2, 12).Value = item.NombresYApellidosdelTitular;
                    worksheet.Cell(i + 2, 13).Value = item.DniDelTitular;
                    worksheet.Cell(i + 2, 14).Value = item.NPlaca;
                    worksheet.Cell(i + 2, 15).Value = item.Plan;
                    worksheet.Cell(i + 2, 16).Value = item.Prima;
                    worksheet.Cell(i + 2, 17).Value = item.CelularDelCliente;
                    worksheet.Cell(i + 2, 18).Value = item.FijoDelCliente;
                    worksheet.Cell(i + 2, 19).Value = item.DniDelAsesor;
                    worksheet.Cell(i + 2, 20).Value = item.NombresYApellidosdelAsesor;
                    worksheet.Cell(i + 2, 21).Value = item.Codigo;
                    worksheet.Cell(i + 2, 22).Value = item.Etiqueta;
                    worksheet.Cell(i + 2, 23).Value = item.ParteGrabacion;
                    worksheet.Cell(i + 2, 24).Value = item.DatoDelLoginDelAsesor;
                }

                // Adjust columns to fit the content
                worksheet.Columns().AdjustToContents();

                // Save the workbook
                //string filePath = @"D:\\Pruebas\\ArchivosCsv\KONECTA_SOAT_RIMAC_TELEMARKETING-Semana-1-julio.xlsx";//testingExcel;
                string filePath = testingExcel;
                workbook.SaveAs(filePath);
                EC_EscribirLog.EscribirLog($"Archivo excel creado: {filePath}");
                
                await UploadFTPArchivo(_rutaFTP, archivoExcel, filePath);
            }


        }
        #endregion

        #region Caraga de Archivos a SFTP Amazon
        public async Task CargaSFTPAmazon(string localFilePath, string directorioRemoto, string rutaArchivoRemoto)
        {
            string _host = _config.GetValue<string>("SFTPConfiguration:host");
            string _username = _config.GetValue<string>("SFTPConfiguration:username");
            string _baseDirectory = _config.GetValue<string>("SFTPConfiguration:baseDirectory");
            string _privateKeyFilePath = _config.GetValue<string>("SFTPConfiguration:privateKeyFilePath");
            string subDirectory = directorioRemoto;
            // Leer clave privada
            var privateKeyFile = new PrivateKeyFile(_privateKeyFilePath);
            var authenticationMethods = new[] { new PrivateKeyAuthenticationMethod(_username, privateKeyFile) };

            // Configurar conexión SFTP
            var connectionInfo = new Renci.SshNet.ConnectionInfo(_host, _username, authenticationMethods);

            try
            {
                using (var sftp = new SftpClient(connectionInfo))
                {
                    sftp.Connect();

                    // Construir la ruta completa del subdirectorio
                    var fullDirectoryPath = $"{_baseDirectory}/{subDirectory}";

                    // Crear el subdirectorio si no existe
                    if (!sftp.Exists(fullDirectoryPath))
                    {
                        sftp.CreateDirectory(fullDirectoryPath);
                    }

                    // Subir el archivo al subdirectorio
                    using (var fileStream = new FileStream(localFilePath, FileMode.Open))
                    {
                        var remoteFileName = System.IO.Path.Combine(fullDirectoryPath, System.IO.Path.GetFileName(localFilePath)).Replace("\\", "/");
                        sftp.UploadFile(fileStream, remoteFileName);
                    }

                    Console.WriteLine("Archivo subido exitosamente.");
                    sftp.Disconnect();
                }
            }
            catch (Exception ex)
            {
                await EC_EscribirLog.EscribirLogAsync($"Error en CargaSFTPAmazon: {ex.Message}");
                throw;
            }


        }
        #endregion

        #region Carga de Archivos FTP
        public async Task UploadFTPAudios(string directorioFTP, string archivoGSM, string archivoLocal)
		{

			string servidorFTP = _config.GetValue<string>("ConfiguracionFTP:Servidor");
			string servidorPuerto = _config.GetValue<string>("ConfiguracionFTP:Puerto");
			string servidorUsuario = _config.GetValue<string>("ConfiguracionFTP:Usuario");
			string servidorPassword = _config.GetValue<string>("ConfiguracionFTP:Password");
			bool servidorTLS = bool.Parse(_config.GetValue<string>("ConfiguracionFTP:UseTls"));
			string _archivoUploadlocal = archivoGSM;
			string _archivoFTP = $"{directorioFTP}/{archivoLocal}";

			#region crear directorio
				await CreateDirectoryAsync(servidorFTP, servidorPuerto, servidorUsuario, servidorPassword, servidorTLS, directorioFTP);
                   Thread.Sleep(3000);
			#endregion


			#region Subir archivos a FTP
			await UploadFileAsync(servidorFTP, servidorPuerto, servidorUsuario, servidorPassword, servidorTLS, _archivoUploadlocal, _archivoFTP);
			#endregion

		}
        #endregion

        #region Cargar archivo excel
        public async Task UploadFTPArchivo(string directorioFTP, string archivoFTP, string archivoLocal)
        {

            string servidorFTP = _config.GetValue<string>("ConfiguracionFTP:Servidor");
            string servidorPuerto = _config.GetValue<string>("ConfiguracionFTP:Puerto");
            string servidorUsuario = _config.GetValue<string>("ConfiguracionFTP:Usuario");
            string servidorPassword = _config.GetValue<string>("ConfiguracionFTP:Password");
            bool servidorTLS = bool.Parse(_config.GetValue<string>("ConfiguracionFTP:UseTls"));
            string _archivoFTP = $"{directorioFTP}/{archivoFTP}";

            #region Subir archivos a FTP
            await UploadFileAsync(servidorFTP, servidorPuerto, servidorUsuario, servidorPassword, servidorTLS, archivoLocal, _archivoFTP);
            #endregion

        }
        #endregion

        #region crear directorio con fluterftp
        public async Task CreateDirectoryAsync(string servidorFTP, string servidorPuerto, string servidorUsuario, string servidorPassword, bool servidorTLS, string directorioFTP)
        {


            string _remotepath = directorioFTP.ToUpper();
            var token = new CancellationToken();
            using (var conn = new AsyncFtpClient(servidorFTP, servidorUsuario, servidorPassword, int.Parse(servidorPuerto)))
            {
                await conn.Connect(token);

                await conn.CreateDirectory("" + _remotepath + "", true, token);
            }
        }
        #endregion

        #region cargar archivos a ruta FTP
        public static async Task UploadFileAsync(string servidorFTP, string servidorPuerto, string servidorUsuario, string servidorPassword, bool servidorTLS, string archivoUploadlocal, string archivoFTP)
        {
            var token = new CancellationToken();
            using (var ftp = new AsyncFtpClient(servidorFTP, servidorUsuario, servidorPassword, int.Parse(servidorPuerto)))
            {
                await ftp.Connect(token);

				// upload a file to an existing FTP directory
				//await ftp.UploadFile(@""+archivoUploadlocal, archivoFTP, token: token);

				//// upload a file and ensure the FTP directory is created on the server
				try
				{
					await ftp.UploadFile(@"" + archivoUploadlocal, archivoFTP, FtpRemoteExists.Overwrite, true, token: token);
				}
				catch (Exception ex)
				{
					Console.WriteLine("Error " + ex.Message.ToString());
					throw;
				}

                //// upload a file and ensure the FTP directory is created on the server, verify the file after upload
                //await ftp.UploadFile(@"D:\Github\FluentFTP\README.md", "/public_html/temp/README.md", FtpRemoteExists.Overwrite, true, FtpVerify.Retry, token: token);

            }
        }
        #endregion

        #region envío de correo
        //public async Task EnviarCorreo(string destinatario, string asunto, string cuerpo)
        public async Task EnviarCorreo(string asunto, string nombresemana)
        {

            EC_SmtpSettings ? _smtpSettings = _config.GetSection("SendEmailSettings").Get<EC_SmtpSettings>();

            string ?_server = _smtpSettings.Server;
            int _port = _smtpSettings.Port;

            MailMessage _message = new MailMessage();
			List<string> _destinatarios = new List<string>();

            _message.From = new MailAddress(_smtpSettings.From);

            #region Agregar destinatarios
            if (_smtpSettings.To.Count > 0)
            {
                foreach (var item in _smtpSettings.To) { _message.To.Add(item); }
            }
            #endregion

            #region Agregamos los destinos de copia
            if (_smtpSettings.CC.Count > 0)
            {
                foreach (var item in _smtpSettings.CC) { _message.CC.Add(item); }
            }
            #endregion

            #region Agregar los destinatarios de copia oculta
            if (_smtpSettings.BCC.Count > 0)
            {
                foreach (var item in _smtpSettings.BCC) { _message.Bcc.Add(item); }
            }
            #endregion

            #region Agregamos asunto y cuerpo

            string _nombredeCarpetayExcel = string.Concat("KONECTA_SOAT_RIMAC_TELEMARKETING_",nombresemana.Replace("-","_")).ToUpper();

            _message.Subject = asunto;
            _message.IsBodyHtml = true;
            //_message.Body = cuerpo;
            _message.Body  = @"<p>Estimados buenos días,</p>";
            _message.Body += @"<p>Se procedió con la carga automática de los audios de Rimac Soat correspondiente a la "+nombresemana+".</p>";
            _message.Body += @"<p>Se realiza la carga del archivo y carpeta con el siguiente nombre:<br>";
            _message.Body += @"<strong>"+_nombredeCarpetayExcel+"</strong></p>";
            _message.Body += @"<p>Saludos.</p>";
            #endregion

            #region enviar el correo
            using (SmtpClient client = new SmtpClient(_server, _port))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;

                try
                {
                    await client.SendMailAsync(_message);
                    Console.WriteLine("Correo enviado exitosamente");
                    EC_EscribirLog.EscribirLog("Correo enviado exitosamente");
                }
                catch (SmtpException smtpEx)
                {
                    // Captura de errores relacionados con el SMTP
                    Console.WriteLine($"Error SMTP al enviar el correo: {smtpEx.Message}");
                    EC_EscribirLog.EscribirLog($"Error SMTP al enviar el correo: {smtpEx.Message} | {_smtpSettings.ObjectToString()}");
                }
                catch (InvalidOperationException invOpEx)
                {
                    // Captura de errores relacionados con operaciones inválidas
                    Console.WriteLine($"Operación inválida: {invOpEx.Message}");
                    EC_EscribirLog.EscribirLog($"Operación inválida: {invOpEx.Message}");
                }
                catch (Exception ex)
                {
                    // Captura de cualquier otro tipo de excepción
                    Console.WriteLine($"Error general al enviar el correo: {ex.Message}");
                    EC_EscribirLog.EscribirLog($"Error general al enviar el correo: {ex.Message}");
                }
            }
            #endregion
        }
        #endregion

        #region obtener semana según rango
        public async Task<string> GetWeekRangeAsync(System.DateTime startDate, System.DateTime endDate)
        {
            return await Task.Run(() =>
            {
                int startWeek = GetWeekOfMonth(startDate);
                int endWeek = GetWeekOfMonth(endDate);

                if (startWeek == endWeek)
                {
                    return $"Semana-{startWeek}-{startDate.ToString("MMMM", CultureInfo.CreateSpecificCulture("es-ES"))}";
                }
                else
                {
                    //return $"Rango de semanas {startWeek}-{endWeek} de {startDate.ToString("MMMM yyyy", CultureInfo.CreateSpecificCulture("es-ES"))}";
                    return $"Semana-{startWeek}-{startDate.ToString("MMMM", CultureInfo.CreateSpecificCulture("es-ES"))}";
                }
            });
        }
        #endregion

        #region Obtener el numero de semana y saber si es último día de la semana
        public async Task<string> ObtenerNombreSemanaUltimoDia(System.DateTime startDate)
        {
            System.DateTime fechaInicio = System.DateTime.Now;
            System.DateTime fechaFin = System.DateTime.Now;
            string sDiaSemana = string.Empty;

           // Obtenemos el día de la semana en base a la fecha de inicio
           DayOfWeek diaSemanaEvaluar = startDate.DayOfWeek;
           
            if (diaSemanaEvaluar == DayOfWeek.Monday)
            {
                fechaInicio = startDate;
                fechaFin = startDate.AddDays(6);
                sDiaSemana = "Lunes";
            }
            else if (diaSemanaEvaluar == DayOfWeek.Sunday) { 
                fechaInicio = startDate.AddDays(-6);
                fechaFin = startDate;
                sDiaSemana = "Domingo";
            }
            else if (diaSemanaEvaluar == DayOfWeek.Tuesday)
            {
                fechaInicio = startDate.AddDays(-1);
                fechaFin = startDate.AddDays(5);
                sDiaSemana = "Martes";
            }
            else if (diaSemanaEvaluar == DayOfWeek.Wednesday)
            {
                fechaInicio = startDate.AddDays(-2);
                fechaFin = startDate.AddDays(4);
                sDiaSemana = "Miercoles";
            }
            else if (diaSemanaEvaluar == DayOfWeek.Thursday)
            {
                fechaInicio = startDate.AddDays(-3);
                fechaFin = startDate.AddDays(3);
                sDiaSemana = "Jueves";
            }
            else if (diaSemanaEvaluar == DayOfWeek.Friday)
            {
                fechaInicio = startDate.AddDays(-4);
                fechaFin = startDate.AddDays(2);
                sDiaSemana = "Viernes";
            }
            else if (diaSemanaEvaluar == DayOfWeek.Saturday)
            {
                fechaInicio = startDate.AddDays(-5);
                fechaFin = startDate.AddDays(1);
                sDiaSemana = "Sabado";
            }


            string nombreDiaSemana = await GetWeekRangeAsync(fechaInicio, fechaFin);
            // Convertimos el nombre del día al español
            
            return $"{nombreDiaSemana}|{sDiaSemana}";
           
        }
        #endregion

        #region Obtener Semana del mes
        private static int GetWeekOfMonth(System.DateTime date)
        {
            // Obtener el primer día del mes
            System.DateTime firstDayOfMonth = new System.DateTime(date.Year, date.Month, 1);

            // Calcular el número de la semana
            TimeSpan difference = date - firstDayOfMonth;
            int weekNumber = (difference.Days / 7) + 1; // +1 porque la primera semana cuenta como semana 1
            return weekNumber;
        }
        #endregion

        #region Obtener el nombre del mes
        public string ObtenerNombreDelMes(System.DateTime startDate)
        {
            return  startDate.ToString("MMMM", new CultureInfo("es-ES")).ToUpper();
        }
        #endregion

        #region Lista excel que se enviara a la base de datos
        public async Task EnviarDatostablaExcel(GC_ImprimirExcel DatosTablaExcel)
        {
            var urlApi = @"https://apigenesyscloud.grupokonecta.pe/RimacSoatDatosContactlist_Services/v1/DatosTablaExcel";

            //var parametros = new {
            //    semana = DatosTablaExcel.semana,
            //    directorioExcel = DatosTablaExcel.directorioExcel,
            //    archivoExcel = DatosTablaExcel.archivoExcel,
            //    Ruta = DatosTablaExcel.ruta,
            //    Proveedor = DatosTablaExcel.proveedor,
            //    Producto = DatosTablaExcel.producto,
            //    ParteDisco = DatosTablaExcel.parteDisco,
            //    Canal = DatosTablaExcel.canal,
            //    Sponsor = DatosTablaExcel.sponsor,
            //    Fecha = DatosTablaExcel.fecha,
            //    Anio = DatosTablaExcel.anio,
            //    Mes = DatosTablaExcel.mes,
            //    Dia = DatosTablaExcel.dia,
            //    Hora = DatosTablaExcel.hora,
            //    NombresApellidosDelTitular = DatosTablaExcel.nombresYApellidosdelTitular,
            //    DniDelTitular = DatosTablaExcel.dniDelTitular,
            //    Nplaca = DatosTablaExcel.nPlaca,
            //    vPlan = DatosTablaExcel.plan,
            //    Prima = DatosTablaExcel.prima,
            //    CelularDelCliente = DatosTablaExcel.celularDelCliente,
            //    FijoDelCliente = DatosTablaExcel.fijoDelCliente,
            //    DniDelAsesor = DatosTablaExcel.dniDelAsesor,
            //    NombresApellidosDelAsesor = DatosTablaExcel.nombresYApellidosdelAsesor,
            //    Codigo = DatosTablaExcel.codigo,
            //    Etiqueta = DatosTablaExcel.etiqueta,
            //    ParteGrabacion = DatosTablaExcel.parteGrabacion,
            //    DatoDelLoguinDelAsesor = DatosTablaExcel.datoDelLoginDelAsesor,
            //    conversationId = DatosTablaExcel.conversationId,
            //    recordingId = DatosTablaExcel.recordingId
            //};

            var parametros = new
            {
                semana = DatosTablaExcel.semana,
                directorioExcel = "--",
                archivoExcel = "--",
                ruta = "--",
                proveedor = "--",
                producto = "--",
                parteDisco = "--",
                canal = "--",
                sponsor = "--",
                fecha = "--",
                anio = "--",
                mes = "--",
                dia = "--",
                hora = "--",
                nombresApellidosDelTitular = "--",
                dniDelTitular = "--",
                nplaca = "--",
                vPlan = "--",
                prima = "--",
                celularDelCliente = "--",
                fijoDelCliente = "--",
                dniDelAsesor = "--",
                nombresApellidosDelAsesor = "--",
                codigo = "--",
                etiqueta = "--",
                parteGrabacion = "--",
                datoDelLoguinDelAsesor = "--",
                conversationId = "--",
                recordingId = "--"
            };

            //string BodyOneToOne = JsonSerializer.Serialize(parametros);
            var BodyOneToOne = JsonConvert.SerializeObject(parametros);
            // Configurar el contenido de la solicitud como JSON
            HttpContent content = new StringContent(BodyOneToOne, Encoding.UTF8, "application/json");

            try
            {
                Console.WriteLine($"URL: {urlApi}");
                Console.WriteLine($"Contenido JSON: {BodyOneToOne}");
                HttpResponseMessage responseApi = new HttpResponseMessage();

                responseApi = await _httpClient.PostAsJsonAsync(urlApi, parametros);
               
                // Realizar la solicitud POST
                
                string _responseBody = await responseApi.Content.ReadAsStringAsync();

                // Verificar el código de estado de la respuesta
                if (responseApi.IsSuccessStatusCode)
                {
                    // La solicitud fue exitosa
                    Console.WriteLine("Solicitud enviada correctamente.");
                    //Log.WriteLog.EscribirLog("Api Ucontinental: Ok- " + sFechaEnvioApi + " - Estado" + responseApi.StatusCode.ToString() + "-" + item.MESSAGEID + "--" + _responseBody);
                }
                else
                {
                    // La solicitud falló
                    //Log.WriteLog.EscribirLog("Api Ucontinental: " + sFechaEnvioApi + " - Error -" + responseApi.Content.ToString());
                    Console.WriteLine($"Error al enviar la solicitud. Código de estado: {responseApi.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                // Capturar y manejar cualquier excepción
                //Log.WriteLog.EscribirLog("Error al ejecutar el Api de Ucontinental:" + sFechaEnvioApi + " -" + ex.Message.ToString());
                Console.WriteLine($"Error al enviar la solicitud: {ex.Message}");
            }

            
        }
        #endregion

        #region Obtener los datos desde la base de datos segun la semana
        public async Task<List<GC_Select_DatosTablaExcel>> ObtenerDatosBD(string nombredelasemana)
        {
            //string url = $"https://localhost:44304/v1/SelectDatosTablaExcel?semana={semana}";

            string url = $"https://apigenesyscloud.grupokonecta.pe/RimacSoatDatosContactlist_Services/v1/SelectDatosTablaExcel?semana={nombredelasemana}";
            List<GC_ImprimirExcel> listaexcel = new List<GC_ImprimirExcel> ();
            // Si necesitas enviar datos en el cuerpo de la solicitud POST, define el objeto aquí
            var requestData = new
            {
                semana = nombredelasemana
                // Agrega otros parámetros necesarios aquí
            };

            try
            {
                var json = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response =  await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode(); // Lanza una excepción si el código de estado no es exitoso

                var responseString = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<List<GC_Select_DatosTablaExcel>>(responseString);

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }

        }
        #endregion

        #region Escribir archivo Log
        public async void EscribirLog(string Message)
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

        #region Crear archivo csv
        public async Task CrearArchivoCsv(List<EC_CSVYanbal> listImprimirCSV)
        {

            string RutaArchivo = listImprimirCSV[0].DirectorioCSV;
            string NombreArchivo = $"{RutaArchivo}/Grabaciones.csv";

            //string NombreArchivo = $"C:/Elmer/Pruebas/Grabaciones/Yanbal/Yanbal/Yanbal/Yanbal_Konecta/ArchivoYabal.csv";

            string filePath = NombreArchivo;
            // Configuración de CsvHelper con el delimitador personalizado
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";"
            };

            // Crear un archivo CSV y escribir los datos
            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            using (var csv = new CsvWriter(writer, csvConfig))
            {
                // Escribir los datos de cada objeto en la lista
                csv.WriteField("ID_ recording");
                csv.WriteField("Conversationid");
                csv.WriteField("Direction");
                csv.WriteField("Duration");
                csv.WriteField("Conversation StartTime");
                csv.WriteField("Conversation EndTime");
                csv.WriteField("Userid");
                csv.WriteField("Agentid");
                csv.WriteField("Wrap Up Code");
                csv.WriteField("ACW");
                csv.WriteField("ANI");
                csv.WriteField("Queue_name");
                csv.WriteField("Name División");
                csv.WriteField("IVR selection");
                csv.WriteField("Hold Time");
                csv.WriteField("Dnis");

                csv.NextRecord();

                foreach (var item in listImprimirCSV)
                {
                    //string cleanedMessage = dialog.Message.Replace("\n", "").Replace("\r", "");

                    csv.WriteField(item.IdRecording);
                    csv.WriteField(item.ConversationId);
                    csv.WriteField(item.Direction);
                    csv.WriteField(item.Duration/1000);
                    csv.WriteField(item.ConversationStartTime);
                    csv.WriteField(item.ConversationEndTime);
                    csv.WriteField(item.Userid);
                    csv.WriteField(item.Agentid);
                    csv.WriteField(item.WrapUpCode);
                    csv.WriteField(item.ACW/1000);
                    csv.WriteField(item.ANI);
                    csv.WriteField(item.QueueName);
                    csv.WriteField(item.NameDivision);
                    csv.WriteField(item.IVRSelection);
                    csv.WriteField(item.HoldTime/1000);
                    csv.WriteField(item.Dnis);
                    
                    csv.NextRecord();
                }
            }

        }
        #endregion

        #region Leer archivos csv desde la ruta indicada
        public async Task<List<GC_LeerCsv>> LeerArchivosCsv(string ruta)
        {
            List<GC_LeerCsv> ArchivosCsvJuntos = new List<GC_LeerCsv>();
            string _folderPath = ruta;

            foreach (var filePath in Directory.GetFiles(_folderPath, "*.csv"))
            {
                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";", // Especifica el delimitador como ';'
                    HasHeaderRecord = true,  // Indica que el CSV tiene cabecera
                    HeaderValidated = null,  // Desactiva la validación de cabeceras
                    MissingFieldFound = null // Desactiva la excepción por campos faltantes
                }))
                {
                   
                    await csv.ReadAsync(); // Lee la primera fila (que es la cabecera) y la ignora
                    csv.ReadHeader(); // Indica que debe empezar a leer el contenido
                    while (csv.Read())
                    {
                        var record = new GC_LeerCsv
                        {
                            Semana = csv.GetField<string>(0),
                            DirectorioExcel = csv.GetField<string>(1),
                            ArchivoExcel = csv.GetField<string>(2),
                            Ruta = csv.GetField<string>(3),
                            Proveedor = csv.GetField<string>(4),
                            Producto = csv.GetField<string>(5),
                            ParteDisco = csv.GetField<string>(6),
                            Canal = csv.GetField<string>(7),
                            Sponsor = csv.GetField<string>(8),
                            Fecha = csv.GetField<string>(9),
                            Anio = csv.GetField<string>(10),
                            Mes = csv.GetField<string>(11),
                            Dia = csv.GetField<string>(12),
                            Hora = csv.GetField<string>(13),
                            NombresYApellidosdelTitular = csv.GetField<string>(14),
                            DniDelTitular = csv.GetField<string>(15),
                            NPlaca = csv.GetField<string>(16),
                            Plan = csv.GetField<string>(17),
                            Prima = csv.GetField<string>(18),
                            CelularDelCliente = csv.GetField<string>(19),
                            FijoDelCliente = csv.GetField<string>(20),
                            DniDelAsesor = csv.GetField<string>(21),
                            NombresYApellidosdelAsesor = csv.GetField<string>(22),
                            Codigo = csv.GetField<string>(23),
                            Etiqueta = csv.GetField<string>(24),
                            ParteGrabacion = csv.GetField<string>(25),
                            DatoDelLoginDelAsesor = csv.GetField<string>(26),
                            ConversationId = csv.GetField<string>(27),
                            RecordingId = csv.GetField<string>(28),
                            ArchivoCsv = csv.GetField<string>(29)
                        };
                        ArchivosCsvJuntos.Add(record);
                    }
                }
            }

            return ArchivosCsvJuntos;

        }
        #endregion

        #region Quitar caracteres especiales a cadena para nomenclatura de audios
        public Task<string>EliminarCaracteresEspeciales(string cadena)
        {
            return Task.Run(() =>
            {
                string cadenaNueva = string.Empty;

                // Obtener los caracteres no permitidos en Windows
                char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();

                // Reemplazar los caracteres no permitidos por una cadena vacía
                cadenaNueva = new string(cadena
                    .Where(ch => !invalidChars.Contains(ch))
                    .ToArray());

                return cadenaNueva;
            });
        }

        #endregion

        #region Subir a SFTP de Konecta
        public async Task<bool> SubirArchivosSFTPKonecta(string archivo, string directorioFTP)
        {
            await EC_EscribirLog.EscribirLogAsync($"Subir archivo a sftp de konect: {archivo}");
            bool respuesta = false;
            string host = _config.GetValue<string>("SFTPConfiguration:konectaFTP");
            string username = _config.GetValue<string>("SFTPConfiguration:userFTP");
            string passFTP = _config.GetValue<string>("SFTPConfiguration:passFTP");
            int puertoFTP = _config.GetValue<int>("SFTPConfiguration:puertoFTP");
            string privateKeyFilePath = _config.GetValue<string>("SFTPConfiguration:privateKeyFilePath");
            string remoteDirectory = directorioFTP;

            #region metodo para cargar archivos a SFTP de Konecta-pacifico
            using (var sftp = new SftpClient(host, puertoFTP, username, passFTP))
            {
                try
                {
                    sftp.Connect();
                   await  EC_EscribirLog.EscribirLogAsync("Conectado al servidor SFTP.");

                    // ✅ Crear directorios recursivamente
                    await CreateDirectoryRecursivelyAsync(sftp, remoteDirectory);

                    ////// Verificar si el directorio remoto existe
                    ////if (!sftp.Exists(remoteDirectory))
                    ////{
                    ////    sftp.CreateDirectory(remoteDirectory);
                    ////    await EC_EscribirLog.EscribirLogAsync($"Directorio '{remoteDirectory}' creado.");
                    ////}
                    ////else
                    ////{
                    ////    await EC_EscribirLog.EscribirLogAsync($"El directorio '{remoteDirectory}' ya existe, se omite su creación.");
                    ////}

                    string fileName = System.IO.Path.GetFileName(archivo);
                    string remoteFilePath = $"{remoteDirectory}/{fileName}";

                    // Verificar si el archivo ya existe en el directorio remoto
                    if (sftp.Exists(remoteFilePath))
                    {
                        await EC_EscribirLog.EscribirLogAsync($"El archivo '{fileName}' ya existe en el directorio remoto, se omite su subida.");
                    }
                    else
                    {
                        // Subir el archivo
                        await Task.Run(async () =>
                        {
                            using (var fileStream = new FileStream(archivo, FileMode.Open))
                            {

                                sftp.UploadFile(fileStream, remoteFilePath);
                                await EC_EscribirLog.EscribirLogAsync($"Archivo '{fileName}' subido correctamente.");
                                respuesta = true;
                            }
                        });

                    }
                }
                catch (Exception ex)
                {
                    await EC_EscribirLog.EscribirLogAsync($"Error al cargar achivo al sftp pacifico: {ex.Message}");
                    respuesta = false;
                }
                finally
                {
                    if (sftp.IsConnected)
                    {
                        sftp.Disconnect();
                        Console.WriteLine("Desconectado del servidor SFTP.");
                    }
                }

            }
            #endregion
            return respuesta;
        }
        #endregion

        #region Creacion recursiva de directorios en SFTP
        private async Task CreateDirectoryRecursivelyAsync(SftpClient sftp, string remotePath)
        {
            try
            {
                await EC_EscribirLog.EscribirLogAsync($"Intentando crear directorio: {remotePath}");

                // Normalizar la ruta (reemplazar \ por /)
                remotePath = remotePath.Replace('\\', '/');

                // Si la ruta ya existe, no hacer nada
                if (sftp.Exists(remotePath))
                {
                    await EC_EscribirLog.EscribirLogAsync($"El directorio '{remotePath}' ya existe.");
                    return;
                }

                // Dividir la ruta en partes
                string[] pathParts = remotePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

                string currentPath = "";

                foreach (string part in pathParts)
                {
                    currentPath += "/" + part;

                    if (!sftp.Exists(currentPath))
                    {
                        try
                        {
                            sftp.CreateDirectory(currentPath);
                            await EC_EscribirLog.EscribirLogAsync($"Directorio creado: {currentPath}");
                        }
                        catch (Exception ex)
                        {
                            await EC_EscribirLog.EscribirLogAsync($"Error creando directorio '{currentPath}': {ex.Message}");
                            throw;
                        }
                    }
                    else
                    {
                        await EC_EscribirLog.EscribirLogAsync($"Directorio existente: {currentPath}");
                    }
                }

                await EC_EscribirLog.EscribirLogAsync($"Estructura de directorios completa creada: {remotePath}");
            }
            catch (Exception ex)
            {
                await EC_EscribirLog.EscribirLogAsync($"Error en CreateDirectoryRecursively: {ex.Message}");
                throw;
            }
        }
        #endregion


        #region Metodo para guardar la información de metadata en Base de datos
        public async Task<string> GuardarMetadataEnBaseDatos(List<EC_CSVYanbal> listImprimirCSV, string connectionString)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var bulkCopy = new SqlBulkCopy(connection)
                {
                    DestinationTableName = "dbo.TB_Grabaciones", // nombre exacto de la tabla
                    BatchSize = 1000,
                    BulkCopyTimeout = 0 // sin límite de tiempo
                };

                // Mapear columnas si los nombres difieren entre el modelo y la tabla
                bulkCopy.ColumnMappings.Add("cIdRecording", "cIdRecording");
                bulkCopy.ColumnMappings.Add("cConversationId", "cConversationId");
                bulkCopy.ColumnMappings.Add("cDirection", "cDirection");
                bulkCopy.ColumnMappings.Add("nDuration", "nDuration");
                bulkCopy.ColumnMappings.Add("dConversationStartTime", "dConversationStartTime");
                bulkCopy.ColumnMappings.Add("dConversationEndTime", "dConversationEndTime");
                bulkCopy.ColumnMappings.Add("cUserId", "cUserId");
                bulkCopy.ColumnMappings.Add("cAgentId", "cAgentId");
                bulkCopy.ColumnMappings.Add("cWrapupcode", "cWrapupcode");
                bulkCopy.ColumnMappings.Add("nAcw", "nAcw");
                bulkCopy.ColumnMappings.Add("cAni", "cAni");
                bulkCopy.ColumnMappings.Add("cQueueName", "cQueueName");
                bulkCopy.ColumnMappings.Add("cNameDivision", "cNameDivision");
                bulkCopy.ColumnMappings.Add("cIVRSelection", "cIVRSelection");
                bulkCopy.ColumnMappings.Add("nHoldTime", "nHoldTime");
                bulkCopy.ColumnMappings.Add("cDnis", "cDnis");
                bulkCopy.ColumnMappings.Add("cDirectorioCSV", "cDirectorioCSV");

                // Convertir la lista a DataTable
                var dataTable = ConvertToDataTable(listImprimirCSV);

                await bulkCopy.WriteToServerAsync(dataTable);

                await EC_EscribirLog.EscribirLogAsync($"OK|Inserción de metadata en base de datos de manera exitosa");
                return $"OK|Inserción de metadata en base de datos de manera exitosa";
            }
            catch(SqlException sqlEx)
            {
                string mensajeError = $"ErrorSQL|Error al insertar metadata en base de datos|Codigo:{sqlEx.ErrorCode}| Mensaje: {sqlEx.Message}";
                await EC_EscribirLog.EscribirLogAsync(mensajeError);
                return mensajeError;
            }
            catch (Exception ex)
            {
                string mensajeError = $"ErrorSQL|Error al insertar metadata en base de datos| Mensaje: {ex.Message}";
                return mensajeError;
            }
        }
        #endregion

        #region Convertir lista de metadata a DataTable
        private static System.Data.DataTable ConvertToDataTable(IEnumerable<EC_CSVYanbal> lista)
        {
            var table = new System.Data.DataTable();
            table.Columns.Add("cIdRecording", typeof(string));
            table.Columns.Add("cConversationId", typeof(string));
            table.Columns.Add("cDirection", typeof(string));
            table.Columns.Add("nDuration", typeof(int));
            table.Columns.Add("dConversationStartTime", typeof(string));
            table.Columns.Add("dConversationEndTime", typeof(string));
            table.Columns.Add("cUserId", typeof(string));
            table.Columns.Add("cAgentId", typeof(string));
            table.Columns.Add("cWrapupcode", typeof(string));
            table.Columns.Add("nAcw", typeof(int));
            table.Columns.Add("cAni", typeof(string));
            table.Columns.Add("cQueueName", typeof(string));
            table.Columns.Add("cNameDivision", typeof(string));
            table.Columns.Add("cIVRSelection", typeof(string));
            table.Columns.Add("nHoldTime", typeof(int));
            table.Columns.Add("cDnis", typeof(string));
            table.Columns.Add("cDirectorioCSV", typeof(string));
            table.Columns.Add("DirectorioCSV", typeof(string));

            foreach (var item in lista)
            {
                table.Rows.Add(
                    item.IdRecording ?? (object)DBNull.Value,
                    item.ConversationId ?? (object)DBNull.Value,
                    item.Direction ?? (object)DBNull.Value,
                    item.Duration ?? (object)DBNull.Value,
                    item.ConversationStartTime ?? (object)DBNull.Value,
                    item.ConversationEndTime ?? (object)DBNull.Value,
                    item.Userid ?? (object)DBNull.Value,
                    item.Agentid ?? (object)DBNull.Value,
                    item.WrapUpCode ?? (object)DBNull.Value,
                    item.ACW ?? (object)DBNull.Value,
                    item.ANI ?? (object)DBNull.Value,
                    item.QueueName ?? (object)DBNull.Value,
                    item.NameDivision ?? (object)DBNull.Value,
                    item.IVRSelection ?? (object)DBNull.Value,
                    item.HoldTime ?? (object)DBNull.Value,
                    item.Dnis ?? (object)DBNull.Value,
                    item.DirectorioCSV ?? (object)DBNull.Value
                );
            }

            return table;
        }
        #endregion

        #region Enviar las grabaciones al Bucket AWS
        public async Task<string> EnviarGrabaciones_a_Bucket(string nombreBucket, List<EC_CSVYanbal> listImprimirCSV, int anio, string nombredelMes, string rutaLocal)
        {
            var resultado = new StringBuilder();

            if (string.IsNullOrWhiteSpace(nombreBucket))
                return "Error|El nombre del bucket es inválido.";

            if (listImprimirCSV == null || listImprimirCSV.Count == 0)
                return "Error|La lista de grabaciones está vacía.";

            if (string.IsNullOrWhiteSpace(nombredelMes))
                return "Error|El nombre del mes no puede estar vacío.";

            string prefix = $"{anio}/{nombredelMes.Trim()}/";
            var transferUtility = new TransferUtility(_s3Client);

            try
            {
                string[] archivos = Directory.GetFiles(rutaLocal, "*.*", SearchOption.TopDirectoryOnly);

                if (archivos.Length == 0)
                    return $"Advertencia|No se encontraron archivos en el directorio {rutaLocal}.";

                // Obtener carpetas para generar el "prefix" lógico en el bucket (por ejemplo 2025/MAYO)
                var directorioInfo = new DirectoryInfo(rutaLocal);
                var nombreMes = directorioInfo.Name;
                var nombreAnio = directorioInfo.Parent?.Name;
               // string prefix = $"{nombreAnio}/{nombreMes}/"; // ejemplo: 2025/MAYO/

                foreach (var archivo in archivos)
                {
                    try
                    {
                        string nombreArchivo = System.IO.Path.GetFileName(archivo);
                        string key = prefix + nombreArchivo;

                        await transferUtility.UploadAsync(archivo, nombreBucket, key);

                        resultado.AppendLine($"OK|Archivo subido: {key}");
                    }
                    catch (AmazonS3Exception s3Ex)
                    {
                        string error = $"Error S3|{s3Ex.Message} (Archivo: {archivo})";
                        await EC_EscribirLog.EscribirLogAsync(error);
                        resultado.AppendLine(error);
                    }
                    catch (Exception ex)
                    {
                        string error = $"Error General|{ex.Message} (Archivo: {archivo})";
                        await EC_EscribirLog.EscribirLogAsync(error);
                        resultado.AppendLine(error);
                    }
                }
            }
            catch (Exception ex)
            {
                string error = $"Error Crítico|{ex.Message}";
                await EC_EscribirLog.EscribirLogAsync(error);
                return error;
            }

            /*
            foreach (var item in listImprimirCSV)
            {
                try
                {
                    string nombreArchivo = $"{item.ConversationId}_{item.IdRecording}.mp3";
                    string rutaArchivo = item.rutacompletaAudio;

                    if (string.IsNullOrWhiteSpace(rutaArchivo) || !File.Exists(rutaArchivo))
                    {
                        string mensajeError = $"Advertencia|Archivo no encontrado: {rutaArchivo}";
                        await EC_EscribirLog.EscribirLogAsync(mensajeError);
                        resultado.AppendLine(mensajeError);
                        continue;
                    }
                    string envioBucket = $"Subiendo archivo: {rutaArchivo} a {nombreBucket}/{prefix + nombreArchivo}";
                    await transferUtility.UploadAsync(rutaArchivo, nombreBucket, prefix + nombreArchivo);

                    string mensajeOk = $"OK|Archivo subido: {prefix + nombreArchivo}";
                    resultado.AppendLine(mensajeOk);
                }
                catch (AmazonS3Exception s3Ex)
                {
                    string mensajeError = $"Error S3|{s3Ex.Message}|Bucket: {nombreBucket}| (Archivo: {item.rutacompletaAudio})";
                    await EC_EscribirLog.EscribirLogAsync(mensajeError);
                    resultado.AppendLine(mensajeError);
                }
                catch (Exception ex)
                {
                    string mensajeError = $"Error General|{ex.Message} (Archivo: {item.rutacompletaAudio})";
                    await EC_EscribirLog.EscribirLogAsync(mensajeError);
                    resultado.AppendLine(mensajeError);
                }
            }
            */

            return resultado.ToString();
        }

        #endregion

        #region Obtener nombre de division
        public async Task<string> GetDivisionName(List<GC_Division> ListDivisions, string divisionID)
        {
            if (ListDivisions == null || ListDivisions.Count == 0)
            {
                return await Task.FromResult("No hay divisiones disponibles.");
            }

            var division = ListDivisions.FirstOrDefault(d => d.id == divisionID);
            if (division != null)
            {
                return await Task.FromResult(division.name?? "SinDivision");
            }
            else
            {
                return await Task.FromResult("División no encontrada.");
            }
        }
        #endregion

        #region Obtener el nombre de la campaña
        public async Task<string> GetCampaignName(AnalyticsConversationWithoutAttributes conversation,  List<EC_Campaign> listCampaign)
        {
            string nombreDeCampania = string.Empty;
            string campaingId = string.Empty;

            try
            {
                // Validar conversación
                if (conversation?.Participants == null || !conversation.Participants.Any())
                {
                    await EC_EscribirLog.EscribirLogAsync($"No se encontraron participantes en la conversación");
                }

                // Validar lista de campañas
                if (listCampaign == null || !listCampaign.Any())
                {
                    await EC_EscribirLog.EscribirLogAsync("Lista de campañas vacía o nula");
                }

                // Obtener ID de campaña
                campaingId = conversation.Participants
                    .Where(p => p.Purpose == AnalyticsParticipantWithoutAttributes.PurposeEnum.Customer)
                    .Where(p => p.Sessions?.Any() == true)
                    .SelectMany(p => p.Sessions)
                    .Where(s => !string.IsNullOrWhiteSpace(s.OutboundCampaignId))
                    .Select(s => s.OutboundCampaignId)
                    .FirstOrDefault() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(campaingId))
                {
                   
                    await EC_EscribirLog.EscribirLogAsync($"No se encontró ID de campaña en participante customer ");
                   
                }

                // Buscar nombre de campaña
                nombreDeCampania = listCampaign
                    .Where(c => !string.IsNullOrWhiteSpace(c.IdCampaign))
                    .FirstOrDefault(c => string.Equals(c.IdCampaign, campaingId, StringComparison.OrdinalIgnoreCase))
                    ?.NameCampaign ?? string.Empty;

                if (string.IsNullOrWhiteSpace(nombreDeCampania))
                {
                    await EC_EscribirLog.EscribirLogAsync($"Campaña con ID '{campaingId}' no encontrada en la lista");
                    
                }

            }
            catch (Exception ex)
            {
               string ErrorMessage = $"Error al obtener información de la campaña: {ex.Message}";
                await EC_EscribirLog.EscribirLogAsync(ErrorMessage);
            }

            if (nombreDeCampania is null || nombreDeCampania == "")
            {
                nombreDeCampania = "SinCampaña";
            }
            return nombreDeCampania;

        }
        #endregion

        #region Obtener el nombre de la campaña de 60 días a más
        public async Task<string> GetCampaignName60DiasMas(AnalyticsConversation conversation, List<EC_Campaign> listCampaign)
        {
            string nombreDeCampania = string.Empty;
            string campaingId = string.Empty;

            try
            {
                // Validar conversación
                if (conversation?.Participants == null || !conversation.Participants.Any())
                {
                    await EC_EscribirLog.EscribirLogAsync($"No se encontraron participantes en la conversación");
                }

                // Validar lista de campañas
                if (listCampaign == null || !listCampaign.Any())
                {
                    await EC_EscribirLog.EscribirLogAsync("Lista de campañas vacía o nula");
                }

                // Obtener ID de campaña
                campaingId = conversation.Participants
                    .Where(p => p.Purpose == AnalyticsParticipant.PurposeEnum.Customer)
                    .Where(p => p.Sessions?.Any() == true)
                    .SelectMany(p => p.Sessions)
                    .Where(s => !string.IsNullOrWhiteSpace(s.OutboundCampaignId))
                    .Select(s => s.OutboundCampaignId)
                    .FirstOrDefault() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(campaingId))
                {

                    await EC_EscribirLog.EscribirLogAsync($"No se encontró ID de campaña en participante customer ");

                }

                // Buscar nombre de campaña
                nombreDeCampania = listCampaign
                    .Where(c => !string.IsNullOrWhiteSpace(c.IdCampaign))
                    .FirstOrDefault(c => string.Equals(c.IdCampaign, campaingId, StringComparison.OrdinalIgnoreCase))
                    ?.NameCampaign ?? string.Empty;

                if (string.IsNullOrWhiteSpace(nombreDeCampania))
                {
                    await EC_EscribirLog.EscribirLogAsync($"Campaña con ID '{campaingId}' no encontrada en la lista");

                }

            }
            catch (Exception ex)
            {
                string ErrorMessage = $"Error al obtener información de la campaña: {ex.Message}";
                await EC_EscribirLog.EscribirLogAsync(ErrorMessage);
            }

            if (nombreDeCampania is null || nombreDeCampania == "")
            {
                nombreDeCampania = "SinCampaña";
            }
            return nombreDeCampania;

        }
        #endregion

        #region Obtener el nombre de la cola
        public async Task<string> GetQueueName(AnalyticsConversationWithoutAttributes conversation, List<GC_Queue> listQueue)
        {
            string nombreDeCola = string.Empty;
            string ? queueId = string.Empty;

            try
            {
                
                if (conversation.Resolutions?.Any() == true)
                {
                    queueId = conversation.Resolutions
                        .Where(r => !string.IsNullOrEmpty(r.QueueId))
                        .Select(r => r.QueueId)
                        .FirstOrDefault() ?? string.Empty;

                }
                else if (queueId == string.Empty)
                {
                 
                    if(conversation.Participants?.Any() == true)
                    {
                         queueId = conversation.Participants
                                    .Where(p => p.Purpose == AnalyticsParticipantWithoutAttributes.PurposeEnum.Acd ||
                                               p.Purpose == AnalyticsParticipantWithoutAttributes.PurposeEnum.Agent)
                                    .Where(p => p.Sessions?.Any() == true)
                                    .SelectMany(p => p.Sessions)
                                    .Where(s => s.Segments?.Any() == true)
                                    .SelectMany(s => s.Segments)
                                    .Where(seg => !string.IsNullOrWhiteSpace(seg.QueueId))
                                    .Select(seg => seg.QueueId)
                                    .FirstOrDefault();
                    }
                }

                    
                if (!string.IsNullOrEmpty(queueId))
                {
                    nombreDeCola = listQueue
                                    .Where(c => !string.IsNullOrWhiteSpace(c.QueueId))
                                    .FirstOrDefault(c => string.Equals(c.QueueId, queueId, StringComparison.OrdinalIgnoreCase))
                                    ?.QueueName ?? string.Empty;
                }
                // Buscar nombre de campaña

                if (string.IsNullOrWhiteSpace(nombreDeCola))
                {
                    await EC_EscribirLog.EscribirLogAsync($"Cola con ID '{queueId}' no encontrada en la lista");
                    nombreDeCola=  "SinCola";
                }

            }
            catch (Exception ex)
            {
                string ErrorMessage = $"Error al obtener información de la campaña: {ex.Message}";
                await EC_EscribirLog.EscribirLogAsync(ErrorMessage);
            }
            return nombreDeCola;


        }
        #endregion

        #region Obtener el nombre de la cola
        public async Task<string> GetQueueName60DiasMas(AnalyticsConversation conversation, List<GC_Queue> listQueue)
        {
            string nombreDeCola = string.Empty;
            string? queueId = string.Empty;

            try
            {

                if (conversation.Resolutions?.Any() == true)
                {
                    queueId = conversation.Resolutions
                        .Where(r => !string.IsNullOrEmpty(r.QueueId))
                        .Select(r => r.QueueId)
                        .FirstOrDefault() ?? string.Empty;

                }
                else if (queueId == string.Empty)
                {

                    if (conversation.Participants?.Any() == true)
                    {
                        queueId = conversation.Participants
                                   .Where(p => p.Purpose == AnalyticsParticipant.PurposeEnum.Acd ||
                                              p.Purpose == AnalyticsParticipant.PurposeEnum.Agent)
                                   .Where(p => p.Sessions?.Any() == true)
                                   .SelectMany(p => p.Sessions)
                                   .Where(s => s.Segments?.Any() == true)
                                   .SelectMany(s => s.Segments)
                                   .Where(seg => !string.IsNullOrWhiteSpace(seg.QueueId))
                                   .Select(seg => seg.QueueId)
                                   .FirstOrDefault();
                    }
                }


                if (!string.IsNullOrEmpty(queueId))
                {
                    nombreDeCola = listQueue
                                    .Where(c => !string.IsNullOrWhiteSpace(c.QueueId))
                                    .FirstOrDefault(c => string.Equals(c.QueueId, queueId, StringComparison.OrdinalIgnoreCase))
                                    ?.QueueName ?? string.Empty;
                }
                // Buscar nombre de campaña

                if (string.IsNullOrWhiteSpace(nombreDeCola))
                {
                    await EC_EscribirLog.EscribirLogAsync($"Cola con ID '{queueId}' no encontrada en la lista");
                    nombreDeCola = "SinCola";
                }

            }
            catch (Exception ex)
            {
                string ErrorMessage = $"Error al obtener información de la campaña: {ex.Message}";
                await EC_EscribirLog.EscribirLogAsync(ErrorMessage);
            }
            return nombreDeCola;


        }
        #endregion

        #region Obtener numeros de telefeno
        public async Task<string> GetNumeroTelefono(AnalyticsConversationWithoutAttributes conversation,string direccionOrigen)
        {
            string ? numeroTelefono = string.Empty;

            if (conversation?.Participants == null || !conversation.Participants.Any())
                return await Task.FromResult(string.Empty);

            if(direccionOrigen== "INBOUND")
            {
                // Obtener el número de teléfono del participante con propósito "Customer"
                numeroTelefono = conversation.Participants
                    .Where(p => p.Purpose == AnalyticsParticipantWithoutAttributes.PurposeEnum.Customer)
                    .Where(p => p.Sessions?.Any() == true)
                    .SelectMany(p => p.Sessions)
                    .Where(s => s.MediaType == AnalyticsSession.MediaTypeEnum.Voice)
                    .Where(s => !string.IsNullOrWhiteSpace(s.Ani))
                    .Select(s => ReemplazarTelefonoxVacio(s.Ani))
                    .FirstOrDefault(tel => !string.IsNullOrWhiteSpace(tel));
            }
            else if (direccionOrigen == "OUTBOUND")
            {
                // Obtener el número de teléfono del participante con propósito "Agent"
                numeroTelefono = conversation.Participants
                    .Where(p => p.Purpose == AnalyticsParticipantWithoutAttributes.PurposeEnum.Agent)
                    .Where(p => p.Sessions?.Any() == true)
                    .SelectMany(p => p.Sessions)
                    .Where(s => s.MediaType == AnalyticsSession.MediaTypeEnum.Voice)
                    .Where(s => !string.IsNullOrWhiteSpace(s.Dnis))
                    .Select(s => ReemplazarTelefonoxVacio(s.Dnis))
                    .FirstOrDefault(tel => !string.IsNullOrWhiteSpace(tel));
            }


            return await Task.FromResult(numeroTelefono ?? string.Empty);
        }
        #endregion

        #region Obtener numeros de telefeno de 60 días a más
        public async Task<string> GetNumeroTelefono60DiasMas(AnalyticsConversation conversation, string direccionOrigen)
        {
            string? numeroTelefono = string.Empty;

            if (conversation?.Participants == null || !conversation.Participants.Any())
                return await Task.FromResult(string.Empty);

            if (direccionOrigen == "INBOUND")
            {
                // Obtener el número de teléfono del participante con propósito "Customer"
                numeroTelefono = conversation.Participants
                    .Where(p => p.Purpose == AnalyticsParticipant.PurposeEnum.Customer)
                    .Where(p => p.Sessions?.Any() == true)
                    .SelectMany(p => p.Sessions)
                    .Where(s => s.MediaType == AnalyticsSession.MediaTypeEnum.Voice)
                    .Where(s => !string.IsNullOrWhiteSpace(s.Ani))
                    .Select(s => ReemplazarTelefonoxVacio(s.Ani))
                    .FirstOrDefault(tel => !string.IsNullOrWhiteSpace(tel));
            }
            else if (direccionOrigen == "OUTBOUND")
            {
                // Obtener el número de teléfono del participante con propósito "Agent"
                numeroTelefono = conversation.Participants
                    .Where(p => p.Purpose == AnalyticsParticipant.PurposeEnum.Agent)
                    .Where(p => p.Sessions?.Any() == true)
                    .SelectMany(p => p.Sessions)
                    .Where(s => s.MediaType == AnalyticsSession.MediaTypeEnum.Voice)
                    .Where(s => !string.IsNullOrWhiteSpace(s.Dnis))
                    .Select(s => ReemplazarTelefonoxVacio(s.Dnis))
                    .FirstOrDefault(tel => !string.IsNullOrWhiteSpace(tel));
            }


            return await Task.FromResult(numeroTelefono ?? string.Empty);
        }
        #endregion

        #region Obtener numero de dni del asesor
        public async Task<string> GetDNIAsesor(CallConversation callConversation)
        {
            if (callConversation?.Participants == null || !callConversation.Participants.Any())
                return await Task.FromResult("NNNNNNNN");

            var dniAsesor = callConversation.Participants
                                                .Where(p => p.Attributes?.Any() == true)
                                                .SelectMany(p => p.Attributes)
                                                .Where(a => a.Key.Equals("wsAgenteDni", StringComparison.OrdinalIgnoreCase))
                                                .FirstOrDefault().Value?.ToString() ?? "";

            // Completar con ceros a la izquierda si es menor a 8 dígitos
            dniAsesor = dniAsesor.PadLeft(8, '0');

            if (string.IsNullOrEmpty(dniAsesor) || dniAsesor == "N")
            {
                dniAsesor= "NNNNNNNN"; // Asignar un valor por defecto si el DNI es nulo o vacío
            }

            return await Task.FromResult(dniAsesor ?? "NNNNNNNN");
        }
        #endregion

        #region Obtener numero de dni del asesor
        public async Task<string> GetDNIAsesor60DiasMas(List<AnalyticsParticipant> participants)
        {
            if (participants == null || !participants.Any())
                return await Task.FromResult("NNNNNNNN");

            var dniAsesor = participants
                            .Where(p => p.Attributes?.Any() == true)
                            .SelectMany(p => p.Attributes)
                            .Where(a => a.Key.Equals("wsAgenteDni", StringComparison.OrdinalIgnoreCase))
                            .FirstOrDefault().Value?.ToString() ?? "";

            // Completar con ceros a la izquierda si es menor a 8 dígitos
            dniAsesor = dniAsesor.PadLeft(8, '0');

            if (string.IsNullOrEmpty(dniAsesor) || dniAsesor == "N")
            {
                dniAsesor = "NNNNNNNN"; // Asignar un valor por defecto si el DNI es nulo o vacío
            }

            return await Task.FromResult(dniAsesor ?? "NNNNNNNN");
        }
        #endregion

        #region Creacion y poblado de XML
        public async Task CreateUpdateXMLGC(List<XmlGrabaciones> listMetadata)
        {
            string fechaCreacionXML = System.DateTime.Now.AddDays(-1).ToString("yyyyMMddHHmmss");
            string eAnio = listMetadata[0].eAnio;
            string eMes = listMetadata[0].eMes;


            var settings = new XmlWriterSettings
            {
                Async = true,
                Indent = true,
                Encoding = Encoding.UTF8
            };

            try
            {
                
                var grabacionesCampaignQueue = listMetadata
                .Where(m => !string.IsNullOrEmpty(m.p_nameCampaignCola))
                .GroupBy(m => m.p_nameCampaignCola);
                
                foreach(var groupMetadata in grabacionesCampaignQueue)
                {
                    int vn = groupMetadata.Count();
                    // Obtiene la ruta de audio del primer registro del grupo
                    string rutaBase = groupMetadata.FirstOrDefault()?.xmlRutadeAudio ?? "";
                    string ArchivoXML = $"{rutaBase}/Resultado_{fechaCreacionXML}.xml";
                    string directorioFTP = groupMetadata.FirstOrDefault()?.xmldirectorioFTP ?? "";

                    using (var stream = new FileStream(ArchivoXML, FileMode.Create, FileAccess.Write, FileShare.None))
                    using (var writer = XmlWriter.Create(stream, settings))
                    {
                        await writer.WriteStartDocumentAsync();
                        await writer.WriteStartElementAsync(null, "Registros", null);
                        await writer.WriteAttributeStringAsync("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                        foreach (var iMetadata in groupMetadata)
                        {
                            if (iMetadata.xmlUrlGCAudio == "NoExisteUri")
                            {
                                await EC_EscribirLog.EscribirLogAsync($"No existe audio para la grabacion: {iMetadata.xmlRecordingID}| conversationID: {iMetadata.conversationID}");
                                continue;
                            }
                            else
                            {
                                writer.WriteStartElement("Llamada");

                                await EscribirElementoAsync(writer, "Empresa", iMetadata.p_empresa);
                                await EscribirElementoAsync(writer, "DNICliente", iMetadata.p_dNICliente);
                                await EscribirElementoAsync(writer, "ApellidoPaterno", iMetadata.p_apellidoPaterno);
                                await EscribirElementoAsync(writer, "ApellidoMaterno", iMetadata.p_apellidoMaterno);
                                await EscribirElementoAsync(writer, "Nombres", iMetadata.p_nombres);
                                await EscribirElementoAsync(writer, "Telefono", iMetadata.p_telefono);
                                await EscribirElementoAsync(writer, "FechaDeServicio", iMetadata.p_fechaDeServicio);
                                await EscribirElementoAsync(writer, "HoraDeServicio", iMetadata.p_horaDeServicio);
                                await EscribirElementoAsync(writer, "NroAsesor", iMetadata.p_NroAsesor);
                                await EscribirElementoAsync(writer, "Proceso", iMetadata.p_Proceso);
                                await EscribirElementoAsync(writer, "Vdn", iMetadata.p_vdn);
                                await EscribirElementoAsync(writer, "Skill", iMetadata.p_skill);
                                await EscribirElementoAsync(writer, "Ramo", iMetadata.p_ramo);
                                await EscribirElementoAsync(writer, "Producto", iMetadata.p_producto);
                                await EscribirElementoAsync(writer, "Resultado", iMetadata.p_resultado);
                                await EscribirElementoAsync(writer, "Subresultado", iMetadata.p_subResultado);

                                writer.WriteEndElement(); // Cierra el elemento "Llamada"

                            }
                        }
                        await writer.WriteEndElementAsync(); // Registros
                        await writer.WriteEndDocumentAsync();
                        await writer.FlushAsync();
                    }
                    await EC_EscribirLog.EscribirLogAsync($"Archivo XML creado correctamente: {ArchivoXML}");
                    bool respuestaOkSFTKonecta = await SubirArchivosSFTPKonecta(ArchivoXML, directorioFTP);
                }
            }
            catch (Exception ex)
            {
                await EC_EscribirLog.EscribirLogAsync($"Error al crear el archivo XML | Mensaje: {ex.Message}");
                throw;
            }
        }

        private static async Task EscribirElementoAsync(XmlWriter writer, string nombreElemento, string valor)
        {
            await writer.WriteStartElementAsync(null, nombreElemento, null);
            await writer.WriteStringAsync(valor ?? string.Empty);
            await writer.WriteEndElementAsync();
        }
        #endregion

        #region Obtener campos para apis de pacifico
        public async Task<EC_ParametrosApiPacifico> ObtenerParametroPacifico(CallConversation callConversation)
        {
            EC_ParametrosApiPacifico parametros = new EC_ParametrosApiPacifico();

            try
            {
                #region primero obtener el valor de wsIG_Id  de los participantes de la conversación
                string wsIG_Id = callConversation?.Participants?
                    .Where(p => p.Attributes?.Any() == true)
                    .SelectMany(p => p.Attributes)
                    .Where(a => a.Key.Equals("wsIG_Id", StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault().Value?.ToString() ?? string.Empty;
                #endregion

                if(string.IsNullOrEmpty(wsIG_Id))
                {
                    await EC_EscribirLog.EscribirLogAsync($"No se encontró el atributo wsIG_Id en los participantes de la conversación: {callConversation.Id}");
                    return parametros;
                }
                else
                {
                    // Si se encontró wsIG_Id, asignarlo a los parámetros
                    await EC_EscribirLog.EscribirLogAsync($"Se encontró el atributo wsIG_Id en los participantes de la conversación: {callConversation.Id} con el valor {wsIG_Id}");

                    #region Obtener valores
                    parametros = await GetDatosPacificoAsync(wsIG_Id,callConversation.Id);
                    #endregion

                }

            }
            catch (Exception ex)
            {
                await EC_EscribirLog.EscribirLogAsync($"Error al obtener parámetros de Pacifico: {ex.Message}");
            }
               

            return parametros;
        }
        #endregion

        #region Obtener campos para apis de pacifico de 60 días a mas
        public async Task<EC_ParametrosApiPacifico> ObtenerParametroPacifico60DiasMas(List<AnalyticsParticipant> Participants)
        {
            EC_ParametrosApiPacifico parametros = new EC_ParametrosApiPacifico();

            try
            {
                if (Participants == null || !Participants.Any())
                {
                    await EC_EscribirLog.EscribirLogAsync("No se encontraron participantes en la conversación");
                    return parametros;
                }
                else
                {
                    // Buscar participante con atributos (generalmente el agente)
                    var participanteConAtributos = Participants?
                    .FirstOrDefault(p => p.Attributes?.Any() == true);

                    if (participanteConAtributos == null)
                    {
                        await EC_EscribirLog.EscribirLogAsync("No se encontró participante con atributos");
                        return parametros;
                    }

                    var attributes = participanteConAtributos.Attributes;
                    //Se obtienen los datos de los atributos
                    //parametros = new EC_ParametrosApiPacifico
                    //{
                    //    dniCliente = attributes.GetValueOrDefault("wsIG_NumDoc")?.ToString() ?? string.Empty,
                    //    id = attributes.GetValueOrDefault("wsIdContacto")?.ToString() ?? string.Empty
                    //    // dfecha = participanteConAtributos.EndTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty
                    //};
                }

            }
            catch (Exception ex)
            {
                await EC_EscribirLog.EscribirLogAsync($"Error al obtener parámetros de Pacifico: {ex.Message}");
            }

            return parametros;
        }
        #endregion

        #region Obtener metadata desde la api de pacifico -- se usa el api de finaliza llamada
        public async Task<List<EC_Metadata>> PostMetadataPacifico()
        {
            string token = await ObtenerTokenPacifico();
            List<EC_Metadata> metadatapacifico = new List<EC_Metadata>();
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    await EC_EscribirLog.EscribirLogAsync("Token de Pacifico no obtenido, no se puede continuar.");
                    return metadatapacifico;
                }
                // Aquí deberías implementar la lógica para obtener los metadatos de Pacifico
                // Por ejemplo, podrías hacer una solicitud HTTP a la API de Pacifico para obtener los metadatos
                // metadatapacifico = await ObtenerMetadatosDesdeApiPacifico(token);
            }
            catch (Exception ex)
            {
                await EC_EscribirLog.EscribirLogAsync($"Error al obtener los metadatos de Pacifico: {ex.Message}");
            }

            return metadatapacifico;
        }
        #endregion

        #region Metodo para restaurar grabaciones desde genesys cloud
        public async Task<bool> RestaurarGrabacionesGenesysCloud(string conversationId, string recordingId, string rutaDestino)
        {
            bool respuesta = false;
            try
            {
                // Aquí deberías implementar la lógica para restaurar las grabaciones desde Genesys Cloud
                // Por ejemplo, podrías hacer una solicitud HTTP a la API de Genesys Cloud para restaurar la grabación
                // respuesta = await RestaurarGrabacionDesdeApiGenesysCloud(conversationId, recordingId, rutaDestino);
                Recording recordingResult = new Recording();

                var apiInstance = new RecordingApi();
                var vConversationId = conversationId;  // string | Conversation ID
                var vRecordingId = recordingId;  // string | Recording ID
                var body = new Recording(); // Recording | recording

                //body.ArchiveDate = DateTime.Now; // Establecer la fecha de archivo a la fecha actual
                bool clearExport = true;

                //Recording result = apiInstance.PutConversationRecording(conversationId, recordingId, body, clearExport);

                recordingResult = await apiInstance.PutConversationRecordingAsync(vConversationId, vRecordingId, body, clearExport);

                #region

                if (recordingResult == null || recordingResult.FileState == Recording.FileStateEnum.Error)
                {
                    await EC_EscribirLog.EscribirLogAsync($"No se pudo restaurar la grabación con ID: {recordingId}");
                    return false;
                }
                else
                {
                    await EC_EscribirLog.EscribirLogAsync($"Se envía a restaurar la grabación con conversationId : {vConversationId} y recordingID: {recordingId} ");
                    while (recordingResult.FileState == Recording.FileStateEnum.Restoring)
                    {
                        Thread.Sleep(1000); // Esperar 1 segundo antes de volver a consultar el estado-
                    }
                }
                #endregion


            }
            catch (ApiException aEx)
            {
                await EC_EscribirLog.EscribirLogAsync($"Error al restaurar grabación desde Genesys Cloud:{aEx.ErrorCode} |  {aEx.Message}");
                respuesta = false;
            }
            catch (Exception ex)
            {
                await EC_EscribirLog.EscribirLogAsync($"Error al restaurar grabación desde Genesys Cloud: {ex.Message}");
                respuesta = false;
            }
            return respuesta;
        }
        #endregion

        #region metodo para obtener el token de pacifico
        public async Task<string> ObtenerTokenPacifico()
        {
            EC_TokenResponse tokenResponse = new EC_TokenResponse();

            // Cargar configuración
            EC_ConfiguracionApisPacfico ? vPacificoConfig;
            var configList = _config.GetSection("ConfiguracionApisPacifico").Get<List<EC_ConfiguracionApisPacfico>>();
            vPacificoConfig = configList?.FirstOrDefault();


            string ? url = vPacificoConfig.Token.Url;
            string? Ocp_Apim_Subscription_Key = vPacificoConfig.Token.OcpApimSubscriptionKey;
            string? clientcredential = vPacificoConfig.Token.ClientCredential;
            string? resource = vPacificoConfig.Token.Resource;

            try
            {
            
            //se prepara la variable para obtener el token
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, url);

            // Agregar headers requeridos
            if (!string.IsNullOrWhiteSpace(Ocp_Apim_Subscription_Key))
            {
                tokenRequest.Headers.Add("Ocp-Apim-Subscription-Key", Ocp_Apim_Subscription_Key);
            }

            if (!string.IsNullOrWhiteSpace(clientcredential))
            {
                tokenRequest.Headers.Add("ClientCredential", clientcredential);
            }

            // Agregar Content-Type y Accept
            tokenRequest.Headers.Add("Accept", "application/json");


            // Crear el body específico que requiere la API
            var tokenBody = new
            {
                resource = resource
            };

            var jsonBody = JsonConvert.SerializeObject(tokenBody);
            tokenRequest.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            await EC_EscribirLog.EscribirLogAsync($"Solicitando token con resource: {tokenBody.resource}");
            await EC_EscribirLog.EscribirLogAsync($"Headers: Ocp-Apim-Subscription-Key: {Ocp_Apim_Subscription_Key?.Substring(0, 8)}..., ClientCredential: {clientcredential?.Substring(0, 8)}...");

            var response = await _httpClient.SendAsync(tokenRequest);


                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                     tokenResponse = JsonConvert.DeserializeObject<EC_TokenResponse>(content);

                    await EC_EscribirLog.EscribirLogAsync("Token generado exitosamente");
                    await EC_EscribirLog.EscribirLogAsync($"Token type: {tokenResponse.TokenType}");
                    await EC_EscribirLog.EscribirLogAsync($"Expires in: {tokenResponse.ExpiresIn} segundos");

                    return tokenResponse.AccessToken;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    await EC_EscribirLog.EscribirLogAsync($"Error generando token de pacifico: {response.StatusCode} - {errorContent}");
                    return "";
                }

            }
            catch (Exception ex)
            {
                await EC_EscribirLog.EscribirLogAsync($"Se tuvo un error al generar token de pacifico: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region Metodo para obtener los datos desde la api de pacifico
        public async Task<EC_ParametrosApiPacifico> GetDatosPacificoAsync(string wsGcId, string conversationId)
        {
            //variable a devolver
            EC_ParametrosApiPacifico vParametrosPacifico = new EC_ParametrosApiPacifico();

            // Cargar configuración
            EC_ConfiguracionApisPacfico? vPacificoConfig;
            var configList = _config.GetSection("ConfiguracionApisPacifico").Get<List<EC_ConfiguracionApisPacfico>>();
            vPacificoConfig = configList?.FirstOrDefault();

            try
            {
                await EC_EscribirLog.EscribirLogAsync($"🔄 Iniciando proceso para wsGcId: {wsGcId}");

                // 1. Generar token
                var token = await ObtenerTokenPacifico();
                if (string.IsNullOrWhiteSpace(token))
                {
                    await EC_EscribirLog.EscribirLogAsync("❌ No se pudo obtener token");
                    return null;
                }

                // 2. Hacer PATCH request con todos los headers
                var url = $"{vPacificoConfig.ObtenerDatos.UrlDatos}{wsGcId}";
                var request = new HttpRequestMessage(HttpMethod.Patch, url);

                // Headers obligatorios
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                request.Headers.Add("Ocp-Apim-Subscription-Key", vPacificoConfig.ObtenerDatos.OcpApimSubscriptionKey);
                request.Headers.Add("Accept", "application/json");

                // ✅ IMPORTANTE: Para PATCH, agregar body vacío o minimal
                var emptyBody = "{}"; // Body JSON vacío
                request.Content = new StringContent(emptyBody, Encoding.UTF8, "application/json");


                await EC_EscribirLog.EscribirLogAsync($"🔄 PATCH request a: {url}");

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    dynamic data = JsonConvert.DeserializeObject(content);

                    // Extraer campos específicos
                    string tNumDoc_c = data?.tNumDoc_c?.ToString() ?? "NNN";
                    string tPerApellidoPaterno_c = data?.tPerApellidoPaterno_c?.ToString() ?? "NNN";
                    string tPerApellidoMaterno_c = data?.tPerApellidoMaterno_c?.ToString() ?? "NNN";
                    string tPerNombre_c = data?.tPerNombre_c?.ToString() ?? "NNN";
                    string producto = data?.chOptyTipifProducto_c?.ToString() ?? "NNN";
                    string subresultado = data?.tOptyTipifSubResultado_c?.ToString() ?? "NNN";
                    string tVDN_c = data?.tVDN_c?.ToString() ?? "NNN";

                    await EC_EscribirLog.EscribirLogAsync($"✅ Producto: '{producto}', SubResultado: '{subresultado}'");

                    return vParametrosPacifico = new EC_ParametrosApiPacifico
                    {
                        tNumDoc_c = tNumDoc_c,
                        tPerApellidoPaterno_c = tPerApellidoPaterno_c,
                        tPerApellidoMaterno_c = tPerApellidoMaterno_c,
                        tPerNombre_c = tPerNombre_c,
                        chOptyTipifProducto_c = producto,
                        tOptyTipifSubResultado_c = subresultado,
                        tVDN_c = tVDN_c,
                    };
    
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await EC_EscribirLog.EscribirLogAsync($"❌ Error en el consumo de la api de pacifico|conversationID: {conversationId}|wsGcId: {wsGcId}|:  {response.StatusCode}: {error}");
                    return vParametrosPacifico;
                }
            }
            catch (HttpRequestException httpex)
            {
                await EC_EscribirLog.EscribirLogAsync($"❌ Error en el consumo de la api de pacifico|conversationID: {conversationId}|wsGcId: {wsGcId}| Excepción HTTP: {httpex.Message}");
                return vParametrosPacifico;
            }
            catch(TaskCanceledException timeoutEx)
            {
                await EC_EscribirLog.EscribirLogAsync($"❌ Error en el consumo de la api de pacifico|conversationID: {conversationId}|wsGcId: {wsGcId}| Excepción de tiempo de espera: {timeoutEx.Message}");
                return vParametrosPacifico;
            }
            catch (Exception ex)
            {
                await EC_EscribirLog.EscribirLogAsync($"❌ Error en el consumo de la api de pacifico|conversationID: {conversationId}|wsGcId: {wsGcId}| Excepción: {ex.Message}");
                return vParametrosPacifico;
            }
        }
        #endregion

    }
}
