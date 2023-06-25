﻿namespace Ryujinx.HLE.HOS.Services.Pcv.Types
{
    enum DeviceCode
    {
        Cpu = 0x40000001,
        Gpu = 0x40000002,
        I2s1 = 0x40000003,
        I2s2 = 0x40000004,
        I2s3 = 0x40000005,
        Pwm = 0x40000006,
        I2c1 = 0x02000001,
        I2c2 = 0x02000002,
        I2c3 = 0x02000003,
        I2c4 = 0x02000004,
        I2c5 = 0x02000005,
        I2c6 = 0x02000006,
        Spi1 = 0x07000000,
        Spi2 = 0x07000001,
        Spi3 = 0x07000002,
        Spi4 = 0x07000003,
        Disp1 = 0x40000011,
        Disp2 = 0x40000012,
        Isp = 0x40000013,
        Vi = 0x40000014,
        Sdmmc1 = 0x40000015,
        Sdmmc2 = 0x40000016,
        Sdmmc3 = 0x40000017,
        Sdmmc4 = 0x40000018,
        Owr = 0x40000019,
        Csite = 0x4000001A,
        Tsec = 0x4000001B,
        Mselect = 0x4000001C,
        Hda2codec2x = 0x4000001D,
        Actmon = 0x4000001E,
        I2cSlow = 0x4000001F,
        Sor1 = 0x40000020,
        Sata = 0x40000021,
        Hda = 0x40000022,
        XusbCoreHostSrc = 0x40000023,
        XusbFalconSrc = 0x40000024,
        XusbFsSrc = 0x40000025,
        XusbCoreDevSrc = 0x40000026,
        XusbSsSrc = 0x40000027,
        UartA = 0x03000001,
        UartB = 0x35000405,
        UartC = 0x3500040F,
        UartD = 0x37000001,
        Host1x = 0x4000002C,
        Entropy = 0x4000002D,
        SocTherm = 0x4000002E,
        Vic = 0x4000002F,
        Nvenc = 0x40000030,
        Nvjpg = 0x40000031,
        Nvdec = 0x40000032,
        Qspi = 0x40000033,
        ViI2c = 0x40000034,
        Tsecb = 0x40000035,
        Ape = 0x40000036,
        AudioDsp = 0x40000037,
        AudioUart = 0x40000038,
        Emc = 0x40000039,
        Plle = 0x4000003A,
        PlleHwSeq = 0x4000003B,
        Dsi = 0x4000003C,
        Maud = 0x4000003D,
        Dpaux1 = 0x4000003E,
        MipiCal = 0x4000003F,
        UartFstMipiCal = 0x40000040,
        Osc = 0x40000041,
        SysBus = 0x40000042,
        SorSafe = 0x40000043,
        XusbSs = 0x40000044,
        XusbHost = 0x40000045,
        XusbDevice = 0x40000046,
        Extperiph1 = 0x40000047,
        Ahub = 0x40000048,
        Hda2hdmicodec = 0x40000049,
        Gpuaux = 0x4000004A,
        UsbD = 0x4000004B,
        Usb2 = 0x4000004C,
        Pcie = 0x4000004D,
        Afi = 0x4000004E,
        PciExClk = 0x4000004F,
        PExUsbPhy = 0x40000050,
        XUsbPadCtl = 0x40000051,
        Apbdma = 0x40000052,
        Usb2TrkClk = 0x40000053,
        XUsbIoPll = 0x40000054,
        XUsbIoPllHwSeq = 0x40000055,
        Cec = 0x40000056,
        Extperiph2 = 0x40000057,
        OscClk = 0x40000080
    }
}
