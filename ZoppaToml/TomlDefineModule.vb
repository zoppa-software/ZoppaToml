Option Strict On
Option Explicit On

''' <summary>定義モジュールです。</summary>
Module TomlDefineModule

    ''' <summary>'\'を表す定数です。</summary>
    Public Const ByteBKSlash As Byte = AscW("\"c)

    ''' <summary>'\r' を表す定数です。</summary>
    Public Const ByteCR As Byte = 13

    ''' <summary>'0'を表す定数です。</summary>
    Public Const ByteCh0 As Byte = AscW("0"c)

    ''' <summary>'1'を表す定数です。</summary>
    Public Const ByteCh1 As Byte = AscW("1"c)

    ''' <summary>'3'を表す定数です。</summary>
    Public Const ByteCh3 As Byte = AscW("3"c)

    ''' <summary>'7'を表す定数です。</summary>
    Public Const ByteCh7 As Byte = AscW("7"c)

    ''' <summary>'9'を表す定数です。</summary>
    Public Const ByteCh9 As Byte = AscW("9"c)

    ''' <summary>','を表す定数です。</summary>
    Public Const ByteComma As Byte = AscW(","c)

    ''' <summary>':'を表す定数です。</summary>
    Public Const ByteColon As Byte = AscW(":"c)

    ''' <summary>'.'を表す定数です。</summary>
    Public Const ByteDot As Byte = AscW("."c)

    ''' <summary>'"'を表す定数です。</summary>
    Public Const ByteDoubleQuot As Byte = AscW(""""c)

    ''' <summary>'='を表す定数です。</summary>
    Public Const ByteEqual As Byte = AscW("="c)

    ''' <summary>'-'を表す定数です。</summary>
    Public Const ByteHyphen As Byte = AscW("-"c)

    ''' <summary>'['を表す定数です。</summary>
    Public Const ByteLBacket As Byte = AscW("["c)

    ''' <summary>'{'を表す定数です。</summary>
    Public Const ByteLBrace As Byte = AscW("{"c)

    ''' <summary>'\n'を表す定数です。</summary>
    Public Const ByteLF As Byte = 10

    ''' <summary>'a'を表す定数です。</summary>
    Public Const ByteLowA As Byte = AscW("a"c)

    ''' <summary>'b'を表す定数です。</summary>
    Public Const ByteLowB As Byte = AscW("b"c)

    ''' <summary>'e'を表す定数です。</summary>
    Public Const ByteLowE As Byte = AscW("e"c)

    ''' <summary>'f'を表す定数です。</summary>
    Public Const ByteLowF As Byte = AscW("f"c)

    ''' <summary>'i'を表す定数です。</summary>
    Public Const ByteLowI As Byte = AscW("i"c)

    ''' <summary>'n'を表す定数です。</summary>
    Public Const ByteLowN As Byte = AscW("n"c)

    ''' <summary>'o'を表す定数です。</summary>
    Public Const ByteLowO As Byte = AscW("o"c)

    ''' <summary>'r'を表す定数です。</summary>
    Public Const ByteLowR As Byte = AscW("r"c)

    ''' <summary>'t'を表す定数です。</summary>
    Public Const ByteLowT As Byte = AscW("t"c)

    ''' <summary>'x'を表す定数です。</summary>
    Public Const ByteLowX As Byte = AscW("x"c)

    ''' <summary>'z'を表す定数です。</summary>
    Public Const ByteLowZ As Byte = AscW("z"c)

    ''' <summary>'-'を表す定数です。</summary>
    Public Const ByteMinus As Byte = AscW("-"c)

    ''' <summary>nullを表す定数です。</summary>
    Public Const ByteNull As Byte = 0

    ''' <summary>'+'を表す定数です。</summary>
    Public Const BytePlus As Byte = AscW("+"c)

    ''' <summary>']'を表す定数です。</summary>
    Public Const ByteRBacket As Byte = AscW("]"c)

    ''' <summary>'}'を表す定数です。</summary>
    Public Const ByteRBrace As Byte = AscW("}"c)

    ''' <summary>'#'を表す定数です。</summary>
    Public Const ByteSharp As Byte = AscW("#"c)

    ''' <summary>'を表す定数です。</summary>
    Public Const ByteSingleQuot As Byte = AscW("'"c)

    ''' <summary>' 'を表す定数です。</summary>
    Public Const ByteSpace As Byte = 32

    ''' <summary>'\t'を表す定数です。</summary>
    Public Const ByteTab As Byte = 9

    ''' <summary>'_'を表す定数です。</summary>
    Public Const ByteUnderBar As Byte = AscW("_"c)

    ''' <summary>'A'を表す定数です。</summary>
    Public Const ByteUpA As Byte = AscW("A"c)

    ''' <summary>'B'を表す定数です。</summary>
    Public Const ByteUpB As Byte = AscW("B"c)

    ''' <summary>'E'を表す定数です。</summary>
    Public Const ByteUpE As Byte = AscW("E"c)

    ''' <summary>'F'を表す定数です。</summary>
    Public Const ByteUpF As Byte = AscW("F"c)

    ''' <summary>'O'を表す定数です。</summary>
    Public Const ByteUpO As Byte = AscW("O"c)

    ''' <summary>'T'を表す定数です。</summary>
    Public Const ByteUpT As Byte = AscW("T"c)

    ''' <summary>'X'を表す定数です。</summary>
    Public Const ByteUpX As Byte = AscW("X"c)

    ''' <summary>'Z'を表す定数です。</summary>
    Public Const ByteUpZ As Byte = AscW("Z"c)

End Module
