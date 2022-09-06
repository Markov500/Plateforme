namespace plateforme.Models
{
    public class DateFormat
    {
        public static string Mois(int num)
        {
            switch (num)
            {
                case 1:
                    return "Jan";
                case 2:
                    return "Fev";
                case 3:
                    return "Mars";
                case 4:
                    return "Avr";
                case 5:
                    return "Mai";
                case 6:
                    return "Jui";
                case 7:
                    return "Juil";
                case 8:
                    return "Aoû";
                case 9:
                    return "Sep";
                case 10:
                    return "Oct";
                case 11:
                    return "Nov";
                case 12:
                    return "Déc";
                default:
                    return "Indéfini";
            }
        }

        public static string AffichageDate(DateTime? laDate)
        {
            if (laDate == null)
                return "Indéfini";
            else
            {
                DateTime date = (DateTime)laDate; 
                string format = "";



                var min = (date.Minute < 10) ? "0" + date.Minute : "" + date.Minute;
                var heure = (date.Hour < 10) ? "0" + date.Hour : "" + date.Hour;
                var jour = (date.Day < 10) ? "0" + date.Day : "" + date.Day;

                format += jour + " " + DateFormat.Mois(date.Month) + " " + date.Year + " à " + heure + ":" + min;


                return format;
            }
           
        }

    }
}
