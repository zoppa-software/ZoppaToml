Option Strict On
Option Explicit On

''' <summary>Tomlドキュメントの構文エラーです。</summary>
Public NotInheritable Class TomlSyntaxException
    Inherits Exception

    ''' <summary>新しいインスタンスを生成します。</summary>
    Public Sub New()
        MyBase.New()
    End Sub

    ''' <summary>新しいインスタンスを生成します。</summary>
    ''' <param name="message">メッセージ。</param>
    Public Sub New(message As String)
        MyBase.New(message)
    End Sub

    ''' <summary>新しいインスタンスを生成します。</summary>
    ''' <param name="message">メッセージ。</param>
    ''' <param name="innerException">内部例外。</param>
    Public Sub New(message As String, innerException As Exception)
        MyBase.New(message, innerException)
    End Sub

    ''' <summary>新しいインスタンスを生成します。</summary>
    ''' <param name="info">シリアル化情報。</param>
    ''' <param name="context">コンテキスト。</param>
    Protected Sub New(info As Runtime.Serialization.SerializationInfo, context As Runtime.Serialization.StreamingContext)
        MyBase.New(info, context)
    End Sub

End Class
