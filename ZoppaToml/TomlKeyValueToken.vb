Option Strict On
Option Explicit On

''' <summary>キー値トークンを表すクラスです。</summary>
Public NotInheritable Class TomlKeyValueToken
    Inherits TomlToken

    Public ReadOnly Keys As TomlToken()

    Public ReadOnly Value As TomlToken

    Public Sub New(type As TokenTypeEnum, rng As RawSource.Range, keys As TomlToken(), value As TomlToken)
        MyBase.New(type, rng)
        Me.Keys = keys
        Me.Value = value
    End Sub

End Class
