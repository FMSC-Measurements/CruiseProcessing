using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;

namespace CruiseProcessing
{
    public partial class GraphReportsDialog : Form
    {
        protected CPbusinessLayer DataLayer { get; }
        private List<ReportsDO> reportList = new List<ReportsDO>();

        protected GraphReportsDialog()
        {
            InitializeComponent();
        }

        public GraphReportsDialog(CPbusinessLayer dataLayer)
            : this()
        {
            DataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
        }

        public void setupDialog()
        {
            //  are there any graph reports selected
            reportList = DataLayer.GetReports();
            List<ReportsDO> justGraphs = reportList.FindAll(
                delegate(ReportsDO r)
                {
                    return r.ReportID.Substring(0, 2) == "GR";
                });
            //  if these reports are not in the list, add them and make all the check boxes false
            if (justGraphs.Count == 0)
            {
                addGraphReports();
                GR01check.Checked = false;
                GR02check.Checked = false;
                GR03check.Checked = false;
                GR04check.Checked = false;
                GR05check.Checked = false;
                GR06check.Checked = false;
                GR07check.Checked = false;
                GR08check.Checked = false;
                GR09check.Checked = false;
                GR10check.Checked = false;
                GR11check.Checked = false;
            }
            else if (justGraphs.Count > 0)
            {
                foreach (ReportsDO jg in justGraphs)
                {
                    switch (jg.ReportID)
                    {
                        case "GR01":
                            if (jg.Selected)
                                GR01check.Checked = true;
                            break;
                        case "GR02":
                            if (jg.Selected)
                                GR02check.Checked = true;
                            break;
                        case "GR03":
                            if (jg.Selected)
                                GR03check.Checked = true;
                            break;
                        case "GR04":
                            if (jg.Selected)
                                GR04check.Checked = true;
                            break;
                        case "GR05":
                            if (jg.Selected)
                                GR05check.Checked = true;
                            break;
                        case "GR06":
                            if (jg.Selected)
                                GR06check.Checked = true;
                            break;
                        case "GR07":
                            if (jg.Selected)
                                GR07check.Checked = true;
                            break;
                        case "GR08":
                            if (jg.Selected)
                                GR08check.Checked = true;
                            break;
                        case "GR09":
                            if (jg.Selected)
                                GR09check.Checked = true;
                            break;
                        case "GR10":
                            if (jg.Selected)
                                GR10check.Checked = true;
                            break;
                        case "GR11":
                            if(jg.Selected)
                                GR11check.Checked = true;
                            break;
                    }   //  end switch
                }   //  end foreach loop
            }   //  endif
            return;
        }   //  end setupDialog


        private void onFinished(object sender, EventArgs e)
        {
            //  which graphs were selected?  Set Selected in reportList and then save the list
            if (GR01check.Checked)
                updateList("GR01", true);
            else updateList("GR01", false);
            
            if (GR02check.Checked)
                updateList("GR02", true);
            else updateList("GR02", false); 
            
            if (GR03check.Checked)
                updateList("GR03", true);
            else updateList("GR03", false);

            if (GR04check.Checked)
                updateList("GR04", true);
            else updateList("GR04", false);

            if (GR05check.Checked)
                updateList("GR05", true);
            else updateList("GR05", false);
            
            if (GR06check.Checked)
                updateList("GR06", true);
            else updateList("GR06", false);

            if (GR07check.Checked)
                updateList("GR07", true);
            else updateList("GR07", false);

            if (GR08check.Checked)
                updateList("GR08", true);
            else updateList("GR08", false);

            if (GR09check.Checked)
                updateList("GR09", true);
            else updateList("GR09", false);

            if (GR10check.Checked)
                updateList("GR10", true);
            else updateList("GR10", false);

            if (GR11check.Checked)
                updateList("GR11", true);
            else updateList("GR11", false);

            //  save reports list
            DataLayer.SaveReports(reportList);
            Close();
            return;
        }   //  end onFinished


        private void updateList(string currRept, bool setSelected)
        {
            //  find report in list and set Selected
            int nthRow = reportList.FindIndex(
                delegate(ReportsDO r)
                {
                    return r.ReportID == currRept;
                });
            if (nthRow >= 0)
                reportList[nthRow].Selected = setSelected;
            return;
        }   //  end updateList


        private void addGraphReports()
        {
            foreach(var a in ReportsDataservice.GRAPH_REPORTS)
            {
                ReportsDO rd = new ReportsDO();
                rd.ReportID = a[0];
                rd.Selected = false;
                rd.Title = a[1];
                reportList.Add(rd);
            }
        }

        private void GR11check_CheckedChanged(object sender, EventArgs e)
        {

        }   //  end addGraphReports
    }
}
