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
        public List<ValueEquationDO> getValueEquations()
        {
            return DAL.From<ValueEquationDO>().Read().ToList();
        }   //  end getValueEquations

        public void SaveValueEquations(List<ValueEquationDO> valList)
        {
            //  need to delete equations in order to update the database
            //  not sure why this has to happen but the way the DAL save works
            //  is if the user deleted an equation, the Save does not consider that
            //  when the equation list is updated ???????
            //  it also doesn't recognize deleting a row and then adding it back in
            //  says it's a constraint violation.  need an undo method for deletes
            //  December 2013
            List<ValueEquationDO> vList = getValueEquations();
            foreach (ValueEquationDO v in vList)
            {
                int nthRow = valList.FindIndex(
                    delegate (ValueEquationDO ved)
                    {
                        return ved.ValueEquationNumber == v.ValueEquationNumber && ved.Species == v.Species;
                    });
                if (nthRow < 0)
                    v.Delete();
            }   //  end foreach loop
            foreach (ValueEquationDO veq in valList)
            {
                if (veq.DAL == null)
                {
                    veq.DAL = DAL;
                }
                veq.Save();
            }   //  end foreach loop

            return;
        }   //  end SaveValueEquations

        public void syncValueEquationToV3()
        {
            List<ValueEquationDO> myVE = this.getValueEquations();

            //delete all from V3 file
            DAL_V3.BeginTransaction();
            try
            {
                //make sure the reports is empty.
                DAL_V3.Execute("DELETE FROM ValueEquation WHERE CruiseID = @p1;", CruiseID);


                foreach (ValueEquationDO ve in myVE)
                {
                    DAL_V3.Execute2(
                    @"INSERT INTO ValueEquation (
                    CruiseID,
                    Species,
                    PrimaryProduct,
                    ValueEquationNumber,
                    Grade,
                    Coefficient1,
                    Coefficient2,
                    Coefficient3,
                    Coefficient4,
                    Coefficient5,
                    Coefficient6
                
                ) VALUES (
                    @CruiseID,
                    @Species,
                    @PrimaryProduct,
                    @ValueEquationNumber,
                    @Grade,
                    @Coefficient1,
                    @Coefficient2,
                    @Coefficient3,
                    @Coefficient4,
                    @Coefficient5,
                    @Coefficient6
                );",
                    new
                    {
                        CruiseID,
                        ve.Species,
                        ve.PrimaryProduct,
                        ve.ValueEquationNumber,
                        ve.Grade,
                        ve.Coefficient1,
                        ve.Coefficient2,
                        ve.Coefficient3,
                        ve.Coefficient4,
                        ve.Coefficient5,
                        ve.Coefficient6
                    });


                }//end for each

                DAL_V3.CommitTransaction();
            }
            catch
            {
                DAL_V3.RollbackTransaction();
                throw;
            }
        }
    }
}
