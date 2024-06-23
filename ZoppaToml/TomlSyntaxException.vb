Option Strict On
Option Explicit On

''' <summary>Tomlドキュメントの構文エラーです。</summary>
Public NotInheritable Class TomlSyntaxException
    Inherits Exception

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

End Class
