using System;
using System.Text;

namespace OneHydra.Common.Utilities.Test.Random
{
    public static class Strings
    {
        #region Fields

        private static readonly System.Random Random = new System.Random();
        private const string AlphaNumericCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        private const string NumericCharacters = "1234567890";

        private static readonly string[] Surnames =
        {
            "Bennet","Blotchet-Halls","Carson","Clarke","DeFrance","del Castillo","Dull","Green","Greene",
            "Gringlesby","Hunter","Karsen","Locksley","MacFeather","McBadden","O'Leary","Panteley","Poel","Powys-Lybbe","Smith",
            "Straight","Stringer","White","Yokomoto"
        };

        private static readonly string[] Forenames =
        {
            "Abraham","Reginald","Cheryl","Michel","Innes","Ann","Marjorie","Matthew","Mark", "Luke", "John",
            "Burt","Lionel","Humphrey","Andrew", "Jenny","Sheryl","Livia","Charlene","Winston","Heather","Michael","Sylvia","Albert",
            "Anne","Meander","Dean","Dirk","Desmond","Akiko"
        };

        private static readonly string[] Countries =
        {
            "Afghanistan","Åland Islands","Albania","Algeria","American Samoa","Andorra","Angola","Anguilla","Antarctica","Antigua and Barbuda","Argentina","Armenia",
            "Aruba","Australia","Austria","Azerbaijan","Bahamas","Bahrain","Bangladesh","Barbados","Belarus","Belgium","Belize","Benin","Bermuda","Bhutan",
            "Bolivia (Plurinational State of)","Bonaire, Sint Eustatius and Saba","Bosnia and Herzegovina","Botswana","Bouvet Island","Brazil","British Indian Ocean Territory",
            "Brunei Darussalam","Bulgaria","Burkina Faso","Burundi","Cambodia","Cameroon","Canada","Cabo Verde","Cayman Islands","Central African Republic","Chad","Chile",
            "China","Christmas Island","Cocos (Keeling) Islands","Colombia","Comoros","Congo","Congo (Democratic Republic of the)","Cook Islands","Costa Rica","Côte d'Ivoire",
            "Croatia","Cuba","Curaçao","Cyprus","Czech Republic","Denmark","Djibouti","Dominica","Dominican Republic","Ecuador","Egypt","El Salvador","Equatorial Guinea",
            "Eritrea","Estonia","Ethiopia","Falkland Islands (Malvinas)","Faroe Islands","Fiji","Finland","France","French Guiana","French Polynesia",
            "French Southern Territories","Gabon","Gambia","Georgia","Germany","Ghana","Gibraltar","Greece","Greenland","Grenada","Guadeloupe","Guam","Guatemala","Guernsey",
            "Guinea","Guinea-Bissau","Guyana","Haiti","Heard Island and McDonald Islands","Holy See","Honduras","Hong Kong","Hungary","Iceland","India","Indonesia",
            "Iran (Islamic Republic of)","Iraq","Ireland","Isle of Man","Israel","Italy","Jamaica","Japan","Jersey","Jordan","Kazakhstan","Kenya","Kiribati",
            "Korea (Democratic People's Republic of)","Korea (Republic of)","Kuwait","Kyrgyzstan","Lao People's Democratic Republic","Latvia","Lebanon","Lesotho","Liberia",
            "Libya","Liechtenstein","Lithuania","Luxembourg","Macao","Macedonia (the former Yugoslav Republic of)","Madagascar","Malawi","Malaysia","Maldives","Mali","Malta",
            "Marshall Islands","Martinique","Mauritania","Mauritius","Mayotte","Mexico","Micronesia (Federated States of)","Moldova (Republic of)","Monaco","Mongolia",
            "Montenegro","Montserrat","Morocco","Mozambique","Myanmar","Namibia","Nauru","Nepal","Netherlands","New Caledonia","New Zealand","Nicaragua","Niger","Nigeria",
            "Niue","Norfolk Island","Northern Mariana Islands","Norway","Oman","Pakistan","Palau","Palestine, State of","Panama","Papua New Guinea","Paraguay","Peru",
            "Philippines","Pitcairn","Poland","Portugal","Puerto Rico","Qatar","Réunion","Romania","Russian Federation","Rwanda","Saint Barthélemy",
            "Saint Helena, Ascension and Tristan da Cunha","Saint Kitts and Nevis","Saint Lucia","Saint Martin (French part)","Saint Pierre and Miquelon",
            "Saint Vincent and the Grenadines","Samoa","San Marino","Sao Tome and Principe","Saudi Arabia","Senegal","Serbia","Seychelles","Sierra Leone","Singapore",
            "Sint Maarten (Dutch part)","Slovakia","Slovenia","Solomon Islands","Somalia","South Africa","South Georgia and the South Sandwich Islands","South Sudan","Spain",
            "Sri Lanka","Sudan","Suriname","Svalbard and Jan Mayen","Swaziland","Sweden","Switzerland","Syrian Arab Republic","Taiwan, Province of China","Tajikistan",
            "Tanzania, United Republic of","Thailand","Timor-Leste","Togo","Tokelau","Tonga","Trinidad and Tobago","Tunisia","Turkey","Turkmenistan","Turks and Caicos Islands",
            "Tuvalu","Uganda","Ukraine","United Arab Emirates","United Kingdom of Great Britain and Northern Ireland","United States of America",
            "United States Minor Outlying Islands","Uruguay","Uzbekistan","Vanuatu","Venezuela (Bolivarian Republic of)","Viet Nam","Virgin Islands (British)",
            "Virgin Islands (U.S.)","Wallis and Futuna","Western Sahara","Yemen","Zambia","Zimbabwe"
        }; 

        #endregion Fields

        #region Methods

        public static string FirstName()
        {
            return Forenames[Random.Next(0, Forenames.Length - 1)];
        }

        public static string LastName()
        {
            return Surnames[Random.Next(0, Surnames.Length - 1)];
        }

        public static string FullName()
        {
            return FirstName() + LastName();
        }

        public static string Country(bool includeNulls = false)
        {
            string country = null;
            if (!includeNulls || (Random.Next(5) != 1))
            {
                country = Countries[Random.Next(0, Countries.Length - 1)];
            }
            return country;
        }

        public static string Email()
        {
            var newGuid = Guid.NewGuid().ToString("N");
            return newGuid.Substring(0, 16) + "@" + newGuid.Substring(16, 16) + ".com";
        }

        public static string Url()
        {
            var newGuid = Guid.NewGuid().ToString("N");
            return "http://" + newGuid.Substring(0, 16) + "." + newGuid.Substring(16, 16) + ".com";
        }

        public static string AlpaNumericString(int size)
        {
            var builder = new StringBuilder(size);
            for (var i = 0; i < size; i++)
            {
                var ch = AlphaNumericCharacters[Random.Next(0, AlphaNumericCharacters.Length - 1)];
                builder.Append(ch);
            }
            return builder.ToString();
        }

        public static string NumericString(int size)
        {
            var builder = new StringBuilder(size);
            for (var i = 0; i < size; i++)
            {
                var ch = NumericCharacters[Random.Next(0, NumericCharacters.Length - 1)];
                builder.Append(ch);
            }
            return builder.ToString();
        }

        #endregion Methods

    }
}
