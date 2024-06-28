Option Strict On
Option Explicit On

''' <summary>キー値トークンを表すクラスです。</summary>
Public NotInheritable Class TomlKeyValueToken
    Inherits TomlToken

    ''' <summary>キーのリストを取得します。</summary>
    ''' <returns>キーのリスト。</returns>
    Public ReadOnly Keys As TomlToken()

    ''' <summary>値を取得します。</summary>
    ''' <returns>値。</returns>
    Public ReadOnly Value As TomlToken

    ''' <summary>新しいインスタンスを生成します。</summary>
    ''' <param name="type">トークンの種類。</param>
    ''' <param name="rng">範囲。</param>
    ''' <param name="keys">キーのリスト。</param>
    ''' <param name="value">値。</param>
    Public Sub New(type As TokenTypeEnum, rng As RawSource.Range, keys As TomlToken(), value As TomlToken)
        MyBase.New(type, rng)
        Me.Keys = keys
        Me.Value = value
    End Sub

End Class
