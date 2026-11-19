namespace TablaEjemplo
{
    public class MainForm : Form
    {
        static readonly string[] Columnas =
        {
            "Fruto inicial", "Frutos comidos del árbol", "Frutos que quedan", "Semillas x árbol", "Total semillas",
            "% Semillas comidas", "Semillas comidas", "Semillas que quedan",
            "% Semillas germinadas", "Semillas que germinan",
            "% Plántulas que sobreviven", "Plántulas que sobreviven"
        };

        // Columnas calculadas (bloqueadas para el usuario).
        static readonly HashSet<int> Calculadas = new() { 2, 4, 6, 7, 9, 11 };
        // Columnas de entrada, en el orden de MiTupla.
        static readonly int[] Entradas = { 0, 1, 3, 5, 8, 10 };

        const string MsgComidos = "los frutos comidos tienen que ser menores a los iniciales";

        readonly RadioButton rbQuemados = new() { Text = "Usar datos quemados", AutoSize = true, Checked = true };
        readonly RadioButton rbManual = new() { Text = "Ingresar datos manualmente", AutoSize = true };
        readonly Label lblCantidad = new() { Text = "Cantidad de resgistros:", AutoSize = true, Anchor = AnchorStyles.Left };
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
            ConfigurarGrid(gridMedidas, new[] { "Variable", "Promedio", "Varianza", "Des. Estándar", "Error Estándar", "Rango (Prom − Error, Prom + Error)" }, soloLectura: true);

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
            gridDatos.CellValueChanged += (_, e) => { if (e.RowIndex >= 0 && !Calculadas.Contains(e.ColumnIndex)) RecalcularFila(gridDatos.Rows[e.RowIndex]); };
            gridDatos.CellValidating += ValidarComidos;
            gridDatos.CellBeginEdit += (_, e) => { if (Calculadas.Contains(e.ColumnIndex)) e.Cancel = true; };

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

            // Las columnas calculadas se bloquean y se muestran en gris.
            foreach (DataGridViewColumn col in g.Columns)
            {
                if (!Columnas.Contains(col.HeaderText) || !Calculadas.Contains(Array.IndexOf(Columnas, col.HeaderText))) continue;
                col.ReadOnly = true;
                col.DefaultCellStyle.BackColor = SystemColors.Control;
                col.DefaultCellStyle.Format = "0.##";
            }

            // Las columnas que empiezan con "%" se muestran con el símbolo después del número.
            foreach (DataGridViewColumn col in g.Columns)
            {
                if (!col.HeaderText.StartsWith("%")) continue;
                col.ValueType = typeof(int);
                col.DefaultCellStyle.Format = "0'%'";
            }

            // Permite escribir "30" o "30%"; el símbolo se quita antes de convertir.
            g.CellParsing += (_, e) =>
            {
                if (e.Value is string s && g.Columns[e.ColumnIndex].HeaderText.StartsWith("%"))
                {
                    s = s.Trim().TrimEnd('%').Trim();
                    if (int.TryParse(s, out int n)) { e.Value = n; e.ParsingApplied = true; }
                }
            };
            g.DataError += (_, e) =>
            {
                MessageBox.Show("Ingrese un número entero.", "Dato inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.ThrowException = false;
                e.Cancel = false;
            };
        }

        void CambiarModo()
        {
            bool manual = rbManual.Checked;
            lblCantidad.Enabled = nudTuplas.Enabled = btnGenerarTabla.Enabled = manual;
            gridDatos.ReadOnly = !manual;
            // Asignar ReadOnly a la grilla desbloquea todas las columnas: se vuelven a bloquear las calculadas.
            foreach (int c in Calculadas) gridDatos.Columns[c].ReadOnly = true;

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
                gridDatos.Rows.Add(FilaCompleta(t));
        }

        static double Num(object? v) => double.TryParse(v?.ToString()?.Trim().TrimEnd('%'), out var d) ? d : double.NaN;

        // Valida que frutos comidos <= frutos iniciales al editar cualquiera de las dos casillas.
        void ValidarComidos(object? sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.ColumnIndex != 0 && e.ColumnIndex != 1) return;
            var fila = gridDatos.Rows[e.RowIndex];
            double nuevo = Num(e.FormattedValue);
            double ini = e.ColumnIndex == 0 ? nuevo : Num(fila.Cells[0].Value);
            double com = e.ColumnIndex == 1 ? nuevo : Num(fila.Cells[1].Value);
            if (!double.IsNaN(ini) && !double.IsNaN(com) && com > ini)
            {
                MessageBox.Show(MsgComidos, "Dato inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
                gridDatos.CancelEdit();
            }
        }

        // Calcula las columnas derivadas a partir de una tupla de entrada.
        static object[] FilaCompleta(MiTupla t)
        {
            double quedan = t.FrutoInicial - t.FrutoComidoArbol;
            double total = quedan * t.SemillasXArbol;
            double comidas = total * t.SemillasComidas / 100.0;
            double sQuedan = total - comidas;
            double germinan = sQuedan * t.SemillasGerminadas / 100.0;
            double plantulas = germinan * t.PlantulasSobreviven / 100.0;
            return new object[]
            {
                t.FrutoInicial, t.FrutoComidoArbol, quedan, t.SemillasXArbol, total,
                t.SemillasComidas, comidas, sQuedan,
                t.SemillasGerminadas, germinan, t.PlantulasSobreviven, plantulas
            };
        }

        void RecalcularFila(DataGridViewRow fila)
        {
            var v = Entradas.Select(i => Num(fila.Cells[i].Value)).ToArray();
            object?[] calc;
            if (v.Any(double.IsNaN)) calc = new object?[6];
            else
            {
                var f = FilaCompleta(new MiTupla
                {
                    FrutoInicial = (int)v[0], FrutoComidoArbol = (int)v[1], SemillasXArbol = (int)v[2],
                    SemillasComidas = (int)v[3], SemillasGerminadas = (int)v[4], PlantulasSobreviven = (int)v[5]
                });
                calc = Calculadas.OrderBy(i => i).Select(i => f[i]).ToArray();
            }
            int k = 0;
            foreach (int i in Calculadas.OrderBy(i => i)) fila.Cells[i].Value = calc[k++];
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
                    int col = Entradas[c];
                    var txt = fila.Cells[col].Value?.ToString()?.Trim();
                    if (!int.TryParse(txt, out v[c]))
                    {
                        gridDatos.CurrentCell = fila.Cells[col];
                        MessageBox.Show($"Casilla inválida o vacía: fila {fila.Index + 1}, \"{Columnas[col]}\".\nIngrese un número entero.",
                            "Datos incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return null;
                    }
                }
                if (v[1] > v[0])
                {
                    gridDatos.CurrentCell = fila.Cells[1];
                    MessageBox.Show(MsgComidos, "Dato inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null;
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
            var filas = new List<object?[]>();
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
                // Si "Frutos que quedan" es negativo, esa columna y todas las siguientes quedan en blanco.
                var f = FilaCompleta(m).Select(x => (object?)x).ToArray();
                if (m.FrutoComidoArbol > m.FrutoInicial)
                    for (int c = 2; c < f.Length; c++) f[c] = null;
                filas.Add(f);
                gridMuestras.Rows.Add(new object?[] { $"#{i}" }.Concat(f).ToArray()!);
            }

            gridMedidas.Rows.Clear();
            for (int c = 0; c < Columnas.Length; c++)
            {
                var datos = filas.Where(f => f[c] != null).Select(f => Convert.ToDouble(f[c]));
                var m = Program.CalcularMedidas(datos);
                if (m == null) gridMedidas.Rows.Add(Columnas[c], "N/A", "N/A", "N/A", "N/A", "N/A");
                else gridMedidas.Rows.Add(Columnas[c], m.Promedio.ToString("F2"), m.Varianza.ToString("F2"),
                    m.DesEstandar.ToString("F2"), m.ErrorEstandar.ToString("F4"), m.Rango);
            }
        }
    }
}
