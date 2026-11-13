namespace TablaEjemplo
{
    // Entidades
    public class MiTupla
    {
        public int FrutoInicial { get; set; }
        public int FrutoComidoArbol { get; set; }
        public int SemillasXArbol { get; set; }
        public int SemillasComidas { get; set; }
        public int SemillasGerminadas { get; set; }
        public int PlantulasSobreviven { get; set; }
    }

    public class MedidasDispersion
    {
        public double Promedio { get; set; }
        public double Varianza { get; set; }
        public double DesEstandar { get; set; }
        public double ErrorEstandar { get; set; }
        public string Rango { get; set; } = "";
    }

    static class Program
    {
        const int AnioPermitido = 2026;

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Limitante: el software solo funciona durante el año de vigencia.
            if (DateTime.Now.Year != AnioPermitido)
            {
                MessageBox.Show(
                    $"Este software solo fue habilitado para la epoca de Biologia 2026-2.\nLa licencia ha expirado y la aplicación se cerrará.",
                    "Licencia expirada", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Application.Run(new MainForm());
        }

        // Cálculo de medidas
        public static MedidasDispersion CalcularMedidas(IEnumerable<int> datos)
        {
            var lista = datos.ToList();
            double promedio = lista.Average();
            double varianza = lista.Select(x => Math.Pow(x - promedio, 2)).Average();
            double desEstandar = Math.Sqrt(varianza);
            double errorEstandar = desEstandar / Math.Sqrt(lista.Count);

            return new MedidasDispersion
            {
                Promedio = promedio,
                Varianza = varianza,
                DesEstandar = desEstandar,
                ErrorEstandar = errorEstandar,
                Rango = $"({lista.Min()} - {lista.Max()})"
            };
        }
    }
}
