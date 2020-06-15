export class UploadDto { 
    public file : FormData;
    public ready: boolean = false;
    public imagePath : string;
    public imageUrl : any;
    
    constructor (file: FormData) {
        this.file = file;
    };
}