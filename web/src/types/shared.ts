import { AdditionalProperties } from '@/types/common';

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
  appearances: appearancesType;
  activityClassCd: string;
  activityClassDesc: string;
  homeLocationAgenId: string;
  homeLocationAgencyName: string;
  homeLocationAgencyCode: string;
  homeLocationRegionName: string;
}

export interface appearancesType {
  responseCd: string;
  responseMessageTxt: string;
  futureRecCount: string;
  historyRecCount: string;
  apprDetail: apprDetailType[];
  additionalProperties: AdditionalProperties;
  additionalProp1: {};
  additionalProp2: {};
  additionalProp3: {};
}

export interface apprDetailType {
  historyYN: string;
  appearanceId: string;
  appearanceDt: string;
  appearanceTm: string;
  appearanceReasonCd: string;
  courtAgencyId: string;
  courtRoomCd: string;
  judgeFullNm: string;
  judgeInitials: string;
  estimatedTimeHour: string;
  estimatedTimeMin: string;
  partOfTrialYN: string;
  appearanceStatusCd: string;
  appearanceResultCd: string;
  appearanceReasonDsc?: string;
  appearanceCcn: string;
  courtLocation?: string;
  supplementalEquipmentTxt: string;
  securityRestrictionTxt: string;
  outOfTownJudgeTxt: string;
  additionalProperties: AdditionalProperties;
  additionalProp1: {};
  additionalProp2: {};
  additionalProp3: {};
}

export interface personType {
  givenNm: string;
  lastNm: string;
}
