using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp2.Clases;
using WindowsFormsApp2.Modelos;

namespace WindowsFormsApp2
{
    public partial class frmCronograma : Form
    {
        private DataClasses3DataContext dataContext;
        public frmCronograma()
        {
            InitializeComponent();
            dataContext = new DataClasses3DataContext();

            // Debug: Verificar conexión y datos
            VerificarDatos();

            CargarEmpresas();
        }

        private void VerificarDatos()
        {
            try
            {
                // Verificar usuario logueado
                MessageBox.Show($"Usuario logueado ID: {Sesion.UsuarioId}", "Debug");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en verificación: {ex.Message}", "Error Debug");
            }
        }

        private void CargarEmpresas()
        {
            try
            {
                // Verificar si hay datos en la sesión
                if (Sesion.UsuarioId <= 0)
                {
                    MessageBox.Show("No hay usuario logueado.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Consultar empresas del usuario logueado
                var empresas = (from e in dataContext.Empresa
                                where e.usuario_id == Sesion.UsuarioId
                                select new
                                {
                                    ID = e.id,
                                    Nombre = e.nombre
                                }).ToList();

                if (empresas.Count == 0)
                {
                    MessageBox.Show("No se encontraron empresas para este usuario.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Agregar opción por defecto
                cmbEmpresas.Items.Clear();
                cmbEmpresas.Items.Add(new { ID = 0, Nombre = "Seleccione una empresa..." });

                foreach (var empresa in empresas)
                {
                    cmbEmpresas.Items.Add(empresa);
                }

                cmbEmpresas.DisplayMember = "Nombre";
                cmbEmpresas.ValueMember = "ID";
                cmbEmpresas.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar empresas: {ex.Message}\n\nDetalles: {ex.InnerException?.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnGenerar_Click(object sender, EventArgs e)
        {
            if (cmbEmpresas.SelectedItem == null || cmbEmpresas.SelectedIndex == 0)
            {
                MessageBox.Show("Por favor, seleccione una empresa.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Obtener el ID de la empresa seleccionada
            dynamic empresaSeleccionada = cmbEmpresas.SelectedItem;
            int empresaId = empresaSeleccionada.ID;

            GenerarCronograma(empresaId);
        }

        private void GenerarCronograma(int empresaId)
        {
            try
            {
                // Limpiar panel si existe contenido previo
                if (panelCronograma != null)
                {
                    panelCronograma.Controls.Clear();
                }

                // Obtener el progreso de la empresa
                var progreso = ObtenerProgresoEmpresa(empresaId);

                // Dibujar el cronograma
                DibujarCronograma(progreso);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar cronograma: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Dictionary<string, bool> ObtenerProgresoEmpresa(int empresaId)
        {
            var progreso = new Dictionary<string, bool>();

            try
            {
                // 1. Misión
                var mision = dataContext.Mision.Any(m => m.empresa_id == empresaId);
                progreso.Add("1. Misión", mision);

                // 2. Visión
                var vision = dataContext.Vision.Any(v => v.empresa_id == empresaId);
                progreso.Add("2. Visión", vision);

                // 3. Valores
                var valores = dataContext.Valores.Any(v => v.empresa_id == empresaId);
                progreso.Add("3. Valores", valores);

                // 4. Objetivos
                var objetivos = dataContext.ObjetivoG.Any(o => o.empresa_id == empresaId);
                progreso.Add("4. Objetivos", objetivos);

                // 5. Análisis FODA
                progreso.Add("5. Análisis FODA", false);

                // 6. Cadena de Valor
                progreso.Add("6. Cadena de Valor", false);

                // 7. Matriz de Participación (No implementado)
                progreso.Add("7. Matriz de Participación", false);

                // 8. Las 5 Fuerzas de Porter (No implementado)
                progreso.Add("8. Las 5 Fuerzas de Porter", false);

                // 9. PEST (No implementado)
                progreso.Add("9. PEST", false);

                // 10. Identificación Estrategia (No implementado)
                progreso.Add("10. Identificación Estrategia", false);

                // 11. Matriz CAME
                var matrizCAME = dataContext.MatrizCAME.Any(m => m.empresa_id == empresaId);
                progreso.Add("11. Matriz CAME", matrizCAME);

                return progreso;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener progreso: {ex.Message}", "Error");
                return new Dictionary<string, bool>();
            }
        }

        private void DibujarCronograma(Dictionary<string, bool> progreso)
        {
            // Limpiar el panel
            panelCronograma.Controls.Clear();
            panelCronograma.Invalidate();

            int inicioX = 20;
            int inicioY = 20;
            int anchoEtapa = 180;
            int altoEtapa = 60;
            int espacioEntre = 40;
            int columnas = 3;
            int filas = 4;

            var etapas = progreso.Keys.ToList();

            for (int i = 0; i < etapas.Count; i++)
            {
                int fila = i / columnas;
                int columna = i % columnas;

                int x = inicioX + (columna * (anchoEtapa + espacioEntre));
                int y = inicioY + (fila * (altoEtapa + espacioEntre + 20));

                // Crear panel para cada etapa
                Panel etapaPanel = new Panel
                {
                    Location = new Point(x, y),
                    Size = new Size(anchoEtapa, altoEtapa),
                    BorderStyle = BorderStyle.FixedSingle
                };

                // Determinar color según el estado
                Color colorFondo;
                Color colorTexto = Color.White;

                if (progreso[etapas[i]])
                {
                    colorFondo = Color.FromArgb(46, 204, 113); // Verde - Completado
                }
                else if (EsEtapaNoImplementada(etapas[i]))
                {
                    colorFondo = Color.FromArgb(149, 165, 166); // Gris - No implementado
                }
                else
                {
                    colorFondo = Color.FromArgb(231, 76, 60); // Rojo - Pendiente
                }

                etapaPanel.BackColor = colorFondo;

                // Añadir texto de la etapa
                Label lblEtapa = new Label
                {
                    Text = etapas[i],
                    ForeColor = colorTexto,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill
                };

                etapaPanel.Controls.Add(lblEtapa);

                // Añadir icono de estado
                Label lblIcono = new Label
                {
                    Size = new Size(20, 20),
                    Location = new Point(anchoEtapa - 25, 5),
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = colorTexto,
                    TextAlign = ContentAlignment.MiddleCenter
                };

                if (progreso[etapas[i]])
                {
                    lblIcono.Text = "✓";
                }
                else if (EsEtapaNoImplementada(etapas[i]))
                {
                    lblIcono.Text = "⚠";
                }
                else
                {
                    lblIcono.Text = "✗";
                }

                etapaPanel.Controls.Add(lblIcono);

                // Dibujar flecha si no es la última etapa
                if (i < etapas.Count - 1)
                {
                    DibujarFlecha(x + anchoEtapa, y + altoEtapa / 2, columna == columnas - 1);
                }

                panelCronograma.Controls.Add(etapaPanel);
            }

            // Añadir leyenda
            AñadirLeyenda();
        }

        private bool EsEtapaNoImplementada(string etapa)
        {
            var etapasNoImplementadas = new List<string>
            {
                "7. Matriz de Participación",
                "8. Las 5 Fuerzas de Porter",
                "9. PEST",
                "10. Identificación Estrategia"
            };

            return etapasNoImplementadas.Contains(etapa);
        }

        private void DibujarFlecha(int x, int y, bool esFinDeLinea)
        {
            Panel flechaPanel = new Panel
            {
                Location = new Point(x + 5, y - 10),
                Size = new Size(30, 20),
                BackColor = Color.Transparent
            };

            flechaPanel.Paint += (sender, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                Pen pen = new Pen(Color.FromArgb(52, 73, 94), 3);

                if (esFinDeLinea)
                {
                    // Flecha hacia abajo para cambio de línea
                    g.DrawLine(pen, 15, 0, 15, 15);
                    g.DrawLine(pen, 10, 10, 15, 15);
                    g.DrawLine(pen, 20, 10, 15, 15);
                }
                else
                {
                    // Flecha horizontal
                    g.DrawLine(pen, 0, 10, 20, 10);
                    g.DrawLine(pen, 15, 5, 20, 10);
                    g.DrawLine(pen, 15, 15, 20, 10);
                }
            };

            panelCronograma.Controls.Add(flechaPanel);
        }

        private void AñadirLeyenda()
        {
            int yLeyenda = 300;
            int xLeyenda = 20;

            // Título de leyenda
            Label lblTituloLeyenda = new Label
            {
                Text = "Leyenda:",
                Location = new Point(xLeyenda, yLeyenda),
                Size = new Size(100, 20),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80)
            };
            panelCronograma.Controls.Add(lblTituloLeyenda);

            // Completado
            Panel completadoPanel = new Panel
            {
                Location = new Point(xLeyenda, yLeyenda + 25),
                Size = new Size(20, 20),
                BackColor = Color.FromArgb(46, 204, 113),
                BorderStyle = BorderStyle.FixedSingle
            };
            panelCronograma.Controls.Add(completadoPanel);

            Label lblCompletado = new Label
            {
                Text = "Completado",
                Location = new Point(xLeyenda + 30, yLeyenda + 25),
                Size = new Size(100, 20),
                ForeColor = Color.FromArgb(44, 62, 80)
            };
            panelCronograma.Controls.Add(lblCompletado);

            // Pendiente
            Panel pendientePanel = new Panel
            {
                Location = new Point(xLeyenda + 150, yLeyenda + 25),
                Size = new Size(20, 20),
                BackColor = Color.FromArgb(231, 76, 60),
                BorderStyle = BorderStyle.FixedSingle
            };
            panelCronograma.Controls.Add(pendientePanel);

            Label lblPendiente = new Label
            {
                Text = "Pendiente",
                Location = new Point(xLeyenda + 180, yLeyenda + 25),
                Size = new Size(100, 20),
                ForeColor = Color.FromArgb(44, 62, 80)
            };
            panelCronograma.Controls.Add(lblPendiente);

            // No implementado
            Panel noImplementadoPanel = new Panel
            {
                Location = new Point(xLeyenda + 290, yLeyenda + 25),
                Size = new Size(20, 20),
                BackColor = Color.FromArgb(149, 165, 166),
                BorderStyle = BorderStyle.FixedSingle
            };
            panelCronograma.Controls.Add(noImplementadoPanel);

            Label lblNoImplementado = new Label
            {
                Text = "No Implementado",
                Location = new Point(xLeyenda + 320, yLeyenda + 25),
                Size = new Size(120, 20),
                ForeColor = Color.FromArgb(44, 62, 80)
            };
            panelCronograma.Controls.Add(lblNoImplementado);
        }

        private void frmCronograma_Load(object sender, EventArgs e)
        {
            // Inicializar el panel del cronograma si no existe
            if (panelCronograma == null)
            {
                // El panel se debe crear en el diseñador
                // panelCronograma = this.panelCronograma;
            }
        }

        private void btnGenerar_Click_1(object sender, EventArgs e)
        {
            if (cmbEmpresas.SelectedItem == null || cmbEmpresas.SelectedIndex == 0)
            {
                MessageBox.Show("Por favor, seleccione una empresa.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Obtener el ID de la empresa seleccionada
            dynamic empresaSeleccionada = cmbEmpresas.SelectedItem;
            int empresaId = empresaSeleccionada.ID;

            GenerarCronograma(empresaId);
        }
    }
}