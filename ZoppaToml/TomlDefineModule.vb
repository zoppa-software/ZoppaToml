Option Strict On
Option Explicit On

''' <summary>定義モジュールです。</summary>
Module TomlDefineModule

    Public Const ByteNull As Byte = 0

    Public Const ByteLF As Byte = 10

    Public Const ByteCR As Byte = 13

    Public Const ByteSpace As Byte = 32

    Public Const ByteTab As Byte = 9

    Public Const ByteSharp As Byte = AscW("#"c)

    Public Const ByteLBacket As Byte = AscW("["c)

    Public Const ByteRBacket As Byte = AscW("]"c)

    Public Const ByteDoubleQuot As Byte = AscW(""""c)

    Public Const ByteSingleQuot As Byte = AscW("'"c)

    Public Const ByteEqual As Byte = AscW("="c)

    Public Const ByteComma As Byte = AscW(","c)

    Public Const ByteDot As Byte = AscW("."c)

    Public Const ByteUpA As Byte = AscW("A"c)

    Public Const ByteUpZ As Byte = AscW("Z"c)

    Public Const ByteLowA As Byte = AscW("a"c)

    Public Const ByteLowZ As Byte = AscW("z"c)

    Public Const ByteCh0 As Byte = AscW("0"c)

    Public Const ByteCh9 As Byte = AscW("9"c)

    Public Const ByteUnderBar As Byte = AscW("_"c)

    Public Const ByteHyphen As Byte = AscW("-"c)

    Public Const ByteBKSlash As Byte = AscW("\"c)

    Public Const ByteLowT As Byte = AscW("t"c)

    Public Const ByteLowF As Byte = AscW("f"c)

End Module
