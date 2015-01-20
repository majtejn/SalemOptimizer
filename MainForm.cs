﻿using SalemOptimizer.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SalemOptimizer
{
    public partial class MainForm : Form
    {
        class ListViewColumnSorter : IComparer
        {
            /// <summary>
            /// Specifies the column to be sorted
            /// </summary>
            private int ColumnToSort;
            /// <summary>
            /// Specifies the order in which to sort (i.e. 'Ascending').
            /// </summary>
            private SortOrder OrderOfSort;
            /// <summary>
            /// Case insensitive comparer object
            /// </summary>
            private CaseInsensitiveComparer ObjectCompare;

            /// <summary>
            /// Class constructor.  Initializes various elements
            /// </summary>
            public ListViewColumnSorter()
            {
                // Initialize the column to '0'
                ColumnToSort = 0;

                // Initialize the sort order to 'none'
                OrderOfSort = SortOrder.None;

                // Initialize the CaseInsensitiveComparer object
                ObjectCompare = new CaseInsensitiveComparer();
            }

            /// <summary>
            /// This method is inherited from the IComparer interface.  It compares the two objects passed using a case insensitive comparison.
            /// </summary>
            /// <param name="x">First object to be compared</param>
            /// <param name="y">Second object to be compared</param>
            /// <returns>The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and positive if 'x' is greater than 'y'</returns>
            public int Compare(object x, object y)
            {
                int compareResult;
                ListViewItem listviewX, listviewY;

                // Cast the objects to be compared to ListViewItem objects
                listviewX = (ListViewItem)x;
                listviewY = (ListViewItem)y;

                // Compare the two items
                if (ColumnToSort == 1) compareResult = int.Parse(listviewX.SubItems[ColumnToSort].Text, NumberStyles.Number).CompareTo(int.Parse(listviewY.SubItems[ColumnToSort].Text, NumberStyles.Number));
                else compareResult = ObjectCompare.Compare(listviewX.SubItems[ColumnToSort].Text, listviewY.SubItems[ColumnToSort].Text);

                // Calculate correct return value based on object comparison
                if (OrderOfSort == SortOrder.Ascending)
                {
                    // Ascending sort is selected, return normal result of compare operation
                    return compareResult;
                }
                else if (OrderOfSort == SortOrder.Descending)
                {
                    // Descending sort is selected, return negative result of compare operation
                    return (-compareResult);
                }
                else
                {
                    // Return '0' to indicate they are equal
                    return 0;
                }
            }

            /// <summary>
            /// Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').
            /// </summary>
            public int SortColumn
            {
                set
                {
                    ColumnToSort = value;
                }
                get
                {
                    return ColumnToSort;
                }
            }

            /// <summary>
            /// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
            /// </summary>
            public SortOrder Order
            {
                set
                {
                    OrderOfSort = value;
                }
                get
                {
                    return OrderOfSort;
                }
            }
        }

        public MainForm()
        {
            InitializeComponent();

            lvInspirationals.ListViewItemSorter = new ListViewColumnSorter();
            lvSolutions.ListViewItemSorter = new ListViewColumnSorter();
        }

        private CancellationTokenSource cancellationTokenSource;      

        private async void btnFindSolution_Click(object sender, EventArgs e)
        {
            try
            {
                btnFindSolution.Enabled = false;
                btnStop.Enabled = true;

                UseWaitCursor = true;

                var problem =
                    new Problem()
                    .Add(ProficiencyKind.ArtsAndCrafts,         nudArtsAndCrafts.Value)
                    .Add(ProficiencyKind.CloakAndDagger,        nudCloakAndDagger.Value)
                    .Add(ProficiencyKind.FaithAndWisdom,        nudFaithAndWisdom.Value)
                    .Add(ProficiencyKind.FloraAndFauna,         nudFloraAndFauna.Value)
                    .Add(ProficiencyKind.HammerAndNail,         nudHammerAndNail.Value)
                    .Add(ProficiencyKind.HerbsAndSprouts,       nudHerbsAndSprouts.Value)
                    .Add(ProficiencyKind.HuntingAndHideworking, nudHuntingAndHideworking.Value)
                    .Add(ProficiencyKind.LawAndLore,            nudLawAndLore.Value)
                    .Add(ProficiencyKind.MinesAndMountains,     nudMinesAndMountains.Value)
                    .Add(ProficiencyKind.NaturalPhilosophy,     nudNaturalPhilosophy.Value)
                    .Add(ProficiencyKind.PerenialPhilosophy,    nudPerenialPhilosophy.Value)
                    .Add(ProficiencyKind.SparksAndEmbers,       nudSparksAndEmbers.Value)
                    .Add(ProficiencyKind.StocksAndCultivars,    nudStocksAndCultivars.Value)
                    .Add(ProficiencyKind.SugarAndSpice,         nudSugarAndSpice.Value)
                    .Add(ProficiencyKind.ThreadAndNeedle,       nudThreadAndNeedle.Value);

                var availableInspirationals = lvInspirationals.CheckedItems.OfType<ListViewItem>().Select(i => ((Inspirational)i.Tag).Clone()).ToArray();

                cancellationTokenSource = new CancellationTokenSource();

                var solvers = Enumerable.Range(1, 4).Select(i => Task.Run(() => new Solver(problem, availableInspirationals, cancellationTokenSource.Token).Solve())).ToArray();

                var best = await Task.WhenAll(solvers);

                // Aggregate the best results and fill them into the results control
                var bestOfAll = 
                    best
                    .SelectMany(i => i)
                    .Select(i => new { Name = i.ToString(), Solution = i.Solution })
                    .GroupBy(i => i.Name)
                    .Select(i => i.First())
                    .OrderBy(i => i.Solution.CostTotal)
                    .Take(5)
                    .ToArray();

                lvSolutions.Items.Clear();

                foreach (var organism in bestOfAll)
                {
                    var lvi = new ListViewItem();
                    lvi.Text = organism.Name;
                    lvi.ForeColor = organism.Solution.IncompletenessPenalty == 0 ? Color.Green : Color.Red;
                    lvi.SubItems.Add(organism.Solution.CostTotal.ToString("###,###,###"));
                    lvSolutions.Items.Add(lvi);
                }
            }
            finally
            {
                btnFindSolution.Enabled = true;
                btnStop.Enabled = false;

                UseWaitCursor = false;
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            cancellationTokenSource.Cancel();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Settings.Default.AvailableInspirationals = string.Join(",", lvInspirationals.CheckedItems.OfType<ListViewItem>().Select(i => i.Text));

            Settings.Default.Save();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            var savedInspirationals = new HashSet<string>(Settings.Default.AvailableInspirationals.Split(','));

            foreach (var inspirational in InspirationalDatabase.Inspirationals)
            {
                var lvi = new ListViewItem();
                lvi.Text = inspirational.Name;
                lvi.Tag = inspirational;
                lvi.Checked = savedInspirationals.Contains(inspirational.Name);
                lvi.SubItems.Add(inspirational.Inspiration.ToString("###,###,###"));

                lvInspirationals.Items.Add(lvi);
            }
        }

        private void listView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            var lv = (ListView)sender;
            var sorter = (ListViewColumnSorter)lv.ListViewItemSorter;

            if (e.Column == sorter.SortColumn)
            {
                sorter.Order = sorter.Order == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                sorter.SortColumn = e.Column;
                sorter.Order = SortOrder.Ascending;
            }

            lv.Sort();
        }
    }
}
