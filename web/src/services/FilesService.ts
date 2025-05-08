import { CourtFileSearchResponse } from '@/types/courtFileSearch';
import { HttpService } from './HttpService';
import { CivilAppearanceDetails } from '@/types/civil/jsonTypes/index';

export class FilesService {
  private httpService: HttpService;
  private readonly baseUrl: string = 'api/files';

  constructor(httpService: HttpService) {
    this.httpService = httpService;
  }

  async searchCriminalFiles(
    queryParams: any
  ): Promise<CourtFileSearchResponse> {
    return await this.httpService.get<CourtFileSearchResponse>(
      `${this.baseUrl}/criminal/search`,
      queryParams
    );
  }

  async searchCivilFiles(queryParams: any): Promise<CourtFileSearchResponse> {
    return await this.httpService.get<CourtFileSearchResponse>(
      `${this.baseUrl}/civil/search?`,
      queryParams
    );
  }
  //civil/{fileId}/appearance-detail/{appearanceId}
  async civilAppearanceDetails(fileId: string, appearanceId: string): Promise<CivilAppearanceDetails> {
    return this.httpService.get<any>(
      `${this.baseUrl}/civil/${fileId}/appearance-detail/${appearanceId}`
    );
  }

  async PCSScivilAppearanceDetails(fileId: string, appearanceId: string): Promise<any> {
    return this.httpService.get<any>(
      `${this.baseUrl}/pcss/civil/${fileId}/appearance-detail/${appearanceId}`
    );
  }
}
