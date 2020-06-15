using CallOfCthulhu;
using CardWizard.Tools;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace CardWizard.View
{
    /// <summary>
    /// WeaponBox.xaml 的交互逻辑
    /// </summary>
    public partial class WeaponBox : UserControl
    {
        private ObservableCollection<Weapon> weapons;

        public WeaponBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 当前角色的武器
        /// </summary>
        public ObservableCollection<Weapon> Weapons
        {
            get => weapons;
            set
            {
                weapons = value;
                MainGrid.ItemsSource = weapons;
            }
        }

        /// <summary>
        /// 数据源
        /// </summary>
        private List<Weapon> DataSource
        {
            get;
            set;
        }

        /// <summary>
        /// 初始化数据表
        /// </summary>
        /// <param name="weaponsSource"></param>
        public void InitializeDataGrid(IEnumerable<Weapon> weaponsSource)
        {
            if (weaponsSource == null || !weaponsSource.Any()) return;
            DataSource = weaponsSource.ToList();
            ColumnWeaponName.ItemsSource = from w in DataSource select w.Name;
            MainGrid.CanUserAddRows = true;
            MainGrid.CanUserDeleteRows = true;
            MainGrid.CellEditEnding += CellEditEnding;
        }

        private void CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            var grid = sender as DataGrid;
            if (e.Column == ColumnWeaponName && e.EditingElement is ComboBox combo)
            {
                var selected = combo.SelectedItem as string;
                var weapon = (from w in DataSource where w.Name.EqualsIgnoreCase(selected) select w).FirstOrDefault();
                if (weapon != default)
                {
                    var item = e.Row.Item as Weapon;
                    item.CopyFrom(weapon);
                }
            }
        }
    }
}
