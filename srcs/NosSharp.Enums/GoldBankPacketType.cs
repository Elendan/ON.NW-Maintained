using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NosSharp.Enums
{
    public enum GoldBankPacketType : byte
    {
        BankMoney = 0,
        Deposit = 1,
        Withdraw = 2,
        OpenBank = 3
    }
}
