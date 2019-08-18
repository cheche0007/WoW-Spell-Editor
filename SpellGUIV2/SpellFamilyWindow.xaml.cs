using SpellEditor.Sources.BLP;
using SpellEditor.Sources.Database;
using SpellEditor.Sources.DBC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SpellEditor
{
    public partial class SpellFamilyWindow
    {
        private IDatabaseAdapter _Adapter;
        private uint _SelectedSpellId;
        private int _SelectedLanguage;
        private ListBox _SharedFamilyList;
        private TextBox _FamilyMask1Txt;
        private TextBox _FamilyMask2Txt;
        private TextBox _FamilyMask3Txt;
        private SpellIconDBC _IconDBC;

        public SpellFamilyWindow(IDatabaseAdapter adapter, SpellIconDBC iconDBC, uint selectedSpellId, int selectedLanguage)
        {
            _Adapter = adapter;
            _IconDBC = iconDBC;
            _SelectedSpellId = selectedSpellId;
            _SelectedLanguage = selectedLanguage;
            InitializeComponent();
        }

        private void _Loaded(object sender, RoutedEventArgs e)
        {
            Application.Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
            BuildUI();
            PopulateUI();
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Console.WriteLine("ERROR: " + e.Exception.Message);
            File.WriteAllText("error.txt", e.Exception.Message, UTF8Encoding.GetEncoding(0));
            e.Handled = true;
        }

        private void BuildUI()
        {
            var familyMask1Label = new Label()
            {
                Content = "Family Mask 1:",
                Margin = new Thickness(5, 5, 5, 5)
            };
            _FamilyMask1Txt = new TextBox()
            {
                Margin = new Thickness(40 + 10, 5, 5, 5)
            };
            var familyMask2Label = new Label()
            {
                Content = "Family Mask 2:",
                Margin = new Thickness(5, 5 + 25, 5, 5)
            };
            _FamilyMask2Txt = new TextBox()
            {
                Margin = new Thickness(40 + 10, 5 + 25, 5, 5)
            };
            var familyMask3Label = new Label()
            {
                Content = "Family Mask 3:",
                Margin = new Thickness(5, 5 + 50, 5, 5)
            };
            _FamilyMask3Txt = new TextBox()
            {
                Margin = new Thickness(40 + 10, 5 + 50, 5, 5)
            };
            var sharedFamilyLabel = new Label()
            {
                Content = "Shared family list:",
                Margin = new Thickness(5, 5 + 75, 5, 5)
            };
            _SharedFamilyList = new ListBox()
            {
                Width = 300,
                Height = 200,
                Margin = new Thickness(5, 5 + 100, 5, 5),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            MainGrid.Children.Add(familyMask1Label);
            MainGrid.Children.Add(_FamilyMask1Txt);
            MainGrid.Children.Add(familyMask2Label);
            MainGrid.Children.Add(_FamilyMask2Txt);
            MainGrid.Children.Add(familyMask3Label);
            MainGrid.Children.Add(_FamilyMask3Txt);
            MainGrid.Children.Add(sharedFamilyLabel);
            MainGrid.Children.Add(_SharedFamilyList);
        }

        private void PopulateUI()
        {
            PopulateSharedFamilyList();
        }

        private void PopulateSharedFamilyList()
        {
            var blpManager = BlpManager.GetInstance();
            var results = _Adapter.Query(BuildQueryString());
            var row = results.Rows[0];
            var familyFlags1 = uint.Parse(row[2].ToString());
            var familyFlags2 = uint.Parse(row[3].ToString());
            var familyFlags3 = uint.Parse(row[4].ToString());
            _FamilyMask1Txt.Text = familyFlags1.ToString();
            _FamilyMask2Txt.Text = familyFlags2.ToString();
            _FamilyMask3Txt.Text = familyFlags3.ToString();
            results = _Adapter.Query(BuildQueryString(familyFlags1, familyFlags2, familyFlags3));
            var newElements = new List<UIElement>();
            for (int i = 0; i < results.Rows.Count; ++i)
            {
                row = results.Rows[i];
                var spellName = row[1].ToString();
                var textBlock = new TextBlock();
                textBlock.Text = string.Format(" {0} - {1}", row[0], spellName);
                var image = new Image();
                var iconId = uint.Parse(row["SpellIconID"].ToString());
                if (iconId > 0)
                {
                    image.ToolTip = iconId.ToString();
                    image.Width = 32;
                    image.Height = 32;
                    image.Margin = new Thickness(1, 1, 1, 1);
                    image.Source = blpManager.GetImageSourceFromBlpPath(_IconDBC.GetIconPath(iconId) + ".blp");
                    var stackPanel = new StackPanel() { Orientation = Orientation.Horizontal };
                    stackPanel.Children.Add(image);
                    stackPanel.Children.Add(textBlock);
                    newElements.Add(stackPanel);
                }
            }
            var newSrc = new List<object>();
            foreach (var element in newElements)
                newSrc.Add(element);
            _SharedFamilyList.ItemsSource = newSrc;
        }

        private string BuildQueryString()
        {
            return "SELECT " +
                    "ID," +
                    $"SpellName{_SelectedLanguage}," +
                    "SpellFamilyFlags," +
                    "SpellFamilyFlags1," +
                    "SpellFamilyFlags2," +
                    "EffectSpellClassMaskA1," +
                    "EffectSpellClassMaskA2," +
                    "EffectSpellClassMaskA3," +
                    "EffectSpellClassMaskB1," +
                    "EffectSpellClassMaskB2," +
                    "EffectSpellClassMaskB3," +
                    "EffectSpellClassMaskC1," +
                    "EffectSpellClassMaskC2," +
                    "EffectSpellClassMaskC3," +
                    "SpellIconID" +
                " FROM spell WHERE " +
                    $"ID = {_SelectedSpellId}";
        }

        private string BuildQueryString(uint familyFlags1, uint familyFlags2, uint familyFlags3)
        {
            return "SELECT " +
                    "ID," +
                    $"SpellName{_SelectedLanguage}," +
                    "SpellFamilyFlags," +
                    "SpellFamilyFlags1," +
                    "SpellFamilyFlags2," +
                    "EffectSpellClassMaskA1," +
                    "EffectSpellClassMaskA2," +
                    "EffectSpellClassMaskA3," +
                    "EffectSpellClassMaskB1," +
                    "EffectSpellClassMaskB2," +
                    "EffectSpellClassMaskB3," +
                    "EffectSpellClassMaskC1," +
                    "EffectSpellClassMaskC2," +
                    "EffectSpellClassMaskC3," +
                    "SpellIconID" +
                " FROM spell WHERE " +
                    $"ID = {_SelectedSpellId} OR " +
                    (familyFlags1 > 0 ? $"(({familyFlags1} & EffectSpellClassMaskA1) = 0) OR " : "") +
                    (familyFlags2 > 0 ? $"(({familyFlags2} & EffectSpellClassMaskB1) = 0) OR " : "") +
                    (familyFlags3 > 0 ? $"(({familyFlags3} & EffectSpellClassMaskC1) = 0)" : "");
        }
    }
}
