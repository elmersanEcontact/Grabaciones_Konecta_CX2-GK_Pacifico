using System.Globalization;

namespace Grabaciones.Services.Econtact
{
	public class EC_Helpers
	{
		#region Validar el telefono y reemplazar caracteres por vacio
		public string ReemplzarTelefonoxVacio(string telefonoxVacio)
		{
			string vTelefono = telefonoxVacio.Replace("tel:51", "")
											 .Replace("tel:0051", "")
											 .Replace("tel:+51", "")
											 .Replace("tel:399551", "")
											 .Replace("tel:", "")
											 .Replace("+", "")
											 .Replace("0051", "")
											 .Replace("sip:", "")
											 .Replace("sip:51", "")
											 .Replace("sip:+51", "")
											 .Replace("sip:0051", "")
											 ;
			return vTelefono;
		}
        #endregion

        #region obtener semana según rango
        private static async Task<string> GetWeekRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await Task.Run(() =>
            {
                int startWeek = GetWeekOfMonth(startDate);
                int endWeek = GetWeekOfMonth(endDate);

                if (startWeek == endWeek)
                {
                    return $"Semana {startWeek} de {startDate.ToString("MMMM yyyy", CultureInfo.CreateSpecificCulture("es-ES"))}";
                }
                else
                {
                    return $"Rango de semanas {startWeek}-{endWeek} de {startDate.ToString("MMMM yyyy", CultureInfo.CreateSpecificCulture("es-ES"))}";
                }
            });
        }
        #endregion

        #region Obtener Semana del mes
        private static int GetWeekOfMonth(System.DateTime date)
        {
            // Obtener el primer día del mes
            System.DateTime firstDayOfMonth = new System.DateTime(date.Year, date.Month, 1);

            // Obtener el primer lunes del mes
            int firstMondayOffset = ((int)DayOfWeek.Monday - (int)firstDayOfMonth.DayOfWeek + 7) % 7;
            System.DateTime firstMonday = firstDayOfMonth.AddDays(firstMondayOffset);

            // Si la fecha es antes del primer lunes, está en la primera semana
            if (date < firstMonday)
            {
                return 1;
            }

            // Calcular el número de la semana
            TimeSpan difference = date - firstMonday;
            int weekNumber = (difference.Days / 7) + 2; // +2 porque el primer lunes cuenta como semana 2
            return weekNumber;
        }
        #endregion
    }
}
