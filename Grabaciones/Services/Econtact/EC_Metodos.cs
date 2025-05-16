using Grabaciones.Services.Interface;
using PureCloudPlatform.Client.V2.Model;
using System.Diagnostics;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
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

namespace Grabaciones.Services.Econtact
{
    public class EC_Metodos: IEC_Metodos
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public EC_Metodos(IConfiguration config, HttpClient HttpClient)
        {
            _config = config;
            _httpClient = HttpClient;
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
			string vTelefono = telefonoxVacio.Replace("tel:", "")
                                             .Replace("tel:+", "")
                                             .Replace("+", "");
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

            EC_SmtpSettings _smtpSettings = _config.GetSection("SendEmailSettings").Get<EC_SmtpSettings>();

            string? _server = _smtpSettings.Server;
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

        #region Subir los archivos en sftp de amazon
        public async Task<bool> SubirArchivosSFTAmazon(string archivo, string nombreSemana, string anio)
        {

            string? host = _config.GetValue<string>("ConfigurationSFTPAmazon:Host");
            string? username = _config.GetValue<string>("ConfigurationSFTPAmazon:Username");
            string? privateKeyFilePath = _config.GetValue<string>("ConfigurationSFTPAmazon:PrivateKeyFilePath");
            string? remoteDirectory = $"{_config.GetValue<string>("ConfigurationSFTPAmazon:RutaServidor")}/{anio}/{nombreSemana}";

            try
            {

                // Cargamos el archivo de clave privada
                var keyFile = new PrivateKeyFile(privateKeyFilePath);
                var keyFiles = new[] { keyFile };


                // Creamos el método de autenticación con clave privada.
                // Si necesitas password, puedes combinarlo con PasswordAuthenticationMethod.
                var authMethods = new AuthenticationMethod[]
                {
                new PrivateKeyAuthenticationMethod(username, keyFiles)
                };

                var connectionInfo = new Renci.SshNet.ConnectionInfo(
                host,
                username,
                authMethods
                );

                using (var sftpClient = new SftpClient(connectionInfo))
                {
                    // Nos conectamos al servidor
                    sftpClient.Connect();

                    #region Crear directorio en el servidor remoto
                        // Validar si el directorio remoto existe, si no, crearlo.
                        // (La función Exists es propia de SftpClient en Renci.SshNet).
                        if (!sftpClient.Exists(remoteDirectory))
                        {
                            sftpClient.CreateDirectory(remoteDirectory);
                            Console.WriteLine($"Directorio remoto '{remoteDirectory}' creado exitosamente.");
                            EC_EscribirLog.EscribirLog($"Directorio remoto '{remoteDirectory}' creado exitosamente.");
                        }
                        else
                        {
                            Console.WriteLine($"Directorio remoto '{remoteDirectory}' ya existe. Se omite creación.");
                            EC_EscribirLog.EscribirLog($"Directorio remoto '{remoteDirectory}' ya existe. Se omite creación.");

                        }
                    #endregion
                    #region Subimos el archivo al directorio remoto de destino

                        string fileName = System.IO.Path.GetFileName(archivo);
                        string remoteFile = remoteDirectory.TrimEnd('/') + "/" + fileName;

                        // Verificamos si el archivo ya existe en el servidor
                        if (sftpClient.Exists(remoteFile))
                        {
                            EC_EscribirLog.EscribirLog($"El archivo '{remoteFile}' ya existe en el servidor. Se omite subida.");
                            Console.WriteLine($"El archivo '{remoteFile}' ya existe en el servidor. Se omite subida.");
                        
                        }
                        // Subida de archivo (envolvemos en Task.Run para que sea asíncrono)
                        EC_EscribirLog.EscribirLog($"Subiendo archivo: {archivo}|{remoteFile}|{fileName}");
                        Console.WriteLine($"Subiendo archivo: {fileName}");

                        await Task.Run(() =>
                        {
                            using (FileStream fs = new FileStream(archivo, FileMode.Open, FileAccess.Read))
                            {
                                sftpClient.UploadFile(fs, remoteFile);
                            }
                        });

                    #endregion


                }


                return true;
            }
            catch (Exception ex)
            {
                EC_EscribirLog.EscribirLog($"Error en la subida de archivos remotos: {ex.Message}");
                return false;
                throw;
            }

        }
        #endregion
    }
}
