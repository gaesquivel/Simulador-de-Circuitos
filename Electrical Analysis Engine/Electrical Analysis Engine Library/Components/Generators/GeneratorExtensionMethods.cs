using CircuitMVVMBase;
using CircuitMVVMBase.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
//using ParseUtils;

namespace ElectricalAnalysis.Components.Generators
{
    public static class GeneratorExtensionMethods
    {

        public static ElectricComponent Parse(this Generator generator, Circuit owner, string component)
        {
            if (component.ToUpper().Contains("SIN"))
            {
                return ((ISineGenerator)generator).Parse(owner, component);
            }
            else if (component.ToUpper().Contains("PULSE"))
            {
                return ((IPulseGenerator)generator).Parse(owner, component);
            }
            else if (component.ToUpper().Contains("AC"))
            {
                #region AC

                string[] elemn = component.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string[] comp1 = elemn[0].Split("_".ToCharArray());

                ACVoltageGenerator ac = new ACVoltageGenerator(owner, comp1[1], elemn[4]);
                double v1 = 0, v2 = 0;
                if (!StringUtils.DecodeString(elemn[6], out v1))
                {
                    NotificationsVM.Instance.Notifications.Add(new Notification("Error to parse: " + elemn[6], Notification.ErrorType.error));
                    return null;
                }
                if (elemn.Length == 8 && StringUtils.DecodeString(elemn[7], out v2))
                    ac.ACVoltage = Complex.FromPolarCoordinates(v1, v2);
                else
                    ac.ACVoltage = new Complex(v1, 0);
                return (ACVoltageGenerator)ac;
                #endregion
            }
            else if (component.ToUpper().Contains("DC"))
            {
                string[] elemn = component.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string[] comp1 = elemn[0].Split("_".ToCharArray());
                if (elemn.Length == 4)
                    return new VoltageGenerator(owner, comp1[1], elemn[3]);
                if (elemn.Length == 5)
                    return new VoltageGenerator(owner, comp1[1], elemn[4]);
            }
            return null;
        }

        public static ElectricComponent Parse(this ISineGenerator generator, Circuit owner, string component)
        {
            if (!component.ToUpper().Contains("SIN"))
                return null;


            //ltspice
            //V1 s 0 SINE(0 10 1k 10u) AC 1

            //orcad
            //V_V1  n1 n2 DC 0 AC 1
            //+SIN 1 1 1k 0 0 0
            try
            {
                string[] elemn = component.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string[] comp1 = elemn[0].Split("_".ToCharArray());

                SineVoltageGenerator vsin = new SineVoltageGenerator(owner, comp1[1]);

                double val = 0;
                if (!ParseUtils.ParseValue(elemn, 6, out val, 0))
                    return null;
                vsin.ACVoltage = val;
                if (!ParseUtils.ParseValue(elemn, 4, out val, 0))
                    return null;
                vsin.Value = val;

                //if (ParseValue(elemn, 9, out val, 0))
                //    return null;
                string result = "";
                if (!ParseUtils.ParseStringValue(elemn, 9, ref result))
                    return null;
                vsin.Amplitude = result;

                if (!ParseUtils.ParseStringValue(elemn, 8, ref result))
                    return null;
                vsin.Offset = result;

                if (!ParseUtils.ParseStringValue(elemn, 10, ref result, "1K", false))
                    return null;
                vsin.Frequency = result;

                if (!ParseUtils.ParseStringValue(elemn, 12, ref result, "0", false))
                    return null;
                vsin.Thau = result;

                if (!ParseUtils.ParseStringValue(elemn, 11, ref result, "0", false))
                    return null;
                vsin.Delay = result;

                if (!ParseUtils.ParseStringValue(elemn, 13, ref result, "0",false))
                    return null;
                vsin.Phase = result;

                return vsin;
            }
            catch (Exception ex)
            {
                NotificationsVM.Instance.Notifications.Add(
                    new Notification(ex));
            }

            return null;
        }

        #region private methods


        #endregion

        public static ElectricComponent Parse(this IPulseGenerator generator, Circuit owner, string component)
        {
            if (!component.ToUpper().Contains("PULSE"))
                return null;


            //ltSpice
            //V1 s 0 PULSE(0 10 10u 1n 1n 500u 1m 6) AC 1

            //Me
            //V1 n1 n2 DC 0 AC 1 PULSE offset amplitude Frequency Ton Delay Trise Tfall Cicles
            //Default     0    1         0       1           1K    50   0     0    0     oo
            //V1 n1 n2 DC 0 AC 1 PULSE 0 10 10u 1n 1n 500u 1m 6
            try
            {
                string[] elemn = component.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string[] comp1 = elemn[0].Split("_".ToCharArray());

                PulseVoltageGenerator vpul = new PulseVoltageGenerator(owner, comp1[1]);
                double val = 0;
                if (StringUtils.DecodeString(elemn[6], out val))
                    vpul.ACVoltage = new Complex(val, 0);
                else
                    NotificationsVM.Instance.Notifications.Add(new Notification("Invalid value:" + elemn[6], Notification.ErrorType.error));
                if (StringUtils.DecodeString(elemn[4], out val))    //DC
                    vpul.Value = val;
                else
                    NotificationsVM.Instance.Notifications.Add(new Notification("Invalid value:" + elemn[4], Notification.ErrorType.error));

                string value = "";
                if (!ParseUtils.ParseStringValue(elemn, 8, ref value, "0", false))
                    return null;
                vpul.Offset = value;

                if (!ParseUtils.ParseStringValue(elemn, 9, ref value, "0", false))
                    return null;
                vpul.Amplitude = value;

                if (!ParseUtils.ParseStringValue(elemn, 10, ref value, "0", false))
                    return null;
                vpul.Frequency = value;
               
                if (!ParseUtils.ParseValue(elemn, 11, out val, 50))
                    return null;
                vpul.OnTime = (float)val;

                if (!ParseUtils.ParseStringValue(elemn, 12, ref value, "0", false))
                    return null;
                vpul.Delay = value;

                if (!ParseUtils.ParseStringValue(elemn, 13, ref value, "0", false))
                    return null;
                vpul.RiseTime = value;

                if (!ParseUtils.ParseStringValue(elemn, 14, ref value, "0", false))
                    return null;
                vpul.FallTime = value;

                if (!ParseUtils.ParseStringValue(elemn, 15, ref value, "0", false))
                    return null;
                vpul.Cicles = value;
                
                return vpul;
            }
            catch (Exception ex)
            {
                NotificationsVM.Instance.Notifications.Add(
                    new Notification(ex));
            }
            
            return null;
        }

    }
}
