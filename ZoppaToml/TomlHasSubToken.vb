Option Strict On
Option Explicit On

''' <summary>テーブルトークンを表すクラスです。</summary>
Public NotInheritable Class TomlHasSubToken
    Inherits TomlToken

    Public ReadOnly SubTokens As TomlToken()

    Public Sub New(type As TokenTypeEnum, rng As RawSource.Range, subtkn As TomlToken())
        MyBase.New(type, rng)
        Me.SubTokens = subtkn
    End Sub

End Class
