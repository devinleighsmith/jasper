import { AdditionalProperties } from '../../common';
import { fileDetailsType } from '@/types/shared';
import { apprDetailType } from '../../shared';

export interface countSentenceType {
  judgesRecommendation: string;
  sentenceTypeDesc: string;
  sntpCd: string;
  sentTermPeriodQty: string;
  sentTermCd: string;
  sentSubtermPeriodQty: string;
  sentSubtermCd: string;
  sentTertiaryTermPeriodQty: string;
  sentTertiaryTermCd: string;
  sentIntermittentYn: string;
  sentMonetaryAmt: string;
  sentDueTtpDt: string;
  sentEffectiveDt: string;
  sentDetailTxt: string;
  sentYcjaAdultYouthCd: string;
  sentCustodySecureYn: string;
  additionalProperties: AdditionalProperties;
  additionalProp1: {};
  additionalProp2: {};
  additionalProp3: {};
}

export interface countType {
  partId: string;
  appearanceDate: string;
  findingDsc: string;
  appcId: string;
  appearanceReason: string;
  sentence: countSentenceType[];
  countNumber: string;
  appearanceResult: string;
  finding: string;
  sectionTxt: string;
  sectionDscTxt: string;
  additionalProperties: AdditionalProperties;
  additionalProp1: {};
  additionalProp2: {};
  additionalProp3: {};
}

export interface documentType {
  partId: string;
  category: string;
  documentTypeDescription: string;
  hasFutureAppearance: boolean;
  docmClassification: string;
  docmId: string;
  issueDate: string;
  docmFormId: string;
  docmFormDsc: string;
  docmDispositionDsc: string;
  docmDispositionDate: string;
  imageId: string;
  documentPageCount: string;
  additionalProperties: AdditionalProperties;
  additionalProp1: {};
  additionalProp2: {};
  additionalProp3: {};
}

export interface chargeType {
  sectionTxt: string;
  sectionDscTxt: string;
  additionalProperties: AdditionalProperties;
  additionalProp1: {};
  additionalProp2: {};
  additionalProp3: {};
}

export interface banType {
  partId: string;
  banTypeCd: string;
  banTypeDescription: string;
  banTypeAct: string;
  banTypeSection: string;
  banTypeSubSection: string;
  banStatuteId: string;
  banCommentText: string;
  banOrderedDate: string;
  banSeqNo: string;
  additionalProperties: AdditionalProperties;
  additionalProp1: {};
  additionalProp2: {};
  additionalProp3: {};
}

export interface witnessType {
  fullName: string;
  witnessTypeDsc: string;
  agencyDsc: string;
  agencyCd: string;
  partId: string;
  lastNm: string;
  givenNm: string;
  witnessTypeCd: string;
  roleTypeCd: string;
  requiredYN: string;
  confidentialYN: string;
  pinCodeTxt: string;
  agencyId: string;
  additionalProperties: AdditionalProperties;
  additionalProp1: {};
  additionalProp2: {};
  additionalProp3: {};
}

export interface criminalTrialRemarkType {
  commentTxt: string;
  additionalProperties: AdditionalProperties;
  additionalProp1: {};
  additionalProp2: {};
  additionalProp3: {};
}

export interface personType {
  givenNm: string;
  lastNm: string;
}

export interface criminalParticipantType extends personType {
  fullName: string;
  document: documentType[];
  hideJustinCounsel: boolean;
  count: countType[];
  ban: banType[];
  partId: string;
  profSeqNo: string;
  orgNm: string;
  warrantYN: string;
  inCustodyYN: string;
  interpreterYN: string;
  detainedYN: string;
  birthDt: string;
  counselRrepId: string;
  counselPartId: string;
  counselLastNm: string;
  counselGivenNm: string;
  counselRelatedRepTypeCd: string;
  counselEnteredDt: string;
  designatedCounselYN: string;
  charge: chargeType[];
  additionalProperties: AdditionalProperties;
  additionalProp1: {};
  additionalProp2: {};
  additionalProp3: {};
}

export interface criminalHearingRestrictionType {
  hearingRestrictionTypeDsc: string;
  hearingRestrictionId: string;
  adjPartId: string;
  adjFullNm: string;
  hearingRestrictionTypeCd: string;
  partId: string;
  profSeqNo: string;
  partNm: string;
  adjInitialsTxt: string;
  justinNo: string;
  hearingRestrictionCcn: string;
  additionalProperties: AdditionalProperties;
  additionalProp1: {};
  additionalProp2: {};
  additionalProp3: {};
}

export interface crownType {
  partId: string;
  assigned: boolean;
  fullName: string;
  lastNm: string;
  givenNm: string;
}

export interface criminalApprDetailType extends apprDetailType, personType {
  counselFullNm: string;
  partId: string;
  profSeqNo: string;
  orgNm: string;
}

export interface criminalAppearancesType {
  responseCd: string;
  responseMessageTxt: string;
  futureRecCount: string;
  historyRecCount: string;
  apprDetail: criminalApprDetailType[];
  additionalProperties: AdditionalProperties;
  additionalProp1: {};
  additionalProp2: {};
  additionalProp3: {};
}
export interface criminalFileDetailsType extends fileDetailsType {
  justinNo: string;
  currentEstimateLenQty: string;
  currentEstimateLenUnit: string;
  initialEstimateLenQty: string;
  initialEstimateLenUnit: string;
  trialStartDt: string;
  mdocSubCategoryDsc: string;
  mdocCcn: string;
  assignedPartNm: string;
  approvedByAgencyCd: string;
  approvedByPartNm: string;
  approvalCrownAgencyTypeCd: string;
  crownEstimateLenQty: number;
  crownEstimateLenDsc: string;
  crownEstimateLenUnit: string;
  caseAgeDays: string;
  indictableYN: string;
  complexityTypeCd: string;
  assignmentTypeDsc: string;
  trialRemark: criminalTrialRemarkType[];
  participant: criminalParticipantType[];
  witness: witnessType[];
  crown: crownType[];
  hearingRestriction: criminalHearingRestrictionType[];
  appearances: criminalAppearancesType;
}