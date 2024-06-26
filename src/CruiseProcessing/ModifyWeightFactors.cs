﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;
using CruiseProcessing.Data;
using Microsoft.Extensions.DependencyInjection;

namespace CruiseProcessing
{
    public partial class ModifyWeightFactors : Form
    {
        public List<BiomassEquationDO> bioList = new List<BiomassEquationDO>();
        protected CpDataLayer DataLayer { get; }
        public IServiceProvider Services { get; }

        protected ModifyWeightFactors()
        {
            InitializeComponent();
        }

        public ModifyWeightFactors(CpDataLayer dataLayer, IServiceProvider services)
            : this()
        {
            DataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }


        public int setupDialog()
        {
            //  first, ask the security question and only allow correct answer to proceed
            SecurityQuestion sq = Services.GetRequiredService<SecurityQuestion>();
            sq.ShowDialog();
            if (sq.securityResponse != "OK")
            {
                Close();
                return -1;
            }

            //  Show this message box until security question is OKd by measurement specialists.  04/2013
            //MessageBox.Show("In the future, a security question will be displayed here.", "INFORMATION", MessageBoxButtons.OK, MessageBoxIcon.Information);

            //  if there are biomass equations, bind to grid
            //  else show message and return
            bioList = DataLayer.getBiomassEquations();
            if (bioList.Count == 0)
            {
                MessageBox.Show("There are no biomass equations available for updating.\n Cannot continue.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Close();
                return 0;
            }   //  endif bioList is empty
            biomassEquationDOBindingSource.DataSource = bioList;
            biomassFactorsList.DataSource = biomassEquationDOBindingSource;
            return 1;
        }   //  end setupDialog


        private void onCancel(object sender, EventArgs e)
        {
            DialogResult nResult = MessageBox.Show("Are you sure you want to cancel?", "CONFIRMATION", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (nResult == DialogResult.Yes)
            {
                Close();
                return;
            }   //  endif on result
        }   //  end onCancel

        private void onFinished(object sender, EventArgs e)
        {
            //  Make sure user wants to save changes made
            DialogResult nResult = MessageBox.Show("Do you want to save changes made?", "CONFIRMATION", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (nResult == DialogResult.Yes)
            {
                Cursor.Current = Cursors.WaitCursor;
                //  
                DataLayer.SaveBiomassEquations(bioList);

                Cursor.Current = this.Cursor;
            }   //  endif nResult
            Close();
            return;
        }   //  end onFinished
    }
}
