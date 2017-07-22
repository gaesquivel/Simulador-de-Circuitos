using CircuitMVVMBase;
using CircuitMVVMBase.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalAnalysis
{
    public static class ParseUtils
    {


        public static bool ParseStringValue(string[] elemn, int Index,
                                    ref string Value, string defaultValue = "",
                                    bool IsNeeded = true)
        {
            if (elemn.Length <= Index)
            {
                Value = defaultValue;
                if (IsNeeded)
                    return false;
                return true;
            }
            Value = elemn[Index];
            return true;
        }

        public static bool ParseValue(string[] elemn, int Index,
                                        out double Result,
                                        double DefaultValue = 0.0,
                                        bool IsOptional = false)
        {
            Result = DefaultValue;
            if (elemn.Length <= Index)
            {
                if (IsOptional)
                    return false;
                return true;
            }

            double val;
            if (StringUtils.DecodeString(elemn[Index], out val))
            {
                Result = val;
                return true;
            }
            else
                NotificationsVM.Instance.Notifications.Add(
                    new Notification("Invalid value:" + elemn[Index],
                                        Notification.ErrorType.error));
            return false;
        }

    }
}
