namespace abcclientes.WinForms.Forms;

partial class CustomerForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        // ─── Search Panel ──────────────────────────────────────────
        var pnlSearch = new Panel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(8, 8, 8, 0) };
        var lblSearch = new Label { Text = "Buscar:", Location = new Point(8, 11), AutoSize = true };
        _txtSearch = new TextBox { Location = new Point(70, 8), Width = 250 };
        var btnSearch = new Button { Text = "Buscar", Location = new Point(330, 6), Width = 75, Height = 25 };
        var btnRefresh = new Button { Text = "Refrescar", Location = new Point(415, 6), Width = 80, Height = 25 };
        pnlSearch.Controls.AddRange(new Control[] { lblSearch, _txtSearch, btnSearch, btnRefresh });

        btnSearch.Click += BtnSearch_Click;
        btnRefresh.Click += BtnRefresh_Click;

        // ─── DataGridView ──────────────────────────────────────────
        _dgvCustomers = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };
        _dgvCustomers.CellClick += DgvCustomers_CellClick;

        // ─── Pagination Panel ───────────────────────────────────────
        var pnlPagination = new Panel { Dock = DockStyle.Bottom, Height = 40, Padding = new Padding(8, 8, 8, 0) };
        _btnFirst = new Button { Text = "<<", Location = new Point(8, 6), Width = 40, Height = 25 };
        _btnPrev = new Button { Text = "<", Location = new Point(55, 6), Width = 40, Height = 25 };
        _lblPageInfo = new Label { Location = new Point(105, 11), AutoSize = true, Text = "Pagina 1 de 1" };
        _btnNext = new Button { Text = ">", Location = new Point(220, 6), Width = 40, Height = 25 };
        _btnLast = new Button { Text = ">>", Location = new Point(267, 6), Width = 40, Height = 25 };
        pnlPagination.Controls.AddRange(new Control[] { _btnFirst, _btnPrev, _lblPageInfo, _btnNext, _btnLast });

        _btnFirst.Click += BtnFirst_Click;
        _btnPrev.Click += BtnPrev_Click;
        _btnNext.Click += BtnNext_Click;
        _btnLast.Click += BtnLast_Click;

        // ─── Form Fields Panel ──────────────────────────────────────
        var pnlFields = new Panel { Dock = DockStyle.Bottom, Height = 180, Padding = new Padding(8), BorderStyle = BorderStyle.FixedSingle };

        var grpFields = new GroupBox { Text = "Datos del Cliente", Dock = DockStyle.Top, Height = 110, Padding = new Padding(8, 8, 8, 0) };

        var lblNombre = new Label { Text = "Nombre *:", Location = new Point(10, 25), AutoSize = true };
        _txtNombre = new TextBox { Location = new Point(90, 22), Width = 250 };
        var lblDireccion = new Label { Text = "Direccion:", Location = new Point(10, 55), AutoSize = true };
        _txtDireccion = new TextBox { Location = new Point(90, 52), Width = 350 };
        var lblTelefono = new Label { Text = "Telefono:", Location = new Point(10, 85), AutoSize = true };
        _txtTelefono = new TextBox { Location = new Point(90, 82), Width = 150 };

        grpFields.Controls.AddRange(new Control[] { lblNombre, _txtNombre, lblDireccion, _txtDireccion, lblTelefono, _txtTelefono });

        var btnCreate = new Button { Text = "Crear", Location = new Point(8, 120), Width = 80, Height = 35, BackColor = Color.LightGreen };
        var btnUpdate = new Button { Text = "Actualizar", Location = new Point(98, 120), Width = 90, Height = 35, BackColor = Color.LightBlue };
        var btnDelete = new Button { Text = "Eliminar", Location = new Point(198, 120), Width = 80, Height = 35, BackColor = Color.Salmon };
        var btnClear = new Button { Text = "Limpiar", Location = new Point(288, 120), Width = 80, Height = 35 };

        btnCreate.Click += BtnCreate_Click;
        btnUpdate.Click += BtnUpdate_Click;
        btnDelete.Click += BtnDelete_Click;
        btnClear.Click += BtnClear_Click;

        pnlFields.Controls.AddRange(new Control[] { grpFields, btnCreate, btnUpdate, btnDelete, btnClear });

        // ─── Form Assembly ──────────────────────────────────────────
        SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(_dgvCustomers)).BeginInit();

        Text = "ABC Clientes - Gestion de Clientes";
        ClientSize = new Size(900, 600);
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(800, 500);

        Controls.Add(_dgvCustomers);
        Controls.Add(pnlFields);
        Controls.Add(pnlPagination);
        Controls.Add(pnlSearch);

        ((System.ComponentModel.ISupportInitialize)(_dgvCustomers)).EndInit();
        ResumeLayout(false);
    }
}
