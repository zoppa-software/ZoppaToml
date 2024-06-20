Imports System
Imports Xunit
Imports ZoppaToml
Imports ZoppaToml.TomlLexical

Public Class RawSourceTest

    <Fact>
    Sub Case01()
        Dim raw As New RawSource("1234567890")

        Dim ptr = raw.GetPointer()
        Assert.Equal("12345...", ptr.TakeChar(5))

        ptr.Skip(8)
        Assert.Equal("90", ptr.TakeChar(5))
    End Sub

    <Fact>
    Sub Case02()
        Dim raw As New RawSource(
"# この行は全てコメントです。
key = ""value""  # 行末までコメントです。
another = ""# これはコメントではありません"""
)

        Dim ans = raw.Lexical()
        Assert.Equal(7, ans.Count)
        Assert.Equal("# この行は全てコメントです。", ans(0).ToString())
        Assert.Equal("key = ""value""", ans(2).ToString())
        Assert.Equal("another = ""# これはコメントではありません""", ans(6).ToString())
    End Sub

    <Fact>
    Sub Case03()
        Dim raw As New RawSource(
"key = ""value"""
)

        Dim ans = raw.Lexical()
        Assert.Equal(1, ans.Count)
        Assert.Equal("key = ""value""", ans(0).ToString())
    End Sub

    <Fact>
    Sub Case04()
        Dim raw As New RawSource(
"key = ""value""
bare_key = ""value""
bare-key = ""value""
1234 = ""value"""
)

        Dim ans = raw.Lexical()
        Assert.Equal(7, ans.Count)
        Assert.Equal("key = ""value""", ans(0).ToString())
        Assert.Equal("bare_key = ""value""", ans(2).ToString())
        Assert.Equal("bare-key = ""value""", ans(4).ToString())
        Assert.Equal("1234 = ""value""", ans(6).ToString())
    End Sub

    <Fact>
    Sub Case05()
        Dim raw As New RawSource(
"""127.0.0.1"" = ""value""
""character encoding"" = ""value""
""ʎǝʞ"" = ""value""
'key2' = ""value""
'quoted ""value""' = ""value"""
)

        Dim ans = raw.Lexical()
        Assert.Equal(9, ans.Count)
        Assert.Equal("""127.0.0.1"" = ""value""", ans(0).ToString())
        Assert.Equal("""character encoding"" = ""value""", ans(2).ToString())
        Assert.Equal("""ʎǝʞ"" = ""value""", ans(4).ToString())
        Assert.Equal("'key2' = ""value""", ans(6).ToString())
        Assert.Equal("'quoted ""value""' = ""value""", ans(8).ToString())
    End Sub

    <Fact>
    Sub Case06()
        Dim raw As New RawSource(
"name = ""Orange""
physical.color = ""orange""
physical.shape = ""round""
site.""google.com"" = true"
)

        Dim ans = raw.Lexical()
        Assert.Equal(7, ans.Count)
        Assert.Equal("name = ""Orange""", ans(0).ToString())
        Assert.Equal("physical.color = ""orange""", ans(2).ToString())
        Assert.Equal("physical.shape = ""round""", ans(4).ToString())
        Assert.Equal("site.""google.com"" = true", ans(6).ToString())
    End Sub

End Class
