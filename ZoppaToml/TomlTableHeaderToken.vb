Option Strict On
Option Explicit On

''' <summary>テーブルトークンを表すクラスです。</summary>
Public NotInheritable Class TomlTableHeaderToken
    Inherits TomlToken

    Public ReadOnly Keys As TomlToken()

    Public Sub New(type As TokenTypeEnum, rng As RawSource.Range, keys As TomlToken())
        MyBase.New(type, rng)
        Me.Keys = keys
    End Sub

End Class
