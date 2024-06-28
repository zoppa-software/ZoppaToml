Option Strict On
Option Explicit On

''' <summary>サブトークンを持つトークンを表すクラスです。</summary>
Public NotInheritable Class TomlHasSubToken
    Inherits TomlToken

    ''' <summary>サブトークンを取得します。</summary>
    ''' <returns>サブトークン。</returns>
    Public ReadOnly SubTokens As TomlToken()

    ''' <summary>新しいインスタンスを生成します。</summary>
    ''' <param name="type">トークンの種類。</param>
    ''' <param name="rng">範囲。</param>
    ''' <param name="subtkn">サブトークン。</param>
    Public Sub New(type As TokenTypeEnum, rng As RawSource.Range, subtkn As TomlToken())
        MyBase.New(type, rng)
        Me.SubTokens = subtkn
    End Sub

End Class
