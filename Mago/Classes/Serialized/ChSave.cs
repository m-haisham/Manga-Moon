using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mago
{
    [Serializable]
    public class ChSave
    {
        public List<byte[]> Images;

        public ChSave()
        {
            Images = new List<byte[]>();
        }

        public ChSave(List<byte[]> list)
        {
            Images = list;
        }
    }
}
