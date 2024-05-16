using CruiseDAL.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.Data
{
    public partial class CpDataLayer
    {
        public List<BiomassEquationDO> getBiomassEquations()
        {
            return DAL.From<BiomassEquationDO>().Read().ToList();
        }   //  end getBiomassEquations

        public void SaveBiomassEquations(List<BiomassEquationDO> bioList)
        {
            foreach (BiomassEquationDO beq in bioList)
            {
                DAL.Save(beq);
            }   //  end foreach loop

            return;
        }   //  end SaveBiomassEquations

        public void ClearBiomassEquations()
        {
            //  make sure filename is complete
            //fileName = checkFileName(fileName);

            //  open connection and delete data
            //using (SQLiteConnection sqlconn = new SQLiteConnection(fileName))
            //{
            //sqlconn.Open();
            //SQLiteCommand sqlcmd = sqlconn.CreateCommand();

            //  delete all rows
            //sqlcmd.CommandText = "DELETE FROM BiomassEquation WHERE rowid>=0";
            DAL.Execute("DELETE FROM BiomassEquation WHERE rowid >= 0");
            //sqlcmd.ExecuteNonQuery();
            //sqlconn.Close();
            //}   //  end using
            return;
        }   //  end clearBiomassEquations

        public void syncBiomassEquationToV3()
        {
            List<BiomassEquationDO> myBE = this.getBiomassEquations();

            //delete all from V3 file
            DAL_V3.BeginTransaction();
            try
            {
                //make sure the reports is empty.
                DAL_V3.Execute("DELETE FROM BiomassEquation WHERE CruiseID = @p1", CruiseID);

                foreach (BiomassEquationDO bioEq in myBE)
                {
                    DAL_V3.Execute2(
@"INSERT INTO BiomassEquation (
    CruiseID,
    Species,
    Product,
    Component,
    LiveDead,
    FIAcode,
    Equation,
    PercentMoisture,
    PercentRemoved,
    MetaData,
    WeightFactorPrimary,
    WeightFactorSecondary
) 
VALUES ( 
    @CruiseID,
    @Species,
    @Product,
    @Component,
    @LiveDead,
    @FIAcode,
    @Equation,
    @PercentMoisture,
    @PercentRemoved,
    @MetaData,
    @WeightFactorPrimary,
    @WeightFactorSecondary
);",
                    new
                    {
                        CruiseID,
                        bioEq.Species,
                        bioEq.Product,
                        bioEq.Component,
                        bioEq.LiveDead,
                        bioEq.FIAcode,
                        bioEq.Equation,
                        bioEq.PercentMoisture,
                        bioEq.PercentRemoved,
                        bioEq.MetaData,
                        bioEq.WeightFactorPrimary,
                        bioEq.WeightFactorSecondary
                    });

                }//end foreach

                DAL_V3.CommitTransaction();
            }
            catch
            {
                DAL_V3.RollbackTransaction();
                throw;
            }

        }//end sync biomass
    }
}
