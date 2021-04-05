using System;
using System.Windows.Forms;
using System.Drawing;
using Testing;

namespace GUI{

    class GUIController : Form{

        public static void OpenMainGUI(){

            if (!MainGUI.Opened){
                
                new MainGUI();

            }

        }

        public static void OpenConfigGUI(){

            if (!ConfigGUI.Opened && MainGUI.Opened){

                new ConfigGUI(MainGUI.Reference);

            }

        }

        // Log something to the output log
        public static void LogOutput<T>(T arg, string prefix = "Bot: ", string end = "\n", bool bold = false, int fontSize = 12){

            MainGUI.Reference.SetOutputFontStyle(fontSize, bold: true);
    
            MainGUI.Reference.Output.AppendText(prefix);

            MainGUI.Reference.SetOutputFontStyle(fontSize, bold: bold);

            MainGUI.Reference.Output.AppendText(arg.ToString() + end);

            MainGUI.Reference.SetOutputFontStyle(12, false);

            MainGUI.Reference.Output.ScrollToCaret();

        }

    }

    class BaseGUI : Form{

        // Generates a Form Control of specific type T with the given parameters; makes instantiating new Controls easier to read
        public static GUIControlWrapper<T> CreateControl<T>(int width, int height, int xPosition, int yPosition, AnchorStyles anchor = 0, int fontSize = 9) where T:System.Windows.Forms.Control, new(){

            var newControl = new GUIControlWrapper<T>(width, height);

            newControl.SetPosition(xPosition, yPosition);

            newControl.SetAnchor(anchor);

            newControl.control.Font = newControl.ConstructFont(fontSize);

            return newControl;

        }

    }

    class MainGUI : BaseGUI{

        public static MainGUI Reference { get; private set; }

        public static bool Opened { get; private set; }

        public bool microphoneToggle { get; private set; }

        public bool voiceOutputToggle { get; private set; }
        
        private string inputValue = "";

        public string InputValue {
            get{
                string temp = "";
                if (inputValue != ""){
                    temp = inputValue;
                    inputValue = "";
                }
                return temp;
            }
        }

        private GUIControlWrapper<RichTextBox> output;

        public RichTextBox Output{
            get{
                return this.output.control;
            }
        }

        private GUIControlWrapper<RichTextBox> input;

        private GUIControlWrapper<Button> openConfigButton;

        private GUIControlWrapper<Button> microphoneButton;

        private GUIControlWrapper<Button> voiceOutputButton;

        public MainGUI(){

            if (Reference == null){

                Reference = this;

            }

            ConstructMainGUI();

        }

        public void SetOutputFontStyle(int fontSize = 12, bool bold = false){

            output.control.SelectionFont = output.ConstructFont(fontSize, bold);

        }

        // Construct the main GUI, containing input boxes and the output log
        public void ConstructMainGUI(){

            if (Opened){

                return;

            }

            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            this.Width = 800;
            this.Height = 600;

            this.MaximizeBox = false;

            var title = CreateControl<Label>(600, 75, 100, 0, AnchorStyles.Left | AnchorStyles.Right);
            title.control.Font = title.ConstructFont(24, bold: true, underline: true);
            title.control.Text = "Voice Activated Assistant";
            title.control.TextAlign = ContentAlignment.MiddleCenter;

            this.output = CreateControl<RichTextBox>(600, 300, 100, 100, AnchorStyles.Left | AnchorStyles.Right, 12);
            this.Output.BorderStyle = BorderStyle.FixedSingle;
            this.Output.ReadOnly = true;
            this.Output.Enter += new EventHandler(this.Output_Focused);

            this.input = CreateControl<RichTextBox>(525, 30, 100, 400, AnchorStyles.Left | AnchorStyles.Right, 12);
            this.input.control.Multiline = false;
            this.input.control.BorderStyle = BorderStyle.FixedSingle;

            var sendButton = CreateControl<Button>(75, 29, 625, 400, AnchorStyles.Left | AnchorStyles.Right, 10);
            sendButton.Text = ">";
            sendButton.control.TextAlign = ContentAlignment.MiddleCenter;
            sendButton.control.Click += new EventHandler(this.SubmitButton_Clicked);
            this.AcceptButton = sendButton.control;

            var clearButton = CreateControl<Button>(108, 75, 100, 475, AnchorStyles.Left, 10);
            clearButton.Text = "Clear Chatlog";
            clearButton.control.TextAlign = ContentAlignment.MiddleCenter;
            clearButton.control.Click += new EventHandler(this.ClearButton_Clicked);

            var runTestsButton = CreateControl<Button>(108, 75, 223, 475, AnchorStyles.Left, 10);
            runTestsButton.Text = "Run Tests";
            runTestsButton.control.TextAlign = ContentAlignment.MiddleCenter;
            runTestsButton.control.Click += new EventHandler(this.RunTestsButton_Clicked);

            this.openConfigButton = CreateControl<Button>(108, 75, 346, 475, AnchorStyles.Right, 10);
            this.openConfigButton.Text = "Open Config";
            this.openConfigButton.control.TextAlign = ContentAlignment.MiddleCenter;
            this.openConfigButton.control.Click += new EventHandler(this.OpenConfigButton_Clicked);

            this.microphoneButton = CreateControl<Button>(108, 75, 469, 475, AnchorStyles.Right, 10);
            this.microphoneButton.Text = "Unmute Microphone";
            this.microphoneButton.control.TextAlign = ContentAlignment.MiddleCenter;
            this.microphoneButton.control.Click += new EventHandler(this.MicrophoneButton_Clicked);

            this.voiceOutputButton = CreateControl<Button>(108, 75, 592, 475, AnchorStyles.Right, 10);
            this.voiceOutputButton.Text = "Unmute Output";
            this.voiceOutputButton.control.TextAlign = ContentAlignment.MiddleCenter;
            this.voiceOutputButton.control.Click += new EventHandler(this.VoiceOutputButton_Clicked);

            this.Controls.Add(title.control);
            this.Controls.Add(this.Output);
            this.Controls.Add(this.input.control);
            this.Controls.Add(clearButton.control);
            this.Controls.Add(sendButton.control);
            this.Controls.Add(runTestsButton.control);
            this.Controls.Add(openConfigButton.control);
            this.Controls.Add(this.microphoneButton.control);
            this.Controls.Add(this.voiceOutputButton.control);

            MainGUI.Opened = true;

            Application.Run(this);

            MainGUI.Opened = false;

            Console.WriteLine("GUI thread closing");

        }

        public void Output_Focused(Object sender, EventArgs e){

            this.input.control.Focus();

        }

        public void SubmitButton_Clicked(Object sender, EventArgs e){

            SubmitInput();

        }
        
        public void ClearButton_Clicked(Object sender, EventArgs e){

            this.Output.Text = "";

        }
        
        public void RunTestsButton_Clicked(Object sender, EventArgs e){

            GUIController.LogOutput("Running tests");
            
            TestController.RunAllTests();

        }
        
        public void OpenConfigButton_Clicked(Object sender, EventArgs e){
            
            GUIController.OpenConfigGUI();

        }
        
        public void MicrophoneButton_Clicked(Object sender, EventArgs e){

            microphoneToggle = !microphoneToggle;

            microphoneButton.Text = microphoneToggle ? "Mute Microphone" : "Unmute Microphone";

        }

        public void VoiceOutputButton_Clicked(Object sender, EventArgs e){
            
            voiceOutputToggle = !voiceOutputToggle;

            voiceOutputButton.Text = voiceOutputToggle ? "Mute Output" : "Unmute Output";

        }

        private void SubmitInput(){

            string value = input.Text;

            if (value != ""){

                GUIController.LogOutput(value, prefix: "User: ");

                inputValue = value;

            }

            input.Text = "";

        }

    }

    class ConfigGUI : BaseGUI{
        
        public static bool Opened { get; private set; }

        private BaseGUI parent;

        private TableLayoutPanel table;

        public ConfigGUI(BaseGUI parent){

            this.parent = parent;

            ConstructConfigGUI();

        }

        public void ConstructConfigGUI(){

            if (Opened){

                return;

            }

            this.Visible = false;

            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            this.Width = 400;
            this.Height = 600;

            this.MaximizeBox = false;

            var title = CreateControl<Label>(300, 50, 50, 0, AnchorStyles.Left | AnchorStyles.Right);
            title.control.Font = title.ConstructFont(24, bold: true, underline: true);
            title.control.Text = "Config Editor";
            title.control.TextAlign = ContentAlignment.MiddleCenter;

            table = new TableLayoutPanel();
            table.Width = 300;
            table.Height = 400;
            table.BorderStyle = BorderStyle.FixedSingle;
            table.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            table.Location = new Point(50, 75);
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 25f));

            // Add table headings
            table.Controls.Add(GenerateTableHeading("Select"), 0, 0);
            table.Controls.Add(GenerateTableHeading("Name"), 1, 0);
            table.Controls.Add(GenerateTableHeading("Path"), 2, 0);

            var removeSelectedButton = CreateControl<Button>(100, 50, 50, 500, AnchorStyles.Left, 10);
            removeSelectedButton.Text = "Remove Selected";
            removeSelectedButton.control.TextAlign = ContentAlignment.MiddleCenter;
            // Click callback

            var addProgramButton = CreateControl<Button>(100, 50, 250, 500, AnchorStyles.Right, 10);
            addProgramButton.Text = "Add Program";
            addProgramButton.control.TextAlign = ContentAlignment.MiddleCenter;
            // Click callback

            this.Controls.Add(title.control);
            this.Controls.Add(table);
            this.Controls.Add(removeSelectedButton.control);
            this.Controls.Add(addProgramButton.control);

            GenerateTableRows();

            ConfigGUI.Opened = true;

            this.ShowDialog(this.parent);

            ConfigGUI.Opened = false;

        }

        private Label GenerateTableHeading(string text){

            var label = new GUIControlWrapper<Label>();
            label.Text = text;
            label.control.TextAlign = ContentAlignment.MiddleCenter;
            label.control.Font = label.ConstructFont(fontSize: 8, bold: true);

            table.RowCount += 1;

            return label.control;

        }

        private void GenerateTableRows(){

            string[] names = { "A", "B", "C", "D" };

            for (int i = 0 ; i < names.Length ; i++){

                var checkbox = new GUIControlWrapper<CheckBox>();
                checkbox.control.CheckAlign = ContentAlignment.MiddleCenter;
                checkbox.SetAnchor(AnchorStyles.None);

                var nameInput = new GUIControlWrapper<TextBox>();
                nameInput.SetAnchor(AnchorStyles.None);

                var pathInput = new GUIControlWrapper<TextBox>();
                pathInput.SetAnchor(AnchorStyles.None);

                table.Controls.Add(checkbox.control, 0, -1);
                table.Controls.Add(nameInput.control, 1, -1);
                table.Controls.Add(pathInput.control, 2, -1);

                table.RowCount += 1;

            }

        }

    }

    // Wrap each Forms Control in order to provide easy-to-use styling methods
    class GUIControlWrapper<T> where T:System.Windows.Forms.Control, new(){

        public T control { get; private set; }

        public string Text {
            get { return control.Text; }
            set { control.Text = value; }
        }

        public GUIControlWrapper(){

            control = new T();

        }

        public GUIControlWrapper(int width, int height){

            control = new T();

            control.Width = width;

            control.Height = height;

        }

        public void SetAnchor(AnchorStyles anchor) => control.Anchor = anchor;

        public void SetPosition(int left, int top) => control.Location = new Point(left, top);

        public Font ConstructFont(float fontSize = 12, bool bold = false, bool italic = false, bool underline = false){

            fontSize = fontSize == 0 ? control.Font.Size : fontSize;

            FontStyle boldStyle = bold ? FontStyle.Bold : 0;

            FontStyle italicStyle = italic ? FontStyle.Italic : 0;

            FontStyle underlineStyle = underline ? FontStyle.Underline : 0;

            return new Font(control.Font.Name, fontSize, boldStyle | italicStyle | underlineStyle);

        }

    }

}