// using System;

// namespace Scv.Api.Documents.Parsers.Models;

// public class ReservedJudgement
// {
//     [CsvHelper.Configuration.Attributes.Name("Appearance Date")]
//     public DateTime AppearanceDate { get; set; }
//     [CsvHelper.Configuration.Attributes.Name("Court Class Category Structure Code")]
//     public required string CourtClass { get; set; }
//     [CsvHelper.Configuration.Attributes.Name("COURT_FILE_NO")]
//     public required string CourtFileNumber { get; set; }
//     [CsvHelper.Configuration.Attributes.Name("File Number")]
//     public required string FileNumber { get; set; }
//     [CsvHelper.Configuration.Attributes.Name("Adjudicator Last Name, First Name")]
//     public required string AdjudicatorLastNameFirstName { get; set; }
//     [CsvHelper.Configuration.Attributes.Name("Adjudicator Type Description")]
//     public required string AdjudicatorTypeDescription { get; set; }
//     [CsvHelper.Configuration.Attributes.Name("Facility Code")]
//     public required string FacilityCode { get; set; }
//     [CsvHelper.Configuration.Attributes.Name("Facility Description")]
//     public required string FacilityDescription { get; set; }
//     [CsvHelper.Configuration.Attributes.Name("Reserved Judgement Yes No Code")]
//     public required char ReservedJudgementYesNoCode { get; set; }
//     [CsvHelper.Configuration.Attributes.Name("RFJ Filed Date")]
//     public DateTime? RFJFiledDate { get; set; }
//     [CsvHelper.Configuration.Attributes.Name("RJ Multi Yes No Code")]
//     public required char RJMultiYesNoCode { get; set; }
//     [CsvHelper.Configuration.Attributes.Name("RJ Outstanding Yes No Code")]
//     public required char RJOutstandingYesNoCode { get; set; }
//     [CsvHelper.Configuration.Attributes.Name("Age in Days")]
//     public required int AgeInDays { get; set; }
//     [CsvHelper.Configuration.Attributes.Name("Age in Months")]
//     public required float AgeInMonths { get; set; }
// }

using System;
using CsvHelper.Configuration;

namespace Scv.Api.Documents.Parsers.Models;

public class ReservedJudgement
{
    [CsvHelper.Configuration.Attributes.Name("Appearance Date")]
    public DateTime AppearanceDate { get; set; }
    [CsvHelper.Configuration.Attributes.Name("Court Class Category Structure Code")]
    public required string CourtClass { get; set; }
    [CsvHelper.Configuration.Attributes.Name("COURT_FILE_NO")]
    public required string CourtFileNumber { get; set; }
    [CsvHelper.Configuration.Attributes.Name("File Number")]
    public required string FileNumber { get; set; }
    [CsvHelper.Configuration.Attributes.Name("Adjudicator Last Name, First Name")]
    public required string AdjudicatorLastNameFirstName { get; set; }
    [CsvHelper.Configuration.Attributes.Name("Adjudicator Type Description")]
    public required string AdjudicatorTypeDescription { get; set; }
    [CsvHelper.Configuration.Attributes.Name("Facility Code")]
    public required string FacilityCode { get; set; }
    [CsvHelper.Configuration.Attributes.Name("Facility Description")]
    public required string FacilityDescription { get; set; }
    [CsvHelper.Configuration.Attributes.Name("Reserved Judgement Yes No Code")]
    public required char ReservedJudgementYesNoCode { get; set; }
    [CsvHelper.Configuration.Attributes.Name("RFJ Filed Date")]
    public DateTime? RFJFiledDate { get; set; }
    [CsvHelper.Configuration.Attributes.Name("RJ Multi Yes No Code")]
    public required char RJMultiYesNoCode { get; set; }
    [CsvHelper.Configuration.Attributes.Name("RJ Outstanding Yes No Code")]
    public required char RJOutstandingYesNoCode { get; set; }
    [CsvHelper.Configuration.Attributes.Name("Age in Days")]
    public required int AgeInDays { get; set; }
    [CsvHelper.Configuration.Attributes.Name("Age in Months")]
    public required float AgeInMonths { get; set; }
}

// public class ReservedJudgement : ClassMap<ReservedJudgement>
// {
//     public DateTime AppearanceDate { get; set; }
//     public required string CourtClass { get; set; }
//     public required string CourtFileNumber { get; set; }
//     public required string FileNumber { get; set; }
//     public required string AdjudicatorLastNameFirstName { get; set; }
//     public required string AdjudicatorTypeDescription { get; set; }
//     public required string FacilityCode { get; set; }
//     public required string FacilityDescription { get; set; }
//     public required char ReservedJudgementYesNoCode { get; set; }
//     public DateTime? RFJFiledDate { get; set; }
//     public required char RJMultiYesNoCode { get; set; }
//     public required char RJOutstandingYesNoCode { get; set; }
//     public required int AgeInDays { get; set; }
//     public required float AgeInMonths { get; set; }
// }