using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SalemOptimizer
{
    public delegate void CallbackDelegate(); 

    public partial class DiffSet : Form
    {
        //List<Inspirational> Inspirationals;
        CallbackDelegate callback;
        
        public DiffSet(CallbackDelegate func)
        {
            InitializeComponent();
            callback = func;
            //Inspirationals = insplist;
        }

        private void DiffSet_Load(object sender, EventArgs e)
        {
            foreach (var inspirational in InspirationalDatabase.Inspirationals)
            {
                
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dGrid);
                row.Cells[0].Value = inspirational.Id;
                row.Cells[1].Value = inspirational.Name;
                row.Cells[2].Value = inspirational.Diff;


                dGrid.Rows.Add(row);
            }
        }

        private void ChangeDiff(int id, int diff)
        {
            foreach (var insp in InspirationalDatabase.Inspirationals)
            {
                if(insp.Id == id){
                    insp.Diff = diff;
                    break;
                }
            }
        }



        private void dGrid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            ChangeDiff((int)dGrid.Rows[e.RowIndex].Cells[0].Value, (int)dGrid.Rows[e.RowIndex].Cells[2].Value);
        }

        private void DiffSet_FormClosed(object sender, FormClosedEventArgs e)
        {
            InspirationalDatabase.SaveToFile("Inspirationals.tab");
            callback();
        }

        private void dGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            ChangeDiff((int)dGrid.Rows[e.RowIndex].Cells[0].Value, int.Parse(dGrid.Rows[e.RowIndex].Cells[2].Value.ToString()));
        }
    }
}
