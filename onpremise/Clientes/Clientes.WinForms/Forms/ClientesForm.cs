using Clientes.WinForms.Models;
using Clientes.WinForms.Services;

namespace Clientes.WinForms.Forms;

/// <summary>
/// Pantalla de gestión de clientes (ABC Clientes).
/// Layout construido programáticamente con un root TableLayoutPanel de 3 filas.
/// </summary>
public class ClientesForm : Form
{
    // -------- Paleta de colores --------
    private static readonly Color CHeaderBar  = Color.FromArgb(31,  31,  47);
    private static readonly Color CGold       = Color.FromArgb(212, 175, 55);
    private static readonly Color CFormBg     = Color.FromArgb(245, 245, 248);
    private static readonly Color CWhite      = Color.White;
    private static readonly Color CBtnDark    = Color.FromArgb(60,  60,  80);
    private static readonly Color CLabelColor = Color.FromArgb(80,  80, 100);
    private static readonly Color CGridHeader = Color.FromArgb(31,  31,  47);
    private static readonly Color CGridAlt    = Color.FromArgb(248, 248, 252);

    private enum Modo { Lectura, Nuevo, Edicion }

    private readonly ClienteApiService _api;

    // -------- Controles --------
    private TextBox _txtId = null!;
    private TextBox _txtNombre = null!;
    private TextBox _txtDireccion = null!;
    private TextBox _txtTelefono = null!;
    private TextBox _txtBuscar = null!;
    private DataGridView _grid = null!;

    private Button _btnNuevo = null!;
    private Button _btnBuscarModo = null!;
    private Button _btnEditar = null!;
    private Button _btnGrabar = null!;
    private Button _btnCancelar = null!;
    private Button _btnEliminar = null!;

    private Label _lblModo = null!;
    private Label _lblEstado = null!;
    private Label _lblInfoRegistro = null!;

    // Paginación
    private Button _btnPrimera = null!;
    private Button _btnAnterior = null!;
    private Button _btnSiguiente = null!;
    private Button _btnUltima = null!;
    private Label _lblPaginacion = null!;

    private Modo _modo = Modo.Lectura;
    private int _pagina = 1;
    private const int TAM_PAGINA = 10;
    private int _totalPaginas = 1;
    private string? _filtroActual;

    public ClientesForm(ClienteApiService api)
    {
        _api = api;
        BuildLayout();
    }

    private void BuildLayout()
    {
        SuspendLayout();

        this.Text = "ABC Clientes";
        this.BackColor = CFormBg;
        this.Font = new Font("Segoe UI", 9.5f);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.MinimumSize = new Size(1000, 680);
        this.WindowState = FormWindowState.Maximized;

        var rootForm = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = CFormBg };
        rootForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        rootForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 68f));  // header
        rootForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 54f));  // toolbar
        rootForm.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));  // content

        var header = BuildHeaderBar(); header.Dock = DockStyle.Fill; rootForm.Controls.Add(header, 0, 0);
        var toolbar = BuildToolbar();  toolbar.Dock = DockStyle.Fill; rootForm.Controls.Add(toolbar, 0, 1);
        var content = BuildContent();  content.Dock = DockStyle.Fill; rootForm.Controls.Add(content, 0, 2);

        Controls.Add(rootForm);

        ResumeLayout(false);

        this.Load += async (_, _) =>
        {
            SetModo(Modo.Lectura);
            await CargarPaginaAsync();
        };
    }

    // =========================================================
    // Row 0 — Header
    // =========================================================
    private Panel BuildHeaderBar()
    {
        var header = new Panel { BackColor = CHeaderBar, Dock = DockStyle.Fill };

        var avatar = new Panel
        {
            Size = new Size(40, 40),
            Location = new Point(16, 14),
            BackColor = CGold
        };
        avatar.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var b = new SolidBrush(CGold);
            e.Graphics.FillEllipse(b, 0, 0, 39, 39);
            using var f = new Font("Segoe UI", 14f, FontStyle.Bold);
            using var fb = new SolidBrush(CHeaderBar);
            var txt = "C";
            var sz = e.Graphics.MeasureString(txt, f);
            e.Graphics.DrawString(txt, f, fb, (40 - sz.Width) / 2, (40 - sz.Height) / 2);
        };

        var lblTitulo = new Label
        {
            Text = "ABC Clientes",
            ForeColor = CGold,
            Font = new Font("Segoe UI", 14f, FontStyle.Bold),
            Location = new Point(66, 12),
            AutoSize = true,
            BackColor = Color.Transparent
        };
        var lblSub = new Label
        {
            Text = "Gestión centralizada de clientes",
            ForeColor = Color.FromArgb(170, 170, 190),
            Font = new Font("Segoe UI", 8.5f),
            Location = new Point(68, 40),
            AutoSize = true,
            BackColor = Color.Transparent
        };

        var lblUsuario = new Label
        {
            Text = "Administrador",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            AutoSize = true,
            Anchor = AnchorStyles.Right,
            BackColor = Color.Transparent
        };
        header.Controls.Add(lblUsuario);
        header.Resize += (_, _) => lblUsuario.Location = new Point(header.Width - lblUsuario.Width - 20, 24);

        header.Controls.Add(avatar);
        header.Controls.Add(lblTitulo);
        header.Controls.Add(lblSub);
        return header;
    }

    // =========================================================
    // Row 1 — Toolbar
    // =========================================================
    private Panel BuildToolbar()
    {
        var toolbar = new Panel { BackColor = CWhite, Dock = DockStyle.Fill };

        _btnNuevo      = CrearBotonToolbar("＋ Nuevo", CBtnDark, 12);
        _btnBuscarModo = CrearBotonToolbar("🔍 Buscar", CBtnDark, 112);
        _btnEditar     = CrearBotonToolbar("✏ Editar", CBtnDark, 212);
        _btnGrabar     = CrearBotonToolbar("💾 Grabar", CGold, 312);
        _btnEliminar   = CrearBotonToolbar("🗑 Eliminar", Color.FromArgb(150, 40, 40), 412);
        _btnCancelar   = CrearBotonToolbar("✕ Cancelar", Color.FromArgb(180, 50, 50), 522);
        _btnGrabar.ForeColor = CHeaderBar;

        _btnNuevo.Click      += (_, _) => SetModo(Modo.Nuevo);
        _btnBuscarModo.Click += async (_, _) => { _txtBuscar.Focus(); await BuscarAsync(); };
        _btnEditar.Click     += (_, _) => { if (_txtId.Text.Length > 0) SetModo(Modo.Edicion); };
        _btnGrabar.Click     += async (_, _) => await GrabarAsync();
        _btnEliminar.Click   += async (_, _) => await EliminarAsync();
        _btnCancelar.Click   += async (_, _) => { SetModo(Modo.Lectura); await CargarPaginaAsync(); };

        _lblModo = new Label
        {
            Text = "MODO LECTURA",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = CLabelColor,
            AutoSize = true,
            Anchor = AnchorStyles.Right,
            BackColor = Color.Transparent
        };
        _lblEstado = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            AutoSize = true,
            Anchor = AnchorStyles.Right,
            BackColor = Color.Transparent
        };

        toolbar.Controls.Add(_lblModo);
        toolbar.Controls.Add(_lblEstado);
        toolbar.Resize += (_, _) =>
        {
            _lblEstado.Location = new Point(toolbar.Width - _lblEstado.Width - 20, 18);
            _lblModo.Location = new Point(_lblEstado.Left - _lblModo.Width - 24, 18);
        };

        toolbar.Controls.AddRange(new Control[] { _btnNuevo, _btnBuscarModo, _btnEditar, _btnGrabar, _btnEliminar, _btnCancelar });
        return toolbar;
    }

    private Button CrearBotonToolbar(string texto, Color color, int x)
    {
        return new Button
        {
            Text = texto,
            FlatStyle = FlatStyle.Flat,
            BackColor = color,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            Size = new Size(96, 34),
            Location = new Point(x, 10),
            FlatAppearance = { BorderSize = 0 },
            Cursor = Cursors.Hand
        };
    }

    // =========================================================
    // Row 2 — Content
    // =========================================================
    private TableLayoutPanel BuildContent()
    {
        var content = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6, BackColor = CFormBg };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 38f));   // título datos
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 200f));  // campos
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 38f));   // título grid
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 48f));   // búsqueda
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));   // grid
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 36f));   // paginación

        var tituloDatos = BuildTitleBar("DATOS GENERALES DEL CLIENTE", true);
        var campos = BuildMasterPanel();
        var tituloGrid = BuildTitleBar("LISTADO DE CLIENTES ACTIVOS", false);
        var busqueda = BuildSearchBar();
        var grid = BuildGrid();
        var paginacion = BuildPaginationBar();

        tituloDatos.Dock = DockStyle.Fill; content.Controls.Add(tituloDatos, 0, 0);
        campos.Dock = DockStyle.Fill;      content.Controls.Add(campos, 0, 1);
        tituloGrid.Dock = DockStyle.Fill;  content.Controls.Add(tituloGrid, 0, 2);
        busqueda.Dock = DockStyle.Fill;    content.Controls.Add(busqueda, 0, 3);
        grid.Dock = DockStyle.Fill;        content.Controls.Add(grid, 0, 4);
        paginacion.Dock = DockStyle.Fill;  content.Controls.Add(paginacion, 0, 5);

        return content;
    }

    private Panel BuildTitleBar(string texto, bool mostrarInfoRegistro)
    {
        var bar = new Panel { BackColor = CHeaderBar, Dock = DockStyle.Fill };
        bar.Paint += (s, e) =>
        {
            using var f = new Font("Segoe UI", 10f, FontStyle.Bold);
            using var b = new SolidBrush(CGold);
            e.Graphics.DrawString("▎ " + texto, f, b, 10, 9);
        };
        if (mostrarInfoRegistro)
        {
            _lblInfoRegistro = new Label
            {
                Text = "",
                ForeColor = Color.FromArgb(200, 200, 215),
                Font = new Font("Segoe UI", 8.5f),
                AutoSize = true,
                Anchor = AnchorStyles.Right,
                BackColor = Color.Transparent
            };
            bar.Controls.Add(_lblInfoRegistro);
            bar.Resize += (_, _) => _lblInfoRegistro.Location = new Point(bar.Width - _lblInfoRegistro.Width - 14, 11);
        }
        return bar;
    }

    private Panel BuildMasterPanel()
    {
        var wrapper = new Panel { Dock = DockStyle.Fill, BackColor = CWhite, Padding = new Padding(14, 8, 14, 8) };

        var fields = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 8, BackColor = CWhite };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        for (int i = 0; i < 8; i++)
            fields.RowStyles.Add(new RowStyle(SizeType.Absolute, i % 2 == 0 ? 18f : 28f));

        var lblId = CrearLabel("Id");
        _txtId = CrearTextBox();
        _txtId.ReadOnly = true;
        _txtId.BackColor = Color.FromArgb(235, 235, 240);
        _txtId.Width = 120;

        var lblNombre = CrearLabel("Nombre (*)");
        _txtNombre = CrearTextBox();
        _txtNombre.MaxLength = 100;

        var lblDireccion = CrearLabel("Dirección");
        _txtDireccion = CrearTextBox();
        _txtDireccion.MaxLength = 200;

        var lblTelefono = CrearLabel("Teléfono (solo dígitos)");
        _txtTelefono = CrearTextBox();
        _txtTelefono.MaxLength = 20;
        _txtTelefono.KeyPress += (s, e) =>
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        };

        foreach (var c in new Control[] { lblId, _txtId, lblNombre, _txtNombre, lblDireccion, _txtDireccion, lblTelefono, _txtTelefono })
            c.Dock = DockStyle.Fill;

        fields.Controls.Add(lblId, 0, 0);
        fields.Controls.Add(_txtId, 0, 1);
        fields.Controls.Add(lblNombre, 0, 2);
        fields.Controls.Add(_txtNombre, 0, 3);
        fields.Controls.Add(lblDireccion, 0, 4);
        fields.Controls.Add(_txtDireccion, 0, 5);
        fields.Controls.Add(lblTelefono, 0, 6);
        fields.Controls.Add(_txtTelefono, 0, 7);

        wrapper.Controls.Add(fields);
        return wrapper;
    }

    private Panel BuildSearchBar()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = CWhite, Padding = new Padding(14, 8, 14, 8) };

        var lbl = new Label
        {
            Text = "Buscar por Nombre o Teléfono:",
            ForeColor = CLabelColor,
            Font = new Font("Segoe UI", 9f),
            Location = new Point(14, 14),
            AutoSize = true
        };
        _txtBuscar = new TextBox
        {
            Location = new Point(210, 10),
            Width = 300,
            Font = new Font("Segoe UI", 9.5f)
        };
        _txtBuscar.KeyDown += async (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; await BuscarAsync(); }
        };
        var btn = new Button
        {
            Text = "Buscar",
            FlatStyle = FlatStyle.Flat,
            BackColor = CBtnDark,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            Size = new Size(90, 28),
            Location = new Point(520, 9),
            FlatAppearance = { BorderSize = 0 },
            Cursor = Cursors.Hand
        };
        btn.Click += async (_, _) => await BuscarAsync();

        panel.Controls.AddRange(new Control[] { lbl, _txtBuscar, btn });
        return panel;
    }

    private DataGridView BuildGrid()
    {
        _grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.None,
            BackgroundColor = Color.White,
            GridColor = Color.FromArgb(220, 220, 230),
            ColumnHeadersHeight = 36,
            EnableHeadersVisualStyles = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AllowUserToAddRows = false,
            AllowUserToResizeRows = false,
            ReadOnly = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false
        };
        _grid.RowTemplate.Height = 30;
        _grid.AlternatingRowsDefaultCellStyle.BackColor = CGridAlt;
        _grid.ColumnHeadersDefaultCellStyle.BackColor = CGridHeader;
        _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
        _grid.DefaultCellStyle.Font = new Font("Segoe UI", 9f);
        _grid.DefaultCellStyle.SelectionBackColor = CGold;
        _grid.DefaultCellStyle.SelectionForeColor = CHeaderBar;

        _grid.DataBindingComplete += (s, e) =>
        {
            if (_grid.Columns.Contains("Activo")) _grid.Columns["Activo"]!.Visible = false;
            if (_grid.Columns.Contains("FechaModificacion")) _grid.Columns["FechaModificacion"]!.Visible = false;
            if (_grid.Columns.Contains("Id")) _grid.Columns["Id"]!.FillWeight = 40;
            if (_grid.Columns.Contains("Nombre")) _grid.Columns["Nombre"]!.FillWeight = 160;
            if (_grid.Columns.Contains("Direccion")) { _grid.Columns["Direccion"]!.HeaderText = "Dirección"; _grid.Columns["Direccion"]!.FillWeight = 180; }
            if (_grid.Columns.Contains("Telefono")) { _grid.Columns["Telefono"]!.HeaderText = "Teléfono"; _grid.Columns["Telefono"]!.FillWeight = 90; }
            if (_grid.Columns.Contains("FechaCreacion")) _grid.Columns["FechaCreacion"]!.HeaderText = "Fecha Creación";
        };
        _grid.CellFormatting += (s, e) =>
        {
            if (_grid.Columns[e.ColumnIndex].Name == "FechaCreacion" && e.Value is DateTime dt)
                e.Value = dt.ToString("dd/MM/yyyy HH:mm");
        };
        _grid.SelectionChanged += (s, e) => MostrarSeleccion();

        return _grid;
    }

    private Panel BuildPaginationBar()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = CFormBg };

        _btnPrimera   = CrearBotonPag("◀◀", 14);
        _btnAnterior  = CrearBotonPag("◀", 54);
        _lblPaginacion = new Label
        {
            Text = "Página 1 de 1",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = CLabelColor,
            Location = new Point(98, 9),
            AutoSize = true
        };
        _btnSiguiente = CrearBotonPag("▶", 230);
        _btnUltima    = CrearBotonPag("▶▶", 270);

        _btnPrimera.Click   += async (_, _) => { _pagina = 1; await CargarPaginaAsync(); };
        _btnAnterior.Click  += async (_, _) => { if (_pagina > 1) { _pagina--; await CargarPaginaAsync(); } };
        _btnSiguiente.Click += async (_, _) => { if (_pagina < _totalPaginas) { _pagina++; await CargarPaginaAsync(); } };
        _btnUltima.Click    += async (_, _) => { _pagina = _totalPaginas; await CargarPaginaAsync(); };

        panel.Controls.AddRange(new Control[] { _btnPrimera, _btnAnterior, _lblPaginacion, _btnSiguiente, _btnUltima });
        return panel;
    }

    private Button CrearBotonPag(string texto, int x) => new()
    {
        Text = texto,
        FlatStyle = FlatStyle.Flat,
        BackColor = CBtnDark,
        ForeColor = Color.White,
        Font = new Font("Segoe UI", 8f, FontStyle.Bold),
        Size = new Size(34, 26),
        Location = new Point(x, 5),
        FlatAppearance = { BorderSize = 0 },
        Cursor = Cursors.Hand
    };

    private Label CrearLabel(string texto) => new()
    {
        Text = texto,
        ForeColor = CLabelColor,
        Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
        TextAlign = ContentAlignment.BottomLeft
    };

    private TextBox CrearTextBox() => new()
    {
        Font = new Font("Segoe UI", 9.5f),
        BorderStyle = BorderStyle.FixedSingle
    };

    // =========================================================
    // Estados / modos
    // =========================================================
    private void SetModo(Modo modo)
    {
        _modo = modo;
        bool editable = modo != Modo.Lectura;

        _txtNombre.ReadOnly = !editable;
        _txtDireccion.ReadOnly = !editable;
        _txtTelefono.ReadOnly = !editable;

        var editBg = editable ? Color.White : Color.FromArgb(248, 248, 250);
        _txtNombre.BackColor = editBg;
        _txtDireccion.BackColor = editBg;
        _txtTelefono.BackColor = editBg;

        _btnGrabar.Enabled = editable;
        _btnCancelar.Visible = editable;
        _btnNuevo.Enabled = !editable;
        _btnEditar.Enabled = !editable;
        _btnEliminar.Enabled = !editable;
        _btnBuscarModo.Enabled = !editable;
        _grid.Enabled = !editable;

        _lblModo.Text = modo switch
        {
            Modo.Nuevo   => "MODO NUEVO",
            Modo.Edicion => "MODO EDICIÓN",
            _            => "MODO LECTURA"
        };

        if (modo == Modo.Nuevo)
        {
            _txtId.Clear();
            _txtNombre.Clear();
            _txtDireccion.Clear();
            _txtTelefono.Clear();
            _lblInfoRegistro.Text = "";
            _txtNombre.Focus();
        }
    }

    private void MostrarSeleccion()
    {
        if (_modo != Modo.Lectura) return;
        if (_grid.CurrentRow?.DataBoundItem is not ClienteDto c) return;

        _txtId.Text = c.Id.ToString();
        _txtNombre.Text = c.Nombre;
        _txtDireccion.Text = c.Direccion ?? "";
        _txtTelefono.Text = c.Telefono ?? "";
        _lblEstado.Text = c.Activo ? "● ACTIVO" : "● INACTIVO";
        _lblEstado.ForeColor = c.Activo ? Color.FromArgb(40, 150, 60) : Color.FromArgb(180, 50, 50);
        _lblInfoRegistro.Text = $"ID: {c.Id}  |  Creado: {c.FechaCreacion:dd/MM/yyyy}";
    }

    // =========================================================
    // Operaciones
    // =========================================================
    private async Task CargarPaginaAsync()
    {
        try
        {
            var data = await _api.ListarAsync(_filtroActual, _pagina, TAM_PAGINA);
            _grid.DataSource = data.Items;
            _totalPaginas = Math.Max(1, data.TotalPaginas);
            _pagina = Math.Min(Math.Max(1, data.Pagina), _totalPaginas);
            _lblPaginacion.Text = $"Página {_pagina} de {_totalPaginas}   ({data.Total} registros)";

            _btnPrimera.Enabled = _btnAnterior.Enabled = _pagina > 1;
            _btnSiguiente.Enabled = _btnUltima.Enabled = _pagina < _totalPaginas;

            if (data.Items.Count > 0) MostrarSeleccion();
            else LimpiarCampos();
        }
        catch (Exception ex)
        {
            MostrarError(ex);
        }
    }

    private async Task BuscarAsync()
    {
        _filtroActual = string.IsNullOrWhiteSpace(_txtBuscar.Text) ? null : _txtBuscar.Text.Trim();
        _pagina = 1;
        await CargarPaginaAsync();
    }

    private async Task GrabarAsync()
    {
        var error = ValidarCampos();
        if (error is not null)
        {
            MessageBox.Show(error, "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var input = new ClienteInput
        {
            Nombre = _txtNombre.Text.Trim(),
            Direccion = string.IsNullOrWhiteSpace(_txtDireccion.Text) ? null : _txtDireccion.Text.Trim(),
            Telefono = string.IsNullOrWhiteSpace(_txtTelefono.Text) ? null : _txtTelefono.Text.Trim()
        };

        try
        {
            if (_modo == Modo.Nuevo)
            {
                var creado = await _api.CrearAsync(input);
                MessageBox.Show($"Cliente creado con Id {creado.Id}.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (_modo == Modo.Edicion)
            {
                var id = int.Parse(_txtId.Text);
                await _api.ActualizarAsync(id, input);
                MessageBox.Show("Cliente actualizado.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            SetModo(Modo.Lectura);
            await CargarPaginaAsync();
        }
        catch (Exception ex)
        {
            MostrarError(ex);
        }
    }

    private async Task EliminarAsync()
    {
        if (_txtId.Text.Length == 0)
        {
            MessageBox.Show("Seleccione un cliente para eliminar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var confirm = MessageBox.Show(
            $"¿Eliminar el cliente '{_txtNombre.Text}'? (se marcará como inactivo)",
            "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;

        try
        {
            await _api.EliminarAsync(int.Parse(_txtId.Text));
            MessageBox.Show("Cliente eliminado.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await CargarPaginaAsync();
        }
        catch (Exception ex)
        {
            MostrarError(ex);
        }
    }

    private string? ValidarCampos()
    {
        if (string.IsNullOrWhiteSpace(_txtNombre.Text))
            return "El nombre es obligatorio.";
        if (_txtNombre.Text.Trim().Length > 100)
            return "El nombre no puede superar los 100 caracteres.";
        if (_txtDireccion.Text.Length > 200)
            return "La dirección no puede superar los 200 caracteres.";
        if (_txtTelefono.Text.Length > 20)
            return "El teléfono no puede superar los 20 caracteres.";
        if (!string.IsNullOrEmpty(_txtTelefono.Text) && !_txtTelefono.Text.All(char.IsDigit))
            return "El teléfono solo puede contener dígitos.";
        return null;
    }

    private void LimpiarCampos()
    {
        _txtId.Clear();
        _txtNombre.Clear();
        _txtDireccion.Clear();
        _txtTelefono.Clear();
        _lblEstado.Text = "";
        _lblInfoRegistro.Text = "";
    }

    private static void MostrarError(Exception ex)
    {
        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
