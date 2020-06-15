export class MapCoordinates {
    id: number;
    coordinates: Array<Marker> = [];
}

export class Marker {
    constructor(public latin: number, public lang: number) {
        this.lat = latin;
        this.lng = lang;
    }
	private lat: number;
	private lng: number;
}
