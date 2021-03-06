using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;


namespace DALGen_Beta
{
    public partial class MainForm : Form
    {
        public List<SQLAttributePicker> attributeList = new List<SQLAttributePicker>();

        public MainForm()
        {
            InitializeComponent();            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeForm();
        }

        private void InitializeForm()
        {
            chkCPP.Checked = false;
            chkCSharp.Checked = false;
            chkJava.Checked = false;
            chkPython.Checked = false;
            chkPHP.Checked = false;
            radTSQL.Checked = false;
            radMySQL.Checked = false;
            radOracle.Checked = false;

            txtSchemaName.Text = String.Empty;
            txtEntityName.Text = String.Empty;
            txtNamespace.Text = String.Empty;
            radTSQL.Checked = true;
            pnlAttributes.Controls.Clear();
            attributeList.Clear();
            // Add one attribute by default
            var picker = new SQLAttributePicker(DBMSType.TSQL);
            attributeList.Add(picker);
            pnlAttributes.Controls.Add(picker);
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InitializeForm();
        }

        private void btnAddAttribute_Click(object sender, EventArgs e)
        {
            var picker = new SQLAttributePicker(GetDBMSType());
            picker.Location = new Point(0, (13 * (attributeList.Count)) + ((attributeList.Count) * picker.Height) + pnlAttributes.AutoScrollPosition.Y);

            attributeList.Add(picker);
            pnlAttributes.Controls.Add(picker);
            picker.Focus();
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            // First, validate each input
            InputValidationResult validationResult = ValidateInput();
            if (validationResult != InputValidationResult.VALID)
            {
                ShowInputErrorDialog(validationResult);
            }
            else
            {
                // Build DALEntity and DALAttributes
                DALEntity entity = new DALEntity(txtEntityName.Text, txtDatabaseName.Text, txtSchemaName.Text, txtNamespace.Text);
                foreach(SQLAttributePicker attributePicker in attributeList)
                {
                    DALAttributes attribute = new DALAttributes(attributePicker.AttributeName, attributePicker.DataType, attributePicker.AttributeSize, attributePicker.IsPrimaryKey, attributePicker.AutoIncrement, attributePicker.IsForeignKey, attributePicker.ReferenceEntity, attributePicker.ReferenceAttribute);
                    entity.Attributes.Add(attribute);
                }

                String outputPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                if (entity.Attributes.Where(x => x.IsPrimaryKey).Count() < 1)
                {
                    DialogResult result = MessageBox.Show("No primary key has been selected for this entity. Without a primary key several DAL method such as Load and Update cannot function appropriately. For this reason, only a class library will be generated. Would you like to continue?", "Warning", MessageBoxButtons.OKCancel);
                    if (result == DialogResult.Cancel)
                        return;
                }

                if (radTSQL.Checked)
                {
                    // Generate TSQL Create Table and Stored Procedure scripts.
                    var tsqlGenerator = new TSQLTemplate();
                    tsqlGenerator.GenerateContent(entity, outputPath);
                }
                if (radMySQL.Checked)
                {
                    // Generate MySQL Create Table and Stored Procedure scripts.
                    var mysqlGenerator = new MySQLTemplate();
                    mysqlGenerator.GenerateContent(entity, outputPath);
                }
                if (radOracle.Checked)
                {
                    // Generate Oracle Create Table and Stored Procedure scripts.
                }
                if (chkCPP.Checked)
                {
                    // Generate C++ DAL
                }
                if (chkCSharp.Checked)
                {
                    // Generate C# DAL
                }
                if (chkJava.Checked)
                {
                    // Generate Java DAL
                }
                if (chkPython.Checked)
                {
                    // Generate Python DAL
                }
                if (chkPHP.Checked)
                {
                    // Generate PHP DAL
                    var phpGenerator = new PHPTemplate();
                    phpGenerator.GenerateContent(entity, outputPath);
                }

                MessageBox.Show(this, "Success", "Content Generated Successfully", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private InputValidationResult ValidateInput()
        {
            InputValidationResult result = InputValidationResult.VALID;

            if (!chkCPP.Checked && !chkCSharp.Checked && !chkJava.Checked && !chkPython.Checked
                && !radMySQL.Checked && !radOracle.Checked && !radTSQL.Checked)
                result = InputValidationResult.NO_OUTPUT;
            else if (String.IsNullOrWhiteSpace(txtEntityName.Text))
                result = InputValidationResult.INVALID_ENTITY_NAME;
            else
                // Finally, validate the attribute list
                foreach (var attribute in attributeList)
                {
                    if (result != InputValidationResult.VALID)
                        break;
                    result = attribute.ValidateInput();
                }
            
            
            return result;
        }

        private void ShowInputErrorDialog(InputValidationResult result)
        {
            var message = "Generic Error";
        
            switch(result)
            {
                case InputValidationResult.NO_OUTPUT:
                    message = "You must select a language to generate.";
                    break;
                case InputValidationResult.INVALID_ENTITY_NAME:
                    message = "You must provide an entity name.";
                    break;
                case InputValidationResult.INVALID_SCHEMA_NAME:
                    message = "You must provide a schema name.";
                    break;
                case InputValidationResult.INVALID_NAMESPACE:
                    message = "You must provide a namespace.";
                    break;
                case InputValidationResult.INVALID_ATTRIBUTE_NAME:
                    message = "You must provide a name for each attribute.";
                    break;
                case InputValidationResult.INVALID_ATTRIBUTE_SIZE:
                    message = "One or more of your attributes have an invalid size.";
                    break;
                case InputValidationResult.INVALID_ATTRIBUTE_REF_ENTITY:
                    message = "Foreign Keys require a reference entity/table.";
                    break;
                case InputValidationResult.INVALID_ATTRIBUTE_REF_ATTRIBUTE:
                    message = "Foreign Keys require an entity's primary key as a reference attribute.";
                    break;
                default:
                    message = "A generic error occurred. Please review your configuration and report bugs to opendevtools@gmail.com";
                    break;
            }

            // Display error dialog with a message for the result
            MessageBox.Show(this, message, "Input Validation Error", MessageBoxButtons.OKCancel);
        }

        private void radTSQL_CheckedChanged(object sender, EventArgs e)
        {
            pnlAttributes.Controls.Clear();
            attributeList.Clear();

            if (radTSQL.Checked)
            {
                // Add one attribute by default
                var picker = new SQLAttributePicker(DBMSType.TSQL);
                attributeList.Add(picker);
                pnlAttributes.Controls.Add(picker);
            }
        }

        private DBMSType GetDBMSType()
        {
            if (radTSQL.Checked)
                return DBMSType.TSQL;
            else if (radMySQL.Checked)
                return DBMSType.MYSQL;
            else if (radOracle.Checked)
                return DBMSType.ORACLE;
            else
                return DBMSType.UNKNOWN;
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InitializeForm();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Coming Soon");
        }

        private void radMySQL_CheckedChanged(object sender, EventArgs e)
        {
            pnlAttributes.Controls.Clear();
            attributeList.Clear();

            if (radMySQL.Checked)
            {
                // Add one attribute by default
                var picker = new SQLAttributePicker(DBMSType.MYSQL);
                attributeList.Add(picker);
                pnlAttributes.Controls.Add(picker);
            }
        }

        private void aboutDALGenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var aboutForm = new AboutForm();
            aboutForm.ShowDialog();
        }
    }
}
