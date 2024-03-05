using CruiseDAL.DataObjects;
using System;
using System.Collections.Generic;

namespace CruiseProcessing
{
    public static class SaleMethods
    {
        //  this will contain methods such as retrieving region or district or whatever from SaleDO
        //  and edit checks
        public static int MoreThanOne(List<SaleDO> saleList)
        {
            if (saleList.Count > 1)
                return 28;
            else if (saleList.Count == 0)
                return 0;
            else return 1;
        }   //  end MoreThanOne

        public static int BlankSaleNum(SaleDO sale)
        {
            if (String.IsNullOrEmpty(sale.SaleNumber)) { return 8; }

            return -1;
        }   //  end BlankSaleNum
    }   //  end SaleMethods
}