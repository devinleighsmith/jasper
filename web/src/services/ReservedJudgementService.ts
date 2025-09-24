import { IHttpService } from './HttpService';
import { ServiceBase } from './ServiceBase';

export class ReservedJudgementService extends ServiceBase {
  constructor(httpService: IHttpService) {
    super(httpService);
  }

  get(
    queryParams: Record<string, any> | undefined
  ): Promise<ReservedJudgementModel[]> {
    return this.httpService.get<ReservedJudgementModel[]>(
      `api/reservedJudgements`,
      queryParams,
      { skipErrorHandler: true }
    );
  }
}

// extract this out elsewhere
export type ReservedJudgementModel = {
    appearanceDate: string;
    courtClass: string;
    fileNumber: string;
    adjudicatorLastNameFirstName: string;
    adjudicatorTypeDescription: string;
    facilityCode: string;
    facilityDescription: string;
    reservedJudgementYesNoCode: string;
    rfjFiledDate?: string;
    rjMultiYesNoCode: string;
    rjOutstandingYesNoCode: string;
    ageInDays: number;
    updatedDate?: string;
};