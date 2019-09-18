using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CruiseDAL.DataObjects;
using CruiseDAL.Schema;

namespace CruiseProcessing
{
    public static class SaleMethods
    {
        //  this will contain methods such as retrieving region or district or whatever from SaleDO
        //  and edit checks
        public static int MoreThanOne(IEnumerable<SaleDO> saleList)
        {
            if (saleList.Count() > 1)
                return 28;
            else if (saleList.Count() == 0)
                return 0;
            else return 1;
        }   //  end MoreThanOne


        public static int BlankSaleNum(IEnumerable<SaleDO> saleList)
        {
            SaleDO sale = saleList.First();
            if (sale.SaleNumber == "" || sale.SaleNumber == null)
                return 8;       //  returns error number for blank field
            else return -1;
        }   //  end BlankSaleNum
    }   //  end SaleMethods
}
