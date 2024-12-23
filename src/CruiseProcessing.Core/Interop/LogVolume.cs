using System;

namespace CruiseProcessing.Interop
{
    public class LogVolume
    {
        public LogVolume()
        { }

        public float this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0: return GrossBoardFoot;
                    case 1: return GrossRemovedBoardFoot;
                    case 2: return NetBoardFoot;
                    case 3: return GrossCubicFoot;
                    case 4: return GrossRemovedCubicFoot;
                    case 5: return NetCubicFoot;
                    case 6: return GrossBoardFootInternational;
                    default: throw new ArgumentOutOfRangeException(nameof(i));
                }
            }
            set
            {
                switch (i)
                {
                    case 0: GrossBoardFoot = value; break;
                    case 1: GrossRemovedBoardFoot = value; break;
                    case 2: NetBoardFoot = value; break;
                    case 3: GrossCubicFoot = value; break;
                    case 4: GrossRemovedCubicFoot = value; break;
                    case 5: NetCubicFoot = value; break;
                    case 6: GrossBoardFootInternational = value; break;
                    default: throw new ArgumentOutOfRangeException(nameof(i));
                }
            }
        }

        public float GrossBoardFoot { get; set; }
        public float GrossRemovedBoardFoot { get; set; }
        public float NetBoardFoot { get; set; }
        public float GrossCubicFoot { get; set; }
        public float GrossRemovedCubicFoot { get; set; }
        public float NetCubicFoot { get; set; }
        public float GrossBoardFootInternational { get; set; }

        //public void FromArray(float[] values)
        //{
        //    if (values.Length != 7)
        //    {
        //        throw new ArgumentException("values must have 7 elements");
        //    }
        //    GrossBoardFoot = values[0];
        //    GrossRemovedBoardFoot = values[1];
        //    NetBoardFoot = values[2];
        //    GrossCubicFoot = values[3];
        //    GrossRemovedCubicFoot = values[4];
        //    NetCubicFoot = values[5];
        //    GrossBoardFootInternational = values[6];
        //}

        public LogVolume FromArray(float[,] values, int row)
        {
            if (values.GetLength(1) != VolumeLibraryInterop.VOLLIBNVB_LOGVOL_SIZE_Y)
            {
                throw new ArgumentException("values should have 7 elements in second dimension");
            }
            GrossBoardFoot = values[row, 0];
            GrossRemovedBoardFoot = values[row, 1];
            NetBoardFoot = values[row, 2];
            GrossCubicFoot = values[row, 3];
            GrossRemovedCubicFoot = values[row, 4];
            NetCubicFoot = values[row, 5];
            GrossBoardFootInternational = values[row, 6];

            return this;
        }
    }
}