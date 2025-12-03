import { beautifyDate } from '@/filters';
import { useCivilFileStore, useCriminalFileStore } from '@/stores';
import { civilDocumentType } from '@/types/civil/jsonTypes';
import { documentType } from '@/types/criminal/jsonTypes';
import { CourtDocumentType, DocumentData } from '@/types/shared';

const ROP = 'rop';
const CSR = 'CSR';
const TRANSCRIPT = 'transcript';

export interface TranscriptDocumentType extends civilDocumentType {
  transcriptOrderId: number;
  transcriptDocumentId: number;
}

export const prepareCriminalDocumentData = (data) => {
  const criminalFileStore = useCriminalFileStore();
  const isTranscript = data.category?.toLowerCase() === TRANSCRIPT;

  const documentData: DocumentData = {
    courtClass:
      criminalFileStore.criminalFileInformation.detailsData.courtClassCd,
    courtLevel:
      criminalFileStore.criminalFileInformation.detailsData.courtLevelCd,
    dateFiled: beautifyDate(data.date),
    documentId: isTranscript ? undefined : data.imageId,
    documentDescription:
      data.category?.toLowerCase() === ROP
        ? 'Record of Proceedings'
        : data.documentTypeDescription,
    fileId: criminalFileStore.criminalFileInformation.fileNumber,
    fileNumberText:
      criminalFileStore.criminalFileInformation.detailsData.fileNumberTxt,
    partId: data.partId,
    profSeqNo: data.profSeqNo,
    location:
      criminalFileStore.criminalFileInformation.detailsData
        .homeLocationAgencyName,
    isCriminal: true,
    partyName: data.fullName,
  };

  // Add transcript metadata if this is a transcript document
  if (isTranscript) {
    documentData.orderId = data.transcriptOrderId;
    documentData.transcriptDocumentId = data.transcriptDocumentId;
  }

  return documentData;
};

export const prepareCivilDocumentData = (data: civilDocumentType) => {
  const isTranscript = data.category?.toLowerCase() === TRANSCRIPT;

  const civilFileStore = useCivilFileStore();
  const documentData: DocumentData = {
    appearanceDate: beautifyDate(data.lastAppearanceDt),
    appearanceId: data.appearanceId ?? data.civilDocumentId,
    dateFiled: beautifyDate(data.filedDt),
    documentDescription: data.documentTypeDescription,
    documentId:
      data.category === 'Reference' ? data.imageId : data.civilDocumentId,
    fileId: civilFileStore.civilFileInformation?.fileNumber,
    fileNumberText:
      civilFileStore.civilFileInformation?.detailsData?.fileNumberTxt,
    courtClass: civilFileStore.civilFileInformation?.detailsData?.courtClassCd,
    courtLevel: civilFileStore.civilFileInformation?.detailsData?.courtLevelCd,
    location:
      civilFileStore.civilFileInformation?.detailsData?.homeLocationAgencyName,
    isCriminal: false,
  };

  // Add transcript metadata if this is a transcript document
  if (isTranscript) {
    const transcriptData = data as TranscriptDocumentType;
    documentData.orderId = transcriptData.transcriptOrderId?.toString();
    documentData.transcriptDocumentId =
      transcriptData.transcriptDocumentId?.toString();
  }

  return documentData;
};

export const getCriminalDocumentType = (
  data: documentType
): CourtDocumentType => {
  if (data.category?.toLowerCase() === ROP) {
    return CourtDocumentType.ROP;
  }
  if (data.category?.toLowerCase() === TRANSCRIPT) {
    return CourtDocumentType.Transcript;
  }
  return CourtDocumentType.Criminal;
};

export const getCivilDocumentType = (
  data: civilDocumentType
): CourtDocumentType => {
  if (data.category?.toLowerCase() === TRANSCRIPT) {
    return CourtDocumentType.Transcript;
  }
  return data.documentTypeCd == CSR
    ? CourtDocumentType.CSR
    : CourtDocumentType.Civil;
};

// Move this mapping to the BE
export const formatDocumentCategory = (document: documentType) => {
  let category = document?.category;
  if (!category) return category;
  if (category === 'PSR') category = 'Report';
  else if (category === 'rop') category = 'ROP';
  else if (category?.toLowerCase() === TRANSCRIPT) category = 'Transcript';
  return category;
};

export const formatDocumentType = (document: documentType) =>
  document.category === 'rop'
    ? 'Record of Proceedings'
    : document.documentTypeDescription;
