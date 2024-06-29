Imports System
Imports System.Dynamic
Imports System.Runtime.Intrinsics.X86
Imports Xunit
Imports ZoppaToml
Imports ZoppaToml.TomlLexical

Public Class Valid2Test

    <Fact>
    Sub StringTest01()
        Dim doc As Object = TomlDocument.LoadFromFile("valid\string\double-quote-escape.toml")

        Assert.Equal("""one""", doc.test.GetValue(Of String)())
    End Sub

    <Fact>
    Sub StringTest02()
        Dim doc As Object = TomlDocument.LoadFromFile("valid\string\empty.toml")

        Assert.Equal("", doc.answer.GetValue(Of String)())
    End Sub

    <Fact>
    Sub StringTest03()
        Dim doc As Object = TomlDocument.LoadFromFile("valid\string\ends-in-whitespace-escape.toml")

        Assert.Equal($"heeee{vbLf}geeee", doc.beee.GetValue(Of String)())
    End Sub

    <Fact>
    Sub StringTest04()
        Dim doc As Object = TomlDocument.LoadFromFile("valid\string\escaped-escape.toml")

        Assert.Equal("\x64", doc.answer.GetValue(Of String)())
    End Sub

    <Fact>
    Sub StringTest05()
        Dim doc As Object = TomlDocument.LoadFromFile("valid\string\escape-esc.toml")

        Assert.Equal(" There is no escape! ", doc.esc.GetValue(Of String)())
    End Sub

    <Fact>
    Sub StringTest06()
        Dim doc As Object = TomlDocument.LoadFromFile("valid\string\escapes.toml")

        Assert.Equal($"|{vbBack}.", doc.backspace.GetValue(Of String)())
        Assert.Equal($"|{vbTab}.", doc.tab.GetValue(Of String)())
        Assert.Equal($"|{vbLf}.", doc.newline.GetValue(Of String)())
        Assert.Equal($"|{vbFormFeed}.", doc.formfeed.GetValue(Of String)())
        Assert.Equal($"|{vbCr}.", doc.carriage.GetValue(Of String)())
        Assert.Equal($"|"".", doc.quote.GetValue(Of String)())
        Assert.Equal($"|\.", doc.backslash.GetValue(Of String)())
        Assert.Equal($"|{ChrW(127)}.", doc.delete.GetValue(Of String)())
        Assert.Equal($"|{ChrW(&H1F)}.", doc.unitseparator.GetValue(Of String)())
        Assert.Equal($"|\u.", doc.notunicode1.GetValue(Of String)())
        Assert.Equal($"|\u.", doc.notunicode2.GetValue(Of String)())
        Assert.Equal($"|\u0075.", doc.notunicode3.GetValue(Of String)())
        Assert.Equal($"|\u.", doc.notunicode4.GetValue(Of String)())
    End Sub

    <Fact>
    Sub StringTest07()
        Dim doc As Object = TomlDocument.LoadFromFile("valid\string\escape-tricky.toml")

        Assert.Equal("String does not end here"" but ends here\", doc.end_esc.GetValue(Of String)())
        Assert.Equal("String ends here\", doc.lit_end_esc.GetValue(Of String)())
        Assert.Equal(" ", doc.multiline_unicode.GetValue(Of String)())
        Assert.Equal("\u0041", doc.multiline_not_unicode.GetValue(Of String)())
        Assert.Equal("When will it end? """"""..."""""" should be here""", doc.multiline_end_esc.GetValue(Of String)())
        Assert.Equal("\u007f", doc.lit_multiline_not_unicode.GetValue(Of String)())
        Assert.Equal("There is no escape\", doc.lit_multiline_end.GetValue(Of String)())
    End Sub

End Class
