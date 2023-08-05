using System.Reflection;

namespace RV.Chess.CBReader.Utils
{
    public record Eco(string Code, string Name);

    internal static class EcoDecoder
    {
        private static readonly (string, string)[] _ecos = LoadEcos();

        internal static Eco Decode(uint code)
        {
            var ecoCode = ((code & 0b1111111110000000) >> 7) - 1;

            if (ecoCode < 0 || ecoCode > 499)
            {
                return new Eco(_ecos[0].Item1, _ecos[0].Item2);
            }

            return new Eco(_ecos[ecoCode].Item1, _ecos[ecoCode].Item2);
        }

        internal static (string, string)[] LoadEcos()
        {
            try
            {
                using var stream = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("RV.Chess.CBReader.Resources.ECO.csv");
                if (stream == null)
                {
                    return Array.Empty<(string, string)>();
                }
                using (var reader = new StreamReader(stream))
                {
                    var line = string.Empty;
                    var ecos = new List<(string, string)>(500);
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split(',');
                        if (parts?.Length >= 2)
                        {
                            ecos.Add((parts[0], parts[1]));
                        }
                    }

                    return ecos.ToArray();
                }

                throw new InvalidDataException("Missing ECO code resource");
            }
            catch
            {
                throw;
            }
        }
    }
}
