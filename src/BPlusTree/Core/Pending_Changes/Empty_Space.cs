using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BPlusTree.Core.Pending_Changes
{
    public class Empty_Space
    {
        Dictionary<long, Block> _base_Address_Index = new Dictionary<long, Block>();
        Dictionary<long, Block> _end_Address_Index = new Dictionary<long, Block>();

        List<Block_Group> Empty_Slots;

    }


 
}
