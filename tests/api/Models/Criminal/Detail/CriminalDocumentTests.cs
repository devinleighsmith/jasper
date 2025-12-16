using Scv.Api.Models.Criminal.Detail;
using Scv.Db.Models;
using Xunit;

namespace tests.api.Models.Criminal.Detail;

public class CriminalDocumentTests
{
    [Fact]
    public void Category_ShouldTransformPSR_ToReport()
    {
        var document = new CriminalDocument { Category = "PSR" };

        Assert.Equal("Report", document.Category);
    }

    [Fact]
    public void Category_ShouldTransformPsrLowercase_ToReport()
    {
        var document = new CriminalDocument { Category = "psr" };

        Assert.Equal("Report", document.Category);
    }

    [Fact]
    public void Category_ShouldTransformPsrMixedCase_ToReport()
    {
        var document = new CriminalDocument { Category = "PsR" };

        Assert.Equal("Report", document.Category);
    }

    [Fact]
    public void Category_ShouldKeepROP_AsROP()
    {
        var document = new CriminalDocument { Category = "ROP" };

        Assert.Equal("ROP", document.Category);
    }

    [Fact]
    public void Category_ShouldTransformRopLowercase_ToROP()
    {
        var document = new CriminalDocument { Category = "rop" };

        Assert.Equal("ROP", document.Category);
    }

    [Fact]
    public void Category_ShouldTransformRopMixedCase_ToROP()
    {
        var document = new CriminalDocument { Category = "RoP" };

        Assert.Equal("ROP", document.Category);
    }

    [Fact]
    public void Category_ShouldCapitalizeFirstLetter_AndLowercaseRest()
    {
        var document = new CriminalDocument { Category = "INITIATING" };

        Assert.Equal("Initiating", document.Category);
    }

    [Fact]
    public void Category_ShouldTransformBAIL_ToBail()
    {
        var document = new CriminalDocument { Category = "BAIL" };

        Assert.Equal("Bail", document.Category);
    }

    [Fact]
    public void Category_ShouldTransformMOTIONS_ToMotions()
    {
        var document = new CriminalDocument { Category = "MOTIONS" };

        Assert.Equal("Motions", document.Category);
    }

    [Fact]
    public void Category_ShouldTransformORDERS_ToOrders()
    {
        var document = new CriminalDocument { Category = "ORDERS" };

        Assert.Equal("Orders", document.Category);
    }

    [Fact]
    public void Category_ShouldTransformPLEADINGS_ToPleadings()
    {
        var document = new CriminalDocument { Category = "PLEADINGS" };

        Assert.Equal("Pleadings", document.Category);
    }

    [Fact]
    public void Category_ShouldTransformAFFIDAVITS_ToAffidavits()
    {
        var document = new CriminalDocument { Category = "AFFIDAVITS" };

        Assert.Equal("Affidavits", document.Category);
    }

    [Fact]
    public void Category_ShouldTransformREFERENCE_ToReference()
    {
        var document = new CriminalDocument { Category = "REFERENCE" };

        Assert.Equal("Reference", document.Category);
    }

    [Fact]
    public void Category_ShouldTransformCSR_ToCsr()
    {
        var document = new CriminalDocument { Category = "CSR" };

        Assert.Equal("Csr", document.Category);
    }

    [Fact]
    public void Category_ShouldHandleLowercaseInput()
    {
        var document = new CriminalDocument { Category = "bail" };

        Assert.Equal("Bail", document.Category);
    }

    [Fact]
    public void Category_ShouldHandleMixedCaseInput()
    {
        var document = new CriminalDocument { Category = "BaIl" };

        Assert.Equal("Bail", document.Category);
    }

    [Fact]
    public void Category_ShouldReturnEmptyString_WhenNull()
    {
        var document = new CriminalDocument { Category = null };

        Assert.Equal(string.Empty, document.Category);
    }

    [Fact]
    public void Category_ShouldReturnEmptyString_WhenEmpty()
    {
        var document = new CriminalDocument { Category = string.Empty };

        Assert.Equal(string.Empty, document.Category);
    }

    [Fact]
    public void Category_ShouldReturnEmptyString_WhenWhitespace()
    {
        var document = new CriminalDocument { Category = "   " };

        Assert.Equal(string.Empty, document.Category);
    }

    [Fact]
    public void Category_ShouldRetainTransformedValue_WhenAccessedMultipleTimes()
    {
        var document = new CriminalDocument { Category = "INITIATING" };

        var firstAccess = document.Category;
        var secondAccess = document.Category;

        Assert.Equal("Initiating", firstAccess);
        Assert.Equal("Initiating", secondAccess);
        Assert.Equal(firstAccess, secondAccess);
    }

    [Fact]
    public void Category_ShouldUpdateValue_WhenSetAgain()
    {
        var document = new CriminalDocument { Category = "BAIL" };
        Assert.Equal("Bail", document.Category);

        document.Category = "ROP";
        Assert.Equal("ROP", document.Category);
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        var document = new CriminalDocument
        {
            PartId = "12345",
            Category = "BAIL",
            DocumentTypeDescription = "Test Document",
            HasFutureAppearance = true
        };

        Assert.Equal("12345", document.PartId);
        Assert.Equal("Bail", document.Category);
        Assert.Equal("Test Document", document.DocumentTypeDescription);
        Assert.True(document.HasFutureAppearance);
    }

    [Fact]
    public void HasFutureAppearance_ShouldBeNullable()
    {
        var document = new CriminalDocument { HasFutureAppearance = null };

        Assert.Null(document.HasFutureAppearance);
    }

    [Fact]
    public void Category_ShouldHandleSingleCharacterInput()
    {
        var document = new CriminalDocument { Category = "a" };

        Assert.Equal("A", document.Category);
    }

    [Fact]
    public void Category_ShouldHandleTwoCharacterInput()
    {
        var document = new CriminalDocument { Category = "ab" };

        Assert.Equal("Ab", document.Category);
    }
}
