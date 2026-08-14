using System.Globalization;

namespace FileVisor.Converters
{
    internal static class UnitConverter
    {
        const string BYTES = " Б";
        const string KILOBYTES = " КБ";
        const string MEGABYTES = " МБ";
        const string GIGABYTES = " ГБ";
        const string TERABYTES = " ТБ";

        const double BINARY_THOUSAND = 1024;
        const double BINARY_MILLION = 1048576;
        const double BINARY_BILLION = 1073741824;
        const double BINARY_TRILLION = 1099511627776;

        internal static string BytesToReadableSize(long bytes)
        {
            double value;
            string unit;

            if (bytes < BINARY_THOUSAND)
            {
                value = bytes;
                unit = BYTES;
            }
            else if (bytes < BINARY_MILLION)
            {
                value = BytesToKilobytes(bytes);
                unit = KILOBYTES;
            }
            else if (bytes < BINARY_BILLION)
            {
                value = BytesToMegaBytes(bytes);
                unit = MEGABYTES;
            }
            else if (bytes < BINARY_TRILLION)
            {
                value = BytesToGigabytes(bytes);
                unit = GIGABYTES;
            }
            else
            {
                value = BytesToTerabytes(bytes);
                unit = TERABYTES;
            }

            if (unit.Equals(BYTES))
                return value + unit;

            return value.ToString("0.00", CultureInfo.CurrentCulture) + unit;
        }

        internal static double BytesToKilobytes(long bytes)
        {
            return bytes / BINARY_THOUSAND;
        }

        internal static double BytesToMegaBytes(long bytes)
        {
            return bytes / BINARY_MILLION;
        }

        internal static double BytesToGigabytes(long bytes)
        {
            return bytes / BINARY_BILLION;
        }

        internal static double BytesToTerabytes(long bytes)
        {
            return bytes / BINARY_TRILLION;
        }
    }
}
