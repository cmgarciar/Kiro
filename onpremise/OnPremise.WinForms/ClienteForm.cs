using System.Text.RegularExpressions;
using OnPremise.WinForms.Models;
using OnPremise.WinForms.Services;

namespace OnPremise.WinForms;

/// <summary>
/// Pantalla de gestión de Clientes (HU1). Layout profesional según el estándar:
/// root TableLayoutPanel de 3 filas (header / toolbar / contenido) para evitar solapes.
/// </summary>
public class ClienteForm : Form
{
    // ---- Paleta de color ----
    private static readonly Color CHeaderBar  = Color.FromArgb(31,  31,  47);
    private static readonly Color CGold       = Color.FromArgb(212, 175, 55);
    private static readonly Color CFormBg     = Color.FromArgb(245, 245, 248);
    private static readonly Color CWhite      = Color.White;
    private static readonly Color CBtnDark    = Color.FromArgb(60,  60,  80);
    private static readonly Color CDanger     = Color.FromArgb(192, 52,  74);
    private static readonly Color CLabelColor = Color.FromArgb(80,  80, 100);
    private static readonly Color CGridHeader = Color.FromArgb(31,  31,  47);
    private static readonly Color CGridAlt    = Color.FromArgb(248, 248, 252);

    private enum FormMode { Read, New, Edit }

    private readonly ClienteApiService _api;

    // Controles maestros
    private TextBox _txtId = null!;
    private TextBox _txtNombre = null!;
    private TextBox _txtDireccion = null!;
    private TextBox _txtTelefono = null!;

    // Toolbar
    private Button _btnNuevo = null!;
    private Button _btnBuscarToolbar = null!;
    private Button _btnEditar = null!;
    private Button _btnGrabar = null!;
    private Button _btnCancelar = null!;
    private Label _lblModo = null!;
    private Label _lblEstado = null!;

    // Búsqueda y grid
    private TextBox _txtSearch = null!;
    private DataGridView _dgv = null!;

    // Paginación
    private Label _lblPagina = null!;
    private Button _btnPrimero = null!;
    private Button _btnAnterior = null!;
    private Button _btnSiguiente = null!;
    private Button _btnUltimo = null!;

    private FormMode _mode = FormMode.Read;
    private int _pageNumber = 1;
    private const int PageSize = 10;
    private int _totalPages = 1;
    private int _totalCount = 0;
    private int? _selectedId;

    public ClienteForm(ClienteApiService api)
    {
        _api = api;
        BuildLayout();
    }

    private void BuildLayout()
    {
        SuspendLayout();

        BackColor = CFormBg;
        Font = new Font("Segoe UI", 9.5f);
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1000, 680);
        WindowState = FormWindowState.Maximized;
        Text = "ABC Clientes";

        var rootForm = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = CFormBg };
        rootForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        rootForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 68f));
        rootForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 54f));
        rootForm.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        var header = BuildHeaderBar(); header.Dock = DockStyle.Fill; rootForm.Controls.Add(header, 0, 0);
        var toolbar = BuildToolbar(); toolbar.Dock = DockStyle.Fill; rootForm.Controls.Add(toolbar, 0, 1);
        var content = BuildContent(); content.Dock = DockStyle.Fill; rootForm.Controls.Add(content, 0, 2);

        Controls.Add(rootForm);

        ResumeLayout(false);

        Load += async (_, _) => await LoadDataAsync();
        SetMode(FormMode.Read);
    }

    // ---- Row 0: Header ----
    private Panel BuildHeaderBar()
    {
        var header = new Panel { BackColor = CHeaderBar, Dock = DockStyle.Fill };

        var avatar = new Panel { Size = new Size(40, 40), Location = new Point(16, 14), BackColor = CGold };
        avatar.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var brush = new SolidBrush(CGold);
            e.Graphics.FillEllipse(brush, 0, 0, 39, 39);
            using var f = new Font("Segoe UI", 14f, FontStyle.Bold);
            TextRenderer.DrawText(e.Graphics, "C", f, new Rectangle(0, 0, 40, 40), CHeaderBar,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        };

        var lblTitle = new Label
        {
            Text = "ABC Clientes",
            ForeColor = CGold,
            Font = new Font("Segoe UI", 14f, FontStyle.Bold),
            Location = new Point(66, 10),
            AutoSize = true,
            BackColor = Color.Transparent
        };
        var lblSub = new Label
        {
            Text = "Gestión centralizada de clientes",
            ForeColor = Color.FromArgb(170, 170, 185),
            Font = new Font("Segoe UI", 8.5f),
            Location = new Point(68, 38),
            AutoSize = true,
            BackColor = Color.Transparent
        };

        var lblUser = new Label
        {
            Text = "Administrador",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            AutoSize = true,
            Anchor = AnchorStyles.Right,
            BackColor = Color.Transparent
        };
        header.Controls.Add(lblUser);
        header.Resize += (_, _) =>
        {
            lblUser.Location = new Point(header.Width - lblUser.Width - 24, 24);
        };

        header.Controls.Add(avatar);
        header.Controls.Add(lblTitle);
        header.Controls.Add(lblSub);
        return header;
    }

    // ---- Row 1: Toolbar ----
    private Panel BuildToolbar()
    {
        var toolbar = new Panel { BackColor = CWhite, Dock = DockStyle.Fill };

        _btnNuevo = MakeButton("＋ Nuevo", CBtnDark, Color.White);
        _btnBuscarToolbar = MakeButton("🔍 Buscar", CBtnDark, Color.White);
        _btnEditar = MakeButton("✏ Editar", CBtnDark, Color.White);
        _btnGrabar = MakeButton("💾 Grabar", CGold, CHeaderBar);
        _btnCancelar = MakeButton("✕ Cancelar", CDanger, Color.White);

        _btnNuevo.Location = new Point(14, 10);
        _btnBuscarToolbar.Location = new Point(126, 10);
        _btnEditar.Location = new Point(238, 10);
        _btnGrabar.Location = new Point(350, 10);
        _btnCancelar.Location = new Point(462, 10);

        _btnNuevo.Click += (_, _) => SetMode(FormMode.New);
        _btnEditar.Click += (_, _) => OnEditarClick();
        _btnGrabar.Click += async (_, _) => await OnGrabarClick();
        _btnCancelar.Click += (_, _) => SetMode(FormMode.Read);
        _btnBuscarToolbar.Click += (_, _) => _txtSearch?.Focus();

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
            Text = "● ACTIVO",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = Color.ForestGreen,
            AutoSize = true,
            Anchor = AnchorStyles.Right,
            BackColor = Color.Transparent,
            Visible = false
        };

        toolbar.Controls.AddRange(new Control[]
        {
            _btnNuevo, _btnBuscarToolbar, _btnEditar, _btnGrabar, _btnCancelar, _lblModo, _lblEstado
        });

        toolbar.Resize += (_, _) =>
        {
            _lblModo.Location = new Point(toolbar.Width - _lblModo.Width - 24, 10);
            _lblEstado.Location = new Point(toolbar.Width - _lblEstado.Width - 24, 30);
        };

        return toolbar;
    }

    // ---- Row 2: Content ----
    private TableLayoutPanel BuildContent()
    {
        var content = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6, BackColor = CFormBg };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 38f));   // título datos
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 210f));  // campos
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 38f));   // título listado
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 48f));   // búsqueda
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));   // grid
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));   // paginación

        content.Controls.Add(MakeSectionTitle("DATOS GENERALES DEL CLIENTE"), 0, 0);
        content.Controls.Add(BuildMasterFields(), 0, 1);
        content.Controls.Add(MakeSectionTitle("LISTADO DE CLIENTES"), 0, 2);
        content.Controls.Add(BuildSearchBar(), 0, 3);
        content.Controls.Add(BuildGrid(), 0, 4);
        content.Controls.Add(BuildPagination(), 0, 5);

        return content;
    }

    private Panel MakeSectionTitle(string text)
    {
        var panel = new Panel { BackColor = CHeaderBar, Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 2) };
        panel.Paint += (s, e) =>
        {
            using var f = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            TextRenderer.DrawText(e.Graphics, "  " + text, f, new Rectangle(8, 0, panel.Width - 16, panel.Height),
                CGold, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
        };
        return panel;
    }

    private Panel BuildMasterFields()
    {
        var container = new Panel { Dock = DockStyle.Fill, BackColor = CWhite, Padding = new Padding(16, 10, 16, 10) };

        var fields = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 1, RowCount = 8, BackColor = CWhite, Width = 520 };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 500f));
        for (int i = 0; i < 8; i++)
            fields.RowStyles.Add(new RowStyle(SizeType.Absolute, i % 2 == 0 ? 18f : 30f));

        _txtId = MakeTextBox(); _txtId.ReadOnly = true; _txtId.BackColor = CFormBg;
        _txtNombre = MakeTextBox(); _txtNombre.MaxLength = 100;
        _txtDireccion = MakeTextBox(); _txtDireccion.MaxLength = 200;
        _txtTelefono = MakeTextBox(); _txtTelefono.MaxLength = 20;
        _txtTelefono.KeyPress += (s, e) =>
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true; // CA5: solo dígitos
        };

        fields.Controls.Add(MakeLabel("Id"), 0, 0);
        fields.Controls.Add(_txtId, 0, 1);
        fields.Controls.Add(MakeLabel("Nombre *"), 0, 2);
        fields.Controls.Add(_txtNombre, 0, 3);
        fields.Controls.Add(MakeLabel("Dirección"), 0, 4);
        fields.Controls.Add(_txtDireccion, 0, 5);
        fields.Controls.Add(MakeLabel("Teléfono"), 0, 6);
        fields.Controls.Add(_txtTelefono, 0, 7);

        foreach (Control c in fields.Controls) c.Dock = DockStyle.Fill;

        container.Controls.Add(fields);
        return container;
    }

    private Panel BuildSearchBar()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = CWhite, Padding = new Padding(16, 8, 16, 8) };

        _txtSearch = new TextBox
        {
            Location = new Point(16, 10),
            Width = 320,
            Font = new Font("Segoe UI", 10f)
        };
        _txtSearch.AccessibleName = "Buscar por nombre o teléfono";
        _txtSearch.PlaceholderText = "Buscar por nombre o teléfono...";
        _txtSearch.KeyDown += async (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; _pageNumber = 1; await LoadDataAsync(); }
        };

        var btnBuscar = MakeButton("🔍 Buscar", CBtnDark, Color.White);
        btnBuscar.Location = new Point(344, 7);
        btnBuscar.Click += async (_, _) => { _pageNumber = 1; await LoadDataAsync(); };

        var btnEliminar = MakeButton("✕ Eliminar", CDanger, Color.White);
        btnEliminar.Location = new Point(456, 7);
        btnEliminar.Click += async (_, _) => await OnEliminarClick();

        panel.Controls.AddRange(new Control[] { _txtSearch, btnBuscar, btnEliminar });
        return panel;
    }

    private DataGridView BuildGrid()
    {
        _dgv = new DataGridView
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
            ReadOnly = true,
            AutoGenerateColumns = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false
        };
        _dgv.RowTemplate.Height = 30;
        _dgv.AlternatingRowsDefaultCellStyle.BackColor = CGridAlt;
        _dgv.ColumnHeadersDefaultCellStyle.BackColor = CGridHeader;
        _dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        _dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
        _dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9f);

        _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", DataPropertyName = "Id", HeaderText = "ID", FillWeight = 10 });
        _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nombre", DataPropertyName = "Nombre", HeaderText = "NOMBRE", FillWeight = 35 });
        _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Direccion", DataPropertyName = "Direccion", HeaderText = "DIRECCIÓN", FillWeight = 35 });
        _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Telefono", DataPropertyName = "Telefono", HeaderText = "TELÉFONO", FillWeight = 20 });
        _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "FechaCreacion", DataPropertyName = "FechaCreacion", HeaderText = "CREADO", FillWeight = 20 });

        _dgv.CellFormatting += (s, e) =>
        {
            if (_dgv.Columns[e.ColumnIndex].Name == "FechaCreacion" && e.Value is DateTime dt)
                e.Value = dt.ToString("dd/MM/yyyy HH:mm");
        };

        _dgv.SelectionChanged += (_, _) => OnGridSelectionChanged();

        return _dgv;
    }

    private Panel BuildPagination()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = CWhite };

        _btnPrimero = MakeButton("◀◀", CHeaderBar, Color.White); _btnPrimero.Width = 50;
        _btnAnterior = MakeButton("◀", CHeaderBar, Color.White); _btnAnterior.Width = 50;
        _btnSiguiente = MakeButton("▶", CHeaderBar, Color.White); _btnSiguiente.Width = 50;
        _btnUltimo = MakeButton("▶▶", CHeaderBar, Color.White); _btnUltimo.Width = 50;

        _lblPagina = new Label
        {
            Text = "Página 1 de 1 (0 registros)",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = CLabelColor,
            AutoSize = true,
            BackColor = Color.Transparent
        };

        _btnPrimero.Click += async (_, _) => { _pageNumber = 1; await LoadDataAsync(); };
        _btnAnterior.Click += async (_, _) => { if (_pageNumber > 1) { _pageNumber--; await LoadDataAsync(); } };
        _btnSiguiente.Click += async (_, _) => { if (_pageNumber < _totalPages) { _pageNumber++; await LoadDataAsync(); } };
        _btnUltimo.Click += async (_, _) => { _pageNumber = _totalPages; await LoadDataAsync(); };

        panel.Controls.AddRange(new Control[] { _btnPrimero, _btnAnterior, _lblPagina, _btnSiguiente, _btnUltimo });

        panel.Resize += (_, _) =>
        {
            int cy = 4;
            int center = panel.Width / 2;
            _lblPagina.Location = new Point(center - _lblPagina.Width / 2, cy + 4);
            _btnAnterior.Location = new Point(_lblPagina.Left - _btnAnterior.Width - 8, cy);
            _btnPrimero.Location = new Point(_btnAnterior.Left - _btnPrimero.Width - 4, cy);
            _btnSiguiente.Location = new Point(_lblPagina.Right + 8, cy);
            _btnUltimo.Location = new Point(_btnSiguiente.Right + 4, cy);
        };

        return panel;
    }

    // ---- Helpers de UI ----
    private static Button MakeButton(string text, Color back, Color fore)
    {
        var btn = new Button
        {
            Text = text,
            BackColor = back,
            ForeColor = fore,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            Size = new Size(104, 34),
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }

    private static Label MakeLabel(string text) => new()
    {
        Text = text,
        ForeColor = CLabelColor,
        Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
        AutoSize = false
    };

    private static TextBox MakeTextBox() => new()
    {
        Font = new Font("Segoe UI", 10f),
        BorderStyle = BorderStyle.FixedSingle
    };

    // ---- Lógica de modos ----
    private void SetMode(FormMode mode)
    {
        _mode = mode;
        bool editable = mode != FormMode.Read;

        _txtNombre.ReadOnly = !editable;
        _txtDireccion.ReadOnly = !editable;
        _txtTelefono.ReadOnly = !editable;
        _txtNombre.BackColor = editable ? Color.White : CFormBg;
        _txtDireccion.BackColor = editable ? Color.White : CFormBg;
        _txtTelefono.BackColor = editable ? Color.White : CFormBg;

        _btnGrabar.Enabled = editable;
        _btnCancelar.Visible = editable;
        _btnEditar.Enabled = mode == FormMode.Read && _selectedId.HasValue;

        _lblModo.Text = mode switch
        {
            FormMode.New => "MODO NUEVO",
            FormMode.Edit => "MODO EDICIÓN",
            _ => "MODO LECTURA"
        };

        if (mode == FormMode.New)
        {
            _selectedId = null;
            _txtId.Clear();
            _txtNombre.Clear();
            _txtDireccion.Clear();
            _txtTelefono.Clear();
            _lblEstado.Visible = false;
            _txtNombre.Focus();
        }
    }

    private void OnEditarClick()
    {
        if (!_selectedId.HasValue)
        {
            MessageBox.Show("Seleccione un cliente del listado para editar.", "Editar",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        SetMode(FormMode.Edit);
    }

    private void OnGridSelectionChanged()
    {
        if (_mode != FormMode.Read) return;
        if (_dgv.CurrentRow?.DataBoundItem is ClienteDto dto)
        {
            _selectedId = dto.Id;
            _txtId.Text = dto.Id.ToString();
            _txtNombre.Text = dto.Nombre;
            _txtDireccion.Text = dto.Direccion ?? string.Empty;
            _txtTelefono.Text = dto.Telefono ?? string.Empty;
            _lblEstado.Visible = true;
            _lblEstado.Text = dto.Activo ? "● ACTIVO" : "● INACTIVO";
            _lblEstado.ForeColor = dto.Activo ? Color.ForestGreen : CDanger;
            _btnEditar.Enabled = true;
        }
    }

    // ---- Acciones CRUD ----
    private async Task OnGrabarClick()
    {
        var nombre = _txtNombre.Text.Trim();
        var direccion = _txtDireccion.Text.Trim();
        var telefono = _txtTelefono.Text.Trim();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            MessageBox.Show("El nombre es obligatorio.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtNombre.Focus();
            return;
        }
        if (!string.IsNullOrEmpty(telefono) && !Regex.IsMatch(telefono, "^[0-9]+$"))
        {
            MessageBox.Show("El teléfono solo puede contener dígitos.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtTelefono.Focus();
            return;
        }

        try
        {
            if (_mode == FormMode.New)
            {
                await _api.CreateAsync(new CreateClienteRequest
                {
                    Nombre = nombre,
                    Direccion = string.IsNullOrEmpty(direccion) ? null : direccion,
                    Telefono = string.IsNullOrEmpty(telefono) ? null : telefono
                });
                MessageBox.Show("Cliente creado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (_mode == FormMode.Edit && _selectedId.HasValue)
            {
                await _api.UpdateAsync(_selectedId.Value, new UpdateClienteRequest
                {
                    Nombre = nombre,
                    Direccion = string.IsNullOrEmpty(direccion) ? null : direccion,
                    Telefono = string.IsNullOrEmpty(telefono) ? null : telefono
                });
                MessageBox.Show("Cliente actualizado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            SetMode(FormMode.Read);
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    private async Task OnEliminarClick()
    {
        if (!_selectedId.HasValue)
        {
            MessageBox.Show("Seleccione un cliente del listado para eliminar.", "Eliminar",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var confirm = MessageBox.Show($"¿Eliminar el cliente Id {_selectedId}? (se marcará como inactivo)",
            "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;

        try
        {
            await _api.DeleteAsync(_selectedId.Value);
            _selectedId = null;
            SetMode(FormMode.Read);
            await LoadDataAsync();
            MessageBox.Show("Cliente eliminado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var search = _txtSearch?.Text;
            var result = await _api.GetAllAsync(_pageNumber, PageSize, search);

            _totalCount = result.TotalCount;
            _totalPages = Math.Max(1, result.TotalPages);
            if (_pageNumber > _totalPages) _pageNumber = _totalPages;

            _dgv.DataSource = result.Items;
            _lblPagina.Text = $"Página {_pageNumber} de {_totalPages} ({_totalCount} registros)";

            _btnPrimero.Enabled = _btnAnterior.Enabled = _pageNumber > 1;
            _btnSiguiente.Enabled = _btnUltimo.Enabled = _pageNumber < _totalPages;

            _dgv.ClearSelection();
            _selectedId = null;
            _btnEditar.Enabled = false;
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    private static void ShowError(Exception ex)
    {
        var msg = ex is ApiException ? ex.Message : $"No se pudo conectar con el servicio.\n\n{ex.Message}";
        MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
