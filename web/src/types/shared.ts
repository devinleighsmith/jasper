export interface DocumentData {
  appearanceDate?: string;
  appearanceId?: string;
  courtClass?: string;
  courtLevel: string;
  dateFiled?: string;
  documentDescription?: string;
  documentId?: string;
  fileId?: string;
  fileNumberText: string;
  location: string;
  partId?: string;
  partyName?: string;
  profSeqNo?: string;
}

export enum CourtDocumentType {
  Civil,
  ProvidedCivil,
  CSR,
  Criminal,
  ROP,
  CriminalZip,
  CivilZip,
}

export interface fileDetailsType {
  responseCd: string;
  responseMessageTxt: string;
  fileNumberTxt: string;
  courtClassCd: string;
  courtClassDescription: string;
  courtLevelCd: string;
  courtLevelDescription: string;
  activityClassCd: string;
  activityClassDesc: string;
  homeLocationAgenId: string;
  homeLocationAgencyName: string;
  homeLocationAgencyCode: string;
  homeLocationRegionName: string;
}
