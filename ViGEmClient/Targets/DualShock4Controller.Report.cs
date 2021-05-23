using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Nefarius.ViGEm.Client.Exceptions;
using Nefarius.ViGEm.Client.Targets.DualShock4;

namespace Nefarius.ViGEm.Client.Targets
{
    internal partial class DualShock4Controller
    {
        public void SetButtonState(DualShock4Button button, bool pressed)
        {
            switch (button)
            {
                case DualShock4SpecialButton specialButton:
                    if (pressed)
                        _nativeReport.bSpecial |= (byte)specialButton.Value;
                    else
                        _nativeReport.bSpecial &= (byte)~specialButton.Value;
                    break;
                case DualShock4Button normalButton:
                    if (pressed)
                        _nativeReport.wButtons |= (ushort)normalButton.Value;
                    else
                        _nativeReport.wButtons &= (ushort)~normalButton.Value;
                    break;
            }

            if (AutoSubmitReport)
                SubmitNativeReport(_nativeReport);
        }

        public void SetDPadDirection(DualShock4DPadDirection direction)
        {
            _nativeReport.wButtons &= unchecked((ushort)~0xF);
            _nativeReport.wButtons |= direction.Value;

            if (AutoSubmitReport)
                SubmitNativeReport(_nativeReport);
        }

        public void SetAxisValue(DualShock4Axis axis, byte value)
        {
            switch (axis.Name)
            {
                case "LeftThumbX":
                    _nativeReport.bThumbLX = value;
                    break;
                case "LeftThumbY":
                    _nativeReport.bThumbLY = value;
                    break;
                case "RightThumbX":
                    _nativeReport.bThumbRX = value;
                    break;
                case "RightThumbY":
                    _nativeReport.bThumbRY = value;
                    break;
            }

            if (AutoSubmitReport)
                SubmitNativeReport(_nativeReport);
        }

        public void SetIMUValue(DualShock4IMU axis, short value) {
            switch (axis.Name) {
                case "GyroX":
                    _nativeReport.wGyroX = value;
                    break;
                case "GyroY":
                    _nativeReport.wGyroY = value;
                    break;
                case "GyroZ":
                    _nativeReport.wGyroZ = value;
                    break;
                case "AccelX":
                    _nativeReport.wAccelX = value;
                    break;
                case "AccelY":
                    _nativeReport.wAccelY = value;
                    break;
                case "AccelZ":
                    _nativeReport.wAccelZ = value;
                    break;
            }

            if (AutoSubmitReport)
                SubmitNativeReport(_nativeReport);
        }

        public void SetIMUTimestamp(ushort ts) {
            _nativeReport.wTimestamp = ts;

            if (AutoSubmitReport)
                SubmitNativeReport(_nativeReport);
        }

        public void SetTouchNumber(byte n) {
            _nativeReport.bTouchPacketsN = n;
        }

        public void PresetTouchpad() {
            _nativeReport.sPreviousTouch1 = _nativeReport.sPreviousTouch0;
            _nativeReport.sPreviousTouch0 = _nativeReport.sCurrentTouch;

            _nativeReport.sCurrentTouch.bPacketCounter += 1;
        }

        unsafe public void SetTouchpad(bool fingerOne, ushort x, ushort y, byte touchId, bool active) {
            if (fingerOne) {
                _nativeReport.sCurrentTouch.bIsUpTrackingNum1 = (byte)(((!active ? 1 : 0) << 7) + touchId);
                _nativeReport.sCurrentTouch.bTouchData1[0] = (byte)(x & 0xFF);
                _nativeReport.sCurrentTouch.bTouchData1[1] = (byte)(((x & 0xF00) >> 8) | ((y & 0xF) << 4));
                _nativeReport.sCurrentTouch.bTouchData1[2] = (byte)((y & 0xFF0) >> 4);
            } else {
                _nativeReport.sCurrentTouch.bIsUpTrackingNum2 = (byte)(((!active ? 1 : 0) << 7) + touchId);
                _nativeReport.sCurrentTouch.bTouchData2[0] = (byte)(x & 0xFF);
                _nativeReport.sCurrentTouch.bTouchData2[1] = (byte)(((x & 0xF00) >> 8) | ((y & 0xF) << 4));
                _nativeReport.sCurrentTouch.bTouchData2[2] = (byte)((y & 0xFF0) >> 4);
            }

            if (AutoSubmitReport)
                SubmitNativeReport(_nativeReport);
        }

        public void SetSliderValue(DualShock4Slider slider, byte value)
        {
            switch (slider.Name)
            {
                case "LeftTrigger":
                    _nativeReport.bTriggerL = value;
                    break;
                case "RightTrigger":
                    _nativeReport.bTriggerR = value;
                    break;
            }

            if (AutoSubmitReport)
                SubmitNativeReport(_nativeReport);
        }

        public void SubmitRawReport(byte[] buffer)
        {
            if (buffer.Length != Marshal.SizeOf<ViGEmClient.DS4_REPORT_EX>())
                throw new ArgumentOutOfRangeException(nameof(buffer), "Supplied buffer has invalid size.");

            _nativeReportEx.Report = buffer;

            var error = ViGEmClient.vigem_target_ds4_update_ex(Client.NativeHandle, NativeHandle, _nativeReportEx);

            switch (error)
            {
                case ViGEmClient.VIGEM_ERROR.VIGEM_ERROR_NONE:
                    break;
                case ViGEmClient.VIGEM_ERROR.VIGEM_ERROR_BUS_INVALID_HANDLE:
                    throw new VigemBusInvalidHandleException();
                case ViGEmClient.VIGEM_ERROR.VIGEM_ERROR_INVALID_TARGET:
                    throw new VigemInvalidTargetException();
                case ViGEmClient.VIGEM_ERROR.VIGEM_ERROR_BUS_NOT_FOUND:
                    throw new VigemBusNotFoundException();
                case ViGEmClient.VIGEM_ERROR.VIGEM_ERROR_NOT_SUPPORTED:
                    throw new VigemNotSupportedException();
                default:
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }
    }
}
