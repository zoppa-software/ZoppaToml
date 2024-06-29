Option Strict On
Option Explicit On

Imports System.Globalization
Imports System.Resources
Imports System.Transactions

''' <summary>メッセージリソース参照。</summary>
Module Message

    ''' <summary>リソースマネージャーのインスタンスを取得します。</summary>
    Private mMamagerInstance As New Lazy(Of ResourceManager)(
        Function()
            Return New ResourceManager("ZoppaToml.Message", GetType(Message).Assembly)
        End Function
    )

    ''' <summary>カルチャ情報のインスタンスを取得します。</summary>
    Private mCultureInfoInstance As New Lazy(Of CultureInfo)(
        Function()
            Dim cultureCode = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
            'cultureCode = "en"
            Return New CultureInfo(cultureCode)
        End Function
    )

    ''' <summary>リソースマネージャーを取得します。</summary>
    Private ReadOnly Property Manager As ResourceManager
        Get
            Return mMamagerInstance.Value
        End Get
    End Property

    ''' <summary>メッセージを取得します。</summary>
    ''' <param name="id">メッセージID。</param>
    ''' <returns>メッセージ。</returns>
    Public Function GetMessage(id As String) As String
        Return Manager.GetString(id, mCultureInfoInstance.Value)
    End Function

    ''' <summary>メッセージを取得します。</summary>
    ''' <param name="id">メッセージID。</param>
    ''' <param name="args">引数リスト。</param>
    ''' <returns>メッセージ。</returns>
    Public Function GetMessage(id As String, ParamArray args() As Object) As String
        Dim ptn = Manager.GetString(id, mCultureInfoInstance.Value)
        Return String.Format(ptn, args)
    End Function

End Module
