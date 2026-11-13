namespace TablaEjemplo
{
    public class MainForm : Form
    {
        static readonly string[] Columnas =
        {
            "Fruto inicial", "Frutos comidos del árbol", "Semillas x árbol",
            "% Semillas comidas", "% Semillas germinadas", "% Plántulas que sobreviven"
        };

        readonly RadioButton rbQuemados = new() { Text = "Usar datos quemados", AutoSize = true, Checked = true };
        readonly RadioButton rbManual = new() { Text = "Ingresar datos manualmente", AutoSize = true };
        readonly Label lblCantidad = new() { Text = "Cantidad de tuplas (filas):", AutoSize = true, Anchor = AnchorStyles.Left };
        readonly NumericUpDown nudTuplas = new() { Minimum = 1, Maximum = 1000, Value = 6, Width = 70 };
        readonly Button btnGenerarTabla = new() { Text = "Crear casillas", AutoSize = true };
        readonly DataGridView gridDatos = new();
        readonly NumericUpDown nudProcesos = new() { Minimum = 1, Maximum = 20, Value = 5, Width = 70 };
        readonly Button btnEjecutar = new() { Text = "Ejecutar", AutoSize = true };
        readonly DataGridView gridMuestras = new();
        readonly DataGridView gridMedidas = new();

        public MainForm()
        {
            Text = "Software Regeneración del Bosque";
            MinimumSize = new Size(900, 650);
            Size = new Size(1000, 750);
            StartPosition = FormStartPosition.CenterScreen;

            ConfigurarGrid(gridDatos, Columnas, soloLectura: true);
            ConfigurarGrid(gridMuestras, new[] { "Proceso" }.Concat(Columnas).ToArray(), soloLectura: true);
            ConfigurarGrid(gridMedidas, new[] { "Variable", "Promedio", "Varianza", "Des. Estándar", "Error Estándar", "Rango (Min - Max)" }, soloLectura: true);

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, Padding = new Padding(10) };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 33));

            var panelOpciones = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = false };
            panelOpciones.Controls.AddRange(new Control[] { rbQuemados, rbManual, lblCantidad, nudTuplas, btnGenerarTabla });

            var panelEjecutar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = false };
            panelEjecutar.Controls.AddRange(new Control[]
            {
                new Label { Text = "Procesos de selección aleatoria (1-20):", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 6, 3, 0) },
                nudProcesos, btnEjecutar
            });

            layout.Controls.Add(panelOpciones, 0, 0);
            layout.Controls.Add(Agrupar("Datos de entrada", gridDatos), 0, 1);
            layout.Controls.Add(panelEjecutar, 0, 2);
            layout.Controls.Add(Agrupar("Resultados de muestras", gridMuestras), 0, 3);
            layout.Controls.Add(Agrupar("Tabla de medidas de dispersión", gridMedidas), 0, 4);
            Controls.Add(layout);

            rbQuemados.CheckedChanged += (_, _) => CambiarModo();
            rbManual.CheckedChanged += (_, _) => CambiarModo();
            btnGenerarTabla.Click += (_, _) => GenerarCasillas((int)nudTuplas.Value);
            btnEjecutar.Click += (_, _) => Ejecutar();

            CambiarModo();
        }

        static GroupBox Agrupar(string titulo, Control c)
        {
            var g = new GroupBox { Text = titulo, Dock = DockStyle.Fill };
            c.Dock = DockStyle.Fill;
            g.Controls.Add(c);
            return g;
        }

        static void ConfigurarGrid(DataGridView g, string[] columnas, bool soloLectura)
        {
            g.AllowUserToAddRows = false;
            g.AllowUserToDeleteRows = false;
            g.AllowUserToResizeRows = false;
            g.RowHeadersVisible = false;
            g.ReadOnly = soloLectura;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            g.BackgroundColor = SystemColors.Window;
            foreach (var c in columnas) g.Columns.Add(c, c);
        }

        void CambiarModo()
        {
            bool manual = rbManual.Checked;
            lblCantidad.Enabled = nudTuplas.Enabled = btnGenerarTabla.Enabled = manual;
            gridDatos.ReadOnly = !manual;

            if (manual)
            {
                // Al pasar a manual se limpia la tabla y se crean casillas vacías.
                gridDatos.Rows.Clear();
                GenerarCasillas((int)nudTuplas.Value);
            }
            else
                CargarQuemados();
        }

        void CargarQuemados()
        {
            gridDatos.Rows.Clear();
            foreach (var t in DatosQuemados())
                gridDatos.Rows.Add(t.FrutoInicial, t.FrutoComidoArbol, t.SemillasXArbol,
                    t.SemillasComidas, t.SemillasGerminadas, t.PlantulasSobreviven);
        }

        // Ajusta la cantidad de filas conservando lo ya escrito.
        void GenerarCasillas(int cantidad)
        {
            while (gridDatos.Rows.Count > cantidad) gridDatos.Rows.RemoveAt(gridDatos.Rows.Count - 1);
            while (gridDatos.Rows.Count < cantidad) gridDatos.Rows.Add();
        }

        static List<MiTupla> DatosQuemados() => new()
        {
            new MiTupla { FrutoInicial = 15, FrutoComidoArbol = 3, SemillasXArbol = 6, SemillasComidas = 30, SemillasGerminadas = 20, PlantulasSobreviven = 10 },
            new MiTupla { FrutoInicial = 15, FrutoComidoArbol = 3, SemillasXArbol = 4, SemillasComidas = 70, SemillasGerminadas = 50, PlantulasSobreviven = 50 },
            new MiTupla { FrutoInicial = 12, FrutoComidoArbol = 2, SemillasXArbol = 6, SemillasComidas = 40, SemillasGerminadas = 50, PlantulasSobreviven = 60 },
            new MiTupla { FrutoInicial = 10, FrutoComidoArbol = 4, SemillasXArbol = 6, SemillasComidas = 70, SemillasGerminadas = 80, PlantulasSobreviven = 30 },
            new MiTupla { FrutoInicial = 5,  FrutoComidoArbol = 1, SemillasXArbol = 4, SemillasComidas = 50, SemillasGerminadas = 20, PlantulasSobreviven = 20 },
            new MiTupla { FrutoInicial = 14, FrutoComidoArbol = 2, SemillasXArbol = 9, SemillasComidas = 20, SemillasGerminadas = 20, PlantulasSobreviven = 10 }
        };

        // Lee la tabla; devuelve null (y avisa) si hay casillas vacías o inválidas.
        List<MiTupla>? LeerTabla()
        {
            var tabla = new List<MiTupla>();
            foreach (DataGridViewRow fila in gridDatos.Rows)
            {
                var v = new int[6];
                for (int c = 0; c < 6; c++)
                {
                    var txt = fila.Cells[c].Value?.ToString()?.Trim();
                    if (!int.TryParse(txt, out v[c]))
                    {
                        gridDatos.CurrentCell = fila.Cells[c];
                        MessageBox.Show($"Casilla inválida o vacía: fila {fila.Index + 1}, \"{Columnas[c]}\".\nIngrese un número entero.",
                            "Datos incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return null;
                    }
                }
                tabla.Add(new MiTupla
                {
                    FrutoInicial = v[0], FrutoComidoArbol = v[1], SemillasXArbol = v[2],
                    SemillasComidas = v[3], SemillasGerminadas = v[4], PlantulasSobreviven = v[5]
                });
            }
            if (tabla.Count == 0)
            {
                MessageBox.Show("Debe haber al menos una tupla.", "Datos incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
            return tabla;
        }

        void Ejecutar()
        {
            gridDatos.EndEdit();
            var tabla = LeerTabla();
            if (tabla == null) return;

            var random = new Random();
            int n = (int)nudProcesos.Value;
            var muestras = new List<MiTupla>();
            gridMuestras.Rows.Clear();

            for (int i = 1; i <= n; i++)
            {
                var m = new MiTupla
                {
                    FrutoInicial = tabla[random.Next(tabla.Count)].FrutoInicial,
                    FrutoComidoArbol = tabla[random.Next(tabla.Count)].FrutoComidoArbol,
                    SemillasXArbol = tabla[random.Next(tabla.Count)].SemillasXArbol,
                    SemillasComidas = tabla[random.Next(tabla.Count)].SemillasComidas,
                    SemillasGerminadas = tabla[random.Next(tabla.Count)].SemillasGerminadas,
                    PlantulasSobreviven = tabla[random.Next(tabla.Count)].PlantulasSobreviven
                };
                muestras.Add(m);
                gridMuestras.Rows.Add($"#{i}", m.FrutoInicial, m.FrutoComidoArbol, m.SemillasXArbol,
                    m.SemillasComidas, m.SemillasGerminadas, m.PlantulasSobreviven);
            }

            var medidas = new (string, MedidasDispersion)[]
            {
                ("Frutos iniciales", Program.CalcularMedidas(muestras.Select(m => m.FrutoInicial))),
                ("Frutos comidos del árbol", Program.CalcularMedidas(muestras.Select(m => m.FrutoComidoArbol))),
                ("Semillas x árbol", Program.CalcularMedidas(muestras.Select(m => m.SemillasXArbol))),
                ("% Semillas comidas", Program.CalcularMedidas(muestras.Select(m => m.SemillasComidas))),
                ("% Semillas germinadas", Program.CalcularMedidas(muestras.Select(m => m.SemillasGerminadas))),
                ("% Plántulas que sobreviven", Program.CalcularMedidas(muestras.Select(m => m.PlantulasSobreviven)))
            };

            gridMedidas.Rows.Clear();
            foreach (var (nombre, m) in medidas)
                gridMedidas.Rows.Add(nombre, m.Promedio.ToString("F2"), m.Varianza.ToString("F2"),
                    m.DesEstandar.ToString("F2"), m.ErrorEstandar.ToString("F4"), m.Rango);
        }
    }
}
