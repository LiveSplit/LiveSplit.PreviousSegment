using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Fetze.WinFormsColor;
using System.Globalization;
using LiveSplit.TimeFormatters;
using LiveSplit.Model;
using LiveSplit.Model.Comparisons;

namespace LiveSplit.UI.Components
{
    public partial class PreviousSegmentSettings : UserControl
    {
        public Color TextColor { get; set; }
        public bool OverrideTextColor { get; set; }
        public Color BackgroundColor { get; set; }
        public Color BackgroundColor2 { get; set; }
        public GradientType BackgroundGradient { get; set; }
        public String GradientString
        {
            get { return BackgroundGradient.ToString(); }
            set { BackgroundGradient = (GradientType)Enum.Parse(typeof(GradientType), value); }
        }

        public TimeAccuracy DeltaAccuracy { get; set; }
        public bool DropDecimals { get; set; }
        public bool Display2Rows { get; set; }

        public String Comparison { get; set; }
        public LiveSplitState CurrentState { get; set; }

        public LayoutMode Mode { get; set; }

        public PreviousSegmentSettings()
        {
            InitializeComponent();

            TextColor = Color.FromArgb(255, 255, 255);
            OverrideTextColor = false;
            BackgroundColor = Color.Transparent;
            BackgroundColor2 = Color.Transparent;
            BackgroundGradient = GradientType.Plain;
            DeltaAccuracy = TimeAccuracy.Tenths;
            DropDecimals = true;
            Comparison = "Current Comparison";
            Display2Rows = false;

            btnTextColor.DataBindings.Add("BackColor", this, "TextColor", false, DataSourceUpdateMode.OnPropertyChanged);
            chkOverride.DataBindings.Add("Checked", this, "OverrideTextColor", false, DataSourceUpdateMode.OnPropertyChanged);
            cmbGradientType.DataBindings.Add("SelectedItem", this, "GradientString", false, DataSourceUpdateMode.OnPropertyChanged);
            btnColor1.DataBindings.Add("BackColor", this, "BackgroundColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnColor2.DataBindings.Add("BackColor", this, "BackgroundColor2", false, DataSourceUpdateMode.OnPropertyChanged);
            chkDropDecimals.DataBindings.Add("Checked", this, "DropDecimals", false, DataSourceUpdateMode.OnPropertyChanged);
            cmbComparison.DataBindings.Add("SelectedItem", this, "Comparison", false, DataSourceUpdateMode.OnPropertyChanged);
        }

        void chkOverride_CheckedChanged(object sender, EventArgs e)
        {
            label1.Enabled = btnTextColor.Enabled = chkOverride.Checked;
        }

        void cmbComparison_SelectedIndexChanged(object sender, EventArgs e)
        {
            Comparison = cmbComparison.SelectedItem.ToString();
        }

        void rdoDeltaTenths_CheckedChanged(object sender, EventArgs e)
        {
            UpdateAccuracy();
        }

        void rdoDeltaSeconds_CheckedChanged(object sender, EventArgs e)
        {
            UpdateAccuracy();
        }

        void PreviousSegmentSettings_Load(object sender, EventArgs e)
        {
            chkOverride_CheckedChanged(null, null);
            cmbComparison.Items.Clear();
            cmbComparison.Items.Add("Current Comparison");
            cmbComparison.Items.AddRange(CurrentState.Run.Comparisons.Where(x => x != BestSplitTimesComparisonGenerator.ComparisonName && x != NoneComparisonGenerator.ComparisonName).ToArray());
            if (!cmbComparison.Items.Contains(Comparison))
                cmbComparison.Items.Add(Comparison);
            rdoDeltaHundredths.Checked = DeltaAccuracy == TimeAccuracy.Hundredths;
            rdoDeltaTenths.Checked = DeltaAccuracy == TimeAccuracy.Tenths;
            rdoDeltaSeconds.Checked = DeltaAccuracy == TimeAccuracy.Seconds;
            if (Mode == LayoutMode.Horizontal)
            {
                chkTwoRows.Enabled = false;
                chkTwoRows.DataBindings.Clear();
                chkTwoRows.Checked = true;
            }
            else
            {
                chkTwoRows.Enabled = true;
                chkTwoRows.DataBindings.Clear();
                chkTwoRows.DataBindings.Add("Checked", this, "Display2Rows", false, DataSourceUpdateMode.OnPropertyChanged);
            }
        }

        void UpdateAccuracy()
        {
            if (rdoDeltaSeconds.Checked)
                DeltaAccuracy = TimeAccuracy.Seconds;
            else if (rdoDeltaTenths.Checked)
                DeltaAccuracy = TimeAccuracy.Tenths;
            else
                DeltaAccuracy = TimeAccuracy.Hundredths;
        }

        void cmbGradientType_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnColor1.Visible = cmbGradientType.SelectedItem.ToString() != "Plain";
            btnColor2.DataBindings.Clear();
            btnColor2.DataBindings.Add("BackColor", this, btnColor1.Visible ? "BackgroundColor2" : "BackgroundColor", false, DataSourceUpdateMode.OnPropertyChanged);
            GradientString = cmbGradientType.SelectedItem.ToString();
        }

        public void SetSettings(XmlNode node)
        {
            var element = (XmlElement)node;
            TextColor = SettingsHelper.ParseColor(element["TextColor"]);
            OverrideTextColor = SettingsHelper.ParseBool(element["OverrideTextColor"]);
            BackgroundColor = SettingsHelper.ParseColor(element["BackgroundColor"]);
            BackgroundColor2 = SettingsHelper.ParseColor(element["BackgroundColor2"]);
            GradientString = SettingsHelper.ParseString(element["BackgroundGradient"]);
            DeltaAccuracy = SettingsHelper.ParseEnum<TimeAccuracy>(element["DeltaAccuracy"]);
            DropDecimals = SettingsHelper.ParseBool(element["DropDecimals"]);
            Comparison = SettingsHelper.ParseString(element["Comparison"]);
            Display2Rows = SettingsHelper.ParseBool(element["Display2Rows"], false);
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            var parent = document.CreateElement("Settings");
            parent.AppendChild(SettingsHelper.ToElement(document, "Version", "1.4"));
            parent.AppendChild(SettingsHelper.ToElement(document, TextColor, "TextColor"));
            parent.AppendChild(SettingsHelper.ToElement(document, "OverrideTextColor", OverrideTextColor));
            parent.AppendChild(SettingsHelper.ToElement(document, BackgroundColor, "BackgroundColor"));
            parent.AppendChild(SettingsHelper.ToElement(document, BackgroundColor2, "BackgroundColor2"));
            parent.AppendChild(SettingsHelper.ToElement(document, "BackgroundGradient", BackgroundGradient));
            parent.AppendChild(SettingsHelper.ToElement(document, "DeltaAccuracy", DeltaAccuracy));
            parent.AppendChild(SettingsHelper.ToElement(document, "DropDecimals", DropDecimals));
            parent.AppendChild(SettingsHelper.ToElement(document, "Comparison", Comparison));
            parent.AppendChild(SettingsHelper.ToElement(document, "Display2Rows", Display2Rows));
            return parent;
        }

        private void ColorButtonClick(object sender, EventArgs e)
        {
            SettingsHelper.ColorButtonClick((Button)sender, this);
        }
    }
}
