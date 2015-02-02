using SalemOptimizer.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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

            ClientSize = Settings.Default.MainFormClientSize;
            cbResultCount.SelectedIndex = 0;
        }

        private CancellationTokenSource cancellationTokenSource;

        private async void btnFindSolution_Click(object sender, EventArgs e)
        {
            if (lvInspirationals.CheckedItems.Count == 0 && !cbUseAll.Checked)
            {
                toolTipError.Show("You have to select at least one available inspirational.", lblInspirationals, 1000);

                return;
            }

            await FindSolution(false);
        }

        Organism[] bestOrganisms;

        private async Task FindSolution(bool useLastBest)
        {
            try
            {
                btnFindSolution.Enabled = false;
                btnRetryWithBest.Enabled = false;
                btnStop.Enabled = true;

                UseWaitCursor = true;

                var prune = cbxPrune.Checked;

                var problem =
                    new Problem()
                    .Add(ProficiencyKind.ArtsAndCrafts, nudArtsAndCrafts.Value)
                    .Add(ProficiencyKind.CloakAndDagger, nudCloakAndDagger.Value)
                    .Add(ProficiencyKind.FaithAndWisdom, nudFaithAndWisdom.Value)
                    .Add(ProficiencyKind.FloraAndFauna, nudFloraAndFauna.Value)
                    .Add(ProficiencyKind.HammerAndNail, nudHammerAndNail.Value)
                    .Add(ProficiencyKind.HerbsAndSprouts, nudHerbsAndSprouts.Value)
                    .Add(ProficiencyKind.HuntingAndHideworking, nudHuntingAndHideworking.Value)
                    .Add(ProficiencyKind.LawAndLore, nudLawAndLore.Value)
                    .Add(ProficiencyKind.MinesAndMountains, nudMinesAndMountains.Value)
                    .Add(ProficiencyKind.NaturalPhilosophy, nudNaturalPhilosophy.Value)
                    .Add(ProficiencyKind.PerenialPhilosophy, nudPerenialPhilosophy.Value)
                    .Add(ProficiencyKind.SparksAndEmbers, nudSparksAndEmbers.Value)
                    .Add(ProficiencyKind.StocksAndCultivars, nudStocksAndCultivars.Value)
                    .Add(ProficiencyKind.SugarAndSpice, nudSugarAndSpice.Value)
                    .Add(ProficiencyKind.ThreadAndNeedle, nudThreadAndNeedle.Value);

                Inspirational[] availableInspirationals;
                if (cbUseAll.Checked)
                {
                     availableInspirationals = InspirationalDatabase.Inspirationals.ToArray();
                }
                else
                {
                     availableInspirationals = lvInspirationals.CheckedItems.OfType<ListViewItem>().Select(i => ((Inspirational)i.Tag)).ToArray();
                }

                cancellationTokenSource = new CancellationTokenSource();

                var solvers = Enumerable.Range(1, 8).Select(i => Task.Run(() => new Solver(problem, availableInspirationals, cancellationTokenSource.Token, useLastBest ? bestOrganisms : null).Solve(prune))).ToArray();

                var best = await Task.WhenAll(solvers);

                // Aggregate the best results and fill them into the results control
                var leaderboard = new Leaderboard(int.Parse(cbResultCount.Items[cbResultCount.SelectedIndex].ToString()), prune);

                foreach (var organism in best.SelectMany(i => i)) leaderboard.AddOrganism(organism);

                var bestOfAll = leaderboard.GetBest();

                bestOrganisms = bestOfAll.Select(i => i.Clone(null)).ToArray();

                lvSolutions.Items.Clear();

                foreach (var organism in bestOfAll)
                {
                    var lvi = new ListViewItem();
                    lvi.Tag = organism;
                    lvi.Text = organism.ToString();
                    lvi.ForeColor = organism.Solution.IncompletenessPenalty == 0 ? Color.Green : Color.Red;
                    lvi.SubItems.Add(organism.Solution.CostTotal.ToString("###,###,###"));
                    lvi.SubItems.Add(organism.Solution.DiffTotal.ToString());
                    lvSolutions.Items.Add(lvi);
                }
            }
            finally
            {
                btnFindSolution.Enabled = true;
                btnRetryWithBest.Enabled = bestOrganisms != null;
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
            Settings.Default.MainFormClientSize = ClientSize;

            Settings.Default.Save();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            RefreshLvInsp();
            foreach (Skill skill in SkillDatabase.Skills)
            {
                combSkill.Items.Add(skill.Name);
            }
        }

        public void RefreshLvInsp()
        {

            string savedinsp = string.Join(",", lvInspirationals.CheckedItems.OfType<ListViewItem>().Select(i => i.Text));
            lvInspirationals.Items.Clear();
            var savedInspirationals = new HashSet<string>(Settings.Default.AvailableInspirationals.Split(','));
            foreach (var inspirational in InspirationalDatabase.Inspirationals)
            {
                var lvi = new ListViewItem();
                lvi.Text = inspirational.Name;
                lvi.Tag = inspirational;
                lvi.Checked = savedInspirationals.Contains(inspirational.Name);
                lvi.SubItems.Add(inspirational.Inspiration.ToString("###,###,###"));
                lvi.SubItems.Add(inspirational.Diff.ToString());

                lvInspirationals.Items.Add(lvi);
            }
            //Settings.Default.AvailableInspirationals = savedinsp;
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

        private void btnResetAll_Click(object sender, EventArgs e)
        {
            nudArtsAndCrafts.Value = 0;
            nudCloakAndDagger.Value = 0;
            nudFaithAndWisdom.Value = 0;
            nudFloraAndFauna.Value = 0;
            nudHammerAndNail.Value = 0;
            nudHerbsAndSprouts.Value = 0;
            nudHuntingAndHideworking.Value = 0;
            nudLawAndLore.Value = 0;
            nudMinesAndMountains.Value = 0;
            nudNaturalPhilosophy.Value = 0;
            nudPerenialPhilosophy.Value = 0;
            nudSparksAndEmbers.Value = 0;
            nudStocksAndCultivars.Value = 0;
            nudSugarAndSpice.Value = 0;
            nudThreadAndNeedle.Value = 0;
        }

        private async void btnRetryWithBest_Click(object sender, EventArgs e)
        {
            if (lvInspirationals.CheckedItems.Count == 0)
            {
                toolTipError.Show("You have to select at least one available inspirational.", lblInspirationals, 1000);

                return;
            }

            await FindSolution(true);
        }

        private void lvSolutions_ItemActivate(object sender, EventArgs e)
        {
            if (lvSolutions.SelectedItems.Count == 0) return;

            SolutionDetailForm.ShowDetail((Organism)lvSolutions.SelectedItems[0].Tag);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DiffSet frmDiff = new DiffSet(RefreshLvInsp);
            frmDiff.Show();
        }

        private void combSkill_SelectedIndexChanged(object sender, EventArgs e)
        {
            Skill skill = SkillDatabase.Skills[combSkill.SelectedIndex];
            nudArtsAndCrafts.Value = skill.Proficiencies[(int)ProficiencyKind.ArtsAndCrafts];
            nudCloakAndDagger.Value = skill.Proficiencies[(int)ProficiencyKind.CloakAndDagger];
            nudFaithAndWisdom.Value = skill.Proficiencies[(int)ProficiencyKind.FaithAndWisdom];
            nudFloraAndFauna.Value = skill.Proficiencies[(int)ProficiencyKind.FloraAndFauna];
            nudHammerAndNail.Value = skill.Proficiencies[(int)ProficiencyKind.HammerAndNail];
            nudHerbsAndSprouts.Value = skill.Proficiencies[(int)ProficiencyKind.HerbsAndSprouts];
            nudHuntingAndHideworking.Value = skill.Proficiencies[(int)ProficiencyKind.HuntingAndHideworking];
            nudLawAndLore.Value = skill.Proficiencies[(int)ProficiencyKind.LawAndLore];
            nudMinesAndMountains.Value = skill.Proficiencies[(int)ProficiencyKind.MinesAndMountains];
            nudNaturalPhilosophy.Value = skill.Proficiencies[(int)ProficiencyKind.NaturalPhilosophy];
            nudPerenialPhilosophy.Value = skill.Proficiencies[(int)ProficiencyKind.PerenialPhilosophy];
            nudSparksAndEmbers.Value = skill.Proficiencies[(int)ProficiencyKind.SparksAndEmbers];
            nudStocksAndCultivars.Value = skill.Proficiencies[(int)ProficiencyKind.StocksAndCultivars];
            nudSugarAndSpice.Value = skill.Proficiencies[(int)ProficiencyKind.SugarAndSpice];
            nudThreadAndNeedle.Value = skill.Proficiencies[(int)ProficiencyKind.ThreadAndNeedle];
        }
    }
}
