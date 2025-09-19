using System;
using CsvHelper.Configuration.Attributes;

namespace Scv.Api.Documents.Parsers.Models;

public class ReservedJudgement
{
    [Name("Appearance Date")]
    public DateTime AppearanceDate { get; set; }
    [Name("Court Class Category Structure Code")]
    public required string CourtClass { get; set; }
    [Name("COURT_FILE_NO")]
    public required string CourtFileNumber { get; set; }
    [Name("File Number")]
    public required string FileNumber { get; set; }
    [Name("Adjudicator Last Name, First Name")]
    public required string AdjudicatorLastNameFirstName { get; set; }
    [Name("Adjudicator Type Description")]
    public required string AdjudicatorTypeDescription { get; set; }
    [Name("Facility Code")]
    public required string FacilityCode { get; set; }
    [Name("Facility Description")]
    public required string FacilityDescription { get; set; }
    [Name("Reserved Judgement Yes No Code")]
    public required char ReservedJudgementYesNoCode { get; set; }
    [Name("RFJ Filed Date")]
    public DateTime? RFJFiledDate { get; set; }
    [Name("RJ Multi Yes No Code")]
    public required char RJMultiYesNoCode { get; set; }
    [Name("RJ Outstanding Yes No Code")]
    public required char RJOutstandingYesNoCode { get; set; }
    [Name("Age in Days")]
    public required int AgeInDays { get; set; }
    [Name("Age in Months")]
    public required float AgeInMonths { get; set; }
}