import { Location } from '@/types';
import { CourtRoomsJsonInfoType } from '../types/common';
import { LocationInfo, roomsInfoType } from '../types/courtlist';
import { HttpService } from './HttpService';

export class LocationService {
  private readonly httpService: HttpService;

  constructor(httpService: HttpService) {
    this.httpService = httpService;
  }

  async getCourtRooms(): Promise<roomsInfoType[]> {
    const courtRoomsJson = await this.httpService.get<CourtRoomsJsonInfoType[]>(
      'api/location/court-rooms'
    );

    courtRoomsJson.sort((a, b) =>
      a.name.toLocaleLowerCase().localeCompare(b.name.toLowerCase())
    );
    const courtRooms: roomsInfoType[] = [];

    courtRoomsJson.forEach((cr) => {
      courtRooms.push({
        text: cr.name,
        value: cr.code,
      });
    });

    return courtRooms;
  }

  async getDashboardLocations(): Promise<Location[]> {
    return await this.httpService.get<Location[]>('api/location/pcss');
  }

  async getLocationsAndCourtRooms(): Promise<LocationInfo[]> {
    // todo: replace this test data with actual api call
    const testData = [
      {
        name: 'Test Court',
        code: '1',
        locationId: '1',
        justinLocationName: 'Test Court',
        justinLocationId: '2',
        active: true,
        courtRooms: [
          {
            room: '1',
            locationId: '1',
            type: 'Courtroom',
          },
          {
            room: '2',
            locationId: '1',
            type: 'Courtroom',
          },
          {
            room: '3',
            locationId: '1',
            type: 'Courtroom',
          },
          {
            room: '4',
            locationId: '1',
            type: 'Courtroom',
          },
        ],
      },
    ] as LocationInfo[];
    return testData;
  }
}
