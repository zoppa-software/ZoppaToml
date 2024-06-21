Option Strict On
Option Explicit On

''' <summary>トークンを表すクラスです。</summary>
Public Class TomlToken

    ''' <summary>トークンの種類を表す列挙体です。</summary>
    Public Enum TokenTypeEnum

        ''' <summary>その他。</summary>
        Other

        ''' <summary>改行。</summary>
        LineFeed

        ''' <summary>空白。</summary>
        Spaces

        ''' <summary>コメント。</summary>
        Comment

        ''' <summary>テーブルヘッダ。</summary>
        TableHeader

        ''' <summary>テーブル配列ヘッダ。</summary>
        TableArrayHeader

        ''' <summary>キーと値。</summary>
        KeyAndValue

        ''' <summary>文字列（リテラル）</summary>
        LiteralString

        ''' <summary>文字列（キー）</summary>
        KeyString

        ''' <summary>正リテラル。</summary>
        TrueLiteral

        ''' <summary>偽リテラル。</summary>
        FalseLiteral

        ''' <summary>数値。</summary>
        NumberLiteral

        ''' <summary>inf。</summary>
        InfLiteral

        ''' <summary>nan。</summary>
        NanLiteral

        ''' <summary>配列。</summary>
        Array

    End Enum

    ' トークンの種類
    Private mType As TokenTypeEnum

    ' 生値参照
    Private mRange As RawSource.Range

#Region "properties"

    ''' <summary>トークンの種類を取得します。</summary>
    ''' <returns>トークンの種類。</returns>
    Public ReadOnly Property TokenType As TokenTypeEnum
        Get
            Return Me.mType
        End Get
    End Property

    ''' <summary>生の値を取得します。</summary>
    ''' <returns>生値。</returns>
    Public ReadOnly Property Raw As Byte()
        Get
            Return Me.mRange.Raw
        End Get
    End Property

    ''' <summary>要素の長さを取得します。</summary>
    ''' <returns>長さ。</returns>
    Public ReadOnly Property Length As Integer
        Get
            Return Me.mRange.Length
        End Get
    End Property

    ''' <summary>インデクサ。</summary>
    ''' <param name="index">インデックス。</param>
    ''' <returns>生値。</returns>
    Default Public ReadOnly Property Items(index As Integer) As Byte
        Get
            Return Me.mRange(index)
        End Get
    End Property

#End Region

    ''' <summary>コンストラクタ。</summary>
    ''' <param name="type">トークンの種類。</param>
    ''' <param name="rng">文字範囲。</param>
    Public Sub New(type As TokenTypeEnum, rng As RawSource.Range)
        Me.mType = type
        Me.mRange = rng
    End Sub

    ''' <summary>文字列表現を取得します。</summary>
    ''' <returns>文字列表現。</returns>
    Public Overrides Function ToString() As String
        Return Me.mRange.ToString()
    End Function

End Class
