// Constants for Unicorn Engine. AUTO-GENERATED FILE, DO NOT EDIT

// ReSharper disable InconsistentNaming
namespace Ryujinx.Tests.Unicorn.Native.Const
{
    public enum Arm64
    {

        // ARM64 CPU

        CPU_ARM64_A57 = 0,
        CPU_ARM64_A53 = 1,
        CPU_ARM64_A72 = 2,
        CPU_ARM64_MAX = 3,
        CPU_ARM64_ENDING = 4,

        // ARM64 registers

        ARM64_REG_INVALID = 0,
        ARM64_REG_X29 = 1,
        ARM64_REG_X30 = 2,
        ARM64_REG_NZCV = 3,
        ARM64_REG_SP = 4,
        ARM64_REG_WSP = 5,
        ARM64_REG_WZR = 6,
        ARM64_REG_XZR = 7,
        ARM64_REG_B0 = 8,
        ARM64_REG_B1 = 9,
        ARM64_REG_B2 = 10,
        ARM64_REG_B3 = 11,
        ARM64_REG_B4 = 12,
        ARM64_REG_B5 = 13,
        ARM64_REG_B6 = 14,
        ARM64_REG_B7 = 15,
        ARM64_REG_B8 = 16,
        ARM64_REG_B9 = 17,
        ARM64_REG_B10 = 18,
        ARM64_REG_B11 = 19,
        ARM64_REG_B12 = 20,
        ARM64_REG_B13 = 21,
        ARM64_REG_B14 = 22,
        ARM64_REG_B15 = 23,
        ARM64_REG_B16 = 24,
        ARM64_REG_B17 = 25,
        ARM64_REG_B18 = 26,
        ARM64_REG_B19 = 27,
        ARM64_REG_B20 = 28,
        ARM64_REG_B21 = 29,
        ARM64_REG_B22 = 30,
        ARM64_REG_B23 = 31,
        ARM64_REG_B24 = 32,
        ARM64_REG_B25 = 33,
        ARM64_REG_B26 = 34,
        ARM64_REG_B27 = 35,
        ARM64_REG_B28 = 36,
        ARM64_REG_B29 = 37,
        ARM64_REG_B30 = 38,
        ARM64_REG_B31 = 39,
        ARM64_REG_D0 = 40,
        ARM64_REG_D1 = 41,
        ARM64_REG_D2 = 42,
        ARM64_REG_D3 = 43,
        ARM64_REG_D4 = 44,
        ARM64_REG_D5 = 45,
        ARM64_REG_D6 = 46,
        ARM64_REG_D7 = 47,
        ARM64_REG_D8 = 48,
        ARM64_REG_D9 = 49,
        ARM64_REG_D10 = 50,
        ARM64_REG_D11 = 51,
        ARM64_REG_D12 = 52,
        ARM64_REG_D13 = 53,
        ARM64_REG_D14 = 54,
        ARM64_REG_D15 = 55,
        ARM64_REG_D16 = 56,
        ARM64_REG_D17 = 57,
        ARM64_REG_D18 = 58,
        ARM64_REG_D19 = 59,
        ARM64_REG_D20 = 60,
        ARM64_REG_D21 = 61,
        ARM64_REG_D22 = 62,
        ARM64_REG_D23 = 63,
        ARM64_REG_D24 = 64,
        ARM64_REG_D25 = 65,
        ARM64_REG_D26 = 66,
        ARM64_REG_D27 = 67,
        ARM64_REG_D28 = 68,
        ARM64_REG_D29 = 69,
        ARM64_REG_D30 = 70,
        ARM64_REG_D31 = 71,
        ARM64_REG_H0 = 72,
        ARM64_REG_H1 = 73,
        ARM64_REG_H2 = 74,
        ARM64_REG_H3 = 75,
        ARM64_REG_H4 = 76,
        ARM64_REG_H5 = 77,
        ARM64_REG_H6 = 78,
        ARM64_REG_H7 = 79,
        ARM64_REG_H8 = 80,
        ARM64_REG_H9 = 81,
        ARM64_REG_H10 = 82,
        ARM64_REG_H11 = 83,
        ARM64_REG_H12 = 84,
        ARM64_REG_H13 = 85,
        ARM64_REG_H14 = 86,
        ARM64_REG_H15 = 87,
        ARM64_REG_H16 = 88,
        ARM64_REG_H17 = 89,
        ARM64_REG_H18 = 90,
        ARM64_REG_H19 = 91,
        ARM64_REG_H20 = 92,
        ARM64_REG_H21 = 93,
        ARM64_REG_H22 = 94,
        ARM64_REG_H23 = 95,
        ARM64_REG_H24 = 96,
        ARM64_REG_H25 = 97,
        ARM64_REG_H26 = 98,
        ARM64_REG_H27 = 99,
        ARM64_REG_H28 = 100,
        ARM64_REG_H29 = 101,
        ARM64_REG_H30 = 102,
        ARM64_REG_H31 = 103,
        ARM64_REG_Q0 = 104,
        ARM64_REG_Q1 = 105,
        ARM64_REG_Q2 = 106,
        ARM64_REG_Q3 = 107,
        ARM64_REG_Q4 = 108,
        ARM64_REG_Q5 = 109,
        ARM64_REG_Q6 = 110,
        ARM64_REG_Q7 = 111,
        ARM64_REG_Q8 = 112,
        ARM64_REG_Q9 = 113,
        ARM64_REG_Q10 = 114,
        ARM64_REG_Q11 = 115,
        ARM64_REG_Q12 = 116,
        ARM64_REG_Q13 = 117,
        ARM64_REG_Q14 = 118,
        ARM64_REG_Q15 = 119,
        ARM64_REG_Q16 = 120,
        ARM64_REG_Q17 = 121,
        ARM64_REG_Q18 = 122,
        ARM64_REG_Q19 = 123,
        ARM64_REG_Q20 = 124,
        ARM64_REG_Q21 = 125,
        ARM64_REG_Q22 = 126,
        ARM64_REG_Q23 = 127,
        ARM64_REG_Q24 = 128,
        ARM64_REG_Q25 = 129,
        ARM64_REG_Q26 = 130,
        ARM64_REG_Q27 = 131,
        ARM64_REG_Q28 = 132,
        ARM64_REG_Q29 = 133,
        ARM64_REG_Q30 = 134,
        ARM64_REG_Q31 = 135,
        ARM64_REG_S0 = 136,
        ARM64_REG_S1 = 137,
        ARM64_REG_S2 = 138,
        ARM64_REG_S3 = 139,
        ARM64_REG_S4 = 140,
        ARM64_REG_S5 = 141,
        ARM64_REG_S6 = 142,
        ARM64_REG_S7 = 143,
        ARM64_REG_S8 = 144,
        ARM64_REG_S9 = 145,
        ARM64_REG_S10 = 146,
        ARM64_REG_S11 = 147,
        ARM64_REG_S12 = 148,
        ARM64_REG_S13 = 149,
        ARM64_REG_S14 = 150,
        ARM64_REG_S15 = 151,
        ARM64_REG_S16 = 152,
        ARM64_REG_S17 = 153,
        ARM64_REG_S18 = 154,
        ARM64_REG_S19 = 155,
        ARM64_REG_S20 = 156,
        ARM64_REG_S21 = 157,
        ARM64_REG_S22 = 158,
        ARM64_REG_S23 = 159,
        ARM64_REG_S24 = 160,
        ARM64_REG_S25 = 161,
        ARM64_REG_S26 = 162,
        ARM64_REG_S27 = 163,
        ARM64_REG_S28 = 164,
        ARM64_REG_S29 = 165,
        ARM64_REG_S30 = 166,
        ARM64_REG_S31 = 167,
        ARM64_REG_W0 = 168,
        ARM64_REG_W1 = 169,
        ARM64_REG_W2 = 170,
        ARM64_REG_W3 = 171,
        ARM64_REG_W4 = 172,
        ARM64_REG_W5 = 173,
        ARM64_REG_W6 = 174,
        ARM64_REG_W7 = 175,
        ARM64_REG_W8 = 176,
        ARM64_REG_W9 = 177,
        ARM64_REG_W10 = 178,
        ARM64_REG_W11 = 179,
        ARM64_REG_W12 = 180,
        ARM64_REG_W13 = 181,
        ARM64_REG_W14 = 182,
        ARM64_REG_W15 = 183,
        ARM64_REG_W16 = 184,
        ARM64_REG_W17 = 185,
        ARM64_REG_W18 = 186,
        ARM64_REG_W19 = 187,
        ARM64_REG_W20 = 188,
        ARM64_REG_W21 = 189,
        ARM64_REG_W22 = 190,
        ARM64_REG_W23 = 191,
        ARM64_REG_W24 = 192,
        ARM64_REG_W25 = 193,
        ARM64_REG_W26 = 194,
        ARM64_REG_W27 = 195,
        ARM64_REG_W28 = 196,
        ARM64_REG_W29 = 197,
        ARM64_REG_W30 = 198,
        ARM64_REG_X0 = 199,
        ARM64_REG_X1 = 200,
        ARM64_REG_X2 = 201,
        ARM64_REG_X3 = 202,
        ARM64_REG_X4 = 203,
        ARM64_REG_X5 = 204,
        ARM64_REG_X6 = 205,
        ARM64_REG_X7 = 206,
        ARM64_REG_X8 = 207,
        ARM64_REG_X9 = 208,
        ARM64_REG_X10 = 209,
        ARM64_REG_X11 = 210,
        ARM64_REG_X12 = 211,
        ARM64_REG_X13 = 212,
        ARM64_REG_X14 = 213,
        ARM64_REG_X15 = 214,
        ARM64_REG_X16 = 215,
        ARM64_REG_X17 = 216,
        ARM64_REG_X18 = 217,
        ARM64_REG_X19 = 218,
        ARM64_REG_X20 = 219,
        ARM64_REG_X21 = 220,
        ARM64_REG_X22 = 221,
        ARM64_REG_X23 = 222,
        ARM64_REG_X24 = 223,
        ARM64_REG_X25 = 224,
        ARM64_REG_X26 = 225,
        ARM64_REG_X27 = 226,
        ARM64_REG_X28 = 227,
        ARM64_REG_V0 = 228,
        ARM64_REG_V1 = 229,
        ARM64_REG_V2 = 230,
        ARM64_REG_V3 = 231,
        ARM64_REG_V4 = 232,
        ARM64_REG_V5 = 233,
        ARM64_REG_V6 = 234,
        ARM64_REG_V7 = 235,
        ARM64_REG_V8 = 236,
        ARM64_REG_V9 = 237,
        ARM64_REG_V10 = 238,
        ARM64_REG_V11 = 239,
        ARM64_REG_V12 = 240,
        ARM64_REG_V13 = 241,
        ARM64_REG_V14 = 242,
        ARM64_REG_V15 = 243,
        ARM64_REG_V16 = 244,
        ARM64_REG_V17 = 245,
        ARM64_REG_V18 = 246,
        ARM64_REG_V19 = 247,
        ARM64_REG_V20 = 248,
        ARM64_REG_V21 = 249,
        ARM64_REG_V22 = 250,
        ARM64_REG_V23 = 251,
        ARM64_REG_V24 = 252,
        ARM64_REG_V25 = 253,
        ARM64_REG_V26 = 254,
        ARM64_REG_V27 = 255,
        ARM64_REG_V28 = 256,
        ARM64_REG_V29 = 257,
        ARM64_REG_V30 = 258,
        ARM64_REG_V31 = 259,

        // pseudo registers
        ARM64_REG_PC = 260,
        ARM64_REG_CPACR_EL1 = 261,

        // thread registers, depreciated, use UC_ARM64_REG_CP_REG instead
        ARM64_REG_TPIDR_EL0 = 262,
        ARM64_REG_TPIDRRO_EL0 = 263,
        ARM64_REG_TPIDR_EL1 = 264,
        ARM64_REG_PSTATE = 265,

        // exception link registers, depreciated, use UC_ARM64_REG_CP_REG instead
        ARM64_REG_ELR_EL0 = 266,
        ARM64_REG_ELR_EL1 = 267,
        ARM64_REG_ELR_EL2 = 268,
        ARM64_REG_ELR_EL3 = 269,

        // stack pointers registers, depreciated, use UC_ARM64_REG_CP_REG instead
        ARM64_REG_SP_EL0 = 270,
        ARM64_REG_SP_EL1 = 271,
        ARM64_REG_SP_EL2 = 272,
        ARM64_REG_SP_EL3 = 273,

        // other CP15 registers, depreciated, use UC_ARM64_REG_CP_REG instead
        ARM64_REG_TTBR0_EL1 = 274,
        ARM64_REG_TTBR1_EL1 = 275,
        ARM64_REG_ESR_EL0 = 276,
        ARM64_REG_ESR_EL1 = 277,
        ARM64_REG_ESR_EL2 = 278,
        ARM64_REG_ESR_EL3 = 279,
        ARM64_REG_FAR_EL0 = 280,
        ARM64_REG_FAR_EL1 = 281,
        ARM64_REG_FAR_EL2 = 282,
        ARM64_REG_FAR_EL3 = 283,
        ARM64_REG_PAR_EL1 = 284,
        ARM64_REG_MAIR_EL1 = 285,
        ARM64_REG_VBAR_EL0 = 286,
        ARM64_REG_VBAR_EL1 = 287,
        ARM64_REG_VBAR_EL2 = 288,
        ARM64_REG_VBAR_EL3 = 289,
        ARM64_REG_CP_REG = 290,

        // floating point control and status registers
        ARM64_REG_FPCR = 291,
        ARM64_REG_FPSR = 292,
        ARM64_REG_ENDING = 293,

        // alias registers
        ARM64_REG_IP0 = 215,
        ARM64_REG_IP1 = 216,
        ARM64_REG_FP = 1,
        ARM64_REG_LR = 2,

        // ARM64 instructions

        ARM64_INS_INVALID = 0,
        ARM64_INS_MRS = 1,
        ARM64_INS_MSR = 2,
        ARM64_INS_SYS = 3,
        ARM64_INS_SYSL = 4,
        ARM64_INS_ENDING = 5,
    }
}
